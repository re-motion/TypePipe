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
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.UnitTests.Expressions;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class MutableMethodInfoObjectMother
  {
    public static MutableMethodInfo Create (
        ProxyType declaringType = null,
        string name = "UnspecifiedMethod",
        MethodAttributes attributes = (MethodAttributes) 7,
        MethodInfo baseMethod = null,
        Type returnType = null,
        IEnumerable<ParameterDeclaration> parameters = null,
        Expression body = null)
    {
      declaringType = declaringType ?? ProxyTypeObjectMother.Create();
      if (baseMethod != null)
        attributes = attributes.Set (MethodAttributes.Virtual);
      // Base method stays null.
      var genericParameters = GenericParameterDeclaration.None;
      returnType = returnType ?? typeof (void);
      var paras = (parameters ?? ParameterDeclaration.None).ToList();
      var memberSelector = new MemberSelector (new BindingFlagsEvaluator ());
      var context = new MethodBodyCreationContext (
          declaringType, false, paras.Select (p => Expression.Parameter (p.Type)), returnType, baseMethod, memberSelector);
      body = body == null && !attributes.IsSet (MethodAttributes.Abstract) ? ExpressionTreeObjectMother.GetSomeExpression (returnType) : body;

      return new MutableMethodInfo (
          declaringType, name, attributes, baseMethod, genericParameters, ctx => returnType, ctx => paras, () => context, ctx => body);
    }

    public static MutableMethodInfo CreateGeneric (
        ProxyType declaringType = null,
        string name = "UnspecifiedMethod",
        MethodAttributes attributes = (MethodAttributes) 7,
        MethodInfo baseMethod = null,
        IEnumerable<GenericParameterDeclaration> genericParameters = null,
        Func<GenericParameterContext,Type> returnTypeProvider = null,
        Func<GenericParameterContext,IEnumerable<ParameterDeclaration>> parameterProvider = null,
        Func<MethodBodyCreationContext,Expression> bodyProvider = null)
    {
      // Base method stays null.
      declaringType = declaringType ?? ProxyTypeObjectMother.Create ();
      if (baseMethod != null)
        attributes = attributes.Set (MethodAttributes.Virtual);
      genericParameters = genericParameters ?? GenericParameterDeclaration.None;
      returnTypeProvider = returnTypeProvider ?? (ctx => typeof (void));
      parameterProvider = parameterProvider ?? (ctx => ParameterDeclaration.None);
      // baseMethod stays null.
      // TODO: body context
      bodyProvider = bodyProvider ?? CreateBodyProvider (attributes.IsSet (MethodAttributes.Abstract));
      
      return new MutableMethodInfo (
          declaringType, name, attributes, baseMethod, genericParameters, returnTypeProvider, parameterProvider, null, bodyProvider);
    }

    private static Func<MethodBodyCreationContext, Expression> CreateBodyProvider (bool isAbstract)
    {
      if (isAbstract)
        return ctx => null;
      else
        return ctx => Expression.Default (ctx.ReturnType);
    }
  }
}