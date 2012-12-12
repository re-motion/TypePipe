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
using System.Runtime.Serialization;
using Microsoft.Scripting.Ast;
using Remotion.Collections;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Descriptors;
using Remotion.TypePipe.MutableReflection.Implementation;
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
  public class MutableType : CustomType, IMutableInfo
  {
    private readonly IRelatedMethodFinder _relatedMethodFinder;
    private readonly IInterfaceMappingComputer _interfaceMappingComputer;
    private readonly IMutableMemberFactory _mutableMemberFactory;

    private readonly TypeAttributes _attributes;
    private readonly Func<Type, InterfaceMapping> _interfacMappingProvider;

    private readonly DoubleCheckedLockingContainer<ReadOnlyCollection<ICustomAttributeData>> _customAttributeDatas;

    private readonly List<Expression> _typeInitializations = new List<Expression>();
    private readonly List<Expression> _instanceInitializations = new List<Expression>();

    private readonly ReadOnlyCollection<Type> _existingInterfaces;
    private readonly List<Type> _addedInterfaces = new List<Type>();

    private readonly MutableTypeMemberCollection<FieldInfo, MutableFieldInfo> _fields;
    private readonly MutableTypeMemberCollection<ConstructorInfo, MutableConstructorInfo> _constructors;
    private readonly MutableTypeMethodCollection _methods;

    public MutableType (
        TypeDescriptor descriptor,
        IMemberSelector memberSelector,
        IRelatedMethodFinder relatedMethodFinder,
        IInterfaceMappingComputer interfaceMappingComputer,
        IMutableMemberFactory mutableMemberFactory)
        : base (
            memberSelector,
            descriptor.UnderlyingSystemInfo,
            descriptor.DeclaringType,
            descriptor.BaseType,
            descriptor.Name,
            descriptor.Namespace,
            descriptor.FullName)
    {
      ArgumentUtility.CheckNotNull ("descriptor", descriptor);
      ArgumentUtility.CheckNotNull ("relatedMethodFinder", relatedMethodFinder);
      ArgumentUtility.CheckNotNull ("interfaceMappingComputer", interfaceMappingComputer);
      ArgumentUtility.CheckNotNull ("mutableMemberFactory", mutableMemberFactory);

      _relatedMethodFinder = relatedMethodFinder;
      _interfaceMappingComputer = interfaceMappingComputer;
      _mutableMemberFactory = mutableMemberFactory;

      _attributes = descriptor.Attributes;
      _interfacMappingProvider = descriptor.InterfaceMappingProvider;

      _customAttributeDatas = new DoubleCheckedLockingContainer<ReadOnlyCollection<ICustomAttributeData>> (descriptor.CustomAttributeDataProvider);

      _existingInterfaces = descriptor.Interfaces;

      _fields = new MutableTypeMemberCollection<FieldInfo, MutableFieldInfo> (this, descriptor.Fields, CreateExistingMutableField);
      _constructors = new MutableTypeMemberCollection<ConstructorInfo, MutableConstructorInfo> (
          this, descriptor.Constructors, CreateExistingMutableConstructor);
      _methods = new MutableTypeMethodCollection (this, descriptor.Methods, CreateExistingMutableMethod);
    }

    public bool IsNew
    {
      get { throw new NotImplementedException ("TODO 4744"); }
    }

    public bool IsModified
    {
      get { throw new NotImplementedException ("TODO 4744"); }
    }

    public ReadOnlyCollection<Expression> TypeInitializations
    {
      get { return _typeInitializations.AsReadOnly(); }
    }

    public ReadOnlyCollection<Expression> InstanceInitializations
    {
      get { return _instanceInitializations.AsReadOnly(); }
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

    public bool CanAddCustomAttributeData
    {
      get { throw new NotImplementedException(); }
    }

    public ReadOnlyCollection<CustomAttributeDeclaration> AddedCustomAttributeDeclarations
    {
      get { throw new NotImplementedException(); }
    }

    public void AddCustomAttribute (CustomAttributeDeclaration customAttributeDeclaration)
    {
      throw new NotImplementedException();
    }

    public override IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      return _customAttributeDatas.Value;
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

    /// <summary>
    /// Adds static initialization code to the type.
    /// The initialization code is guaranteed to be executed exactly once sometime before the statically-initilized members are accessed.
    /// </summary>
    /// <remarks>
    /// The exact time when the initialization code runs is not defined.
    /// </remarks>
    /// <param name="initializationProvider">A provider returning a type initialization.</param>
    /// <seealso cref="TypeInitializations"/>
    public void AddTypeInitialization (Func<InitializationBodyContext, Expression> initializationProvider)
    {
      ArgumentUtility.CheckNotNull ("initializationProvider", initializationProvider);

      var initialization = _mutableMemberFactory.CreateInitialization (this, true, initializationProvider);
      _typeInitializations.Add (initialization);
    }

    /// <summary>
    /// Adds instance initialization code.
    /// The initialization code is executed exactly once after object creation or deserialization.
    /// </summary>
    /// <remarks>
    /// <note type="warning">
    /// The fact that the instance initialization code is executed after deserialization means that side effects may be applied twice, once when
    /// the original object is constructed and once when it is (later) deserialized.
    /// </note>
    /// <para>
    /// The added initializations are not executed when instances of the type are created directly through the
    /// <see cref="FormatterServices.GetUninitializedObject"/> API, which creates an object of a type without invoking any constructor.
    /// If possible, use <see cref="IObjectFactory.GetUninitializedObject"/> on <see cref="IObjectFactory"/> which is a simple wrapper but also
    /// executes the specified instance initializations.
    /// </para>
    /// </remarks>
    /// <param name="initializationProvider">A provider returning an instance initialization.</param>
    /// <seealso cref="InstanceInitializations"/>
    public void AddInstanceInitialization (Func<InitializationBodyContext, Expression> initializationProvider)
    {
      ArgumentUtility.CheckNotNull ("initializationProvider", initializationProvider);

      var initialization = _mutableMemberFactory.CreateInitialization (this, false, initializationProvider);
      _instanceInitializations.Add (initialization);
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

    public MutableFieldInfo AddField (string name, Type type, FieldAttributes attributes = FieldAttributes.Private)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("type", type);

      var field = _mutableMemberFactory.CreateField (this, name, type, attributes);
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

      var constructor = _mutableMemberFactory.CreateConstructor (this, attributes, parameterDeclarations, bodyProvider);
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
      // bodyProvider is null for abstract methods

      var method = _mutableMemberFactory.CreateMethod (this, name, attributes, returnType, parameterDeclarations, bodyProvider);
      _methods.Add (method);

      return method;
    }

    public MutableMethodInfo AddAbstractMethod (
        string name, MethodAttributes attributes, Type returnType, IEnumerable<ParameterDeclaration> parameterDeclarations)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("returnType", returnType);
      ArgumentUtility.CheckNotNull ("parameterDeclarations", parameterDeclarations);

      attributes = attributes.Set (MethodAttributes.Abstract | MethodAttributes.Virtual);
      return AddMethod (name, attributes, returnType, parameterDeclarations, bodyProvider: null);
    }

    public MutableMethodInfo AddExplicitOverride (MethodInfo overriddenMethodBaseDefinition, Func<MethodBodyCreationContext, Expression> bodyProvider)
    {
      ArgumentUtility.CheckNotNull ("overriddenMethodBaseDefinition", overriddenMethodBaseDefinition);
      ArgumentUtility.CheckNotNull ("bodyProvider", bodyProvider);

      var overrideMethod = _mutableMemberFactory.CreateExplicitOverride (this, overriddenMethodBaseDefinition, bodyProvider);
      _methods.Add (overrideMethod);

      return overrideMethod;
    }

    /// <summary>
    /// Returns a <see cref="MutableMethodInfo"/> that can be used to modify the behavior of the given <paramref name="method"/>.
    /// </summary>
    /// <remarks>
    /// Depending on the <see cref="MemberInfo.DeclaringType"/> of <paramref name="method"/> this method returns the following.
    /// <list type="number">
    ///   <item>
    ///     Modified type
    ///     <list type="bullet">
    ///       <item>The corresponding <see cref="MutableMethodInfo"/> from the <see cref="ExistingMutableMethods"/> collection.</item>
    ///     </list>
    ///   </item>
    ///   <item>
    ///     Base type
    ///     <list type="bullet">
    ///       <item>An existing mutable override for the base method, or</item>
    ///       <item>a newly created override (implicit or explicit if necessary).</item>
    ///     </list>
    ///   </item>
    ///   <item>
    ///     Interface type
    ///     <list type="bullet">
    ///       <item>An existing mutable implementation, or</item>
    ///       <item>a newly created implementation, or</item>
    ///       <item>an existing mutable override for a base implementation, or</item>
    ///       <item>a newly created override for a base implementation (implicit or explicit if necessary).</item>
    ///     </list>
    ///   </item>
    /// </list>
    /// </remarks>
    /// <param name="method">The <see cref="MethodInfo"/> to get a <see cref="MutableMethodInfo"/> for.</param>
    /// <returns>
    /// The <see cref="MutableMethodInfo"/> corresponding to <paramref name="method"/>, an override for a base method or an implementation for 
    /// an interface method.
    /// </returns>
    public MutableMethodInfo GetOrAddMutableMethod (MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);
      Assertion.IsNotNull (method.DeclaringType);

      var mutableMethod = _methods.GetMutableMember (method);
      if (mutableMethod == null)
      {
        bool isNewlyCreated;
        mutableMethod = _mutableMemberFactory.GetOrCreateMethodOverride (this, method, out isNewlyCreated);
        if (isNewlyCreated)
          _methods.Add (mutableMethod);
      }

      return mutableMethod;
    }

    public override InterfaceMapping GetInterfaceMap (Type interfaceType)
    {
      ArgumentUtility.CheckNotNull ("interfaceType", interfaceType);

      return GetInterfaceMap (interfaceType, allowPartialInterfaceMapping: false);
    }

    /// <summary>
    /// Returns an interface mapping for the specified interface type. 
    /// If <paramref name="allowPartialInterfaceMapping"/> is set to <c>true</c>, an interface mapping will be returned even if the interface is not
    /// fully implemented. This may occur after calls to <see cref="AddInterface"/>.
    /// </summary>
    /// <param name="interfaceType">The <see cref="Type"/> of the interface of which to retrieve a mapping.</param>
    /// <param name="allowPartialInterfaceMapping">Whether or not to allow partial interface mappings.</param>
    /// <returns></returns>
    public InterfaceMapping GetInterfaceMap (Type interfaceType, bool allowPartialInterfaceMapping)
    {
      ArgumentUtility.CheckNotNull ("interfaceType", interfaceType);

      return _interfaceMappingComputer.ComputeMapping (this, _interfacMappingProvider, _methods, interfaceType, allowPartialInterfaceMapping);
    }

    protected override TypeAttributes GetAttributeFlagsImpl ()
    {
      var hasAbstractMethods = GetMethods (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
          .Where (m => m.IsAbstract)
          .Select (m => m.GetBaseDefinition())
          .Except (AllMutableMethods.SelectMany (m => m.AddedExplicitBaseDefinitions))
          .Any();

      if (hasAbstractMethods)
        return _attributes | TypeAttributes.Abstract;
      else
        return _attributes & ~TypeAttributes.Abstract;
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

    public override ConstructorInfo[] GetConstructors (BindingFlags bindingAttr)
    {
      CheckBindingFlagsNotStatic (bindingAttr);

      return base.GetConstructors (bindingAttr);
    }

    protected override ConstructorInfo GetConstructorImpl (
        BindingFlags bindingAttr, Binder binderOrNull, CallingConventions callConvention, Type[] typesOrNull, ParameterModifier[] modifiersOrNull)
    {
      CheckBindingFlagsNotStatic (bindingAttr);

      return base.GetConstructorImpl (bindingAttr, binderOrNull, callConvention, typesOrNull, modifiersOrNull);
    }

    private static void CheckBindingFlagsNotStatic (BindingFlags bindingAttr)
    {
      if ((bindingAttr & BindingFlags.Static) == BindingFlags.Static)
      {
        var method = MemberInfoFromExpressionUtility.GetMethod ((MutableType obj) => obj.AddTypeInitialization (null));
        var message = string.Format (
            "Type initializers (static constructors) cannot be modified via this API, use {0}.{1} instead.", typeof (MutableType).Name, method.Name);
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

    private MutableFieldInfo CreateExistingMutableField (FieldInfo originalField)
    {
      var descriptor = FieldDescriptor.Create (originalField);
      return new MutableFieldInfo (this, descriptor);
    }

    private MutableConstructorInfo CreateExistingMutableConstructor (ConstructorInfo originalConstructor)
    {
      var descriptor = ConstructorDescriptor.Create (originalConstructor);
      return new MutableConstructorInfo (this, descriptor);
    }

    private MutableMethodInfo CreateExistingMutableMethod (MethodInfo originalMethod)
    {
      var descriptor = MethodDescriptor.Create (originalMethod, _relatedMethodFinder);
      return new MutableMethodInfo (this, descriptor);
    }
  } 
}