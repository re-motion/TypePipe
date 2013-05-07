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
      public string DomainMethod (int i, object o)
      {
        Dev.Null = i;
        Dev.Null = o;
        return "";
      }
    }
  }
}