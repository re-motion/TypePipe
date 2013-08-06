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
using Remotion.TypePipe.Dlr.Ast;
using Remotion.FunctionalProgramming;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection.Implementation.MemberFactory;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Represents a mutable type which allows overriding base members, the addition of new members and custom attributes.
  /// </summary>
  public class MutableType : CustomType, IMutableMember
  {
    private const BindingFlags c_allMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    private readonly IInterfaceMappingComputer _interfaceMappingComputer;
    private readonly IMutableMemberFactory _mutableMemberFactory;

    private readonly CustomAttributeContainer _customAttributes = new CustomAttributeContainer();
    private readonly List<MutableType> _addedNestedTypes = new List<MutableType>();
    private readonly InstanceInitialization _initialization = new InstanceInitialization();
    private readonly List<Type> _addedInterfaces = new List<Type>();
    private readonly List<MutableFieldInfo> _addedFields = new List<MutableFieldInfo>();
    private readonly List<MutableConstructorInfo> _addedConstructors = new List<MutableConstructorInfo>();
    private readonly List<MutableMethodInfo> _addedMethods = new List<MutableMethodInfo>();
    private readonly List<MutablePropertyInfo> _addedProperties = new List<MutablePropertyInfo>();
    private readonly List<MutableEventInfo> _addedEvents = new List<MutableEventInfo>();

    private MutableConstructorInfo _typeInitializer;
    private bool? _hasAbstractMethods = null;

    public MutableType (
        IMemberSelector memberSelector,
        Type baseType,
        string name,
        string @namespace,
        TypeAttributes attributes,
        Type declaringType,
        IInterfaceMappingComputer interfaceMappingComputer,
        IMutableMemberFactory mutableMemberFactory)
        : base (memberSelector, name, @namespace, attributes, null, EmptyTypes)
    {
      // Base type may be null (for interfaces).
      // Declaring type may be null.
      ArgumentUtility.CheckNotNull ("interfaceMappingComputer", interfaceMappingComputer);
      ArgumentUtility.CheckNotNull ("mutableMemberFactory", mutableMemberFactory);

      SetDeclaringType (declaringType);
      SetBaseType (baseType);

      _interfaceMappingComputer = interfaceMappingComputer;
      _mutableMemberFactory = mutableMemberFactory;
    }

    public MutableType MutableDeclaringType
    {
      get { return (MutableType) DeclaringType; }
    }

    public ReadOnlyCollection<MutableType> AddedNestedTypes
    {
      get { return _addedNestedTypes.AsReadOnly(); }
    } 

    public MutableConstructorInfo MutableTypeInitializer
    {
      get { return _typeInitializer; }
    }

    public InstanceInitialization Initialization
    {
      get { return _initialization; }
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

    public ReadOnlyCollection<MutableEventInfo> AddedEvents
    {
      get { return _addedEvents.AsReadOnly(); }
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

    public MutableType AddNestedType (string typeName, TypeAttributes attributes, Type baseType)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("typeName", typeName);
      // Base type can be null

      var nestedType = _mutableMemberFactory.CreateNestedType (this, typeName, attributes, baseType);
      _addedNestedTypes.Add (nestedType);

      return nestedType;
    }

    public MutableConstructorInfo AddTypeInitializer (Func<ConstructorBodyCreationContext, Expression> bodyProvider)
    {
      ArgumentUtility.CheckNotNull ("bodyProvider", bodyProvider);

      return AddConstructor (MethodAttributes.Private | MethodAttributes.Static, ParameterDeclaration.None, bodyProvider);
    }

    /// <summary>
    /// Adds instance initialization code. The initialization code is executed exactly once after object creation or deserialization.
    /// Use <see cref="InitializationBodyContext.InitializationSemantics"/> to distinguish between object creation and deserialization.
    /// </summary>
    /// <remarks>
    /// <note type="warning">
    /// The fact that the instance initialization code is executed after deserialization means that side effects may be applied twice, once when
    /// the original object is constructed and once when it is (later) deserialized.
    /// </note>
    /// <para>
    /// The added initializations are not executed when instances of the type are created directly through the
    /// <see cref="FormatterServices.GetUninitializedObject"/> API, which creates an object of a type without invoking any constructor.
    /// Such instances must be prepared with <see cref="IPipeline.PrepareExternalUninitializedObject"/> before usage.
    /// </para>
    /// </remarks>
    /// <param name="initializationProvider">A provider returning an instance initialization.</param>
    /// <seealso cref="Initialization"/>
    public void AddInitialization (Func<InitializationBodyContext, Expression> initializationProvider)
    {
      ArgumentUtility.CheckNotNull ("initializationProvider", initializationProvider);

      var initialization = _mutableMemberFactory.CreateInitialization (this, initializationProvider);
      _initialization.Expressions.Add (initialization);
    }

    public void AddInterface (Type interfaceType, bool throwIfAlreadyImplemented = true)
    {
      ArgumentUtility.CheckNotNull ("interfaceType", interfaceType);

      if (!interfaceType.IsInterface)
        throw new ArgumentException ("Type must be an interface.", "interfaceType");

      // TODO 4744: Check that interface is visible.

      var alreadyImplemented = _addedInterfaces.Contains (interfaceType);
      if (alreadyImplemented && throwIfAlreadyImplemented)
      {
        var message = string.Format ("Interface '{0}' is already implemented.", interfaceType.Name);
        throw new ArgumentException (message, "interfaceType");
      }

      if (!alreadyImplemented)
        _addedInterfaces.Add (interfaceType);
    }

    public MutableFieldInfo AddField (string name, FieldAttributes attributes, Type type)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("type", type);

      var field = _mutableMemberFactory.CreateField (this, name, type, attributes);
      _addedFields.Add (field);

      return field;
    }

    public MutableConstructorInfo AddConstructor (
        MethodAttributes attributes, IEnumerable<ParameterDeclaration> parameters, Func<ConstructorBodyCreationContext, Expression> bodyProvider)
    {
      ArgumentUtility.CheckNotNull ("parameters", parameters);
      ArgumentUtility.CheckNotNull ("bodyProvider", bodyProvider);

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
        MethodAttributes attributes,
        IEnumerable<GenericParameterDeclaration> genericParameters,
        Func<GenericParameterContext, Type> returnTypeProvider,
        Func<GenericParameterContext, IEnumerable<ParameterDeclaration>> parameterProvider,
        Func<MethodBodyCreationContext, Expression> bodyProvider)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("genericParameters", genericParameters);
      ArgumentUtility.CheckNotNull ("returnTypeProvider", returnTypeProvider);
      ArgumentUtility.CheckNotNull ("parameterProvider", parameterProvider);
      // Body provider may be null (for abstract methods).

      var method = _mutableMemberFactory.CreateMethod (this, name, attributes, genericParameters, returnTypeProvider, parameterProvider, bodyProvider);
      AddTrackedMethod (method);

      return method;
    }

    public MutableMethodInfo AddExplicitOverride (MethodInfo overriddenMethodBaseDefinition, Func<MethodBodyCreationContext, Expression> bodyProvider)
    {
      ArgumentUtility.CheckNotNull ("overriddenMethodBaseDefinition", overriddenMethodBaseDefinition);
      ArgumentUtility.CheckNotNull ("bodyProvider", bodyProvider);

      var overrideMethod = _mutableMemberFactory.CreateExplicitOverride (this, overriddenMethodBaseDefinition, bodyProvider);
      AddTrackedMethod(overrideMethod);

      return overrideMethod;
    }

    /// <summary>
    /// Returns an existing or creates a new override (implicit; or explicit if necessary) for <paramref name="overriddenMethod"/>.
    /// </summary>
    /// <param name="overriddenMethod">The base method that should be overridden.</param>
    /// <returns>A <see cref="MutableMethodInfo"/> that is an override of the base method.</returns>
    /// <exception cref="NotSupportedException">
    /// If the specified method cannot be overridden, e.g., it is final or not accessible from the proxy.
    /// </exception>
    public MutableMethodInfo GetOrAddOverride (MethodInfo overriddenMethod)
    {
      ArgumentUtility.CheckNotNull ("overriddenMethod", overriddenMethod);

      bool isNewlyCreated;
      var method = _mutableMemberFactory.GetOrCreateOverride (this, overriddenMethod, out isNewlyCreated);
      if (isNewlyCreated)
        AddTrackedMethod (method);

      return method;
    }

    /// <summary>
    /// Returns an existing or creates a new implementation (or re-implementation) for <paramref name="interfaceMethod"/>.
    /// Note that the specified implementation is not invoked if a final base implementation is invoked directly (not via the interface).
    /// </summary>
    /// <param name="interfaceMethod">The interface method that should be implemented.</param>
    /// <returns>A <see cref="MutableMethodInfo"/> that is an implementation of the interface method.</returns>
    /// <exception cref="NotSupportedException">
    /// If the specified method cannot be implemented (or re-implemented), e.g., it is not accessible from the proxy.
    /// </exception>
    /// <remarks>
    /// This method returns one of the following:
    /// <list type="number">
    ///   <item>An existing mutable implementation.</item>
    ///   <item>A newly created implementation (if no base implementation).</item>
    ///   <item>An existing mutable override for a base implementation.</item>
    ///   <item>A newly created override (implicit; or explicit if necessary) for a base implementation (if base implementation is not final).</item>
    ///   <item>A newly created re-implementation which calls the base implementation.</item>
    /// </list>
    /// </remarks>
    public MutableMethodInfo GetOrAddImplementation (MethodInfo interfaceMethod)
    {
      ArgumentUtility.CheckNotNull ("interfaceMethod", interfaceMethod);

      bool isNewlyCreated;
      var method = _mutableMemberFactory.GetOrCreateImplementation (this, interfaceMethod, out isNewlyCreated);
      if (isNewlyCreated)
        AddTrackedMethod (method);

      return method;
    }

    public MutablePropertyInfo AddProperty (
        string name,
        Type type,
        IEnumerable<ParameterDeclaration> indexParameters,
        MethodAttributes accessorAttributes,
        Func<MethodBodyCreationContext, Expression> getBodyProvider,
        Func<MethodBodyCreationContext, Expression> setBodyProvider)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("type", type);
      ArgumentUtility.CheckNotNull ("indexParameters", indexParameters);
      // Get body provider may be null (for write-only properties).
      // Set body provider may be null (for read-only properties).

      var property = _mutableMemberFactory.CreateProperty (this, name, type, indexParameters, accessorAttributes, getBodyProvider, setBodyProvider);
      _addedProperties.Add (property);

      if (property.MutableGetMethod != null)
        AddTrackedMethod (property.MutableGetMethod);
      if (property.MutableSetMethod != null)
        AddTrackedMethod (property.MutableSetMethod);

      return property;
    }

    public MutablePropertyInfo AddProperty (string name, PropertyAttributes attributes, MutableMethodInfo getMethod, MutableMethodInfo setMethod)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      // Set method may be null (for write-only properties).
      // Get method may be null (for read-only properties).

      var property = _mutableMemberFactory.CreateProperty (this, name, attributes, getMethod, setMethod);
      _addedProperties.Add (property);

      return property;
    }

    public MutableEventInfo AddEvent (
        string name,
        Type handlerType,
        MethodAttributes accessorAttributes,
        Func<MethodBodyCreationContext, Expression> addBodyProvider,
        Func<MethodBodyCreationContext, Expression> removeBodyProvider,
        Func<MethodBodyCreationContext, Expression> raiseBodyProvider = null)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("handlerType", handlerType);
      ArgumentUtility.CheckNotNull ("addBodyProvider", addBodyProvider);
      ArgumentUtility.CheckNotNull ("removeBodyProvider", removeBodyProvider);
      // Raise body provider may be null.

      var event_ = _mutableMemberFactory.CreateEvent (
          this, name, handlerType, accessorAttributes, addBodyProvider, removeBodyProvider, raiseBodyProvider);
      _addedEvents.Add (event_);

      AddTrackedMethod (event_.MutableAddMethod);
      AddTrackedMethod (event_.MutableRemoveMethod);
      if (event_.MutableRaiseMethod != null)
        AddTrackedMethod(event_.MutableRaiseMethod);

      return event_;
    }

    public MutableEventInfo AddEvent (
        string name, EventAttributes attributes, MutableMethodInfo addMethod, MutableMethodInfo removeMethod, MutableMethodInfo raiseMethod = null)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("addMethod", addMethod);
      ArgumentUtility.CheckNotNull ("removeMethod", removeMethod);
      // Raise method may be null.

      var event_ = _mutableMemberFactory.CreateEvent (this, name, attributes, addMethod, removeMethod, raiseMethod);
      _addedEvents.Add (event_);

      return event_;
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

      if (IsInterface)
        throw new NotSupportedException ("Method GetInterfaceMap is not supported by interface types.");

      Assertion.IsNotNull (BaseType);
      return _interfaceMappingComputer.ComputeMapping (this, BaseType.GetInterfaceMap, interfaceType, allowPartialInterfaceMapping);
    }

    protected override TypeAttributes GetAttributeFlagsImpl ()
    {
      var attributes = base.GetAttributeFlagsImpl();

      var isSerializable = _customAttributes.AddedCustomAttributes.Any (a => a.Type == typeof (SerializableAttribute));
      if (isSerializable)
        attributes |= TypeAttributes.Serializable;

      if (attributes.IsSet (TypeAttributes.Interface))
        return attributes | TypeAttributes.Abstract;

      if (HasAbstractMethods())
        return attributes | TypeAttributes.Abstract;
      else
        return attributes & ~TypeAttributes.Abstract;
    }

    protected override IEnumerable<Type> GetAllNestedTypes()
    {
      return GetAllMembers(_addedNestedTypes, b => EmptyTypes);
    }

    protected override IEnumerable<Type> GetAllInterfaces ()
    {
      return GetAllMembers (_addedInterfaces, b => b.GetInterfaces()).Distinct();
    }

    protected override IEnumerable<FieldInfo> GetAllFields ()
    {
      return GetAllMembers (_addedFields, b => b.GetFields (c_allMembers));
    }

    protected override IEnumerable<ConstructorInfo> GetAllConstructors ()
    {
      return _typeInitializer != null
                 ? _addedConstructors.Cast<ConstructorInfo>().Concat (_typeInitializer)
                 : _addedConstructors.Cast<ConstructorInfo>();
    }

    protected override IEnumerable<MethodInfo> GetAllMethods ()
    {
      return GetAllMembers (
          _addedMethods,
          baseType =>
          {
            var overriddenBaseDefinitions = new HashSet<MethodInfo> (_addedMethods.Select (MethodBaseDefinitionCache.GetBaseDefinition));
            return baseType.GetMethods (c_allMembers).Where (m => !overriddenBaseDefinitions.Contains (MethodBaseDefinitionCache.GetBaseDefinition (m)));
          });
    }

    protected override IEnumerable<PropertyInfo> GetAllProperties ()
    {
      return GetAllMembers (_addedProperties, b => b.GetProperties (c_allMembers));
    }

    protected override IEnumerable<EventInfo> GetAllEvents ()
    {
      return GetAllMembers (_addedEvents, b => b.GetEvents (c_allMembers));
    }

    private bool HasAbstractMethods ()
    {
      if (_hasAbstractMethods == null)
      {
        // Note: GetAllMethods is used because this is faster than GetMethods (...), even though static methods aren't required.
        _hasAbstractMethods = GetAllMethods()
            .Where (m => m.IsAbstract)
            .Select (MethodBaseDefinitionCache.GetBaseDefinition)
            .Except (AddedMethods.SelectMany (m => m.AddedExplicitBaseDefinitions))
            .Any();
      }
      return _hasAbstractMethods.Value;
    }

    private IEnumerable<T> GetAllMembers<T, TMutable> (IEnumerable<TMutable> addedMembers, Func<Type, IEnumerable<T>> baseMemberProvider)
        where TMutable : T
    {
      if (BaseType == null)
        return addedMembers.Cast<T>();

      return addedMembers.Cast<T>().Concat (baseMemberProvider (BaseType));
    }

    private void AddTrackedMethod (MutableMethodInfo mutableMethod)
    {
      _addedMethods.Add (mutableMethod);

      // Cases when adding a method:
      // - We add an abstract method
      //   + We had abstract methods (flag == true) => no change (flag = true)
      //   + We had no abstract methods (flag == false) => we now have abstract nethods (flag = true)
      //   => Set to true in any case

      // - We add a non-abstract method 
      //   + We had abstract methods (flag == true) => recalculate if (and only if) it overrides an abstract method (flag = null)
      //   + We had no abstract methods (flag == false) => no change (flag = false)

      if (mutableMethod.IsAbstract)
        _hasAbstractMethods = true;
      else if (mutableMethod.BaseMethod != null && mutableMethod.BaseMethod.IsAbstract)
        _hasAbstractMethods = null;
      else if (mutableMethod.AddedExplicitBaseDefinitions.Any (m => m.IsAbstract))
        _hasAbstractMethods = null;

      // Cases when setting a body:
      // - The body was null and no longer is => recalculate (flag = null)
      // - The body is null and wasn't before => we now have abstract methods (flag = true) [cannot currently happen]
      // - The body is null/not null as it was before => no change

      mutableMethod.BodyChanged += (sender, args) =>
      {
        Assertion.IsNotNull (args.NewBody);
        if (args.OldBody == null && args.NewBody != null)
          _hasAbstractMethods = null;
      };

      // Cases when setting an explicit base definition:
      // - The base definition is abstract and the overrider is not => recalculate (flag = null)
      // - Otherwise => no change
      mutableMethod.ExplicitBaseDefinitionAdded += (sender, args) =>
      {
        if (args.AddedExplicitBaseDefinition.IsAbstract && !((MutableMethodInfo) sender).IsAbstract)
          _hasAbstractMethods = null;
      };
    }
  }
}