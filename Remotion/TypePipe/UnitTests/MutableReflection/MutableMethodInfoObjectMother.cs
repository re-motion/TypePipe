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
using Microsoft.Scripting.Ast;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection.Descriptors;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class MutableMethodInfoObjectMother
  {
    public static MutableMethodInfo Create (
        MutableType declaringType = null,
        string name = "UnspecifiedMethod",
        MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.HideBySig,
        Type returnType = null,
        IEnumerable<ParameterDeclaration> parameterDeclarations = null,
        MethodInfo baseMethod = null,
        Expression body = null)
    {
      return CreateForNew (declaringType, name, attributes, returnType, parameterDeclarations, baseMethod, body);
    }

    public static MutableMethodInfo CreateForNew (
        MutableType declaringType = null,
        string name = "UnspecifiedMethod",
        MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.HideBySig,
        Type returnType = null,
        IEnumerable<ParameterDeclaration> parameterDeclarations = null,
        MethodInfo baseMethod = null,
        Expression body = null)
    {
      var descriptor = MethodDescriptorObjectMother.CreateForNew (
          name, attributes, returnType, parameterDeclarations, body: body, baseMethod: baseMethod);

      return new MutableMethodInfo (
          declaringType ?? MutableTypeObjectMother.Create(),
          descriptor);
    }

    public static MutableMethodInfo CreateForExisting (MutableType declaringType = null, MethodInfo underlyingMethod = null)
    {
      int i;
      underlyingMethod = underlyingMethod
                         ?? NormalizingMemberInfoFromExpressionUtility.GetMethod ((UnspecifiedType obj) => obj.UnspecifiedMethod (out i, 0.7));
      declaringType = declaringType ?? MutableTypeObjectMother.CreateForExisting (underlyingMethod.DeclaringType);

      var descriptor = MethodDescriptorObjectMother.CreateForExisting (underlyingMethod);

      return new MutableMethodInfo (declaringType, descriptor);
    }

    public static MutableMethodInfo CreateForExistingAndModify (MethodInfo underlyingMethod = null)
    {
      var method = CreateForExisting (underlyingMethod: underlyingMethod ?? ReflectionObjectMother.GetSomeModifiableMethod());
      MutableMethodInfoTestHelper.ModifyMethod (method);
      return method;
    }

    private class UnspecifiedType
    {
      public string UnspecifiedMethod (out int i, double d) { i = 7; Dev.Null = d; return ""; }
    }
  }
}