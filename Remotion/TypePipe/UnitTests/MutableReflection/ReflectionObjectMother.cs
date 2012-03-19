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
using System.Reflection;
using Remotion.Utilities;
using System.Linq;
using Remotion.Collections;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class ReflectionObjectMother
  {
    private class ClassWithMembers
    {
      public ValueType VaueTypeField = null;
      public ValueType ValueTypeProperty { get; set; }
      public string StringProperty { get; set; }
    }

    private static readonly Random s_random = new Random();

    private static readonly Type[] s_types = EnsureNoNulls (new[] { typeof (DateTime), typeof (string) });
    private static readonly Type[] s_unsealedTypes = EnsureNoNulls (new[] { typeof (object), typeof (List<int>) });
    private static readonly Type[] s_interfaceTypes = EnsureNoNulls (new[] { typeof (IDisposable), typeof (IServiceProvider) });
    private static readonly Type[] s_otherInterfaceTypes = EnsureNoNulls (new[] { typeof (IComparable), typeof (ICloneable) });
    private static readonly MemberInfo[] s_members = EnsureNoNulls (new MemberInfo[] { typeof (DateTime).GetProperty ("Now"), typeof (string).GetMethod ("get_Length") });
    private static readonly FieldInfo[] s_fields = EnsureNoNulls (new[] { typeof (string).GetField ("Empty"), typeof (Type).GetField ("EmptyTypes") });
    private static readonly ConstructorInfo[] s_defaultCtors = EnsureNoNulls (new[] { typeof (object).GetConstructor (Type.EmptyTypes), typeof (List<int>).GetConstructor (Type.EmptyTypes) });

    private static readonly Dictionary<Type, PropertyInfo> s_propertiesByType = typeof (ClassWithMembers).GetProperties().ToDictionary (pi => pi.PropertyType);
    private static readonly Dictionary<Type, FieldInfo> s_fieldsByType = typeof (ClassWithMembers).GetFields ().ToDictionary (fi => fi.FieldType);
    
    public static Type GetSomeType ()
    {
      return GetRandomElement (s_types);
    }

    public static Type GetSomeUnsealedType ()
    {
      return GetRandomElement (s_unsealedTypes);
    }

    public static Type GetSomeInterfaceType ()
    {
      return GetRandomElement (s_interfaceTypes);
    }

    public static Type GetSomeDifferentInterfaceType ()
    {
      return GetRandomElement (s_otherInterfaceTypes);
    }

    public static MemberInfo GetSomeMember ()
    {
      return GetRandomElement (s_members);
    }

    public static FieldInfo GetSomeField ()
    {
      return GetRandomElement (s_fields);
    }

    public static ConstructorInfo GetSomeDefaultConstructor ()
    {
      return GetRandomElement (s_defaultCtors);
    }

    public static PropertyInfo GetPropertyWithType (Type propertyType)
    {
      var property = s_propertiesByType.GetValueOrDefault (propertyType);
      if (property == null)
      {
        var message = string.Format ("There is no property with type '{0}'. Please add it to '{1}'.", propertyType, typeof (ClassWithMembers));
        throw new NotSupportedException (message);
      }

      return property;
    }

    public static FieldInfo GetFieldWithType (Type fieldType)
    {
      var field = s_fieldsByType.GetValueOrDefault (fieldType);
      if (field == null)
      {
        var message = string.Format ("There is no field with type '{0}'. Please add it to '{1}'.", fieldType, typeof (ClassWithMembers));
        throw new NotSupportedException (message);
      }

      return field;
    }

    private static T GetRandomElement<T> (T[] array)
    {
      var index = s_random.Next(array.Length);
      return array[index];
    }

    private static T[] EnsureNoNulls<T> (T[] items)
    {
      foreach (var item in items)
        Assertion.IsNotNull (item);
      return items;
    }
  }
}