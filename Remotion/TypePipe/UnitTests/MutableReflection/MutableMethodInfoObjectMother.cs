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
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.UnitTests.Expressions;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class MutableMethodInfoObjectMother
  {
    public static MutableMethodInfo Create (
        ProxyType declaringType = null,
        string name = "UnspecifiedMethod",
        MethodAttributes attributes = (MethodAttributes) 7,
        Type returnType = null,
        IEnumerable<ParameterDeclaration> parameters = null,
        MethodInfo baseMethod = null,
        Expression body = null,
        IEnumerable<Type> genericParameters = null)
    {
      declaringType = declaringType ?? ProxyTypeObjectMother.Create();
      if (baseMethod != null)
        attributes = attributes.Set (MethodAttributes.Virtual);
      returnType = returnType ?? typeof (void);
      parameters = parameters ?? ParameterDeclaration.None;
      // baseMethod stays null.
      body = body == null && !attributes.IsSet (MethodAttributes.Abstract) ? ExpressionTreeObjectMother.GetSomeExpression (returnType) : body;
      genericParameters = genericParameters ?? Type.EmptyTypes;

      return new MutableMethodInfo (declaringType, name, attributes, genericParameters, returnType, parameters, baseMethod, body);
    }
  }
}