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

namespace Remotion.TypePipe.MutableReflection
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

    public Expression CreateInitialization (
        MutableType declaringType, bool isStatic, Func<InitializationBodyContext, Expression> initializationProvider)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("initializationProvider", initializationProvider);

      var context = new InitializationBodyContext (declaringType, isStatic, _memberSelector);
      return BodyProviderUtility.GetNonNullBody (initializationProvider, context);
    }

    public MutableFieldInfo CreateMutableField (MutableType declaringType, string name, Type type, FieldAttributes attributes)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("type", type);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);

      var signature = new FieldSignature (type);
      if (declaringType.AllMutableFields.Any (f => f.Name == name && FieldSignature.Create (f).Equals (signature)))
        throw new ArgumentException ("Field with equal signature already exists.", "name");

      var descriptor = FieldDescriptor.Create (type, name, attributes);
      var field = new MutableFieldInfo (declaringType, descriptor);

      return field;
    }

    public MutableConstructorInfo CreateMutableConstructor (
        MutableType declaringType,
        MethodAttributes attributes,
        IEnumerable<ParameterDeclaration> parameterDeclarations,
        Func<ConstructorBodyCreationContext, Expression> bodyProvider)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("parameterDeclarations", parameterDeclarations);
      ArgumentUtility.CheckNotNull ("bodyProvider", bodyProvider);

      var invalidAttributes =
          new[]
          {
              MethodAttributes.Abstract, MethodAttributes.HideBySig, MethodAttributes.PinvokeImpl,
              MethodAttributes.RequireSecObject, MethodAttributes.UnmanagedExport, MethodAttributes.Virtual
          };
      CheckForInvalidAttributes ("constructor", invalidAttributes, attributes);

      if (attributes.IsSet (MethodAttributes.Static))
      {
        var method = MemberInfoFromExpressionUtility.GetMethod ((MutableType obj) => obj.AddTypeInitialization (null));
        var message = string.Format (
            "Type initializers (static constructors) cannot be added via this API, use {0}.{1} instead.", typeof (MutableType).Name, method.Name);
        throw new NotSupportedException (message);
      }

      var parameterDescriptors = ParameterDescriptor.CreateFromDeclarations (parameterDeclarations);
      var signature = new MethodSignature (typeof (void), parameterDescriptors.Select (pd => pd.Type), 0);
      if (declaringType.AllMutableConstructors.Any (ctor => signature.Equals (MethodSignature.Create (ctor))))
        throw new ArgumentException ("Constructor with equal signature already exists.", "parameterDeclarations");

      var parameterExpressions = parameterDescriptors.Select (pd => pd.Expression);
      var context = new ConstructorBodyCreationContext (declaringType, parameterExpressions, _memberSelector);
      var body = BodyProviderUtility.GetTypedBody (typeof (void), bodyProvider, context);

      var descriptor = ConstructorDescriptor.Create (attributes, parameterDescriptors, body);
      var constructor = new MutableConstructorInfo (declaringType, descriptor);

      return constructor;
    }

    public MutableMethodInfo CreateMutableMethod (
        MutableType declaringType,
        string name,
        MethodAttributes attributes,
        Type returnType,
        IEnumerable<ParameterDeclaration> parameterDeclarations,
        Func<MethodBodyCreationContext, Expression> bodyProvider)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("returnType", returnType);
      ArgumentUtility.CheckNotNull ("parameterDeclarations", parameterDeclarations);
      // bodyProvider is null for abstract methods

      // TODO XXXX: if it is an implicit method override, it needs the same visibility (or more public visibility?)!

      var isAbstract = attributes.IsSet (MethodAttributes.Abstract);
      if (!isAbstract)
        ArgumentUtility.CheckNotNull ("bodyProvider", bodyProvider);

      var invalidAttributes = new[] { MethodAttributes.PinvokeImpl, MethodAttributes.RequireSecObject, MethodAttributes.UnmanagedExport };
      CheckForInvalidAttributes ("method", invalidAttributes, attributes);

      var isVirtual = attributes.IsSet (MethodAttributes.Virtual);
      var isNewSlot = attributes.IsSet (MethodAttributes.NewSlot);
      if (isAbstract && !isVirtual)
        throw new ArgumentException ("Abstract methods must also be virtual.", "attributes");
      if (!isVirtual && isNewSlot)
        throw new ArgumentException ("NewSlot methods must also be virtual.", "attributes");

      var parameterDescriptors = ParameterDescriptor.CreateFromDeclarations (parameterDeclarations);

      var signature = new MethodSignature (returnType, parameterDescriptors.Select (pd => pd.Type), 0);
      if (declaringType.AllMutableMethods.Any (m => m.Name == name && signature.Equals (MethodSignature.Create (m))))
      {
        var message = string.Format ("Method '{0}' with equal signature already exists.", name);
        throw new ArgumentException (message, "parameterDeclarations");
      }

      var baseMethod = isVirtual && !isNewSlot ? _relatedMethodFinder.GetMostDerivedVirtualMethod (name, signature, declaringType.BaseType) : null;
      if (baseMethod != null)
        CheckNotFinalForOverride (baseMethod);

      var parameterExpressions = parameterDescriptors.Select (pd => pd.Expression);
      var isStatic = attributes.IsSet (MethodAttributes.Static);
      var context = new MethodBodyCreationContext (declaringType, parameterExpressions, isStatic, baseMethod, _memberSelector);
      var body = bodyProvider == null ? null : BodyProviderUtility.GetTypedBody (returnType, bodyProvider, context);

      var descriptor = MethodDescriptor.Create (
          name, attributes, returnType, parameterDescriptors, baseMethod, false, false, false, body);
      var method = new MutableMethodInfo (declaringType, descriptor);

      return method;
    }

    public MutableMethodInfo GetOrCreateMutableMethodOverride (MutableType declaringType, MethodInfo method, out bool isNewlyCreated)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("method", method);
      Assertion.IsNotNull (method.DeclaringType);

      // TODO 4972: Use TypeEqualityComparer (for Equals and IsSubclassOf)
