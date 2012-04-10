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
using System.Reflection;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Utilities;

namespace Remotion.TypePipe.UnitTests.Expressions
{
  // This class was copied from re-linq: Remotion.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeComparer
  public class ExpressionTreeComparer
  {
    public static void CheckAreEqualTrees (Expression expectedTree, Expression actualTree)
    {
      var comparer = new ExpressionTreeComparer (expectedTree.DebugView, actualTree.DebugView);
      comparer.CheckAreEqualNodes (expectedTree, actualTree);
    }

    private readonly object _expectedInitial;
    private readonly object _actualInitial;

    public ExpressionTreeComparer (object expectedInitial, object actualInitial)
    {
      ArgumentUtility.CheckNotNull ("expectedInitial", expectedInitial);
      ArgumentUtility.CheckNotNull ("actualInitial", actualInitial);

      _expectedInitial = expectedInitial;
      _actualInitial = actualInitial;
    }

    public void CheckAreEqualNodes (Expression expected, Expression actual)
    {
      if (expected == null)
        Assert.IsNull (actual, GetMessage (expected, actual, "Null nodes"));
      else
      {
        Assert.AreEqual (expected.GetType(), actual.GetType(), GetMessage (expected, actual, "NodeType"));
        Assert.AreEqual (expected.NodeType, actual.NodeType, GetMessage (expected, actual, "NodeType"));
        Assert.AreEqual (expected.Type, actual.Type, GetMessage (expected, actual, "Type"));
        CheckAreEqualObjects (expected, actual);
      }
    }

    public void CheckAreEqualObjects (object expected, object actual)
    {
      Assert.AreEqual (expected.GetType(), actual.GetType(), GetMessage (expected, actual, "GetType()"));

      foreach (PropertyInfo property in expected.GetType().GetProperties (BindingFlags.Instance | BindingFlags.Public))
      {
        object value1 = property.GetValue (expected, null);
        object value2 = property.GetValue (actual, null);
        CheckAreEqualProperties (property, property.PropertyType, value1, value2, expected, actual);
      }
    }

    private void CheckAreEqualProperties (PropertyInfo property, Type valueType, object value1, object value2, object e1, object e2)
    {
      if (typeof (Expression).IsAssignableFrom (valueType))
      {
        Expression subNode1 = (Expression) value1;
        Expression subNode2 = (Expression) value2;
        CheckAreEqualNodes (subNode1, subNode2);
      }
      else if (typeof (MemberBinding).IsAssignableFrom (valueType) || typeof (ElementInit).IsAssignableFrom (valueType))
        CheckAreEqualObjects (value1, value2);
      else if (typeof (IList).IsAssignableFrom (valueType))
      {
        IList list1 = (IList) value1;
        IList list2 = (IList) value2;
        if (list1 == null || list2 == null)
          Assert.AreEqual (list1, list2, "One of the lists in " + property.Name + " is null.");
        else
        {
          Assert.AreEqual (list1.Count, list2.Count, GetMessage (e1, e2, "Number of elements in " + property.Name));
          for (int i = 0; i < list1.Count; ++i)
          {
            var elementType1 = list1[i] != null ? list1[i].GetType() : typeof (object);
            var elementType2 = list2[i] != null ? list2[i].GetType() : typeof (object);
            Assert.AreSame (
                elementType1,
                elementType2,
                string.Format (
                    "The item types of the items in the lists in {0} differ: One is '{1}', the other is '{2}'.\nTree 1: {3}\nTree 2: {4}",
                    property.Name,
                    elementType1,
                    elementType2,
                    _expectedInitial,
                    _actualInitial));

            CheckAreEqualProperties (property, elementType1, list1[i], list2[i], e1, e2);
          }
        }
      }
      else
        Assert.AreEqual (value1, value2, GetMessage (e1, e2, "Property " + property.Name));
    }

    private string GetMessage (object e1, object e2, string context)
    {
      return string.Format (
          "Trees are not equal: {0}\nNode 1: {1}\nNode 2: {2}\nTree 1: {3}\nTree 2: {4}", context, e1, e2, _expectedInitial, _actualInitial);
    }
  }
}