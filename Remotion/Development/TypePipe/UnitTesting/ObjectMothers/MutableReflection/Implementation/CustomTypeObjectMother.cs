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
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection.Implementation
{
  public static class CustomTypeObjectMother
  {
    public static CustomType Create (
        IMemberSelector memberSelector = null,
        Type baseType = null,
        Type declaringType = null,
        string name = "CustomType",
        string @namespace = null,
        TypeAttributes attributes = (TypeAttributes) 7,
        IEnumerable<ICustomAttributeData> customAttributeDatas = null,
        IEnumerable<Type> nestedTypes = null,
        IEnumerable<Type> interfaces = null,
        IEnumerable<FieldInfo> fields = null,
        IEnumerable<ConstructorInfo> constructors = null,
        IEnumerable<MethodInfo> methods = null,
        IEnumerable<PropertyInfo> properties = null,
        IEnumerable<EventInfo> events = null,
        Type genericTypeDefinition = null,
        IEnumerable<Type> typeArguments = null)
    {
      memberSelector = memberSelector ?? new MemberSelector (new BindingFlagsEvaluator());
      baseType = baseType ?? typeof (UnspecifiedType);
      // Declaring type stays null.
      // Generic type definition stays null.
      var typeArgs = (typeArguments ?? Type.EmptyTypes).ToList();

      var customType =
          new TestableCustomType (
              name,
              @namespace,
              attributes,
              genericTypeDefinition,
              typeArgs)
          {
              CustomAttributeDatas = customAttributeDatas ?? new ICustomAttributeData[0],
              NestedTypes = nestedTypes ?? Type.EmptyTypes,
              Interfaces = interfaces ?? Type.EmptyTypes,
              Fields = fields ?? new FieldInfo[0],
              Constructors = constructors ?? new ConstructorInfo[0],
              Methods = methods ?? new MethodInfo[0],
              Properties = properties ?? new PropertyInfo[0],
              Events = events ?? new EventInfo[0]
          };
      customType.SetMemberSelector (memberSelector);
      customType.CallSetBaseType (baseType);
      customType.CallSetDeclaringType (declaringType);

      return customType;
    }

    public class UnspecifiedType { }
  }
}