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
using Remotion.Text;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.Utilities;
using Remotion.FunctionalProgramming;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// Implements <see cref="IMutableMemberFactory"/>.
  /// </summary>
  public class MutableMemberFactory : IMutableMemberFactory
  {
    private struct MethodSignatureItems
    {
      public readonly ICollection<GenericParameter> GenericParameters;
      public readonly Type ReturnType;
      public readonly ICollection<ParameterDeclaration> ParameterDeclarations;

      public MethodSignatureItems (
          ICollection<GenericParameter> genericParameters, Type returnType, ICollection<ParameterDeclaration> parameterDeclarations)
      {
        ReturnType = returnType;
        ParameterDeclarations = parameterDeclarations;
        GenericParameters = genericParameters;
      }
    }

    private static readonly IMethodSignatureStringBuilderHelper s_methodSignatureStringBuilderHelper =
        new GenericParameterCompatibleMethodSignatureStringBuilderHelper();

    private static readonly FieldAttributes[] s_invalidFieldAttributes =
        new[]
        {
            FieldAttributes.Literal, FieldAttributes.HasFieldMarshal, FieldAttributes.HasDefault, FieldAttributes.HasFieldRVA
        };

    private static readonly MethodAttributes[] s_invalidConstructorAttributes =
        new[]
        {
            MethodAttributes.Final, MethodAttributes.Virtual, MethodAttributes.CheckAccessOnOverride, MethodAttributes.Abstract,
            MethodAttributes.PinvokeImpl, MethodAttributes.UnmanagedExport, MethodAttributes.RequireSecObject
        };

    private static readonly MethodAttributes[] s_invalidMethodAttributes = new[] { MethodAttributes.RequireSecObject };

    private static readonly PropertyAttributes[] s_invalidPropertyAttributes =
        new[]
        {
            PropertyAttributes.HasDefault, PropertyAttributes.Reserved2, PropertyAttributes.Reserved3, PropertyAttributes.Reserved4
        };

    private static readonly EventAttributes[] s_invalidEventAttributes = new EventAttributes[0];

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
      return ProviderUtility.GetNonNullValue (initializationProvider, context, "initializationProvider");
    }

    public MutableFieldInfo CreateField (ProxyType declaringType, string name, Type type, FieldAttributes attributes)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("type", type);

      if (type == typeof (void))
        throw new ArgumentException ("Field cannot be of type void.", "type");

      CheckForInvalidAttributes ("fields", s_invalidFieldAttributes, attributes, "attributes");

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

      CheckForInvalidAttributes ("constructors", s_invalidConstructorAttributes, attributes, "attributes");

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

      var attr = attributes.Set (MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
      return new MutableConstructorInfo (declaringType, attr, paras, body);
    }

    public MutableMethodInfo CreateMethod (
        ProxyType declaringType,
        string name,
        MethodAttributes attributes,
        IEnumerable<GenericParameterDeclaration> genericParameters,
        Func<GenericParameterContext, Type> returnTypeProvider,
        Func<GenericParameterContext, IEnumerable<ParameterDeclaration>> parameterProvider,
        Func<MethodBodyCreationContext, Expression> bodyProvider)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("genericParameters", genericParameters);
      ArgumentUtility.CheckNotNull ("returnTypeProvider", returnTypeProvider);
      ArgumentUtility.CheckNotNull ("parameterProvider", parameterProvider);
      // Body provider may be null (for abstract methods).

      // TODO : virtual and static is an invalid combination

      var isAbstract = attributes.IsSet (MethodAttributes.Abstract);
      if (!isAbstract && bodyProvider == null)
        throw new ArgumentNullException ("bodyProvider", "Non-abstract methods must have a body.");
      if (isAbstract && bodyProvider != null)
        throw new ArgumentException ("Abstract methods cannot have a body.", "bodyProvider");

      CheckForInvalidAttributes ("methods", s_invalidMethodAttributes, attributes, "attributes");

      var isVirtual = attributes.IsSet (MethodAttributes.Virtual);
      var isNewSlot = attributes.IsSet (MethodAttributes.NewSlot);
      if (isAbstract && !isVirtual)
        throw new ArgumentException ("Abstract methods must also be virtual.", "attributes");
      if (!isVirtual && isNewSlot)
        throw new ArgumentException ("NewSlot methods must also be virtual.", "attributes");

      var methodItems = GetMethodSignatureItems (declaringType, genericParameters, returnTypeProvider, parameterProvider);

      var signature = new MethodSignature (
          methodItems.ReturnType,
          methodItems.ParameterDeclarations.Select (pd => pd.Type),
          methodItems.GenericParameters.Count,
          s_methodSignatureStringBuilderHelper);
      if (declaringType.AddedMethods.Any (m => m.Name == name && MethodSignature.Create (m).Equals (signature)))
        throw new InvalidOperationException ("Method with equal name and signature already exists.");

      var baseMethod = GetBaseMethod (declaringType, name, signature, isVirtual, isNewSlot);
      // TODO : if it is an implicit baseMethod override, it needs at least the same ore more public visibility

      var body = GetMethodBody (declaringType, attributes, bodyProvider, methodItems, baseMethod);

      return new MutableMethodInfo (
          declaringType, name, attributes, methodItems.GenericParameters, methodItems.ReturnType, methodItems.ParameterDeclarations, baseMethod, body);
    }

    // TODO: Make private, move copy to MutableMemberFactoryTest
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
      // Body provider may be null (for abstract methods).

      return CreateMethod (declaringType, name, attributes, GenericParameterDeclaration.None, ctx => returnType, ctx => parameters, bodyProvider);
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
        MethodAttributes accessorAttributes,
        Func<MethodBodyCreationContext, Expression> getBodyProvider,
        Func<MethodBodyCreationContext, Expression> setBodyProvider)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("type", type);
      ArgumentUtility.CheckNotNull ("indexParameters", indexParameters);
      // Get body provider may be null.
      // Set body provider may be null.

      CheckForInvalidAttributes ("property accessor methods", s_invalidMethodAttributes, accessorAttributes, "accessorAttributes");

      if (getBodyProvider == null && setBodyProvider == null)
        throw new ArgumentException ("At least one accessor body provider must be specified.", "getBodyProvider");

      var indexParams = indexParameters.ConvertToCollection();
      var signature = new PropertySignature (type, indexParams.Select (pd => pd.Type));
      if (declaringType.AddedProperties.Any (p => p.Name == name && PropertySignature.Create (p).Equals (signature)))
        throw new InvalidOperationException ("Property with equal name and signature already exists.");

      var attributes = accessorAttributes | MethodAttributes.SpecialName;
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

      CheckForInvalidAttributes ("properties", s_invalidPropertyAttributes, attributes, "attributes");

      if (getMethod == null && setMethod == null)
        throw new ArgumentException ("Property must have at least one accessor.", "getMethod");

      var readWriteProperty = getMethod != null && setMethod != null;
      if (readWriteProperty && getMethod.IsStatic != setMethod.IsStatic)
        throw new ArgumentException ("Accessor methods must be both either static or non-static.", "getMethod");

      if (getMethod != null && !ReferenceEquals (getMethod.DeclaringType, declaringType))
        throw new ArgumentException ("Get method is not declared on the current type.", "getMethod");
      if (setMethod != null && !ReferenceEquals (setMethod.DeclaringType, declaringType))
        throw new ArgumentException ("Set method is not declared on the current type.", "setMethod");

      if (getMethod != null && getMethod.ReturnType == typeof (void))
        throw new ArgumentException ("Get accessor must be a non-void method.", "getMethod");
      if (setMethod != null && setMethod.ReturnType != typeof (void))
        throw new ArgumentException ("Set accessor must have return type void.", "setMethod");

      var getSignature = getMethod != null ? new PropertySignature (getMethod.ReturnType, getMethod.GetParameters().Select (p => p.ParameterType)) : null;
      var setParameters = setMethod != null ? setMethod.GetParameters().Select (p => p.ParameterType).ToList() : null;
      var setSignature = setMethod != null ? new PropertySignature (setParameters.Last(), setParameters.Take (setParameters.Count - 1)) : null;

      if (readWriteProperty && !getSignature.Equals (setSignature))
        throw new ArgumentException ("Get and set accessor methods must have a matching signature.", "setMethod");

      var signature = getSignature ?? setSignature;
      if (declaringType.AddedProperties.Any (p => p.Name == name && PropertySignature.Create (p).Equals (signature)))
        throw new InvalidOperationException ("Property with equal name and signature already exists.");

      return new MutablePropertyInfo (declaringType, name, attributes, getMethod, setMethod);
    }

    public MutableEventInfo CreateEvent (
        ProxyType declaringType,
        string name,
        Type handlerType,
        MethodAttributes accessorAttributes,
        Func<MethodBodyCreationContext, Expression> addBodyProvider,
        Func<MethodBodyCreationContext, Expression> removeBodyProvider,
        Func<MethodBodyCreationContext, Expression> raiseBodyProvider)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("handlerType", handlerType, typeof (Delegate));
      ArgumentUtility.CheckNotNull ("addBodyProvider", addBodyProvider);
      ArgumentUtility.CheckNotNull ("removeBodyProvider", removeBodyProvider);
      // Raise body provider may be null.

      CheckForInvalidAttributes ("event accessor methods", s_invalidMethodAttributes, accessorAttributes, "accessorAttributes");

      var signature = new EventSignature (handlerType);
      if (declaringType.AddedEvents.Any (e => e.Name == name && EventSignature.Create (e).Equals (signature)))
        throw new InvalidOperationException ("Event with equal name and signature already exists.");

      var attributes = accessorAttributes | MethodAttributes.SpecialName;
      var addRemoveParameters = new[] { new ParameterDeclaration (handlerType, "handler") };

      var addMethod = CreateMethod (declaringType, "add_" + name, attributes, typeof (void), addRemoveParameters, addBodyProvider);
      var removeMethod = CreateMethod (declaringType, "remove_" + name, attributes, typeof (void), addRemoveParameters, removeBodyProvider);

      MutableMethodInfo raiseMethod = null;
      if (raiseBodyProvider != null)
      {
        var invokeMethod = GetInvokeMethod (handlerType);
        var raiseParameters = invokeMethod.GetParameters().Select (p => new ParameterDeclaration (p.ParameterType, p.Name, p.Attributes));
        raiseMethod = CreateMethod (declaringType, "raise_" + name, attributes, invokeMethod.ReturnType, raiseParameters, raiseBodyProvider);
      }
      
      return new MutableEventInfo (declaringType, name, EventAttributes.None, addMethod, removeMethod, raiseMethod);
    }

    public MutableEventInfo CreateEvent (
        ProxyType declaringType,
        string name,
        EventAttributes attributes,
        MutableMethodInfo addMethod,
        MutableMethodInfo removeMethod,
        MutableMethodInfo raiseMethod)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("addMethod", addMethod);
      ArgumentUtility.CheckNotNull ("removeMethod", removeMethod);
      // Raise method may be null.

      CheckForInvalidAttributes ("events", s_invalidEventAttributes, attributes, "attributes");

      if (addMethod.IsStatic != removeMethod.IsStatic || (raiseMethod != null && raiseMethod.IsStatic != addMethod.IsStatic))
        throw new ArgumentException ("Accessor methods must be all either static or non-static.", "addMethod");

      if (!ReferenceEquals (addMethod.DeclaringType, declaringType))
        throw new ArgumentException ("Add method is not declared on the current type.", "addMethod");
      if (!ReferenceEquals (removeMethod.DeclaringType, declaringType))
        throw new ArgumentException ("Remove method is not declared on the current type.", "removeMethod");
      if (raiseMethod != null && !ReferenceEquals (raiseMethod.DeclaringType, declaringType))
        throw new ArgumentException ("Raise method is not declared on the current type.", "raiseMethod");

      if (addMethod.ReturnType != typeof (void))
        throw new ArgumentException ("Add method must have return type void.", "addMethod");
      if (removeMethod.ReturnType != typeof (void))
        throw new ArgumentException ("Remove method must have return type void.", "removeMethod");

      var addMethodParameterTypes = addMethod.GetParameters().Select (p => p.ParameterType).ToList();
      var removeMethodParameterTypes = removeMethod.GetParameters().Select (p => p.ParameterType).ToList();

      if (addMethodParameterTypes.Count != 1 || !addMethodParameterTypes[0].IsSubclassOf (typeof (Delegate)))
        throw new ArgumentException ("Add method must have a single parameter that is assignable to 'System.Delegate'.", "addMethod");
      if (removeMethodParameterTypes.Count != 1 || !removeMethodParameterTypes[0].IsSubclassOf (typeof (Delegate)))
        throw new ArgumentException ("Remove method must have a single parameter that is assignable to 'System.Delegate'.", "removeMethod");

      if (addMethodParameterTypes.Single() != removeMethodParameterTypes.Single())
        throw new ArgumentException ("The type of the handler parameter is different for the add and remove method.", "removeMethod");

      var handlerType = addMethodParameterTypes.Single();
      var invokeMethod = GetInvokeMethod (handlerType);
      if (raiseMethod != null && !MethodSignature.AreEqual (raiseMethod, invokeMethod))
        throw new ArgumentException ("The signature of the raise method does not match the handler type.", "raiseMethod");

      var signature = new EventSignature (handlerType);
      if (declaringType.AddedEvents.Any (e => e.Name == name && EventSignature.Create (e).Equals (signature)))
        throw new InvalidOperationException ("Event with equal name and signature already exists.");

      return new MutableEventInfo (declaringType, name, attributes, addMethod, removeMethod, raiseMethod);
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

    private MethodInfo GetInvokeMethod (Type delegateType)
    {
      Assertion.IsTrue (delegateType.IsSubclassOf (typeof (Delegate)));
      return delegateType.GetMethod ("Invoke", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    }

    private void CheckForInvalidAttributes<T> (string memberKind, T[] invalidAttributes, T attributes, string parameterName)
    {
      var hasInvalidAttributes = invalidAttributes.Any (a =>  IsSet (attributes, a));
      if (hasInvalidAttributes)
      {
        var invalidAttributeList = SeparatedStringBuilder.Build (", ", invalidAttributes.Select (x => Enum.GetName (typeof (T), x)));
        var message = string.Format ("The following {0} are not supported for {1}: {2}.", typeof(T).Name, memberKind, invalidAttributeList);
        throw new ArgumentException (message, parameterName);
      }
    }

    private MethodSignatureItems GetMethodSignatureItems (
        ProxyType declaringType,
        IEnumerable<GenericParameterDeclaration> genericParameters,
        Func<GenericParameterContext, Type> returnTypeProvider,
        Func<GenericParameterContext, IEnumerable<ParameterDeclaration>> parameterProvider)
    {
      var genericParameterDeclarations = genericParameters.ConvertToCollection ();
      var memberSelector = new MemberSelector (new BindingFlagsEvaluator ());
      var genericParams = genericParameterDeclarations
          .Select ((p, i) => new GenericParameter (memberSelector, i, p.Name, declaringType.Namespace, p.Attributes)).ToList ();

      var genericParameterContext = new GenericParameterContext (genericParams.Cast<Type> ());
      foreach (var paraAndDecl in genericParams.Zip (genericParameterDeclarations, (p, d) => new { Parameter = p, Declaration = d }))
      {
        paraAndDecl.Parameter.SetBaseTypeConstraint (paraAndDecl.Declaration.BaseConstraintProvider (genericParameterContext));
        paraAndDecl.Parameter.SetInterfaceConstraints (paraAndDecl.Declaration.InterfaceConstraintsProvider (genericParameterContext));
      }

      var returnType = ProviderUtility.GetNonNullValue (returnTypeProvider, genericParameterContext, "returnTypeProvider");
      var parameters = ProviderUtility.GetNonNullValue (parameterProvider, genericParameterContext, "parameterProvider").ConvertToCollection ();

      return new MethodSignatureItems (genericParams, returnType, parameters);
    }

    private MethodInfo GetBaseMethod (ProxyType declaringType, string name, MethodSignature signature, bool isVirtual, bool isNewSlot)
    {
      var baseMethod = isVirtual && !isNewSlot ? _relatedMethodFinder.GetMostDerivedVirtualMethod (name, signature, declaringType.BaseType) : null;
      if (baseMethod != null)
        CheckNotFinalForOverride (baseMethod);
      return baseMethod;
    }

    private Expression GetMethodBody (
        ProxyType declaringType, MethodAttributes attributes, Func<MethodBodyCreationContext, Expression> bodyProvider, MethodSignatureItems signatureItems, MethodInfo baseMethod)
    {
      var parameterExpressions = signatureItems.ParameterDeclarations.Select (pd => pd.Expression);
      var isStatic = attributes.IsSet (MethodAttributes.Static);
      var context = new MethodBodyCreationContext (declaringType, isStatic, parameterExpressions, signatureItems.ReturnType, baseMethod, _memberSelector);
      var body = bodyProvider == null ? null : BodyProviderUtility.GetTypedBody (signatureItems.ReturnType, bodyProvider, context);
      return body;
    }

    private bool IsSet<T> (T actual, T expected)
    {
      var f1 = (int) (object) actual;
      var f2 = (int) (object) expected;

      return (f1 & f2) == f2;
    }
  }
}