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
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Collections;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Serialization.Implementation;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.Serialization.Implementation
{
  [TestFixture]
  public class SerializedFieldHandlerTest
  {
    private SerializedFieldHandler _handler;

    [SetUp]
    public void SetUp ()
    {
      _handler = new SerializedFieldHandler();
    }

    [Test]
    public void GetSerializedFieldMapping_Filtering ()
    {
      var field1 = NormalizingMemberInfoFromExpressionUtility.GetField (() => s_staticField);
      var field2 = NormalizingMemberInfoFromExpressionUtility.GetField (() => _instanceField);
      var field3 = NormalizingMemberInfoFromExpressionUtility.GetField (() => _nonSerializedField);

      var result = _handler.GetSerializedFieldMapping (new[] { field1, field2, field3 });

      Assert.That (result, Is.EqualTo (new[] { Tuple.Create ("<tp>_instanceField", field2) }));
    }

    [Test]
    public void GetSerializedFieldMapping_SameName ()
    {
      FieldInfo field1 = MutableFieldInfoObjectMother.Create (name: "abc", type: typeof (int));
      FieldInfo field2 = MutableFieldInfoObjectMother.Create (name: "abc", type: typeof (string));

      var result = _handler.GetSerializedFieldMapping (new[] { field1, field2 });

      Assert.That (result, Is.EqualTo (new[] { Tuple.Create ("<tp>abc@System.Int32", field1), Tuple.Create ("<tp>abc@System.String", field2) }));
    }


    //Expression @this, Expression serializationInfo, IEnumerable<Tuple<string, FieldInfo>> serializedFieldMapping



    [Test]
    public void BuildFieldSerializationExpressions ()
    {
      var field = ReflectionObjectMother.GetSomeInstanceField();
      var @this = ExpressionTreeObjectMother.GetSomeExpression (field.DeclaringType);
      var serializationInfo = ExpressionTreeObjectMother.GetSomeExpression (typeof (SerializationInfo));
      var fieldMapping = new[] { Tuple.Create ("abc", field) };

      var result = _handler.BuildFieldSerializationExpressions (@this, serializationInfo, fieldMapping).Single();

      Assert.That (result, Is.InstanceOf<MethodCallExpression>());
      var methodCallExpression = (MethodCallExpression) result;
      Assert.That (methodCallExpression.Object, Is.SameAs (serializationInfo));
      Assert.That (methodCallExpression.Method.Name, Is.EqualTo ("AddValue"));
      Assert.That (methodCallExpression.Arguments, Has.Count.EqualTo (2));
      Assert.That (methodCallExpression.Arguments[0], Is.TypeOf<ConstantExpression>().And.Property ("Value").EqualTo ("abc"));
      CheckFieldExpression (methodCallExpression.Arguments[1], @this, field);
    }

    [Test]
    public void BuildFieldDesrializationExpressions ()
    {
      var field = ReflectionObjectMother.GetSomeInstanceField();
      var @this = ExpressionTreeObjectMother.GetSomeExpression (field.DeclaringType);
      var serializationInfo = ExpressionTreeObjectMother.GetSomeExpression (typeof (SerializationInfo));
      var fieldMapping = new[] { Tuple.Create ("abc", field) };

      var result = _handler.BuildFieldDeserializationExpressions (@this, serializationInfo, fieldMapping).Single();

      Assert.That (result, Is.InstanceOf<BinaryExpression>());
      var binaryExpression = (BinaryExpression) result;
      CheckFieldExpression (binaryExpression.Left, @this, field);
      Assert.That (binaryExpression.Right, Is.InstanceOf<UnaryExpression>());
      var unaryExpression = (UnaryExpression) binaryExpression.Right;
      Assert.That (unaryExpression.Type, Is.SameAs (field.FieldType));
      Assert.That (unaryExpression.Operand, Is.InstanceOf<MethodCallExpression>());
      var methodCallExpression = (MethodCallExpression) unaryExpression.Operand;
      Assert.That (methodCallExpression.Object, Is.SameAs (serializationInfo));
      var getValueMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((SerializationInfo obj) => obj.GetValue ("", null));
      Assert.That (methodCallExpression.Method, Is.EqualTo (getValueMethod));
      Assert.That (methodCallExpression.Arguments, Has.Count.EqualTo (2));
      Assert.That (methodCallExpression.Arguments[0], Is.TypeOf<ConstantExpression>().And.Property ("Value").EqualTo ("abc"));
      Assert.That (methodCallExpression.Arguments[1], Is.TypeOf<ConstantExpression> ().And.Property ("Value").EqualTo (field.FieldType));
    }

    private void CheckFieldExpression (Expression expression, Expression expectedThis, FieldInfo expectedField)
    {
      Assert.That (expression, Is.InstanceOf<MemberExpression>());
      var memberExpression = (MemberExpression) expression;
      Assert.That (memberExpression.Expression, Is.SameAs (expectedThis));
      Assert.That (memberExpression.Member, Is.SameAs (expectedField));
    }

    private static int s_staticField = 0;
    private int _instanceField = 0;
    [NonSerialized]
    private int _nonSerializedField = 0;
  }
}