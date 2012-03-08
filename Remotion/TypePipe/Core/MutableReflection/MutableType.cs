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
    private readonly Type _requestedType;
    private readonly ITypeTemplate _typeTemplate;
    private readonly List<Type> _addedInterfaces = new List<Type>();
    private readonly List<FieldInfo> _addedFields = new List<FieldInfo>();
    // TODO: rework
    private readonly List<FutureConstructorInfo> _addedConstructors = new List<FutureConstructorInfo> ();

    public MutableType (Type requestedType, ITypeTemplate typeTemplate)
    {
      ArgumentUtility.CheckNotNull ("requestedType", requestedType);
      ArgumentUtility.CheckNotNull ("typeTemplate", typeTemplate);

      _requestedType = requestedType;
      _typeTemplate = typeTemplate;
    }

    public Type RequestedType
    {
      get { return _requestedType; }
    }

    public ITypeTemplate TypeTemplate
    {
      get { return _typeTemplate; }
    }

    public ReadOnlyCollection<Type> AddedInterfaces
    {
      get { return _addedInterfaces.AsReadOnly(); }
    }

    public ReadOnlyCollection<FieldInfo> AddedFields
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
        throw new InvalidOperationException (string.Format ("Interface '{0}' is already implemented.", interfaceType));

      _addedInterfaces.Add (interfaceType);
    }

    public FieldInfo AddField (string name, Type type, FieldAttributes attributes)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("type", type);

      if (GetAllFields ().Any (field => field.Name == name))
        throw new InvalidOperationException ("Field with name '_bla' already exists.");

      var fieldInfo = new FutureFieldInfo (this, name, type, attributes);
      _addedFields.Add (fieldInfo);

      return fieldInfo;
    }

    // TODO: Don't take a futureconstructorinfo but a field type
    public void AddConstructor (FutureConstructorInfo futureConstructorInfo)
    {
      ArgumentUtility.CheckNotNull ("futureConstructorInfo", futureConstructorInfo);

      // TODO: Ensure that constructors + addedConstructors does not contain a constructor with same signature

      _addedConstructors.Add (futureConstructorInfo);
    }

    public override Type[] GetInterfaces ()
    {
      return _typeTemplate.GetInterfaces().Concat(AddedInterfaces).ToArray();
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
      get { throw new NotImplementedException(); }
    }

    protected override TypeAttributes GetAttributeFlagsImpl ()
    {
      throw new NotImplementedException();
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

    protected override ConstructorInfo GetConstructorImpl (BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
    {
      throw new NotImplementedException();
    }

    public override ConstructorInfo[] GetConstructors (BindingFlags bindingAttr)
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

    protected override MethodInfo GetMethodImpl (string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
    {
      throw new NotImplementedException();
    }

    public override MethodInfo[] GetMethods (BindingFlags bindingAttr)
    {
      throw new NotImplementedException();
    }

    public override FieldInfo GetField (string name, BindingFlags bindingAttr)
    {
      throw new NotImplementedException();
    }

    public override FieldInfo[] GetFields (BindingFlags bindingAttr)
    {
      return _typeTemplate.GetFields (bindingAttr).Concat (AddedFields).ToArray();
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
  }
}