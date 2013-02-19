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
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Scripting.Ast;
using Remotion.FunctionalProgramming;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Represents a subclass proxy type, which allows overriding base members and the addition of new members and custom attributes.
  /// </summary>
  /// <remarks>
  /// Avoid using the members <see cref="CustomType.UnderlyingSystemType"/> and <see cref="Type.IsAssignableFrom"/> in combination with
  /// <see cref="ProxyType"/> instances. Use <see cref="TypeExtensions.IsAssignableFromFast"/> instead.
  /// </remarks>
  public class ProxyType : CustomType, IMutableMember
  {
    private const BindingFlags c_allInstanceMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    private const BindingFlags c_allMembers = c_allInstanceMembers | BindingFlags.Static;

    private readonly IUnderlyingTypeFactory _underlyingTypeFactory;
    private readonly IInterfaceMappingComputer _interfaceMappingComputer;
    private readonly IMutableMemberFactory _mutableMemberFactory;

    private readonly CustomAttributeContainer _customAttributes = new CustomAttributeContainer();
    private readonly List<Expression> _initializations = new List<Expression>();
    private readonly List<Type> _addedInterfaces = new List<Type>();
    private readonly List<MutableFieldInfo> _addedFields = new List<MutableFieldInfo>();
    private readonly List<MutableConstructorInfo> _addedConstructors = new List<MutableConstructorInfo>();
    private readonly List<MutableMethodInfo> _addedMethods = new List<MutableMethodInfo>();
    private readonly List<MutablePropertyInfo> _addedProperties = new List<MutablePropertyInfo>();

    private Type _underlyingSystemType;
    private MutableConstructorInfo _typeInitializer;

    public ProxyType (
        IMemberSelector memberSelector,
        IUnderlyingTypeFactory underlyingTypeFactory,
        Type baseType,
        string name,
        string @namespace,
        string fullName,
        TypeAttributes attributes,
        IInterfaceMappingComputer interfaceMappingComputer,
        IMutableMemberFactory mutableMemberFactory)
        : base (memberSelector, name, @namespace, fullName, attributes, false, false, EmptyTypes)
    {
      ArgumentUtility.CheckNotNull ("underlyingTypeFactory", underlyingTypeFactory);
      ArgumentUtility.CheckNotNull ("interfaceMappingComputer", interfaceMappingComputer);
      ArgumentUtility.CheckNotNull ("mutableMemberFactory", mutableMemberFactory);
      Assertion.IsTrue (baseType.IsRuntimeType());

      // TODO (maybe): check that baseType.IsVisible
      if (CanNotBeSubclassed (baseType))
      {
        throw new ArgumentException (
            "Proxied type must not be sealed, an interface, a value type, an enum, a delegate, an array, a byref type, a pointer, "
            + "a generic parameter, contain generic parameters and must have an accessible constructor.",
            "baseType");
      }
      SetDeclaringType (null);
      SetBaseType (baseType);

      _underlyingTypeFactory = underlyingTypeFactory;
      _interfaceMappingComputer = interfaceMappingComputer;
      _mutableMemberFactory = mutableMemberFactory;
    }

    public ProxyType MutableDeclaringType
    {
      get { return (ProxyType) DeclaringType; }
    }

    /// <summary>
    /// Returns a dummy representation of the underlying system type. Do not use the returned type for any kind of analysis. Accessing this property
    /// may cause significant overhead. It is only implemented as internal parts of <see cref="System.Reflection"/> depend on it.
    /// The method <see cref="Type.IsAssignableFrom"/> uses this property internally; use <see cref="TypeExtensions.IsAssignableFromFast"/> instead.
    /// </summary>
    /// <returns> A dummy representation of the underlying system type for the <see cref="ProxyType"/>.</returns>
    [DebuggerBrowsable (DebuggerBrowsableState.Never)]
    public override Type UnderlyingSystemType
    {
      get { return _underlyingSystemType = _underlyingSystemType ?? _underlyingTypeFactory.CreateUnderlyingSystemType (BaseType, _addedInterfaces); }
    }

    public MutableConstructorInfo MutableTypeInitializer
    {
      get { return _typeInitializer; }
    }

    public ReadOnlyCollection<Expression> Initializations
    {
      get { return _initializations.AsReadOnly(); }
    }

    public ReadOnlyCollection<Type> AddedInterfaces
    {
      get { return _addedInterfaces.AsReadOnly(); }
    }

    public ReadOnlyCollection<MutableFieldInfo> AddedFields
    {
      get { return _addedFields.AsReadOnly(); }
    }

    /// <summary>
    /// Gets the added instance constructors. Use <see cref="MutableTypeInitializer"/> to retrieve the static constructor.
    /// </summary>
    /// <value>
    /// The added constructors.
    /// </value>
    public ReadOnlyCollection<MutableConstructorInfo> AddedConstructors
    {
      get { return _addedConstructors.AsReadOnly(); }
    }

    public ReadOnlyCollection<MutableMethodInfo> AddedMethods
    {
      get { return _addedMethods.AsReadOnly(); }
    }

    public ReadOnlyCollection<MutablePropertyInfo> AddedProperties
    {
      get { return _addedProperties.AsReadOnly(); }
    }

    public ReadOnlyCollection<CustomAttributeDeclaration> AddedCustomAttributes
    {
      get { return _customAttributes.AddedCustomAttributes; }
    }

    public void AddCustomAttribute (CustomAttributeDeclaration customAttribute)
    {
      ArgumentUtility.CheckNotNull ("customAttribute", customAttribute);

      _customAttributes.AddCustomAttribute (customAttribute);
    }

    public override IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      return _customAttributes.AddedCustomAttributes.Cast<ICustomAttributeData>();
    }

    public MutableConstructorInfo AddTypeInitializer (Func<ConstructorBodyCreationContext, Expression> bodyProvider)
    {
      ArgumentUtility.CheckNotNull ("bodyProvider", bodyProvider);

      return AddConstructor (MethodAttributes.Private | MethodAttributes.Static, ParameterDeclaration.EmptyParameters, bodyProvider);
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
    /// Such instances must be prepared with <see cref="IObjectFactory.PrepareExternalUninitializedObject"/> before usage.
    /// </para>
    /// </remarks>
    /// <param name="initializationProvider">A provider returning an instance initialization.</param>
    /// <seealso cref="Initializations"/>
    public void AddInitialization (Func<InitializationBodyContext, Expression> initializationProvider)
    {
      ArgumentUtility.CheckNotNull ("initializationProvider", initializationProvider);

      var initialization = _mutableMemberFactory.CreateInitialization (this, initializationProvider);
      _initializations.Add (initialization);
    }

    public void AddInterface (Type interfaceType)
    {
      ArgumentUtility.CheckNotNull ("interfaceType", interfaceType);

      if (!interfaceType.IsInterface)
        throw new ArgumentException ("Type must be an interface.", "interfaceType");

      // TODO : check that interface is visible

      if (_addedInterfaces.Contains (interfaceType))
      {
        var message = string.Format ("Interface '{0}' is already implemented.", interfaceType.Name);
        throw new ArgumentException (message, "interfaceType");
      }

      _addedInterfaces.Add (interfaceType);
      _underlyingSystemType = null;
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
        MethodAttributes attributes = MethodAttributes.Public,
        IEnumerable<ParameterDeclaration> parameters = null,
        Func<ConstructorBodyCreationContext, Expression> bodyProvider = null)
    {
      // Parameters may be null.
      // Body provider may be null.
      parameters = parameters ?? ParameterDeclaration.EmptyParameters;
      bodyProvider = bodyProvider ?? (ctx => Expression.Empty());

      var constructor = _mutableMemberFactory.CreateConstructor (this, attributes, parameters, bodyProvider);
      if (constructor.IsStatic)
      {
        Assertion.IsNull (_typeInitializer);
        _typeInitializer = constructor;
      }
      else
        _addedConstructors.Add (constructor);

      return constructor;
    }

    public MutableMethodInfo AddMethod (
        string name,
        MethodAttributes attributes = MethodAttributes.Public,
        Type returnType = null,
        IEnumerable<ParameterDeclaration> parameters = null,
        Func<MethodBodyCreationContext, Expression> bodyProvider = null)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      // Return type may be null.
      // Parameters may be null.
      // Body provider is null (stays null) for abstract methods.
      returnType = returnType ?? typeof (void);
      parameters = parameters ?? ParameterDeclaration.EmptyParameters;

      var method = _mutableMemberFactory.CreateMethod (this, name, attributes, returnType, parameters, bodyProvider);
      _addedMethods.Add (method);

      return method;
    }

    public MutableMethodInfo AddExplicitOverride (MethodInfo overriddenMethodBaseDefinition, Func<MethodBodyCreationContext, Expression> bodyProvider)
    {
      ArgumentUtility.CheckNotNull ("overriddenMethodBaseDefinition", overriddenMethodBaseDefinition);
      ArgumentUtility.CheckNotNull ("bodyProvider", bodyProvider);

      var overrideMethod = _mutableMemberFactory.CreateExplicitOverride (this, overriddenMethodBaseDefinition, bodyProvider);
      _addedMethods.Add (overrideMethod);

      return overrideMethod;
    }

    /// <summary>
    /// Returns a <see cref="MutableMethodInfo"/> that can be used to modify the behavior of the given <paramref name="baseMethod"/>.
    /// </summary>
    /// <param name="baseMethod">The <see cref="MethodInfo"/> to get a <see cref="MutableMethodInfo"/> for.</param>
    /// <returns>
    /// The <see cref="MutableMethodInfo"/> corresponding to <paramref name="baseMethod"/>, an override for a base method or an implementation for 
    /// an interface method.
    /// </returns>
    /// <remarks>
    /// Depending on the <see cref="MemberInfo.DeclaringType"/> of <paramref name="baseMethod"/> this method returns the following.
    /// <list type="number">
    ///   <item>
    ///     Proxy type
    ///     <list type="bullet">
    ///       <item>The corresponding <see cref="MutableMethodInfo"/> from the <see cref="AddedMethods"/> collection.</item>
    ///     </list>
    ///   </item>
    ///   <item>
    ///     Base type
    ///     <list type="bullet">
    ///       <item>An existing mutable override for the base method, or</item>
    ///       <item>a newly created override (implicit; or explicit if necessary).</item>
    ///     </list>
    ///   </item>
    ///   <item>
    ///     Interface type
    ///     <list type="bullet">
    ///       <item>An existing mutable implementation, or</item>
    ///       <item>a newly created implementation, or</item>
    ///       <item>an existing mutable override for a base implementation, or</item>
    ///       <item>a newly created override for a base implementation (implicit; or explicit if necessary).</item>
    ///     </list>
    ///   </item>
    /// </list>
    /// </remarks>
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

    public MutablePropertyInfo AddProperty (
        string name,
        Type type,
        IEnumerable<ParameterDeclaration> indexParameters = null,
        MethodAttributes accessorAttributes = MethodAttributes.Public,
        Func<MethodBodyCreationContext, Expression> getBodyProvider = null,
        Func<MethodBodyCreationContext, Expression> setBodyProvider = null)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("type", type);
      // Index parameters may be null.
      // Get body provider may be null (for write-only properties).
      // Set body provider may be null (for read-only properties).
      indexParameters = indexParameters ?? ParameterDeclaration.EmptyParameters;

      var property = _mutableMemberFactory.CreateProperty (this, name, type, indexParameters, accessorAttributes, getBodyProvider, setBodyProvider);
      _addedProperties.Add (property);

      if (property.MutableGetMethod != null)
        _addedMethods.Add (property.MutableGetMethod);
      if (property.MutableSetMethod != null)
        _addedMethods.Add (property.MutableSetMethod);

      return property;
    }

    public MutablePropertyInfo AddProperty (
        string name, PropertyAttributes attributes = PropertyAttributes.None, MutableMethodInfo getMethod = null, MutableMethodInfo setMethod = null)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      // Getter may be null.
      // Setter may be null.
      // Custom attributes may be null.

      var property = _mutableMemberFactory.CreateProperty (this, name, attributes, getMethod, setMethod);
      _addedProperties.Add (property);

      return property;
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
      Assertion.IsNotNull (BaseType);

      return _interfaceMappingComputer.ComputeMapping (this, BaseType.GetInterfaceMap, interfaceType, allowPartialInterfaceMapping);
    }

    protected override TypeAttributes GetAttributeFlagsImpl ()
    {
      var hasAbstractMethods = GetMethods (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
          .Where (m => m.IsAbstract)
          .Select (m => m.GetBaseDefinition())
          .Except (AddedMethods.SelectMany (m => m.AddedExplicitBaseDefinitions))
          .Any();
      var isSerializable = _customAttributes.AddedCustomAttributes.Any (a => a.Type == typeof (SerializableAttribute));

      var attributes = base.GetAttributeFlagsImpl();
      if (isSerializable)
        attributes |= TypeAttributes.Serializable;

      if (hasAbstractMethods)
        return attributes | TypeAttributes.Abstract;
      else
        return attributes & ~TypeAttributes.Abstract;
    }

    protected override IEnumerable<Type> GetAllInterfaces ()
    {
      Assertion.IsNotNull (BaseType);

      return _addedInterfaces.Concat (BaseType.GetInterfaces()).Distinct();
    }

    protected override IEnumerable<FieldInfo> GetAllFields ()
    {
      Assertion.IsNotNull (BaseType);

      return _addedFields.Cast<FieldInfo>().Concat (BaseType.GetFields (c_allMembers));
    }

    protected override IEnumerable<ConstructorInfo> GetAllConstructors ()
    {
      return _typeInitializer != null
                 ? _addedConstructors.Cast<ConstructorInfo>().Concat (_typeInitializer)
                 : _addedConstructors.Cast<ConstructorInfo>();
    }

    protected override IEnumerable<MethodInfo> GetAllMethods ()
    {
      Assertion.IsNotNull (BaseType);

      var overriddenBaseDefinitions = new HashSet<MethodInfo> (_addedMethods.Select (mi => mi.GetBaseDefinition()));
      var filteredBaseMethods = BaseType.GetMethods (c_allMembers).Where (m => !overriddenBaseDefinitions.Contains (m.GetBaseDefinition()));

      return _addedMethods.Cast<MethodInfo>().Concat (filteredBaseMethods);
    }

    protected override IEnumerable<PropertyInfo> GetAllProperties ()
    {
      Assertion.IsNotNull (BaseType);

      return _addedProperties.Cast<PropertyInfo>().Concat (BaseType.GetProperties (c_allMembers));
    }

    protected override IEnumerable<EventInfo> GetAllEvents ()
    {
      // TODO; implement correctly
      var all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
      return BaseType.GetEvents (all);
    }

    private static bool CanNotBeSubclassed (Type type)
    {
      return type.IsSealed
             || type.IsInterface
             || typeof (Delegate).IsAssignableFromFast (type)
             || type.ContainsGenericParameters
             || !HasAccessibleConstructor (type);
    }

    private static bool HasAccessibleConstructor (Type type)
    {
      return type.GetConstructors (c_allInstanceMembers).Where (SubclassFilterUtility.IsVisibleFromSubclass).Any();
    }
  } 
}