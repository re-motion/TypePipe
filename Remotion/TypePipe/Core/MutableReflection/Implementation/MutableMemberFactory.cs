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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Scripting.Ast;
using Remotion.Reflection.MemberSignatures;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.Utilities;
using Remotion.FunctionalProgramming;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// Implements <see cref="IMutableMemberFactory"/>.
  /// </summary>
  public class MutableMemberFactory : IMutableMemberFactory
  {
    private readonly IMemberSelector _memberSelector;
    private readonly IRelatedMethodFinder _relatedMethodFinder;

    public MutableMemberFactory (IMemberSelector memberSelector, IRelatedMethodFinder relatedMethodFinder)
    {
      ArgumentUtility.CheckNotNull ("memberSelector", memberSelector);
      ArgumentUtility.CheckNotNull ("relatedMethodFinder", relatedMethodFinder);

      _memberSelector = memberSelector;
      _relatedMethodFinder = relatedMethodFinder;
    }

    public Expression CreateInitialization (ProxyType declaringType, Func<InitializationBodyContext, Expression> initializationProvider)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("initializationProvider", initializationProvider);

      var context = new InitializationBodyContext (declaringType, _memberSelector);
      return BodyProviderUtility.GetNonNullBody (initializationProvider, context);
    }

    public MutableFieldInfo CreateField (ProxyType declaringType, string name, Type type, FieldAttributes attributes)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("type", type);

      if (type == typeof (void))
        throw new ArgumentException ("Field cannot be of type void.", "type");

      var signature = new FieldSignature (type);
      if (declaringType.AddedFields.Any (f => f.Name == name && FieldSignature.Create (f).Equals (signature)))
        throw new InvalidOperationException ("Field with equal name and signature already exists.");

      return new MutableFieldInfo (declaringType, name, type, attributes);
    }

    public MutableConstructorInfo CreateConstructor (
        ProxyType declaringType,
        MethodAttributes attributes,
        IEnumerable<ParameterDeclaration> parameters,
        Func<ConstructorBodyCreationContext, Expression> bodyProvider)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("parameters", parameters);
      ArgumentUtility.CheckNotNull ("bodyProvider", bodyProvider);

      var invalidAttributes =
          new[]
          {
              MethodAttributes.Abstract, MethodAttributes.PinvokeImpl, MethodAttributes.RequireSecObject,
              MethodAttributes.UnmanagedExport, MethodAttributes.Virtual
          };
      CheckForInvalidAttributes ("constructor", invalidAttributes, attributes);

      var isStatic = attributes.IsSet (MethodAttributes.Static);
      var paras = parameters.ConvertToCollection();
      if (isStatic && paras.Count != 0)
        throw new ArgumentException ("A type initializer (static constructor) cannot have parameters.", "parameters");

      var signature = new MethodSignature (typeof (void), paras.Select (p => p.Type), 0);
      if (declaringType.AddedConstructors.Any (ctor => ctor.IsStatic == isStatic && MethodSignature.Create (ctor).Equals (signature)))
        throw new InvalidOperationException ("Constructor with equal signature already exists.");

      var parameterExpressions = paras.Select (p => p.Expression);
      var context = new ConstructorBodyCreationContext (declaringType, isStatic, parameterExpressions, _memberSelector);
      var body = BodyProviderUtility.GetTypedBody (typeof (void), bodyProvider, context);

      return new MutableConstructorInfo (declaringType, attributes, paras, body);
    }

    public MutableMethodInfo CreateMethod (
        ProxyType declaringType,
        string name,
        MethodAttributes attributes,
        Type returnType,
        IEnumerable<ParameterDeclaration> parameters,
        Func<MethodBodyCreationContext, Expression> bodyProvider)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("returnType", returnType);
      ArgumentUtility.CheckNotNull ("parameters", parameters);
      // bodyProvider is null for abstract methods

      // TODO : virtual and static is an invalid combination
      // TODO : if it is an implicit baseMethod override, it needs the same visibility (or more public visibility?)!

      var isAbstract = attributes.IsSet (MethodAttributes.Abstract);
      if (!isAbstract && bodyProvider == null)
        throw new ArgumentNullException ("bodyProvider", "Non-abstract methods must have a body.");
      if (isAbstract && bodyProvider != null)
        throw new ArgumentException ("Abstract methods cannot have a body.", "bodyProvider");

      var invalidAttributes = new[] { MethodAttributes.PinvokeImpl, MethodAttributes.RequireSecObject, MethodAttributes.UnmanagedExport };
      CheckForInvalidAttributes ("method", invalidAttributes, attributes);

      var isVirtual = attributes.IsSet (MethodAttributes.Virtual);
      var isNewSlot = attributes.IsSet (MethodAttributes.NewSlot);
      if (isAbstract && !isVirtual)
        throw new ArgumentException ("Abstract methods must also be virtual.", "attributes");
      if (!isVirtual && isNewSlot)
        throw new ArgumentException ("NewSlot methods must also be virtual.", "attributes");

      var paras = parameters.ConvertToCollection();
      var signature = new MethodSignature (returnType, paras.Select (pd => pd.Type), genericParameterCount: 0);
      if (declaringType.AddedMethods.Any (m => m.Name == name && MethodSignature.Create (m).Equals (signature)))
        throw new InvalidOperationException ("Method with equal name and signature already exists.");

      var baseMethod = isVirtual && !isNewSlot ? _relatedMethodFinder.GetMostDerivedVirtualMethod (name, signature, declaringType.BaseType) : null;
      if (baseMethod != null)
        CheckNotFinalForOverride (baseMethod);

      var parameterExpressions = paras.Select (pd => pd.Expression);
      var isStatic = attributes.IsSet (MethodAttributes.Static);
      var context = new MethodBodyCreationContext (declaringType, isStatic, parameterExpressions, returnType, baseMethod, _memberSelector);
      var body = bodyProvider == null ? null : BodyProviderUtility.GetTypedBody (returnType, bodyProvider, context);

      return new MutableMethodInfo (declaringType, name, attributes, returnType, paras, baseMethod, body);
    }

    public MutableMethodInfo CreateExplicitOverride (
        ProxyType declaringType, MethodInfo overriddenMethodBaseDefinition, Func<MethodBodyCreationContext, Expression> bodyProvider)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("overriddenMethodBaseDefinition", overriddenMethodBaseDefinition);
      ArgumentUtility.CheckNotNull ("bodyProvider", bodyProvider);

      return PrivateCreateExplicitOverrideAllowAbstract (declaringType, overriddenMethodBaseDefinition, bodyProvider);
    }

    public MutableMethodInfo GetOrCreateOverride (ProxyType declaringType, MethodInfo baseMethod, out bool isNewlyCreated)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("baseMethod", baseMethod);
      Assertion.IsNotNull (baseMethod.DeclaringType);

      // ReSharper disable PossibleUnintendedReferenceComparison
      if (!baseMethod.DeclaringType.IsAssignableFromFast (declaringType) || declaringType == baseMethod.DeclaringType)
      // ReSharper restore PossibleUnintendedReferenceComparison
      {
        var message = string.Format (
            "Method is declared by a type outside of the proxy base class hierarchy: '{0}'.", baseMethod.DeclaringType.Name);
        throw new ArgumentException (message, "baseMethod");
      }

      if (!baseMethod.IsVirtual)
        throw new NotSupportedException ("Only virtual methods can be overridden.");

      if (baseMethod.DeclaringType.IsInterface)
      {
        baseMethod = GetOrCreateImplementationMethod (declaringType, baseMethod, out isNewlyCreated);
        if (baseMethod is MutableMethodInfo)
          return (MutableMethodInfo) baseMethod;

        Assertion.IsTrue (baseMethod.IsVirtual, "It's possible to get an interface implementation that is not virtual (in verifiable code).");
      }

      var baseDefinition = baseMethod.GetBaseDefinition();
      var existingMutableOverride = _relatedMethodFinder.GetOverride (baseDefinition, declaringType.AddedMethods);
      if (existingMutableOverride != null)
      {
        isNewlyCreated = false;
        return existingMutableOverride;
      }
      isNewlyCreated = true;

      var overrideBaseMethod = _relatedMethodFinder.GetMostDerivedOverride (baseDefinition, declaringType.BaseType);
      CheckNotFinalForOverride (overrideBaseMethod);
      var bodyProviderOrNull =
          overrideBaseMethod.IsAbstract
              ? null
              : new Func<MethodBodyCreationContext, Expression> (ctx => ctx.CallBase (overrideBaseMethod, ctx.Parameters.Cast<Expression>()));

      var methods = declaringType.GetMethods (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
      var needsExplicitOverride = _relatedMethodFinder.IsShadowed (baseDefinition, methods);
      if (needsExplicitOverride)
        return PrivateCreateExplicitOverrideAllowAbstract (declaringType, baseDefinition, bodyProviderOrNull);

      var attributes = MethodOverrideUtility.GetAttributesForImplicitOverride (overrideBaseMethod);
      var parameters = ParameterDeclaration.CreateForEquivalentSignature (baseDefinition);

      return CreateMethod (declaringType, overrideBaseMethod.Name, attributes, overrideBaseMethod.ReturnType, parameters, bodyProviderOrNull);
    }

    public MutablePropertyInfo CreateProperty (
        ProxyType declaringType,
        string name,
        Type type,
        IEnumerable<ParameterDeclaration> indexParameters,
        Func<MethodBodyCreationContext, Expression> getBodyProvider,
        Func<MethodBodyCreationContext, Expression> setBodyProvider)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("type", type);
      // Index parameters may be null.
      // Get body provider may be null.
      // Set body provider may be null.

      if (getBodyProvider == null && setBodyProvider == null)
        throw new ArgumentException ("At least one accessor body provider must be specified.", "getBodyProvider");

      var indexParams = indexParameters != null ? indexParameters.ConvertToCollection() : ParameterDeclaration.EmptyParameters;
      var signature = new PropertySignature (type, indexParams.Select (pd => pd.Type));
      if (declaringType.AddedProperties.Any (p => p.Name == name && PropertySignature.Create (p).Equals (signature)))
        throw new InvalidOperationException ("Property with equal name and signature already exists.");

      var attributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
      MutableMethodInfo getMethod = null, setMethod = null;
      if (getBodyProvider != null)
        getMethod = CreateMethod (declaringType, "get_" + name, attributes, type, indexParams, getBodyProvider);
      if (setBodyProvider != null)
      {
        var setterParams = indexParams.Concat (new ParameterDeclaration (type, "value"));
        setMethod = CreateMethod (declaringType, "set_" + name, attributes, typeof (void), setterParams, setBodyProvider);
      }

      return new MutablePropertyInfo (declaringType, name, PropertyAttributes.None, getMethod, setMethod);
    }

    public MutablePropertyInfo CreateProperty (
        ProxyType declaringType, string name, PropertyAttributes attributes, MutableMethodInfo getMethod, MutableMethodInfo setMethod)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      // Get method may be null.
      // Set method may be null.

      if (getMethod == null && setMethod == null)
        throw new ArgumentException ("Property must have at least one accessor.", "getMethod");

      var readWriteProperty = getMethod != null && setMethod != null;
      if (readWriteProperty && getMethod.IsStatic != setMethod.IsStatic)
        throw new ArgumentException ("Accessor methods must be both either static or non-static.", "getMethod");

      if (getMethod != null && !ReferenceEquals (getMethod.DeclaringType, declaringType))
        throw new ArgumentException ("Accessor method must be declared on the current type.", "getMethod");
      if (setMethod != null && !ReferenceEquals (setMethod.DeclaringType, declaringType))
        throw new ArgumentException ("Accessor method must be declared on the current type.", "setMethod");

      if (getMethod != null && getMethod.ReturnType == typeof (void))
        throw new ArgumentException ("Get accessor must be a non-void method.", "getMethod");
      if (setMethod != null && setMethod.ReturnType != typeof (void))
        throw new ArgumentException ("Set accessor must have return type void.", "setMethod");

      // TODO: index parameters: need to match if "full property"

      // TODO 5421: argument checks (no property with same name and signature)

      return new MutablePropertyInfo (declaringType, name, attributes, getMethod, setMethod);
    }

    private MutableMethodInfo PrivateCreateExplicitOverrideAllowAbstract (
        ProxyType proxyType, MethodInfo overriddenMethodBaseDefinition, Func<MethodBodyCreationContext, Expression> bodyProviderOrNull)
    {
      Assertion.IsTrue (bodyProviderOrNull != null || overriddenMethodBaseDefinition.IsAbstract);

      var name = MethodOverrideUtility.GetNameForExplicitOverride (overriddenMethodBaseDefinition);
      var attributes = MethodOverrideUtility.GetAttributesForExplicitOverride (overriddenMethodBaseDefinition);
      if (bodyProviderOrNull != null)
        attributes = attributes.Unset (MethodAttributes.Abstract);
      var parameters = ParameterDeclaration.CreateForEquivalentSignature (overriddenMethodBaseDefinition);

      var method = CreateMethod (proxyType, name, attributes, overriddenMethodBaseDefinition.ReturnType, parameters, bodyProviderOrNull);
      method.AddExplicitBaseDefinition (overriddenMethodBaseDefinition);

      return method;
    }

    private MethodInfo GetOrCreateImplementationMethod (ProxyType proxyType, MethodInfo ifcMethod, out bool isNewlyCreated)
    {
      var interfaceMap = proxyType.GetInterfaceMap (ifcMethod.DeclaringType, allowPartialInterfaceMapping: true);
      var index = Array.IndexOf (interfaceMap.InterfaceMethods, ifcMethod);
      var implementation = interfaceMap.TargetMethods[index];

      if (implementation == null)
      {
        var parameters = ParameterDeclaration.CreateForEquivalentSignature (ifcMethod);
        try
        {
          isNewlyCreated = true;
          return CreateMethod (proxyType, ifcMethod.Name, ifcMethod.Attributes, ifcMethod.ReturnType, parameters, bodyProvider: null);
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

    private void CheckNotFinalForOverride (MethodInfo overridenMethod)
    {
      if (overridenMethod.IsFinal)
      {
        Assertion.IsNotNull (overridenMethod.DeclaringType);
        var message = string.Format ("Cannot override final method '{0}.{1}'.", overridenMethod.DeclaringType.Name, overridenMethod.Name);
        throw new NotSupportedException (message);
      }
    }

    private void CheckForInvalidAttributes (string memberKind, MethodAttributes[] invalidAttributes, MethodAttributes attributes)
    {
      var hasInvalidAttributes = invalidAttributes.Any (x => attributes.IsSet (x));
      if (hasInvalidAttributes)
      {
        var invalidAttributeList = string.Join (", ", invalidAttributes.Select (x => Enum.GetName (typeof (MethodAttributes), x)).ToArray());
        var message = string.Format ("The following MethodAttributes are not supported for {0}s: {1}.", memberKind, invalidAttributeList);
        throw new ArgumentException (message, "attributes");
      }
    }
  }
}