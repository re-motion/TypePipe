// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Scripting.Ast;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Implementation.MemberFactory
{
  /// <summary>
  /// A factory that retrieves or creates method overrides.
  /// </summary>
  public class MethodOverrideFactory
  {
    private readonly IRelatedMethodFinder _relatedMethodFinder;
    private readonly IMethodFactory _methodFactory;

    public MethodOverrideFactory (IRelatedMethodFinder relatedMethodFinder, IMethodFactory methodFactory)
    {
      ArgumentUtility.CheckNotNull ("relatedMethodFinder", relatedMethodFinder);
      ArgumentUtility.CheckNotNull ("methodFactory", methodFactory);

      _relatedMethodFinder = relatedMethodFinder;
      _methodFactory = methodFactory;
    }

    public MutableMethodInfo CreateExplicitOverride (
        ProxyType declaringType, MethodInfo overriddenMethodBaseDefinition, Func<MethodBodyCreationContext, Expression> bodyProvider)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("overriddenMethodBaseDefinition", overriddenMethodBaseDefinition);
      ArgumentUtility.CheckNotNull ("bodyProvider", bodyProvider);

      return PrivateCreateExplicitOverrideAllowAbstract (declaringType, overriddenMethodBaseDefinition, bodyProvider);
    }

    public MutableMethodInfo GetOrCreateOverride (ProxyType declaringType, MethodInfo overriddenMethod, out bool isNewlyCreated)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("overriddenMethod", overriddenMethod);
      Assertion.IsNotNull (overriddenMethod.DeclaringType);

      if (!overriddenMethod.IsVirtual)
        throw new ArgumentException ("Only virtual methods can be overridden.", "overriddenMethod");

      if (overriddenMethod.IsGenericMethodInstantiation())
      {
        throw new ArgumentException (
            "The specified method must be either a non-generic method or a generic method definition; it cannot be a method instantiation.",
            "overriddenMethod");
      }

      // ReSharper disable PossibleUnintendedReferenceComparison
      if (!overriddenMethod.DeclaringType.IsTypePipeAssignableFrom (declaringType) || declaringType == overriddenMethod.DeclaringType)
          // ReSharper restore PossibleUnintendedReferenceComparison
      {
        var message = string.Format (
            "Method is declared by a type outside of the proxy base class hierarchy: '{0}'.", overriddenMethod.DeclaringType.Name);
        throw new ArgumentException (message, "overriddenMethod");
      }

      if (overriddenMethod.DeclaringType.IsInterface)
      {
        overriddenMethod = GetOrCreateImplementationMethod (declaringType, overriddenMethod, out isNewlyCreated);
        if (overriddenMethod is MutableMethodInfo)
          return (MutableMethodInfo) overriddenMethod;

        Assertion.IsTrue (overriddenMethod.IsVirtual, "It's possible to get an interface implementation that is not virtual (in verifiable code).");
      }

      var baseDefinition = overriddenMethod.GetBaseDefinition();
      var existingMutableOverride = _relatedMethodFinder.GetOverride (baseDefinition, declaringType.AddedMethods);
      if (existingMutableOverride != null)
      {
        isNewlyCreated = false;
        return existingMutableOverride;
      }
      isNewlyCreated = true;

      var baseMethod = GetBaseMethod (declaringType, baseDefinition);
      var bodyProvider = CreateBodyProvider (baseMethod);

      var methods = declaringType.GetMethods (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
      var needsExplicitOverride = _relatedMethodFinder.IsShadowed (baseDefinition, methods);
      if (needsExplicitOverride)
        return PrivateCreateExplicitOverrideAllowAbstract (declaringType, baseDefinition, bodyProvider);

      var attributes = MethodOverrideUtility.GetAttributesForImplicitOverride (baseMethod);
      return CreateOverride (baseMethod, declaringType, baseMethod.Name, attributes, bodyProvider);
    }

    private MethodInfo GetBaseMethod (ProxyType declaringType, MethodInfo baseDefinition)
    {
      var baseMethod = _relatedMethodFinder.GetMostDerivedOverride (baseDefinition, declaringType.BaseType);
      if (baseMethod.IsFinal)
      {
        Assertion.IsNotNull (baseMethod.DeclaringType);
        var message = string.Format ("Cannot override final method '{0}.{1}'.", baseMethod.DeclaringType.Name, baseMethod.Name);
        throw new NotSupportedException (message);
      }

      return baseMethod;
    }

    private static Func<MethodBodyCreationContext, Expression> CreateBodyProvider (MethodInfo baseMethod)
    {
      if (baseMethod.IsAbstract)
        return null;

      if (baseMethod.IsGenericMethodDefinition)
        return ctx => ctx.CallBase (baseMethod.MakeTypePipeGenericMethod (ctx.GenericParameters.ToArray()), ctx.Parameters.Cast<Expression>());
      else
        return ctx => ctx.CallBase (baseMethod, ctx.Parameters.Cast<Expression>());
    }

    private MutableMethodInfo PrivateCreateExplicitOverrideAllowAbstract (
        ProxyType declaringType, MethodInfo overriddenMethodBaseDefinition, Func<MethodBodyCreationContext, Expression> bodyProviderOrNull)
    {
      Assertion.IsTrue (bodyProviderOrNull != null || overriddenMethodBaseDefinition.IsAbstract);

      var name = MethodOverrideUtility.GetNameForExplicitOverride (overriddenMethodBaseDefinition);
      var attributes = MethodOverrideUtility.GetAttributesForExplicitOverride (overriddenMethodBaseDefinition);
      if (bodyProviderOrNull != null)
        attributes = attributes.Unset (MethodAttributes.Abstract);

      var method = CreateOverride (overriddenMethodBaseDefinition, declaringType, name, attributes, bodyProviderOrNull);
      method.AddExplicitBaseDefinition (overriddenMethodBaseDefinition);

      return method;
    }

    private MethodInfo GetOrCreateImplementationMethod (ProxyType declaringType, MethodInfo ifcMethod, out bool isNewlyCreated)
    {
      var interfaceMap = declaringType.GetInterfaceMap (ifcMethod.DeclaringType, allowPartialInterfaceMapping: true);
      var index = Array.IndexOf (interfaceMap.InterfaceMethods, ifcMethod);
      var implementation = interfaceMap.TargetMethods[index];

      if (implementation == null)
      {
        try
        {
          isNewlyCreated = true;
          return CreateOverride (ifcMethod, declaringType, ifcMethod.Name, ifcMethod.Attributes, bodyProvider: null);
        }
        catch (InvalidOperationException)
        {
          var message = string.Format (
              "Interface method '{0}' cannot be implemented because a method with equal name and signature already exists. "
              + "Use {1}.{2} to create an explicit implementation.",
              ifcMethod.Name,
              typeof (ProxyType).Name,
              MemberInfoFromExpressionUtility.GetMethod ((ProxyType obj) => obj.AddExplicitOverride (null, null)).Name);
          throw new InvalidOperationException (message);
        }
      }
      else
      {
        isNewlyCreated = false;
        return implementation;
      }
    }

    private MutableMethodInfo CreateOverride (
        MethodInfo overriddenMethod,
        ProxyType declaringType,
        string name,
        MethodAttributes attributes,
        Func<MethodBodyCreationContext, Expression> bodyProvider)
    {
      var md = MethodDeclaration.CreateEquivalent (overriddenMethod);
      return _methodFactory.CreateMethod (
          declaringType, name, attributes, md.GenericParameters, md.ReturnTypeProvider, md.ParameterProvider, bodyProvider);
    }
  }
}