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
using System.Reflection;
using Remotion.TypePipe.Dlr.Ast;
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
        MutableType declaringType, MethodInfo overriddenMethodBaseDefinition, Func<MethodBodyCreationContext, Expression> bodyProvider)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("overriddenMethodBaseDefinition", overriddenMethodBaseDefinition);
      ArgumentUtility.CheckNotNull ("bodyProvider", bodyProvider);

      return PrivateCreateExplicitOverrideAllowAbstract (declaringType, overriddenMethodBaseDefinition, bodyProvider);
    }

    public MutableMethodInfo GetOrCreateOverride (MutableType declaringType, MethodInfo overriddenMethod, out bool isNewlyCreated)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("overriddenMethod", overriddenMethod);
      Assertion.IsNotNull (overriddenMethod.DeclaringType);

      if (!overriddenMethod.IsVirtual)
        throw new ArgumentException ("Only virtual methods can be overridden.", "overriddenMethod");

      CheckIsNotMethodInstantiation (overriddenMethod, "overriddenMethod");

      if (!declaringType.IsSubclassOf (overriddenMethod.DeclaringType))
      {
        var message = string.Format ("Method is declared by type '{0}' outside of the proxy base class hierarchy.", overriddenMethod.DeclaringType.Name);
        throw new ArgumentException (message, "overriddenMethod");
      }

      var baseDefinition = overriddenMethod.GetBaseDefinition();
      var existingMutableOverride = _relatedMethodFinder.GetOverride (baseDefinition, declaringType.AddedMethods);
      if (existingMutableOverride != null)
      {
        isNewlyCreated = false;
        return existingMutableOverride;
      }
      isNewlyCreated = true;

      var baseMethod = _relatedMethodFinder.GetMostDerivedOverride (baseDefinition, declaringType.BaseType);
      var bodyProvider = CreateBodyProvider (baseMethod);

      var methods = declaringType.GetMethods (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
      var needsExplicitOverride = _relatedMethodFinder.IsShadowed (baseDefinition, methods);
      if (needsExplicitOverride)
        return PrivateCreateExplicitOverrideAllowAbstract (declaringType, baseDefinition, bodyProvider);

      var attributes = MethodOverrideUtility.GetAttributesForImplicitOverride (baseMethod);
      return CreateMethod (declaringType, baseMethod, baseMethod.Name, attributes, bodyProvider);
    }

    public MutableMethodInfo GetOrCreateImplementation (MutableType declaringType, MethodInfo interfaceMethod, out bool isNewlyCreated)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("interfaceMethod", interfaceMethod);
      Assertion.IsNotNull (interfaceMethod.DeclaringType);

      if (!interfaceMethod.DeclaringType.IsInterface)
        throw new ArgumentException ("The specified method is not an interface method.", "interfaceMethod");

      CheckIsNotMethodInstantiation (interfaceMethod, "interfaceMethod");

      // ReSharper disable PossibleUnintendedReferenceComparison
      if (!interfaceMethod.DeclaringType.IsTypePipeAssignableFrom (declaringType))
      // ReSharper restore PossibleUnintendedReferenceComparison
      {
        Assertion.IsNotNull (interfaceMethod.DeclaringType);
        var message = string.Format (
            "Method is declared by an interface that is not implemented by the proxy: '{0}'.", interfaceMethod.DeclaringType.Name);
        throw new ArgumentException (message, "interfaceMethod");
      }

      var baseImplementation = GetOrCreateImplementationMethod (declaringType, interfaceMethod, out isNewlyCreated);
      if (baseImplementation is MutableMethodInfo)
        return (MutableMethodInfo) baseImplementation;

      Assertion.IsTrue (baseImplementation.IsVirtual, "It is not possible to get an interface implementation that is not virtual (in verifiable code).");

      // Re-implement if final.
      if (baseImplementation.IsFinal)
      {
        if (!SubclassFilterUtility.IsVisibleFromSubclass (baseImplementation))
        {
          Assertion.IsNotNull (baseImplementation.DeclaringType);
          var message = string.Format (
              "Cannot re-implement interface method '{0}' because its base implementation on '{1}' is not accessible.",
              interfaceMethod.Name,
              baseImplementation.DeclaringType.Name);
          throw new NotSupportedException (message);
        }

        declaringType.AddInterface (interfaceMethod.DeclaringType, throwIfAlreadyImplemented: false);

        var attributes = interfaceMethod.Attributes.Unset (MethodAttributes.Abstract);
        Func<MethodBodyCreationContext, Expression> bodyProvider = ctx => ctx.DelegateToBase (baseImplementation);

        isNewlyCreated = true;
        return CreateMethod (declaringType, interfaceMethod, interfaceMethod.Name, attributes, bodyProvider);
      }

      return GetOrCreateOverride (declaringType, baseImplementation, out isNewlyCreated);
    }

    private Func<MethodBodyCreationContext, Expression> CreateBodyProvider (MethodInfo baseMethod)
    {
      if (baseMethod.IsAbstract)
        return null;

      return ctx => ctx.DelegateToBase (baseMethod);
    }

    private MutableMethodInfo PrivateCreateExplicitOverrideAllowAbstract (
        MutableType declaringType, MethodInfo overriddenMethodBaseDefinition, Func<MethodBodyCreationContext, Expression> bodyProviderOrNull)
    {
      Assertion.IsTrue (bodyProviderOrNull != null || overriddenMethodBaseDefinition.IsAbstract);

      var name = MethodOverrideUtility.GetNameForExplicitOverride (overriddenMethodBaseDefinition);
      var attributes = MethodOverrideUtility.GetAttributesForExplicitOverride (overriddenMethodBaseDefinition);
      if (bodyProviderOrNull != null)
        attributes = attributes.Unset (MethodAttributes.Abstract);

      var method = CreateMethod (declaringType, overriddenMethodBaseDefinition, name, attributes, bodyProviderOrNull);
      method.AddExplicitBaseDefinition (overriddenMethodBaseDefinition);

      return method;
    }

    private MethodInfo GetOrCreateImplementationMethod (MutableType declaringType, MethodInfo ifcMethod, out bool isNewlyCreated)
    {
      var interfaceMap = declaringType.GetInterfaceMap (ifcMethod.DeclaringType, allowPartialInterfaceMapping: true);
      var index = Array.IndexOf (interfaceMap.InterfaceMethods, ifcMethod);
      var implementation = interfaceMap.TargetMethods[index];

      if (implementation != null)
      {
        isNewlyCreated = false;
        return implementation;
      }
      isNewlyCreated = true;

      try
      {
        return CreateMethod (declaringType, ifcMethod, ifcMethod.Name, ifcMethod.Attributes, bodyProvider: null);
      }
      catch (InvalidOperationException)
      {
        var message = string.Format (
            "Interface method '{0}' cannot be implemented because a method with equal name and signature already exists. "
            + "Use AddExplicitOverride to create an explicit implementation.",
            ifcMethod.Name);
        throw new InvalidOperationException (message);
      }
    }

    private MutableMethodInfo CreateMethod (
        MutableType declaringType,
        MethodInfo template,
        string name,
        MethodAttributes attributes,
        Func<MethodBodyCreationContext, Expression> bodyProvider)
    {
      var md = MethodDeclaration.CreateEquivalent (template);
      return _methodFactory.CreateMethod (
          declaringType, name, attributes, md.GenericParameters, md.ReturnTypeProvider, md.ParameterProvider, bodyProvider);
    }

    private static void CheckIsNotMethodInstantiation (MethodInfo method, string parameterName)
    {
      if (method.IsGenericMethodInstantiation ())
      {
        throw new ArgumentException (
            "The specified method must be either a non-generic method or a generic method definition; it cannot be a method instantiation.",
            parameterName);
      }
    }
  }
}