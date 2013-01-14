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
using Remotion.TypePipe.MutableReflection.BodyBuilding;
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
    private readonly IInterfaceMappingComputer _interfaceMappingComputer;
    private readonly IMutableMemberFactory _mutableMemberFactory;

    // TODO 5309: Remove container, use List (probably)
    private readonly MutableInfoCustomAttributeContainer _customAttributeContainer = new MutableInfoCustomAttributeContainer();
    private readonly Func<Type, InterfaceMapping> _interfaceMappingProvider;

    // TODO remove.
    private readonly List<Expression> _typeInitializations = new List<Expression>();

    private readonly List<Expression> _instanceInitializations = new List<Expression>();
    private readonly List<Type> _addedInterfaces = new List<Type>();
    private readonly MutableTypeMemberCollection<FieldInfo, MutableFieldInfo> _fields = null;
    private readonly MutableTypeMemberCollection<ConstructorInfo, MutableConstructorInfo> _constructors = null;
    private readonly MutableTypeMethodCollection _methods = null;

    private TypeAttributes _attributes;

    public MutableType (
        Type declaringType,
        Type baseType,
        string name,
        string @namespace,
        string fullname,
        TypeAttributes attributes,
        Func<Type, InterfaceMapping> interfaceMappingProvider,
        IMemberSelector memberSelector,
        IInterfaceMappingComputer interfaceMappingComputer,
        IMutableMemberFactory mutableMemberFactory)
        : base (memberSelector, declaringType, baseType, name, @namespace, fullname)
    {
      ArgumentUtility.CheckNotNull ("interfaceMappingProvider", interfaceMappingProvider);
      ArgumentUtility.CheckNotNull ("memberSelector", memberSelector);
      ArgumentUtility.CheckNotNull ("interfaceMappingComputer", interfaceMappingComputer);
      ArgumentUtility.CheckNotNull ("mutableMemberFactory", mutableMemberFactory);

      _attributes = attributes;
      _interfaceMappingProvider = interfaceMappingProvider;
      _interfaceMappingComputer = interfaceMappingComputer;
      _mutableMemberFactory = mutableMemberFactory;
    }

    // TODO 5309: Remove
    public bool IsNew
    {
      get { return false; }
    }

    // TODO 5309: Remove
    public bool IsModified
    {
      get { throw new NotImplementedException ("TODO 4744"); }
    }

    // TODO 5309: Replace with static ctor
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

    // TODO 5309: Remove
    public bool CanAddCustomAttributes
    {
      // TODO 4695
      get { return true; }
    }

    public ReadOnlyCollection<CustomAttributeDeclaration> AddedCustomAttributes
    {
      get { return _customAttributeContainer.AddedCustomAttributes; }
    }

    public void AddCustomAttribute (CustomAttributeDeclaration customAttributeDeclaration)
    {
      ArgumentUtility.CheckNotNull ("customAttributeDeclaration", customAttributeDeclaration);

      _customAttributeContainer.AddCustomAttribute (customAttributeDeclaration);

      if (customAttributeDeclaration.Type == typeof (SerializableAttribute))
        _attributes |= TypeAttributes.Serializable;
    }

    public override IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      return _customAttributeContainer.GetCustomAttributeData();
    }

    // TODO 5309: Close 4972 as Won't fix, remove TODO comments.
    // TODO 4972: Replace usages with TypeEqualityComparer.
    public bool IsAssignableTo (Type other)
    {
      ArgumentUtility.CheckNotNull ("other", other);

      // TODO 4972: Use TypeEqualityComparer.
      return UnderlyingSystemType.Equals (other)
             || other.IsAssignableFrom (BaseType)
             || GetInterfaces ().Any (other.IsAssignableFrom);
    }

    // TODO 5309: Remove, replace with static ctor
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

      // TODO 5309: Should only check _addedInterfaces for duplicates
      if (GetInterfaces ().Contains (interfaceType))
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
    ///       <item>The corresponding <see cref="MutableMethodInfo"/> from the <see cref="AddedMethods"/> collection.</item>
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
    // TODO 5309: Rename to GetOrAddOverride
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

      // TODO 5309: If _methods is changed to _addedMethods, change this accordingly
      return _interfaceMappingComputer.ComputeMapping (this, _interfaceMappingProvider, _methods, interfaceType, allowPartialInterfaceMapping);
    }

    protected override TypeAttributes GetAttributeFlagsImpl ()
    {
      var hasAbstractMethods = GetMethods (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
          .Where (m => m.IsAbstract)
          .Select (m => m.GetBaseDefinition())
          .Except (AddedMethods.SelectMany (m => m.AddedExplicitBaseDefinitions))
          .Any();

      if (hasAbstractMethods)
        return _attributes | TypeAttributes.Abstract;
      else
        return _attributes & ~TypeAttributes.Abstract;
    }

    protected override IEnumerable<Type> GetAllInterfaces ()
    {
      Assertion.IsNotNull (BaseType);

      // TODO test.
      return _addedInterfaces.Concat (BaseType.GetInterfaces()).Distinct();
    }

    protected override IEnumerable<FieldInfo> GetAllFields ()
    {
      // TODO 5309: Concat here, make _fields a simple List
      return _fields;
    }

    protected override IEnumerable<ConstructorInfo> GetAllConstructors ()
    {
      // TODO 5309: Concat here, make _constructors a simple List
      return _constructors;
    }

    protected override IEnumerable<MethodInfo> GetAllMethods ()
    {
      // TODO 5309: Concat here, make _methods a simple List
      // TODO 5309: Remove overridden members here.
      // TODO 5309: Remove MutableTypeMethodCollection, MutableTypeMemberCollection
      return _methods;
    }

    // TODO 5309: Remove
    public override ConstructorInfo[] GetConstructors (BindingFlags bindingAttr)
    {
      CheckBindingFlagsNotStatic (bindingAttr);

      return base.GetConstructors (bindingAttr);
    }

    // TODO 5309: Remove
    protected override ConstructorInfo GetConstructorImpl (
        BindingFlags bindingAttr, Binder binderOrNull, CallingConventions callConvention, Type[] typesOrNull, ParameterModifier[] modifiersOrNull)
    {
      CheckBindingFlagsNotStatic (bindingAttr);

      return base.GetConstructorImpl (bindingAttr, binderOrNull, callConvention, typesOrNull, modifiersOrNull);
    }

    // TODO 5309: Remove
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
  } 
}