// ReSharper disable CheckForReferenceEqualityInstead.1
      if (!declaringType.UnderlyingSystemType.Equals (method.DeclaringType) && !declaringType.IsSubclassOf (method.DeclaringType))
// ReSharper restore CheckForReferenceEqualityInstead.1
      {
        var message = string.Format ("Method is declared by a type outside of this type's class hierarchy: '{0}'.", method.DeclaringType.Name);
        throw new ArgumentException (message, "method");
      }

      if (!method.IsVirtual)
        throw new NotSupportedException ("A method declared in a base type must be virtual in order to be modified.");

      var baseDefinition = method.GetBaseDefinition();
      var existingMutableOverride = _relatedMethodFinder.GetOverride (baseDefinition, declaringType.AllMutableMethods);
      if (existingMutableOverride != null)
      {
        isNewlyCreated = false;
        return existingMutableOverride;
      }

      var methods = declaringType.GetMethods (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
      var needsExplicitOverride = _relatedMethodFinder.IsShadowed (baseDefinition, methods);
      var baseMethod = _relatedMethodFinder.GetMostDerivedOverride (baseDefinition, declaringType.BaseType);
      CheckNotFinalForOverride (baseMethod);

      var name = needsExplicitOverride ? MethodOverrideUtility.GetNameForExplicitOverride (baseMethod) : baseMethod.Name;
      var attributes = needsExplicitOverride
                           ? MethodOverrideUtility.GetAttributesForExplicitOverride (baseMethod)
                           : MethodOverrideUtility.GetAttributesForImplicitOverride (baseMethod);
      var returnType = baseMethod.ReturnType;
      var parameterDeclarations = ParameterDeclaration.CreateForEquivalentSignature (baseMethod);
      var bodyProvider = baseMethod.IsAbstract
                             ? null
                             : new Func<MethodBodyCreationContext, Expression> (
                                   ctx => ctx.GetBaseCall (baseMethod, ctx.Parameters.Cast<Expression>()));

      var addedOverride = CreateMutableMethod (
          declaringType, name, attributes, returnType, parameterDeclarations, bodyProvider);
      if (needsExplicitOverride)
        addedOverride.AddExplicitBaseDefinition (baseDefinition);

      isNewlyCreated = true;
      return addedOverride;
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

    private void CheckNotFinalForOverride (MethodInfo overridenMethod)
    {
      if (overridenMethod.IsFinal)
      {
        Assertion.IsNotNull (overridenMethod.DeclaringType);
        var message = string.Format ("Cannot override final method '{0}.{1}'.", overridenMethod.DeclaringType.Name, overridenMethod.Name);
        throw new NotSupportedException (message);
      }
    }
  }
}