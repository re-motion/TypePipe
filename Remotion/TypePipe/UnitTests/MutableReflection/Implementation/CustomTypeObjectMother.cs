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
using Remotion.TypePipe.MutableReflection.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  public static class CustomTypeObjectMother
  {
    public static CustomType Create (
        IMemberSelector memberSelector = null,
        Type baseType = null,
        string name = "CustomType",
        string @namespace = "My",
        string fullName = "My.CustomType",
        TypeAttributes attributes = (TypeAttributes) 7,
        IEnumerable<Type> interfaces = null,
        IEnumerable<FieldInfo> fields = null,
        IEnumerable<ConstructorInfo> constructors = null,
        IEnumerable<MethodInfo> methods = null,
        IEnumerable<PropertyInfo> properties = null,
        IEnumerable<EventInfo> events = null,
        bool isGenericType = false,
        bool isGenericTypeDefinition = false,
        IEnumerable<Type> typeArguments = null)
    {
      memberSelector = memberSelector ?? MockRepository.GenerateStub<IMemberSelector>();
      // Declaring type stays null.
      baseType = baseType ?? typeof (UnspecifiedType);
      typeArguments = typeArguments ?? Type.EmptyTypes;

      var customType =
          new TestableCustomType (
              memberSelector,
              name,
              @namespace,
              fullName,
              attributes,
              isGenericType,
              isGenericTypeDefinition,
              typeArguments)
          {
              Interfaces = interfaces ?? Type.EmptyTypes,
              Fields = fields ?? new FieldInfo[0],
              Constructors = constructors ?? new ConstructorInfo[0],
              Methods = methods ?? new MethodInfo[0],
              Properties = properties ?? new PropertyInfo[0],
              Events = events ?? new EventInfo[0]
          };
      customType.CallSetBaseType (baseType);

      return customType;
    }

    public class UnspecifiedType { }
  }
}