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
    private MutableType _nonSerializableDeserializationCallbackType;
    private MutableType _serializableInterfaceMissingCtorType;

    private MethodInfo _someInitializationMethod;

    [SetUp]
    public void SetUp ()
    {
      _enabler = new ProxySerializationEnabler();

      _nonSerializableType = MutableTypeObjectMother.Create();
      _serializableType = MutableTypeObjectMother.CreateForExisting (typeof (SerializableType));
      _serializableInterfaceType = MutableTypeObjectMother.CreateForExisting (typeof (SerializableInterfaceType));
      _nonSerializableDeserializationCallbackType = MutableTypeObjectMother.CreateForExisting (typeof (NonSerializableDeserializationCallbackType));
      _serializableInterfaceMissingCtorType = MutableTypeObjectMother.CreateForExisting (typeof (SerializableInterfaceMissingCtorType));

      _someInitializationMethod = ReflectionObjectMother.GetSomeInstanceMethod();
    }

    [Test]
    public void MakeSerializable_NonSerializableType_AddedFields_WithInitializations ()
    {
      _nonSerializableType.AddField ("abc", typeof (int));

      _enabler.MakeSerializable (_nonSerializableType, _someInitializationMethod);

      Assert.That (_nonSerializableType.AddedInterfaces, Is.Empty);
      Assert.That (_nonSerializableType.AllMutableMethods, Is.Empty);
    }

    [Test]
    public void MakeSerializable_SerializableType_AddedFields ()
    {
      _serializableType.AddField ("abc", typeof (int));

      _enabler.MakeSerializable (_serializableType, initializationMethod: null);

      Assert.That (_serializableType.AddedInterfaces, Is.Empty);
      Assert.That (_serializableType.AllMutableMethods, Is.Empty);
    }

    [Test]
    public void MakeSerializable_SerializableType_WithInitializations ()
    {
      var initMethod = _serializableType.AddMethod ("InitMethod", 0, typeof (void), ParameterDeclaration.EmptyParameters, ctx => Expression.Empty());

      _enabler.MakeSerializable (_serializableType, initMethod);

      Assert.That (_serializableType.AddedInterfaces, Is.EqualTo (new[] { typeof (IDeserializationCallback) }));
      Assert.That (_serializableType.AllMutableMethods.Count(), Is.EqualTo (2));

      var method = _serializableType.AddedMethods[1];
      Assert.That (method.Name, Is.EqualTo ("System.Runtime.Serialization.IDeserializationCallback.OnDeserialization"));
      Assert.That (method.GetParameters().Select (p => p.ParameterType), Is.EqualTo (new[] { typeof (object) }));
      var expectedBody = MethodCallExpression.Call (new ThisExpression (_serializableType), initMethod);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, method.Body);
    }

    [Test]
    public void MakeSerializable_SerializableType_WithInitializations_Implements_IDeserializationCallback ()
    {
      _serializableType.AddInterface (typeof (IDeserializationCallback));
      var interfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IDeserializationCallback obj) => obj.OnDeserialization (null));
      var method = _serializableType.GetOrAddMutableMethod (interfaceMethod);
      var oldBody = ExpressionTreeObjectMother.GetSomeExpression (typeof (void));
      method.SetBody (ctx => oldBody);

      _enabler.MakeSerializable (_serializableType, _someInitializationMethod);

      Assert.That (_serializableType.AllMutableMethods.Count(), Is.EqualTo (1));
      var expectedBody = Expression.Block (
          typeof (void), oldBody, MethodCallExpression.Call (new ThisExpression (_serializableType), _someInitializationMethod));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, method.Body);
    }

    [Test]
    public void MakeSerializable_SerializableType_WithInitializations_NullIntializationMethod ()
    {
      _serializableType.AddInstanceInitialization (ctx => ExpressionTreeObjectMother.GetSomeExpression());

      _enabler.MakeSerializable (_serializableType, initializationMethod: null);

      Assert.That (_serializableType.AddedInterfaces, Is.Empty);
      Assert.That (_serializableType.AllMutableMethods, Is.Empty);
    }

    [Test]
    public void MakeSerializable_SerializableInterfaceType_AddedFields_WithInitializations ()
    {
      var deserializationCtor = _serializableInterfaceType.AllMutableConstructors.Single();
      Assert.That (deserializationCtor.IsModified, Is.False);
      var oldCtorBody = deserializationCtor.Body;
      var field1 = _serializableInterfaceType.AddField ("field", typeof (int));
      var field2 = _serializableInterfaceType.AddField ("xxx", typeof (int));
      var field3 = _serializableInterfaceType.AddField ("xxx", typeof (string));
      _serializableInterfaceType.AddField ("does not matter", typeof (double), FieldAttributes.Static);

      _enabler.MakeSerializable (_serializableInterfaceType, _someInitializationMethod);

      Assert.That (_serializableInterfaceType.AddedInterfaces, Is.Empty);
      Assert.That (_serializableInterfaceType.AllMutableMethods.Count(), Is.EqualTo (1));
      Assert.That (_serializableInterfaceType.AllMutableConstructors, Is.EqualTo (new[] { deserializationCtor }));
      Assert.That (deserializationCtor.IsModified, Is.True);

      var method = _serializableInterfaceType.ExistingMutableMethods.Single();
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
          );
      ExpressionTreeComparer.CheckAreEqualTrees (expectedCtorBody, deserializationCtor.Body);
    }

    [Test]
    public void MakeSerializable_NonDeserializationCallbackType_WithInitializations ()
    {
      _enabler.MakeSerializable (_nonSerializableDeserializationCallbackType, _someInitializationMethod);

      Assert.That (_nonSerializableDeserializationCallbackType.AddedInterfaces, Is.Empty);
      Assert.That (_nonSerializableDeserializationCallbackType.AllMutableMethods.Count (), Is.EqualTo (1));
      var method = _nonSerializableDeserializationCallbackType.ExistingMutableMethods.Single ();
      Assert.That (method.IsModified, Is.False);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "The modified type implements 'ISerializable' but does not define a deserialization constructor.")]
    public void MakeSerializable_SerializableInterfaceType_AddedFields_UnaccessibleCtor ()
    {
      _enabler.MakeSerializable (_serializableInterfaceMissingCtorType, _someInitializationMethod);
    }

    [Serializable]
    class SerializableType { }

    class SerializableInterfaceType : ISerializable
    {
      public SerializableInterfaceType (SerializationInfo info, StreamingContext context) { }
      public virtual void GetObjectData (SerializationInfo info, StreamingContext context) { }
    }

    class NonSerializableDeserializationCallbackType : IDeserializationCallback
    {
      public void OnDeserialization (object sender) { }
    }

    class SerializableInterfaceMissingCtorType : ISerializable
    {
      public virtual void GetObjectData (SerializationInfo info, StreamingContext context) { }
    }

    void InitializeMethod () { }
  }
}