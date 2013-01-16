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
  /// Represents a subclass proxy type <see cref="Type"/>, which allows overriding base members and the addition of new members and custom attributes.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     TODO 4972: Update docs to use TypeEqualityComparer.
  ///     OBSOLETE: When an instance of a <see cref="ProxyType"/> is to be compared for equality with another <see cref="Type"/> instance, the
  ///     IsEquivalentTo method should be used rather than comparing via <see cref="object.Equals(object)"/>.
  ///   </para>
  /// </remarks>
  public class ProxyType : CustomType, IMutableInfo
  {
    private const BindingFlags c_all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    private readonly IInterfaceMappingComputer _interfaceMappingComputer;
    private readonly IMutableMemberFactory _mutableMemberFactory;

    // TODO 5309: Remove container, use List (probably)
    private readonly CustomAttributeContainer _customAttributeContainer = new CustomAttributeContainer();

    // TODO remove.
    private readonly List<Expression> _typeInitializations = new List<Expression>();

    private readonly List<Expression> _instanceInitializations = new List<Expression>();
    private readonly List<Type> _addedInterfaces = new List<Type>();
    private readonly List<MutableFieldInfo> _addedFields = new List<MutableFieldInfo>();
    private readonly List<MutableConstructorInfo> _addedConstructors = new List<MutableConstructorInfo>();
    private readonly List<MutableMethodInfo> _addedMethods = new List<MutableMethodInfo>(); 

    private TypeAttributes _attributes;

    public ProxyType (
        Type baseType,
        string name,
        string @namespace,
        string fullName,
        TypeAttributes attributes,
        IMemberSelector memberSelector,
        IInterfaceMappingComputer interfaceMappingComputer,
        IMutableMemberFactory mutableMemberFactory)
        : base (memberSelector, null, baseType, name, @namespace, fullName)
    {
      ArgumentUtility.CheckNotNull ("memberSelector", memberSelector);
      ArgumentUtility.CheckNotNull ("interfaceMappingComputer", interfaceMappingComputer);
      ArgumentUtility.CheckNotNull ("mutableMemberFactory", mutableMemberFactory);

      _attributes = attributes;
      _interfaceMappingComputer = interfaceMappingComputer;
      _mutableMemberFactory = mutableMemberFactory;
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
      get { return _addedFields.AsReadOnly(); }
    }

    public ReadOnlyCollection<MutableConstructorInfo> AddedConstructors
    {
      get { return _addedConstructors.AsReadOnly(); }
    }

    public ReadOnlyCollection<MutableMethodInfo> AddedMethods
    {
      get { return _addedMethods.AsReadOnly(); }
    }

    // TODO 5309: Remove

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
      return _customAttributeContainer.AddedCustomAttributes.Cast<ICustomAttributeData>();
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
      _addedFields.Add (field);

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
      _addedConstructors.Add (constructor);

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
      _addedMethods.Add (method);

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
      _addedMethods.Add (overrideMethod);

      return overrideMethod;
    }

    // TODO update docs.
    /// <summary>
    /// Returns a <see cref="MutableMethodInfo"/> that can be used to modify the behavior of the given <paramref name="baseMethod"/>.
    /// </summary>
    /// <remarks>
    /// Depending on the <see cref="MemberInfo.DeclaringType"/> of <paramref name="baseMethod"/> this method returns the following.
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
    /// <param name="baseMethod">The <see cref="MethodInfo"/> to get a <see cref="MutableMethodInfo"/> for.</param>
    /// <returns>
    /// The <see cref="MutableMethodInfo"/> corresponding to <paramref name="baseMethod"/>, an override for a base method or an implementation for 
    /// an interface method.
    /// </returns>
    public MutableMethodInfo GetOrAddOverride (MethodInfo baseMethod)
    {
      ArgumentUtility.CheckNotNull ("baseMethod", baseMethod);
      Assertion.IsNotNull (baseMethod.DeclaringType);

      bool isNewlyCreated;
      var method = _mutableMemberFactory.GetOrCreateOverride (this, baseMethod, out isNewlyCreated);
      if (isNewlyCreated)
        _addedMethods.Add (method);

      return method;
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
      // BaseType.GetInterfaceMap ??
      return _interfaceMappingComputer.ComputeMapping (this, BaseType.GetInterfaceMap, interfaceType, allowPartialInterfaceMapping);
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
      Assertion.IsNotNull (BaseType);

      return _addedFields.Cast<FieldInfo>().Concat (BaseType.GetFields (c_all));
    }

    protected override IEnumerable<ConstructorInfo> GetAllConstructors ()
    {
      return _addedConstructors.Cast<ConstructorInfo>();
    }

    protected override IEnumerable<MethodInfo> GetAllMethods ()
    {
      Assertion.IsNotNull (BaseType);

      var overriddenBaseDefinitions = new HashSet<MethodInfo> (_addedMethods.Select (mi => mi.GetBaseDefinition()));
      var filteredBaseMethods = BaseType.GetMethods (c_all).Where (m => !overriddenBaseDefinitions.Contains (m.GetBaseDefinition()));

      return _addedMethods.Cast<MethodInfo>().Concat (filteredBaseMethods);
    }
  } 
}