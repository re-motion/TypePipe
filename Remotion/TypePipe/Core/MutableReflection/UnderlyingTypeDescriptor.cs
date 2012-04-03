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
using System.Linq;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Defines the characteristics of a type.
  /// </summary>
  /// <remarks>
  /// This is used by <see cref="MutableType"/> to represent the original type, before any mutations.
  /// </remarks>
  public class UnderlyingTypeDescriptor
  {
    public static UnderlyingTypeDescriptor Create (Type originalType, IMemberFilter memberFilter)
    {
      ArgumentUtility.CheckNotNull ("originalType", originalType);
      ArgumentUtility.CheckNotNull ("memberFilter", memberFilter);

      // TODO 4695
      if (CanNotBeSubclassed (originalType, memberFilter))
        throw new ArgumentException ("Original type must not be sealed, an interface, a value type, an enum, a delegate, contain generic"
                                     + " parameters and must have an accessible constructor.", "originalType");

      return new UnderlyingTypeDescriptor (
          originalType,
          originalType.BaseType,
          originalType.Name,
          originalType.Namespace,
          originalType.FullName,
          originalType.ToString (),
          originalType.Attributes,
          Array.AsReadOnly (originalType.GetInterfaces ()),
          GetAllFields (originalType, memberFilter).ToList().AsReadOnly(),
          GetAllInstanceConstructors (originalType, memberFilter).ToList().AsReadOnly());
    }

    private static bool CanNotBeSubclassed (Type type, IMemberFilter memberFilter)
    {
      return type.IsSealed
             || type.IsInterface
             || typeof (Delegate).IsAssignableFrom (type)
             || type.ContainsGenericParameters
             || !HasAccessibleConstructor (type, memberFilter);
    }

    private static bool HasAccessibleConstructor (Type type, IMemberFilter memberFilter)
    {
      return GetAllInstanceConstructors (type, memberFilter).Any();
    }

    private static IEnumerable<FieldInfo> GetAllFields (Type originalType, IMemberFilter memberFilter)
    {
      var bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
      var fieldInfos = originalType.GetFields (bindingAttr);
      var filteredFields = memberFilter.FilterFields (fieldInfos);

      return filteredFields;
    }

    private static IEnumerable<ConstructorInfo> GetAllInstanceConstructors (Type originalType, IMemberFilter memberFilter)
    {
      var bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
      var constructorInfos = originalType.GetConstructors (bindingAttr);
      return memberFilter.FilterConstructors (constructorInfos);
    }

    private readonly Type _underlyingSystemType;

    private readonly Type _baseType;
    private readonly string _name;
    private readonly string _namespace;
    private readonly string _fullName;
    private readonly string _stringRepresentation;
    private readonly TypeAttributes _attributes;

    private readonly ReadOnlyCollection<Type> _interfaces;
    private readonly ReadOnlyCollection<FieldInfo> _fields;
    private readonly ReadOnlyCollection<ConstructorInfo> _constructors;

    private UnderlyingTypeDescriptor (
        Type underlyingSystemType,
        Type baseType,
        string name,
        string @namespace,
        string fullName,
        string stringRepresentation,
        TypeAttributes attributes,
        ReadOnlyCollection<Type> interfaces,
        ReadOnlyCollection<FieldInfo> fields,
        ReadOnlyCollection<ConstructorInfo> constructors)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNullOrEmpty ("fullName", fullName);
      ArgumentUtility.CheckNotNullOrEmpty ("stringRepresentation", stringRepresentation);
      ArgumentUtility.CheckNotNull ("interfaces", interfaces);
      ArgumentUtility.CheckNotNull ("fields", fields);
      ArgumentUtility.CheckNotNull ("constructors", constructors);

      _underlyingSystemType = underlyingSystemType;
      _baseType = baseType;
      _name = name;
      _namespace = @namespace;
      _fullName = fullName;
      _stringRepresentation = stringRepresentation;
      _attributes = attributes;
      _interfaces = interfaces;
      _fields = fields;
      _constructors = constructors;
    }

    public Type UnderlyingSystemType
    {
      get { return _underlyingSystemType; }
    }

    public Type BaseType
    {
      get { return _baseType; }
    }

    public string Name
    {
      get { return _name; }
    }

    public string Namespace
    {
      get { return _namespace; }
    }

    public string FullName
    {
      get { return _fullName; }
    }

    public string StringRepresentation
    {
      get { return _stringRepresentation; }
    }

    public TypeAttributes Attributes
    {
      get { return _attributes; }
    }

    public ReadOnlyCollection<Type> Interfaces
    {
      get { return _interfaces; }
    }

    public ReadOnlyCollection<FieldInfo> Fields
    {
      get { return _fields; }
    }

    public ReadOnlyCollection<ConstructorInfo> Constructors
    {
      get { return _constructors; }
    }
  }
}