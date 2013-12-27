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

namespace Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection
{
  public static class CustomAttributeReflectionObjectMother
  {
    private class ClassWithMembers
    {
      public ValueType VaueTypeField = null;
      public int? NullableIntField = 0;
      public int IntField = 0;
      public object ObjectField = null;

      public ValueType ValueTypeProperty { get; set; }
      public string StringProperty { get; set; }
      public object ObjectProperty { get; set; }
      public int? NullableIntProperty { get; set; }
      public int IntProperty { get; set; }
    }

    private static readonly Dictionary<Type, PropertyInfo> s_propertiesByType =
        typeof (ClassWithMembers).GetProperties().ToDictionary (pi => pi.PropertyType);

    private static readonly Dictionary<Type, FieldInfo> s_fieldsByType = 
        typeof (ClassWithMembers).GetFields().ToDictionary (fi => fi.FieldType);

    public static PropertyInfo GetPropertyWithType (Type propertyType)
    {
      PropertyInfo property;
      if (!s_propertiesByType.TryGetValue (propertyType, out property))
      {
        var message = String.Format ("There is no property with type '{0}'. Please add it to '{1}'.", propertyType, typeof (ClassWithMembers));
        throw new NotSupportedException (message);
      }

      return property;
    }

    public static FieldInfo GetFieldWithType (Type fieldType)
    {
      FieldInfo field;
      if (! s_fieldsByType.TryGetValue (fieldType, out field))
      {
        var message = String.Format ("There is no field with type '{0}'. Please add it to '{1}'.", fieldType, typeof (ClassWithMembers));
        throw new NotSupportedException (message);
      }

      return field;
    }
  }
}