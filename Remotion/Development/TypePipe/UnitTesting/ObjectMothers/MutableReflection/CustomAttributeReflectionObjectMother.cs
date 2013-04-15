// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Remotion.Collections;

namespace Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection
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

    private static readonly Dictionary<Type, PropertyInfo> s_propertiesByType = typeof (ClassWithMembers).GetProperties().ToDictionary (pi => pi.PropertyType);
    private static readonly Dictionary<Type, FieldInfo> s_fieldsByType = typeof (ClassWithMembers).GetFields ().ToDictionary (fi => fi.FieldType);

    public static PropertyInfo GetPropertyWithType (Type propertyType)
    {
      var property = s_propertiesByType.GetValueOrDefault (propertyType);
      if (property == null)
      {
        var message = String.Format ("There is no property with type '{0}'. Please add it to '{1}'.", propertyType, typeof (ClassWithMembers));
        throw new NotSupportedException (message);
      }

      return property;
    }

    public static FieldInfo GetFieldWithType (Type fieldType)
    {
      var field = s_fieldsByType.GetValueOrDefault (fieldType);
      if (field == null)
      {
        var message = String.Format ("There is no field with type '{0}'. Please add it to '{1}'.", fieldType, typeof (ClassWithMembers));
        throw new NotSupportedException (message);
      }

      return field;
    }
  }
}