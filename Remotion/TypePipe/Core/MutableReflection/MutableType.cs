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
using System.Collections.ObjectModel;
using System.Reflection;
using Microsoft.Scripting.Ast;
using Remotion.Collections;
using Remotion.FunctionalProgramming;
using Remotion.Reflection.MemberSignatures;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Represents a <see cref="Type"/>, which allows to add or modify members.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     TODO 4972: Update docs to use TypeEqualityComparer.
  ///     OBSOLETE: When an instance of a <see cref="MutableType"/> is to be compared for equality with another <see cref="Type"/> instance, the
  ///     IsEquivalentTo method should be used rather than comparing via <see cref="object.Equals(object)"/>.
  ///   </para>
  /// </remarks>
  public class MutableType : CustomType, IMutableMember
  {
    private readonly IMemberSelector _memberSelector;
    private readonly IRelatedMethodFinder _relatedMethodFinder;

    private readonly DoubleCheckedLockingContainer<ReadOnlyCollection<ICustomAttributeData>> _customAttributeDatas;

    private readonly ReadOnlyCollection<Type> _existingInterfaces;
    private readonly List<Type> _addedInterfaces = new List<Type> ();

    private readonly MutableTypeMemberCollection<FieldInfo, MutableFieldInfo> _fields;
    private readonly MutableTypeMemberCollection<ConstructorInfo, MutableConstructorInfo> _constructors;
    private readonly MutableTypeMemberCollection<MethodInfo, MutableMethodInfo> _methods;

    private TypeAttributes _attributes;

    public MutableType (
        UnderlyingTypeDescriptor underlyingTypeDescriptor,
        IMemberSelector memberSelector,
        IRelatedMethodFinder relatedMethodFinder)
        : base (
            memberSelector,
            underlyingTypeDescriptor.UnderlyingSystemInfo,
            underlyingTypeDescriptor.DeclaringType,
            underlyingTypeDescriptor.BaseType,
            underlyingTypeDescriptor.Name,
            underlyingTypeDescriptor.Namespace,
            underlyingTypeDescriptor.FullName)
    {
      ArgumentUtility.CheckNotNull ("underlyingTypeDescriptor", underlyingTypeDescriptor);
      ArgumentUtility.CheckNotNull ("memberSelector", memberSelector);
      ArgumentUtility.CheckNotNull ("relatedMethodFinder", relatedMethodFinder);

      _memberSelector = memberSelector;
      _relatedMethodFinder = relatedMethodFinder;

      _customAttributeDatas =
          new DoubleCheckedLockingContainer<ReadOnlyCollection<ICustomAttributeData>> (underlyingTypeDescriptor.CustomAttributeDataProvider);

      _attributes = underlyingTypeDescriptor.Attributes;
      _existingInterfaces = underlyingTypeDescriptor.Interfaces;

      _fields = new MutableTypeMemberCollection<FieldInfo, MutableFieldInfo> (this, underlyingTypeDescriptor.Fields, CreateExistingMutableField);
      _constructors = new MutableTypeMemberCollection<ConstructorInfo, MutableConstructorInfo> (
          this, underlyingTypeDescriptor.Constructors, CreateExistingMutableConstructor);
      _methods = new MutableTypeMethodCollection (this, underlyingTypeDescriptor.Methods, CreateExistingMutableMethod);
    }

    public bool IsNew
    {
      get { throw new NotImplementedException ("TODO 4744"); }
    }

    public bool IsModified
    {
      get { throw new NotImplementedException ("TODO 4744"); }
    }

    public ReadOnlyCollection<Type> AddedInterfaces
    {
      get { return _addedInterfaces.AsReadOnly(); }
    }

    public ReadOnlyCollection<MutableFieldInfo> AddedFields
    {
      get { return _fields.AddedMembers; }
    }

    public ReadOnlyCollection<MutableConstructorInfo> AddedConstructors
    {
      get { return _constructors.AddedMembers; }
    }

    public ReadOnlyCollection<MutableMethodInfo> AddedMethods
    {
      get { return _methods.AddedMembers; }
    }

    public ReadOnlyCollection<Type> ExistingInterfaces
    {
      get { return _existingInterfaces; }
    }

    public ReadOnlyCollectionDecorator<MutableFieldInfo> ExistingMutableFields
    {
      get { return _fields.ExistingDeclaredMembers; }
    }

    public ReadOnlyCollectionDecorator<MutableConstructorInfo> ExistingMutableConstructors
    {
      get { return _constructors.ExistingDeclaredMembers; }
    }

    public ReadOnlyCollectionDecorator<MutableMethodInfo> ExistingMutableMethods
    {
      get { return _methods.ExistingDeclaredMembers; }
    }

    public IEnumerable<MutableFieldInfo> AllMutableFields
    {
      get { return _fields.AllMutableMembers; }
    }

    public IEnumerable<MutableConstructorInfo> AllMutableConstructors
    {
      get { return _constructors.AllMutableMembers; }
    }

    public IEnumerable<MutableMethodInfo> AllMutableMethods
    {
      get { return _methods.AllMutableMembers; }
    }

    // TODO 4972: Replace usages with TypeEqualityComparer.

    public bool IsAssignableTo (Type other)
    {
      ArgumentUtility.CheckNotNull ("other", other);

      // TODO 4972: Use TypeEqualityComparer.
      return UnderlyingSystemType.Equals (other)
             || other.IsAssignableFrom (BaseType)
             || GetInterfaces ().Any (other.IsAssignableFrom);
    }

    public void AddInterface (Type interfaceType)
    {
      ArgumentUtility.CheckNotNull ("interfaceType", interfaceType);

      if (!interfaceType.IsInterface)
        throw new ArgumentException ("Type must be an interface.", "interfaceType");

      if (GetInterfaces().Contains (interfaceType))
      {
        var message = string.Format ("Interface '{0}' is already implemented.", interfaceType.Name);
        throw new ArgumentException (message, "interfaceType");
      }

      _addedInterfaces.Add (interfaceType);
    }

    public MutableFieldInfo AddField (Type type, string name, FieldAttributes attributes = FieldAttributes.Private)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("type", type);

      var signature = new FieldSignature (type);
      if (AllMutableFields.Any (f => f.Name == name && FieldSignature.Create (f).Equals (signature)))
        throw new ArgumentException ("Field with equal name and signature already exists.", "name");

      var descriptor = UnderlyingFieldInfoDescriptor.Create (type, name, attributes);
      var field = new MutableFieldInfo (this, descriptor);

      _fields.Add (field);

      return field;
    }

    public MutableFieldInfo GetMutableField (FieldInfo field)
    {
      ArgumentUtility.CheckNotNull ("field", field);

      return GetMutableMemberOrThrow (_fields, field, "field");
    }

    public MutableConstructorInfo AddConstructor (
        MethodAttributes attributes,
        IEnumerable<ParameterDeclaration> parameterDeclarations,
        Func<ConstructorBodyCreationContext, Expression> bodyProvider)
    {
      ArgumentUtility.CheckNotNull ("parameterDeclarations", parameterDeclarations);
      ArgumentUtility.CheckNotNull ("bodyProvider", bodyProvider);

      var invalidAttributes =
          new[]
          {
              MethodAttributes.Abstract, MethodAttributes.HideBySig, MethodAttributes.PinvokeImpl,
              MethodAttributes.RequireSecObject, MethodAttributes.UnmanagedExport, MethodAttributes.Virtual
          };
      CheckForInvalidAttributes ("constructor", invalidAttributes, attributes);

      if ((attributes & MethodAttributes.Static) != 0)
        throw new ArgumentException ("Adding static constructors is not (yet) supported.", "attributes");

      var parameterDescriptors = UnderlyingParameterInfoDescriptor.CreateFromDeclarations (parameterDeclarations).ConvertToCollection();

      var signature = new MethodSignature (typeof (void), parameterDescriptors.Select (pd => pd.Type), 0);
      if (AllMutableConstructors.Any (ctor => signature.Equals (MethodSignature.Create (ctor))))
        throw new ArgumentException ("Constructor with equal signature already exists.", "parameterDeclarations");

      var parameterExpressions = parameterDescriptors.Select (pd => pd.Expression);
      var context = new ConstructorBodyCreationContext (this, parameterExpressions, _memberSelector);
      var body = BodyProviderUtility.GetTypedBody (typeof (void), bodyProvider, context);

      var descriptor = UnderlyingConstructorInfoDescriptor.Create (attributes, parameterDescriptors, body);
      var constructor = new MutableConstructorInfo (this, descriptor);

      _constructors.Add (constructor);

      return constructor;
    }

    public MutableConstructorInfo GetMutableConstructor (ConstructorInfo constructor)
    {
      ArgumentUtility.CheckNotNull ("constructor", constructor);

      return GetMutableMemberOrThrow(_constructors, constructor, "constructor");
    }

    public MutableMethodInfo AddMethod (
        string name,
        MethodAttributes attributes,
        Type returnType,
        IEnumerable<ParameterDeclaration> parameterDeclarations,
        Func<MethodBodyCreationContext, Expression> bodyProvider)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("returnType", returnType);
      ArgumentUtility.CheckNotNull ("parameterDeclarations", parameterDeclarations);

      // TODO XXXX: if it is an implicit method override, it needs the same visibility (or more public visibility?)!
      // TODO 5099: add check attributes to be virtual if also abstract
      // TODO 5099: check bodyProvider for null if attributes doesn't contain Abstract flag
      // bodyProvider is null for abstract methods

      var invalidAttributes = new[] { MethodAttributes.PinvokeImpl, MethodAttributes.RequireSecObject, MethodAttributes.UnmanagedExport };
      CheckForInvalidAttributes ("method", invalidAttributes, attributes);

      var isVirtual = attributes.IsSet (MethodAttributes.Virtual);
      var isNewSlot = attributes.IsSet (MethodAttributes.NewSlot);
      if (!isVirtual && isNewSlot)
        throw new ArgumentException ("NewSlot methods must also be virtual.", "attributes");

      var parameterDescriptors = UnderlyingParameterInfoDescriptor.CreateFromDeclarations (parameterDeclarations).ConvertToCollection();

      var signature = new MethodSignature (returnType, parameterDescriptors.Select (pd => pd.Type), 0);
      // Fix code duplication?
      if (AllMutableMethods.Any (m => m.Name == name && signature.Equals (MethodSignature.Create (m))))
      {
        var message = string.Format ("Method '{0}' with equal signature already exists.", name);
        throw new ArgumentException (message, "name");
      }

      var baseMethod = isVirtual && !isNewSlot ? _relatedMethodFinder.GetMostDerivedVirtualMethod (name, signature, BaseType) : null;
      if (baseMethod != null)
        CheckNotFinalForOverride (baseMethod);

      var parameterExpressions = parameterDescriptors.Select (pd => pd.Expression);
      var isStatic = attributes.IsSet (MethodAttributes.Static);
      var context = new MethodBodyCreationContext (this, parameterExpressions, isStatic, baseMethod, _memberSelector);
      var body = bodyProvider == null ? null : BodyProviderUtility.GetTypedBody (returnType, bodyProvider, context);

      var descriptor = UnderlyingMethodInfoDescriptor.Create (
        name, attributes, returnType, parameterDescriptors, baseMethod, false, false, false, body);
      var method = CreateMutableMethod (descriptor);

      _methods.Add (method);

      if (method.IsAbstract)
        _attributes |= TypeAttributes.Abstract;

      return method;
    }

    public MutableMethodInfo AddAbstractMethod (
        string name,
        MethodAttributes attributes,
        Type returnType,
        IEnumerable<ParameterDeclaration> parameterDeclarations)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("returnType", returnType);
      ArgumentUtility.CheckNotNull ("parameterDeclarations", parameterDeclarations);

      attributes = attributes.Set (MethodAttributes.Abstract | MethodAttributes.Virtual);
      return AddMethod (name, attributes, returnType, parameterDeclarations, bodyProvider: null);
    }

    /// <summary>
    /// Returns a <see cref="MutableMethodInfo"/> that can be used to modify the behavior of the given <paramref name="method"/>. If the method
    /// is declared on the modified type, it returns the corresponding <see cref="MutableMethodInfo"/> from the <see cref="ExistingMutableMethods"/>
    /// collection. If it is declared on a base type, this method returns an override for it, creating one if necessary.
    /// </summary>
    /// <param name="method">The <see cref="MethodInfo"/> to get a <see cref="MutableMethodInfo"/> for.</param>
    /// <returns>
    /// The <see cref="MutableMethodInfo"/> corresponding to <paramref name="method"/> or an override of the method.
    /// </returns>
    public MutableMethodInfo GetOrAddMutableMethod (MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);
      Assertion.IsNotNull (method.DeclaringType);

      // TODO 4972: Use TypeEqualityComparer (for Equals and IsSubclassOf)
      if (!UnderlyingSystemType.Equals (method.DeclaringType) && !IsSubclassOf (method.DeclaringType))
      {
        var message = string.Format ("Method is declared by a type outside of this type's class hierarchy: '{0}'.", method.DeclaringType.Name);
        throw new ArgumentException (message, "method");
      }

      var mutableMethod = _methods.GetMutableMember (method);
      if (mutableMethod != null)
        return mutableMethod;

      if (!method.IsVirtual)
        throw new NotSupportedException ("A method declared in a base type must be virtual in order to be modified.");

      var baseDefinition = method.GetBaseDefinition();
      var existingMutableOverride = _relatedMethodFinder.GetOverride (baseDefinition, AllMutableMethods);
      if (existingMutableOverride != null)
        return existingMutableOverride;

      var needsExplicitOverride = _relatedMethodFinder.IsShadowed (baseDefinition, _methods);
      var baseMethod = _relatedMethodFinder.GetMostDerivedOverride (baseDefinition, BaseType);
      CheckNotFinalForOverride (baseMethod);

      var name = needsExplicitOverride ? MethodOverrideUtility.GetNameForExplicitOverride (baseMethod) : baseMethod.Name;
      var attributes = needsExplicitOverride
                           ? MethodOverrideUtility.GetAttributesForExplicitOverride (baseMethod)
                           : MethodOverrideUtility.GetAttributesForImplicitOverride (baseMethod);
      var returnType = baseMethod.ReturnType;
      var parameterDeclarations = ParameterDeclaration.CreateForEquivalentSignature (baseMethod).ConvertToCollection();
      var bodyProvider = baseMethod.IsAbstract
                             ? null
                             : new Func<MethodBodyCreationContext, Expression> (
                                   ctx => ctx.GetBaseCall (baseMethod, ctx.Parameters.Cast<Expression>()));

      var addedOverride = AddMethod (name, attributes, returnType, parameterDeclarations, bodyProvider);
      if (needsExplicitOverride)
        addedOverride.AddExplicitBaseDefinition (baseDefinition);

      return addedOverride;
    }

    public virtual void Accept (IMutableTypeUnmodifiedMutableMemberHandler handler)
    {
      ArgumentUtility.CheckNotNull ("handler", handler);

      foreach (var field in ExistingMutableFields.Where (f => !f.IsModified))
        handler.HandleUnmodifiedField (field);
      foreach (var ctor in ExistingMutableConstructors.Where (c => !c.IsModified))
        handler.HandleUnmodifiedConstructor (ctor);
      foreach (var method in ExistingMutableMethods.Where (m => !m.IsModified))
        handler.HandleUnmodifiedMethod (method);
    }

    public virtual void Accept (IMutableTypeModificationHandler handler)
    {
      ArgumentUtility.CheckNotNull ("handler", handler);

      foreach (var ifc in AddedInterfaces)
        handler.HandleAddedInterface (ifc);

      foreach (var field in _fields.AddedMembers)
        handler.HandleAddedField (field);
      foreach (var ctor in _constructors.AddedMembers)
        handler.HandleAddedConstructor (ctor);
      foreach (var method in _methods.AddedMembers)
        handler.HandleAddedMethod (method);

      foreach (var ctor in ExistingMutableConstructors.Where (c => c.IsModified))
        handler.HandleModifiedConstructor (ctor);
      foreach (var method in ExistingMutableMethods.Where (m => m.IsModified))
        handler.HandleModifiedMethod (method);
    }

    public override IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      return _customAttributeDatas.Value;
    }

    protected override TypeAttributes GetAttributeFlagsImpl ()
    {
      return _attributes;
    }

    protected override IEnumerable<Type> GetAllInterfaces ()
    {
      return _existingInterfaces.Concat (_addedInterfaces);
    }

    protected override IEnumerable<FieldInfo> GetAllFields ()
    {
      return _fields;
    }

    protected override IEnumerable<ConstructorInfo> GetAllConstructors ()
    {
      return _constructors;
    }

    protected override IEnumerable<MethodInfo> GetAllMethods ()
    {
      return _methods;
    }

    private void CheckForInvalidAttributes (string memberKind, MethodAttributes[] invalidAttributes, MethodAttributes attributes)
    {
      var hasInvalidAttributes = invalidAttributes.Any (x => attributes.IsSet (x));
      if (hasInvalidAttributes)
      {
        var invalidAttributeList = string.Join (", ", invalidAttributes.Select (x => Enum.GetName (typeof (MethodAttributes), x)).ToArray());
        var message = string.Format ("The following MethodAttributes are not supported for {0}s: {1}.",  memberKind, invalidAttributeList);
        throw new ArgumentException (message, "attributes");
      }
    }

    private void CheckNotFinalForOverride (MethodInfo overridenMethod)
    {
      if (overridenMethod.IsFinal)
      {
        var message = string.Format ("Cannot override final method '{0}.{1}'.", overridenMethod.DeclaringType.Name, overridenMethod.Name);
        throw new NotSupportedException (message);
      }
    }

    private TMutableMember GetMutableMemberOrThrow<TMember, TMutableMember> (
        MutableTypeMemberCollection<TMember, TMutableMember> collection, TMember member, string memberType)
        where TMember: MemberInfo
        where TMutableMember: TMember
    {
      // TODO 4972: Use TypeEqualityComparer.
      if (!UnderlyingSystemType.Equals (member.DeclaringType))
      {
        var message = string.Format ("The given {0} is declared by a different type: '{1}'.", memberType, member.DeclaringType);
        throw new ArgumentException (message, memberType);
      }

      var mutableMember = collection.GetMutableMember (member);
      if (mutableMember == null)
      {
        var message = string.Format ("The given {0} cannot be modified.", memberType);
        throw new NotSupportedException (message);
      }

      return mutableMember;
    }

    private MutableMethodInfo CreateMutableMethod (UnderlyingMethodInfoDescriptor descriptor)
    {
      return new MutableMethodInfo (this, descriptor, MakeConcreteIfPossible);
    }

    private void MakeConcreteIfPossible ()
    {
      var implementsAllMethods = GetMethods (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).All (m => !m.IsAbstract);
      if (implementsAllMethods)
        _attributes &= ~TypeAttributes.Abstract;
    }

    private MutableFieldInfo CreateExistingMutableField (FieldInfo originalField)
    {
      var descriptor = UnderlyingFieldInfoDescriptor.Create (originalField);
      return new MutableFieldInfo (this, descriptor);
    }

    private MutableConstructorInfo CreateExistingMutableConstructor (ConstructorInfo originalConstructor)
    {
      var descriptor = UnderlyingConstructorInfoDescriptor.Create (originalConstructor);
      return new MutableConstructorInfo (this, descriptor);
    }

    private MutableMethodInfo CreateExistingMutableMethod (MethodInfo originalMethod)
    {
      var descriptor = UnderlyingMethodInfoDescriptor.Create (originalMethod, _relatedMethodFinder);
      return CreateMutableMethod (descriptor);
    }
  } 
}