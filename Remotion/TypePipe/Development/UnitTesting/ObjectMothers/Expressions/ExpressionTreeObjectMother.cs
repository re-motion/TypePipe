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
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Expressions;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.Expressions;

namespace Remotion.Development.TypePipe.UnitTesting.ObjectMothers.Expressions
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

    public static Expression GetSomeWritableExpression (Type type)
    {
      return Expression.Variable (type);
    }

    public static ThisExpression GetSomeThisExpression (Type type = null)
    {
      return new ThisExpression (type ?? ReflectionObjectMother.GetSomeType());
    }

    public static NewDelegateExpression GetSomeNewDelegateExpression ()
    {
      var delegateType = typeof (Func<int, object, string>);
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.DomainMethod (7, null));
      var target = GetSomeExpression (method.DeclaringType);
      return new NewDelegateExpression (delegateType, target, method);
    }

    public static BoxAndCastExpression GetSomeBoxAndCastExpression ()
    {
      var operand = GetSomeExpression();
      var type = ReflectionObjectMother.GetSomeType();
      return new BoxAndCastExpression (operand, type);
    }

    public static UnboxExpression GetSomeUnboxExpression ()
    {
      var operand = GetSomeExpression();
      var type = ReflectionObjectMother.GetSomeType();
      return new UnboxExpression (operand, type);
    }

    class DomainType
    {
      // ReSharper disable UnusedParameter.Local
      public string DomainMethod (int i, object o)
      // ReSharper restore UnusedParameter.Local
      {
        return "";
      }
    }
  }
}