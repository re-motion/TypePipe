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
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.Expressions;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class MutableMethodInfoObjectMother
  {
    public static MutableMethodInfo Create (
        ProxyType declaringType = null,
        string name = "UnspecifiedMethod",
        MethodAttributes attributes = (MethodAttributes) 7,
        ParameterDeclaration returnParameter = null,
        IEnumerable<ParameterDeclaration> parameters = null,
        MethodInfo baseMethod = null,
        Expression body = null)
    {
      declaringType = declaringType ?? ProxyTypeObjectMother.Create();
      returnParameter = returnParameter ?? ParameterDeclaration.CreateReturnParameter (typeof (void));
      parameters = parameters ?? ParameterDeclaration.EmptyParameters;
      // baseMethod stays null.
      body = body == null && !attributes.IsSet (MethodAttributes.Abstract)
                 ? ExpressionTreeObjectMother.GetSomeExpression (returnParameter.Type)
                 : body;

      return new MutableMethodInfo (declaringType, name, attributes, returnParameter, parameters, baseMethod, body);
    }

    // tODO remove
    public static MutableMethodInfo CreateForNew (
        ProxyType declaringType = null,
        string name = "UnspecifiedMethod",
        MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.HideBySig,
        Type returnType = null,
        IEnumerable<ParameterDeclaration> parameterDeclarations = null,
        MethodInfo baseMethod = null,
        Expression body = null)
    {
      return null;
    }

    // TODO remove
    public static MutableMethodInfo CreateForExisting (ProxyType declaringType = null, MethodInfo underlyingMethod = null)
    {
      return null;
    }

    public static MutableMethodInfo CreateForExistingAndModify (MethodInfo underlyingMethod = null)
    {
      var method = CreateForExisting (underlyingMethod: underlyingMethod ?? ReflectionObjectMother.GetSomeModifiableMethod());
      MutableMethodInfoTestHelper.ModifyMethod (method);
      return method;
    }
  }
}