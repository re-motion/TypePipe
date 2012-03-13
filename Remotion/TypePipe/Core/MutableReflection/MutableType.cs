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
using System.Globalization;
using System.Reflection;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Represents a <see cref="Type"/> that can be changed. Changes are recorded and, depending on the concrete <see cref="MutableType"/>, applied
  /// to an existing type or to a newly created type.
  /// </summary>
  public class MutableType : Type
  {
    private readonly ITypeInfo _originalTypeInfo;
    private readonly List<Type> _addedInterfaces = new List<Type>();
    private readonly List<FutureFieldInfo> _addedFields = new List<FutureFieldInfo>();
    private readonly List<FutureConstructorInfo> _addedConstructors = new List<FutureConstructorInfo>();

    public MutableType (ITypeInfo originalTypeInfo)
    {
      ArgumentUtility.CheckNotNull ("originalTypeInfo", originalTypeInfo);

      _originalTypeInfo = originalTypeInfo;
    }

    public Type OriginalType
    {
      get
      {
        var runtimeType = _originalTypeInfo.GetRuntimeType();
        return runtimeType.HasValue ? runtimeType.Value() : this;
      }
    }

    public ReadOnlyCollection<Type> AddedInterfaces
    {
      get { return _addedInterfaces.AsReadOnly(); }
    }

    public ReadOnlyCollection<FutureFieldInfo> AddedFields
    {
      get { return _addedFields.AsReadOnly(); }
    }

    public ReadOnlyCollection<FutureConstructorInfo> AddedConstructors
    {
      get { return _addedConstructors.AsReadOnly (); }
    }

    public void AddInterface (Type interfaceType)
    {
      ArgumentUtility.CheckNotNull ("interfaceType", interfaceType);

      if (!interfaceType.IsInterface)
        throw new ArgumentException ("Type must be an interface.", "interfaceType");

      if (GetInterfaces ().Contains (interfaceType))
        throw new ArgumentException (string.Format ("Interface '{0}' is already implemented.", interfaceType), "interfaceType");

      _addedInterfaces.Add (interfaceType);
    }

    public FutureFieldInfo AddField (string name, Type type, FieldAttributes attributes)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("type", type);

      if (GetAllFields ().Any (field => field.Name == name))
        throw new ArgumentException (string.Format ("Field with name '{0}' already exists.", name), "name");

      var fieldInfo = new FutureFieldInfo (this, name, type, attributes);
      _addedFields.Add (fieldInfo);

      return fieldInfo;
    }

    // TODO Type Pipe: Add method attributes.
    public FutureConstructorInfo AddConstructor (Type[] parameterTypes)
    {
      ArgumentUtility.CheckNotNull ("parameterTypes", parameterTypes);

      // TODO Type Pipe: Use MemberSignatureEqualityComparer to compare the signatures (create constructorInfo before checking).
      if (GetAllConstructors ().Any (ctor => ctor.GetParameters ().Select (p => p.ParameterType).SequenceEqual (parameterTypes)))
        throw new ArgumentException (string.Format ("Constructor with same signature already exists."), "parameterTypes");

      var parameters = parameterTypes.Select (type => new FutureParameterInfo (type)).ToArray();
      var constructorInfo = new FutureConstructorInfo (this, parameters);
      _addedConstructors.Add (constructorInfo);

      return constructorInfo;
    }

    public override Type[] GetInterfaces ()
    {
      return _originalTypeInfo.GetInterfaces().Concat(AddedInterfaces).ToArray();
    }

    public override Type GetElementType ()
    {
      throw new NotImplementedException();
    }

    protected override bool HasElementTypeImpl ()
    {
      return false;
    }

    public override Guid GUID
    {
      get { throw new NotImplementedException(); }
    }

    public override Module Module
    {
      get { throw new NotImplementedException(); }
    }

    public  override Assembly Assembly
    {
      get { return null; }
    }

    public override string FullName
    {
      get { throw new NotImplementedException(); }
    }

    public override string Namespace
    {
      get { throw new NotImplementedException(); }
    }

    public override string AssemblyQualifiedName
    {
      get { throw new NotImplementedException(); }
    }

    public override Type BaseType
    {
      get { return _originalTypeInfo.GetBaseType(); }
    }

    protected override TypeAttributes GetAttributeFlagsImpl ()
    {
      return _originalTypeInfo.GetAttributeFlags();
    }

    protected override bool IsArrayImpl ()
    {
      throw new NotImplementedException();
    }

    protected  override bool IsByRefImpl ()
    {
      return false;
    }

    protected override bool IsPointerImpl ()
    {
      throw new NotImplementedException();
    }

    protected override bool IsPrimitiveImpl ()
    {
      throw new NotImplementedException();
    }

    protected override bool IsCOMObjectImpl ()
    {
      throw new NotImplementedException();
    }

    public override MemberInfo[] GetMembers (BindingFlags bindingAttr)
    {
      throw new NotImplementedException();
    }

    public override object InvokeMember (string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
    {
      throw new NotImplementedException();
    }

    public override Type GetInterface (string name, bool ignoreCase)
    {
      throw new NotImplementedException();
    }

    public override EventInfo GetEvent (string name, BindingFlags bindingAttr)
    {
      throw new NotImplementedException();
    }

    public override EventInfo[] GetEvents (BindingFlags bindingAttr)
    {
      throw new NotImplementedException();
    }

    public override Type[] GetNestedTypes (BindingFlags bindingAttr)
    {
      throw new NotImplementedException();
    }

    public override Type GetNestedType (string name, BindingFlags bindingAttr)
    {
      throw new NotImplementedException();
    }

    protected override PropertyInfo GetPropertyImpl (string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
    {
      throw new NotImplementedException();
    }

    public override PropertyInfo[] GetProperties (BindingFlags bindingAttr)
    {
      throw new NotImplementedException();
    }

    protected override ConstructorInfo GetConstructorImpl (BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
    {
      // TODO Type Pipe: Implement using GetConstructors, then call binder.SelectMethod or DefaultBinder.SelectMethod.
      throw new NotImplementedException();
    }

    public override ConstructorInfo[] GetConstructors (BindingFlags bindingAttr)
    {
      // TODO Type Pipe: BindingFlags should also affect which 'added' constructors are returned. Add BindingFlagsEvaluator.HasRightVisibility (MethodAttributes, BindingFlags), BindingFlagsEvaluator.HasRightInstanceOrStaticFlag (MethodAttributes, BindingFlags)
      return _originalTypeInfo.GetConstructors (bindingAttr).Concat (AddedConstructors.Cast<ConstructorInfo>()).ToArray();
    }

    protected override MethodInfo GetMethodImpl (string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
    {
      // TODO Type Pipe: Implement using GetMethods, add and use BindingFlagsEvaluator.HasRightName (string actualName, string expectedName, BindingFlags bindingFlags), then apply binder/DefaultBinder
      throw new NotImplementedException ();
    }

    public override MethodInfo[] GetMethods (BindingFlags bindingAttr)
    {
      // TODO Type Pipe: Like GetConstructors.
      throw new NotImplementedException ();
    }

    public override FieldInfo GetField (string name, BindingFlags bindingAttr)
    {
      // TODO Type Pipe: Like GetMethod, but filter on name only, no binder.
      throw new NotImplementedException ();
    }

    public override FieldInfo[] GetFields (BindingFlags bindingAttr)
    {
      // TODO Type Pipe: bindingAttr should also affect which 'added' fields are returned. Add BindingFlagsEvaluator.HasRightVisibility (FieldAttributes, BindingFlags), BindingFlagsEvaluator.HasRightInstanceOrStaticFlag (FieldAttributes, BindingFlags)
      return _originalTypeInfo.GetFields (bindingAttr).Concat (AddedFields.Cast<FieldInfo>()).ToArray();
    }

    public  override Type UnderlyingSystemType
    {
      get { return this; }
    }

    public override object[] GetCustomAttributes (bool inherit)
    {
      throw new NotImplementedException();
    }

    public override bool IsDefined (Type attributeType, bool inherit)
    {
      throw new NotImplementedException();
    }

    public override string Name
    {
      get { throw new NotImplementedException(); }
    }

    public override object[] GetCustomAttributes (Type attributeType, bool inherit)
    {
      throw new NotImplementedException();
    }

    private FieldInfo[] GetAllFields ()
    {
      return GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
    }

    private ConstructorInfo[] GetAllConstructors ()
    {
      return GetConstructors (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance); // Do not include type initializer
    }
  }
}