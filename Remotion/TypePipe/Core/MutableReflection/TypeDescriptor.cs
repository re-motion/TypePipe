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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Remotion.TypePipe.MutableReflection.ReflectionEmit;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Defines the characteristics of a type.
  /// </summary>
  /// <remarks>
  /// This is used by <see cref="MutableType"/> to represent a type, before any mutations.
  /// </remarks>
  public class TypeDescriptor : DescriptorBase<Type>
  {
    private const BindingFlags c_allInstanceMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    private const BindingFlags c_allMembers = c_allInstanceMembers | BindingFlags.Static;

    public static TypeDescriptor Create (Type underlyingType)
    {
      ArgumentUtility.CheckNotNull ("underlyingType", underlyingType);

      if (underlyingType is MutableType)
        throw new ArgumentException ("Original type must not be another mutable type.", "underlyingType");

      // TODO 4695
      if (CanNotBeSubclassed (underlyingType))
      {
        throw new ArgumentException (
            "Original type must not be sealed, an interface, a value type, an enum, a delegate, an array, a byref type, a pointer, "
            + "a generic parameter, contain generic parameters and must have an accessible constructor.",
            "underlyingType");
      }

      return new TypeDescriptor (
          underlyingType,
          underlyingType.DeclaringType,
          underlyingType.BaseType,
          underlyingType.Name,
          underlyingType.Namespace,
          underlyingType.FullName,
          underlyingType.Attributes,
          GetCustomAttributeProvider (underlyingType),
          Array.AsReadOnly (underlyingType.GetInterfaces()),
          underlyingType.GetFields (c_allMembers).ToList().AsReadOnly(),
          underlyingType.GetConstructors (c_allInstanceMembers).ToList().AsReadOnly(),
          underlyingType.GetMethods (c_allMembers).Where (m => !m.IsGenericMethod).ToList().AsReadOnly());
    }

    private static bool CanNotBeSubclassed (Type type)
    {
      return type.IsSealed
             || type.IsInterface
             || typeof (Delegate).IsAssignableFrom (type)
             || type.ContainsGenericParameters
             || !HasAccessibleConstructor (type);
    }

    private static bool HasAccessibleConstructor (Type type)
    {
      // TODO 4695 
      return type.GetConstructors (c_allInstanceMembers).Where (SubclassFilterUtility.IsVisibleFromSubclass).Any();
    }

    private readonly Type _declaringType;
    private readonly Type _baseType;
    private readonly string _namespace;
    private readonly string _fullName;
    private readonly TypeAttributes _attributes;

    private readonly ReadOnlyCollection<Type> _interfaces;
    private readonly ReadOnlyCollection<FieldInfo> _fields;
    private readonly ReadOnlyCollection<ConstructorInfo> _constructors;
    private readonly ReadOnlyCollection<MethodInfo> _methods;

    private TypeDescriptor (
        Type underlyingType,
        Type declaringType,
        Type baseType,
        string name,
        string @namespace,
        string fullName,
        TypeAttributes attributes,
        Func<ReadOnlyCollection<ICustomAttributeData>> customAttributeDataProvider,
        ReadOnlyCollection<Type> interfaces,
        ReadOnlyCollection<FieldInfo> fields,
        ReadOnlyCollection<ConstructorInfo> constructors,
        ReadOnlyCollection<MethodInfo> methods)
        : base (underlyingType, name, customAttributeDataProvider)
    {
      Assertion.IsNotNull (fullName);
      Assertion.IsNotNull (interfaces);
      Assertion.IsNotNull (fields);
      Assertion.IsNotNull (constructors);
      Assertion.IsNotNull (methods);

      _declaringType = declaringType;
      _baseType = baseType;
      _namespace = @namespace;
      _fullName = fullName;
      _attributes = attributes;
      _interfaces = interfaces;
      _fields = fields;
      _constructors = constructors;
      _methods = methods;
    }

    public Type DeclaringType
    {
      get { return _declaringType; }
    }

    public Type BaseType
    {
      get { return _baseType; }
    }

    public string Namespace
    {
      get { return _namespace; }
    }

    public string FullName
    {
      get { return _fullName; }
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

    public ReadOnlyCollection<MethodInfo> Methods
    {
      get { return _methods; }
    }
  }
}