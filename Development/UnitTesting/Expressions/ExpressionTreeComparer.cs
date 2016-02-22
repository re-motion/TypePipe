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
using System.Collections;
using System.Linq;
using System.Reflection;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.Utilities;

namespace Remotion.TypePipe.Development.UnitTesting.Expressions
{
  /// <summary>
  /// Compares two <see cref="Expression"/> trees constructed from <B>DLR AST</B> expressions.
  /// </summary>
  public class ExpressionTreeComparer
  {
    public static void CheckAreEqualTrees (Expression expectedTree, Expression actualTree)
    {
      ArgumentUtility.CheckNotNull ("expectedTree", expectedTree);
      ArgumentUtility.CheckNotNull ("actualTree", actualTree);

      var comparer = new ExpressionTreeComparer (expectedTree.DebugView, actualTree.DebugView);
      comparer.CheckAreEqualNodes (expectedTree, actualTree);
    }

    private readonly string _expectedInitial;
    private readonly string _actualInitial;

    private ExpressionTreeComparer (string expectedInitial, string actualInitial)
    {
      ArgumentUtility.CheckNotNull ("expectedInitial", expectedInitial);
      ArgumentUtility.CheckNotNull ("actualInitial", actualInitial);

      _expectedInitial = expectedInitial;
      _actualInitial = actualInitial;
    }

    private void CheckAreEqualNodes (Expression expected, Expression actual)
    {
      if (expected == null)
      {
        Assert.AreEqual (null, actual, GetMessage (null, actual, "Null nodes"));
      }
      else
      {
        CheckAreEqualExpressionTypes (expected, actual);
        Assert.AreEqual (expected.NodeType, actual.NodeType, GetMessage (expected, actual, "NodeType"));
        Assert.AreEqual (expected.Type, actual.Type, GetMessage (expected, actual, "Type"));
        CheckAreEqualObjects (expected, actual);
      }
    }

    private void CheckAreEqualObjects (object expected, object actual)
    {
      if (expected is Expression)
      {
        Assert.IsInstanceOf<Expression> (actual, GetMessage (expected, actual, "NodeType"));
        CheckAreEqualExpressionTypes ((Expression) expected, (Expression) actual);
      }
      else
        Assert.AreEqual (expected.GetType(), actual.GetType(), GetMessage (expected, actual, "GetType()"));

      foreach (PropertyInfo property in expected.GetType().GetProperties (BindingFlags.Instance | BindingFlags.Public))
      {
        object value1 = property.GetValue (expected, null);
        object value2 = property.GetValue (actual, null);
        CheckAreEqualProperties (property, property.PropertyType, value1, value2, expected, actual);
      }
    }

    private void CheckAreEqualExpressionTypes (Expression expected, Expression actual)
    {
      if (expected is BlockExpression)
        Assert.IsInstanceOf<BlockExpression> (actual, GetMessage (expected, actual, "NodeType"));
      else if (expected is MethodCallExpression)
        Assert.IsInstanceOf<MethodCallExpression> (actual, GetMessage (expected, actual, "NodeType"));
      else
        Assert.AreEqual (expected.GetType(), actual.GetType(), GetMessage (expected, actual, "NodeType"));
    }

    private void CheckAreEqualProperties (PropertyInfo property, Type valueType, object value1, object value2, object expected, object actual)
    {
      var structurallyComparedTypes = new[] { typeof (MemberBinding), typeof (ElementInit) };
      if (typeof (Expression).IsAssignableFrom (valueType))
      {
        Expression subNode1 = (Expression) value1;
        Expression subNode2 = (Expression) value2;
        CheckAreEqualNodes (subNode1, subNode2);
      }
      else if (structurallyComparedTypes.Any (t => t.IsAssignableFrom (valueType)))
      {
        CheckAreEqualObjects (value1, value2);
      }
      else if (typeof (IList).IsAssignableFrom (valueType))
      {
        IList list1 = (IList) value1;
        IList list2 = (IList) value2;
        if (list1 == null || list2 == null)
        {
          Assert.AreEqual (list1, list2, "One of the lists in " + property.Name + " is null.");
        }
        else
        {
          Assert.AreEqual (list1.Count, list2.Count, GetMessage (expected, actual, "Number of elements in " + property.Name));
          for (int i = 0; i < list1.Count; ++i)
          {
            var elementType1 = list1[i] != null ? list1[i].GetType() : typeof (object);
            var elementType2 = list2[i] != null ? list2[i].GetType() : typeof (object);

            if (typeof (BlockExpression).IsAssignableFrom (elementType1))
            {
              Assert.AreEqual (
                  true,
                  typeof (BlockExpression).IsAssignableFrom (elementType2),
                  GetMessage (elementType1, elementType2, "BlockExpression"));
            }
            else if (typeof (MethodCallExpression).IsAssignableFrom (elementType1))
            {
              Assert.AreEqual (
                  true,
                  typeof (MethodCallExpression).IsAssignableFrom (elementType2),
                  GetMessage (elementType1, elementType2, "MethodCallExpression"));
            }
            else
            {
              Assert.AreEqual (
                  elementType1,
                  elementType2,
                  string.Format (
                      "The item types of the items in the lists in {0} differ: One is '{1}', the other is '{2}'.\nExpected tree: {3}\nActual tree: {4}",
                      property.Name,
                      elementType1,
                      elementType2,
                      _expectedInitial,
                      _actualInitial));
            }

            CheckAreEqualProperties (property, elementType1, list1[i], list2[i], expected, actual);
          }
        }
      }
      else if (value1 is NonVirtualCallMethodInfoAdapter && value2 is NonVirtualCallMethodInfoAdapter)
      {
        var adapter1 = (NonVirtualCallMethodInfoAdapter) value1;
        var adapter2 = (NonVirtualCallMethodInfoAdapter) value2;
        var ctorAdapter1 = adapter1.AdaptedMethod as ConstructorAsMethodInfoAdapter;
        var ctorAdapter2 = adapter2.AdaptedMethod as ConstructorAsMethodInfoAdapter;

        Assert.AreEqual (
            true,
            adapter1.AdaptedMethod == adapter2.AdaptedMethod
            || (ctorAdapter1 != null && ctorAdapter2 != null && ctorAdapter1.AdaptedConstructor == ctorAdapter2.AdaptedConstructor),
            "Adapted MethodInfo is not equal (non-virtual or ctor).");
      }
      else if (value1 is MemberInfo && value2 is MemberInfo)
      {
        Assert.AreEqual (
            true,
            MemberInfoEqualityComparer<MemberInfo>.Instance.Equals ((MemberInfo) value1, (MemberInfo) value2),
            GetMessage (value1, value2, "MemberInfos"));
      }
      else
      {
        Assert.AreEqual (value1, value2, GetMessage (expected, actual, "Property " + property.Name));
      }
    }

    private string GetMessage (object expected, object actual, string context)
    {
      return string.Format (
          "Trees are not equal: {0}\nExpected node: {1}\nActual   node: {2}\nExpected tree:\n{3}\nActual tree:\n{4}",
          context,
          expected,
          actual,
          _expectedInitial,
          _actualInitial);
    }
  }
}