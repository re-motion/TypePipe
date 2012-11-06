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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class MutableMethodInfoObjectMother
  {
    public static MutableMethodInfo Create (
        MutableType declaringType = null,
        string name = "UnspecifiedMethod",
        MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig,
        Type returnType = null,
        IEnumerable<ParameterDeclaration> parameterDeclarations = null,
        MethodInfo baseMethod = null,
        Expression body = null)
    {
      return CreateForNew (
          declaringType, name, methodAttributes, returnType, parameterDeclarations, baseMethod, body);
    }

    public static MutableMethodInfo CreateForExisting (MethodInfo underlyingMethod = null)
    {
      int i;
      underlyingMethod = underlyingMethod
                         ?? NormalizingMemberInfoFromExpressionUtility.GetMethod ((UnspecifiedType obj) => obj.UnspecifiedMethod (out i, 0.7));
      var declaringType = MutableTypeObjectMother.CreateForExisting (underlyingMethod.DeclaringType);
      var descriptor = UnderlyingMethodInfoDescriptorObjectMother.CreateForExisting (underlyingMethod);

      return new MutableMethodInfo (declaringType, descriptor);
    }

    public static MutableMethodInfo CreateForExistingAndModify (MethodInfo underlyingMethod = null)
    {
      var method = CreateForExisting (underlyingMethod ?? ReflectionObjectMother.GetSomeModifiableMethod());
      MutableMethodInfoTestHelper.ModifyMethod (method);
      return method;
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
      var descriptor = UnderlyingMethodInfoDescriptorObjectMother.CreateForNew (
          name, attributes, returnType, parameterDeclarations, body: body, baseMethod: baseMethod);

      return new MutableMethodInfo (
          declaringType ?? MutableTypeObjectMother.Create(),
          descriptor);
    }

    private class UnspecifiedType
    {
      public string UnspecifiedMethod (out int i, double d) { i = 7; return ""; }
    }
  }
}