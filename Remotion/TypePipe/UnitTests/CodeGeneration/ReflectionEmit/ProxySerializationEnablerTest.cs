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
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.TypePipe.UnitTests.MutableReflection;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class ProxySerializationEnablerTest
  {
    private ProxySerializationEnabler _enabler;

    private MutableType _nonSerializableType;
    private MutableType _serializableType;
    private MutableType _serializableInterfaceType;
    private MutableType _missingCtor;

    [SetUp]
    public void SetUp ()
    {
      _enabler = new ProxySerializationEnabler();

      _nonSerializableType = MutableTypeObjectMother.CreateForExisting (typeof (NonSerializableType));
      _serializableType = MutableTypeObjectMother.CreateForExisting (typeof (SerializableType));
      _serializableInterfaceType = MutableTypeObjectMother.CreateForExisting (typeof (SerializableInterfaceType));
      _missingCtor = MutableTypeObjectMother.CreateForExisting (typeof (MissingCtor));
    }

    [Test]
    public void MakeSerializable_NonSerializableType ()
    {
      var defaultCtor = _nonSerializableType.AllMutableConstructors.Single();

      _enabler.MakeSerializable (_nonSerializableType);

      Assert.That (_nonSerializableType.AddedInterfaces, Is.Empty);
      Assert.That (_nonSerializableType.AllMutableMethods, Is.Empty);
      Assert.That (_nonSerializableType.AllMutableConstructors, Is.EqualTo (new[] { defaultCtor }));
    }

    [Test]
    public void MakeSerializable_SerializableType_AddedFields ()
    {
      var defaultCtor = _serializableType.AllMutableConstructors.Single();
      _serializableInterfaceType.AddField ("abc", typeof (int));

      _enabler.MakeSerializable (_serializableType);

      Assert.That (_serializableType.AddedInterfaces, Is.Empty);
      Assert.That (_serializableType.AllMutableMethods, Is.Empty);
      Assert.That (_serializableType.AllMutableConstructors, Is.EqualTo (new[] { defaultCtor }));
    }

    [Test]
    public void MakeSerializable_SerializableInterfaceType_AddedFields ()
    {
      var deserializationCtor = _serializableInterfaceType.AllMutableConstructors.Single();
      Assert.That (deserializationCtor.IsModified, Is.False);
      var oldCtorBody = deserializationCtor.Body;
      var field1 = _serializableInterfaceType.AddField ("field", typeof (int));
      var field2 = _serializableInterfaceType.AddField ("xxx", typeof (int));
      var field3 = _serializableInterfaceType.AddField ("xxx", typeof (string));
      _serializableInterfaceType.AddField ("does not matter", typeof (double), FieldAttributes.Static);

      _enabler.MakeSerializable (_serializableInterfaceType);

      Assert.That (_serializableInterfaceType.AddedInterfaces, Is.Empty);
      Assert.That (_serializableInterfaceType.AllMutableMethods.Count(), Is.EqualTo (1));
      Assert.That (_serializableInterfaceType.AllMutableConstructors, Is.EqualTo (new[] { deserializationCtor }));
      Assert.That (deserializationCtor.IsModified, Is.True);

      var method = _serializableInterfaceType.AllMutableMethods.Single();
      Assert.That (method.Name, Is.EqualTo ("GetObjectData"));
      Assert.That (method.GetParameters().Select (p => p.ParameterType), Is.EqualTo (new[] { typeof (SerializationInfo), typeof (StreamingContext) }));

      var serializationInfo = method.ParameterExpressions[0];
      var thisExpr = new ThisExpression (_serializableInterfaceType);
      var field1Expr = Expression.Field (thisExpr, field1);
      var field2Expr = Expression.Field (thisExpr, field2);
      var field3Expr = Expression.Field (thisExpr, field3);
      var baseMethod =
          NormalizingMemberInfoFromExpressionUtility.GetMethod ((SerializableInterfaceType obj) => obj.GetObjectData (null, new StreamingContext()));
      var expectedBody = Expression.Block (
          new OriginalBodyExpression (baseMethod, typeof (void), method.ParameterExpressions.Cast<Expression>()),
          Expression.Call (serializationInfo, "AddValue", Type.EmptyTypes, Expression.Constant ("<tp>field"), field1Expr),
          Expression.Call (serializationInfo, "AddValue", Type.EmptyTypes, Expression.Constant ("<tp>xxx@System.Int32"), field2Expr),
          Expression.Call (serializationInfo, "AddValue", Type.EmptyTypes, Expression.Constant ("<tp>xxx@System.String"), field3Expr));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, method.Body);

      var getValue = NormalizingMemberInfoFromExpressionUtility.GetMethod ((SerializationInfo obj) => obj.GetValue ("", null));
      var expectedCtorBody = Expression.Block (
          typeof (void),
          Expression.Block (
              oldCtorBody,
              Expression.Assign (
                  field1Expr,
                  Expression.Convert (
                      Expression.Call (serializationInfo, getValue, Expression.Constant ("<tp>field"), Expression.Constant (typeof (int))),
                      typeof (int))),
              Expression.Assign (
                  field2Expr,
                  Expression.Convert (
                      Expression.Call (serializationInfo, getValue, Expression.Constant ("<tp>xxx@System.Int32"), Expression.Constant (typeof (int))),
                      typeof (int))),
              Expression.Assign (
                  field3Expr,
                  Expression.Convert (
                      Expression.Call (
                          serializationInfo, getValue, Expression.Constant ("<tp>xxx@System.String"), Expression.Constant (typeof (string))),
                      typeof (string)))
              ));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedCtorBody, deserializationCtor.Body);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "The modified type implements 'ISerializable' but does not define a deserialization constructor.")]
    public void MakeSerializable_SerializableInterfaceType_AddedFields_UnaccessibleCtor ()
    {
      _enabler.MakeSerializable (_missingCtor);
    }
    
    private class NonSerializableType { }

    [Serializable]
    private class SerializableType { }

    [Serializable]
    private class SerializableInterfaceType : ISerializable
    {
      public SerializableInterfaceType (SerializationInfo info, StreamingContext context) { }
      public virtual void GetObjectData (SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    private class MissingCtor : ISerializable
    {
      public virtual void GetObjectData (SerializationInfo info, StreamingContext context) { }
    }
  }
}