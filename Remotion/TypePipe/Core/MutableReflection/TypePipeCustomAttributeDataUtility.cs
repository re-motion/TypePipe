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
using System.Linq;
using System.Reflection;
using Remotion.FunctionalProgramming;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  public static class TypePipeCustomAttributeDataUtility
  {
    public static TypePipeCustomAttributeData Create (CustomAttributeDeclaration customAttributeDeclaration)
    {
      var ctorTypes = customAttributeDeclaration.AttributeConstructorInfo.GetParameters().Select (p => p.ParameterType);
      var ctorArguments = ctorTypes.Zip (customAttributeDeclaration.ConstructorArguments, (t, v) => new TypePipeCustomAttributeTypedArgument (t, v));

      var namedArguments = customAttributeDeclaration.NamedArguments.Select (
          d =>
          new TypePipeCustomAttributeNamedArgument (
              d.MemberInfo,
              new TypePipeCustomAttributeTypedArgument (
                  GetMemberType (d.MemberInfo),
                  d.Value)));

      return new TypePipeCustomAttributeData (customAttributeDeclaration.AttributeConstructorInfo, ctorArguments, namedArguments);
    }

    public static TypePipeCustomAttributeData Create (CustomAttributeData customAttributeData)
    {
      throw new NotImplementedException();
    }

    private static Type GetMemberType (MemberInfo member)
    {
      Assertion.IsTrue (member is FieldInfo || member is PropertyInfo);

      return member is FieldInfo
                 ? ((FieldInfo) member).FieldType
                 : ((PropertyInfo) member).PropertyType;
    }
  }
}