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
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Generics
{
  /// <summary>
  /// Represents a generic type parameter on a generic type or method definition.
  /// </summary>
  public class MutableGenericParameter : CustomType, IMutableMember
  {
    private const BindingFlags c_allMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    private readonly CustomAttributeContainer _customAttributeContainer = new CustomAttributeContainer();

    private readonly int _position;
    private readonly GenericParameterAttributes _genericParameterAttributes;

    private MemberInfo _declaringMember;
    private IReadOnlyCollection<Type> _constraints = EmptyTypes.ToList().AsReadOnly();

    public MutableGenericParameter (
        int position,
        string name,
        string @namespace,
        GenericParameterAttributes genericParameterAttributes)
        : base (name, @namespace, attributes: TypeAttributes.Public, genericTypeDefinition: null, typeArguments: EmptyTypes)
    {
      Assertion.IsTrue (position >= 0);

      _position = position;
      _genericParameterAttributes = genericParameterAttributes;

      var baseType = genericParameterAttributes.IsSet (GenericParameterAttributes.NotNullableValueTypeConstraint) ? typeof (ValueType) : typeof (object);
      SetBaseType (baseType);
    }

    public MutableType MutableDeclaringType
    {
      get { return (MutableType) DeclaringType; }
    }

    public IReadOnlyCollection<CustomAttributeDeclaration> AddedCustomAttributes
    {
      get { return _customAttributeContainer.AddedCustomAttributes; }
    }

    public override string FullName
    {
      get { return null; }
    }

    public override bool IsGenericParameter
    {
      get { return true; }
    }

    public override int GenericParameterPosition
    {
      get { return _position; }
    }

    public override GenericParameterAttributes GenericParameterAttributes
    {
      get { return _genericParameterAttributes; }
    }

    public override MethodBase DeclaringMethod
    {
      get { return _declaringMember as MethodBase; }
    }

    public void InitializeDeclaringMember (MemberInfo declaringMember)
    {
      ArgumentUtility.CheckNotNull ("declaringMember", declaringMember);
      Assertion.IsTrue (declaringMember is MutableType || declaringMember is MutableMethodInfo);

      if (_declaringMember != null)
        throw new InvalidOperationException ("InitializeDeclaringMember must be called exactly once.");

      SetDeclaringType (declaringMember as Type ?? declaringMember.DeclaringType);
      _declaringMember = declaringMember;
    }

    public void SetGenericParameterConstraints (IEnumerable<Type> constraints)
    {
      ArgumentUtility.CheckNotNull ("constraints", constraints);

      var cons = constraints.ToList().AsReadOnly();

      if (cons.Any (c => c.IsValueType || c == typeof (ValueType)))
        throw new ArgumentException ("A generic parameter cannot be constrained by a value type.", "constraints");

      var baseTypes = cons.Where (c => c.IsClass && !c.IsGenericParameter).ToList();
      if (baseTypes.Count > 1)
        throw new ArgumentException ("A generic parameter cannot have multiple base constraints.", "constraints");
      var baseType = baseTypes.SingleOrDefault ();

      if (baseType != null)
      {
        if (_genericParameterAttributes.IsSet (GenericParameterAttributes.NotNullableValueTypeConstraint))
          throw new ArgumentException ("A generic parameter cannot have a base constraint if the NotNullableValueTypeConstraint flag is set.", "constraints");

        SetBaseType (baseType);
      }

      _constraints = cons;
    }

    public override Type[] GetGenericParameterConstraints ()
    {
      return _constraints.ToArray();
    }

    public void AddCustomAttribute (CustomAttributeDeclaration customAttribute)
    {
      ArgumentUtility.CheckNotNull ("customAttribute", customAttribute);

      _customAttributeContainer.AddCustomAttribute (customAttribute);
    }

    public override IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      return _customAttributeContainer.AddedCustomAttributes.Cast<ICustomAttributeData>();
    }

    public override IEnumerable<Type> GetAllNestedTypes ()
    {
      throw new NotImplementedException();
    }

    public override IEnumerable<Type> GetAllInterfaces ()
    {
      Assertion.IsNotNull (BaseType);

      return _constraints.Where (c => c.IsInterface).Concat (BaseType.GetInterfaces()).Distinct();
    }

    public override IEnumerable<FieldInfo> GetAllFields ()
    {
      Assertion.IsNotNull (BaseType);

      return BaseType.GetFields (c_allMembers);
    }

    public override IEnumerable<ConstructorInfo> GetAllConstructors ()
    {
      if (_genericParameterAttributes.IsSet (GenericParameterAttributes.DefaultConstructorConstraint))
        yield return new GenericParameterDefaultConstructor (this);
    }

    public override IEnumerable<MethodInfo> GetAllMethods ()
    {
      Assertion.IsNotNull (BaseType);

      return BaseType.GetMethods (c_allMembers);
    }

    public override IEnumerable<PropertyInfo> GetAllProperties ()
    {
      Assertion.IsNotNull (BaseType);

      return BaseType.GetProperties (c_allMembers);
    }

    public override IEnumerable<EventInfo> GetAllEvents ()
    {
      Assertion.IsNotNull (BaseType);

      return BaseType.GetEvents (c_allMembers);
    }
  }
}