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
using Microsoft.Scripting.Ast;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.UnitTests.MutableReflection;

namespace Remotion.TypePipe.UnitTests.Expressions
{
  public static class ExpressionTreeObjectMother
  {
    public static Expression GetSomeExpression ()
    {
      return Expression.Constant ("some expression");
    }

    public static Expression GetSomeExpression (Type type)
    {
      return Expression.Default (type);
    }

    public static ThisExpression GetSomeThisExpression ()
    {
      return new ThisExpression (ReflectionObjectMother.GetSomeType());
    }

    public static OriginalBodyExpression GetSomeOriginalBodyExpression ()
    {
      var method = ReflectionObjectMother.GetSomeMethod();
      return new OriginalBodyExpression (method, method.ReturnType, Enumerable.Empty<Expression>());
    }

    public static NewDelegateExpression GetSomeNewDelegateExpression ()
    {
      var delegateType = typeof (Action); // TODO 5080: must match
      var method = ReflectionObjectMother.GetSomeMethod();
      var target = GetSomeExpression (method.DeclaringType);
      return new NewDelegateExpression (delegateType, target, method);
    }
  }
}