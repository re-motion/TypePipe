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
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.Serialization.Implementation;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class ProxySerializationEnablerTest
  {
    private ISerializedFieldHandler _serializedFieldHandlerMock;
    
    private ProxySerializationEnabler _enabler;

    private MutableType _someType;
    private MutableType _serializableInterfaceType;
    private MutableType _serializableType;
    private MutableType _deserializationCallbackType;
    private MutableType _serializableInterfaceWithDeserializationCallbackType;

    private MethodInfo _someInitializationMethod;

    [SetUp]
    public void SetUp ()
    {
      _serializedFieldHandlerMock = MockRepository.GenerateStrictMock<ISerializedFieldHandler>();

      _enabler = new ProxySerializationEnabler (_serializedFieldHandlerMock);

      _someType = MutableTypeObjectMother.CreateForExisting (typeof (SomeType));
      _serializableType = MutableTypeObjectMother.CreateForExisting (typeof (SerializableType));
      _serializableInterfaceType = MutableTypeObjectMother.CreateForExisting (typeof (SerializableInterfaceType));
      _deserializationCallbackType = MutableTypeObjectMother.CreateForExisting (typeof (DeserializationCallbackType));
      _serializableInterfaceWithDeserializationCallbackType =
          MutableTypeObjectMother.CreateForExisting (typeof (SerializableWithDeserializationCallbackType));

      _someInitializationMethod = ReflectionObjectMother.GetSomeInstanceMethod();
    }

    [Test]
    public void MakeSerializable_SerializableInterfaceType ()
    {
      StubFilterWithNoSerializedFields();

      _enabler.MakeSerializable (_serializableInterfaceType, _someInitializationMethod);

      Assert.That (_serializableInterfaceType.AddedInterfaces, Is.Empty);
      Assert.That (_serializableInterfaceType.AllMutableMethods.Count (), Is.EqualTo (1));
      var method = _serializableInterfaceType.ExistingMutableMethods.Single ();
      Assert.That (method.IsModified, Is.False);
    }

    [Test]
    public void MakeSerializable_SerializableInterfaceType_SerializedFields ()
    {
      var deserializationCtor = _serializableInterfaceType.AllMutableConstructors.Single();
      Assert.That (deserializationCtor.IsModified, Is.False);
      var oldCtorBody = deserializationCtor.Body;
      var field = _serializableInterfaceType.AddField ("field", typeof (int));
      var fakeField1 = MutableFieldInfoObjectMother.Create (_serializableInterfaceType, "field", typeof (int));
      var fakeField2 = MutableFieldInfoObjectMother.Create (_serializableInterfaceType, "xxx", typeof (int));
      var fakeField3 = MutableFieldInfoObjectMother.Create (_serializableInterfaceType, "xxx", typeof (string));
      _serializedFieldHandlerMock
          .Expect (mock => mock.GetSerializedFields (Arg<IEnumerable<FieldInfo>>.List.Equal (new[] { field })))
          .Return (new[] { fakeField1, fakeField2, fakeField3 });

      _enabler.MakeSerializable (_serializableInterfaceType, _someInitializationMethod);

      _serializedFieldHandlerMock.VerifyAllExpectations();
      Assert.That (_serializableInterfaceType.AddedInterfaces, Is.Empty);
      Assert.That (_serializableInterfaceType.AllMutableMethods.Count(), Is.EqualTo (1));
      Assert.That (_serializableInterfaceType.AllMutableConstructors, Is.EqualTo (new[] { deserializationCtor }));
      Assert.That (deserializationCtor.IsModified, Is.True);

      var method = _serializableInterfaceType.ExistingMutableMethods.Single();
      Assert.That (method.Name, Is.EqualTo ("GetObjectData"));
      Assert.That (method.GetParameters().Select (p => p.ParameterType), Is.EqualTo (new[] { typeof (SerializationInfo), typeof (StreamingContext) }));

      var serializationInfo = method.ParameterExpressions[0];
      var thisExpr = new ThisExpression (_serializableInterfaceType);
      var field1Expr = Expression.Field (thisExpr, fakeField1);
      var field2Expr = Expression.Field (thisExpr, fakeField2);
      var field3Expr = Expression.Field (thisExpr, fakeField3);
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
    public void MakeSerializable_SomeType_SerializedFields ()
    {
      StubFilterWithSerializedFields (_someType);

      _enabler.MakeSerializable (_someType, _someInitializationMethod);

      Assert.That (_someType.AddedInterfaces, Is.Empty);
      Assert.That (_someType.AllMutableMethods, Is.Empty);
    }

    [Test]
    public void MakeSerializable_SomeType ()
    {
      StubFilterWithNoSerializedFields();

      _enabler.MakeSerializable (_someType, initializationMethod: null);

      Assert.That (_someType.AddedInterfaces, Is.Empty);
      Assert.That (_someType.AllMutableMethods, Is.Empty);
    }

    [Test]
    public void MakeSerializableType_SerializableType_WithInitializations ()
    {
      StubFilterWithNoSerializedFields();
      var initMethod = MutableMethodInfoObjectMother.Create (
          _serializableType, parameterDeclarations: ParameterDeclaration.EmptyParameters, returnType: typeof (void));

      _enabler.MakeSerializable (_serializableType, initMethod);

      Assert.That (_serializableType.AddedInterfaces, Is.EqualTo (new[] { typeof (IDeserializationCallback) }));
      Assert.That (_serializableType.AllMutableMethods.Count (), Is.EqualTo (1));

      var method = _serializableType.AddedMethods.Single();
      Assert.That (method.Name, Is.EqualTo ("System.Runtime.Serialization.IDeserializationCallback.OnDeserialization"));
      Assert.That (method.GetParameters ().Select (p => p.ParameterType), Is.EqualTo (new[] { typeof (object) }));
      var expectedBody = MethodCallExpression.Call (new ThisExpression (_serializableType), initMethod);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, method.Body);
    }

    [Test]
    public void MakeSerializableType_DeserializationCallbackType_WithInitializations ()
    {
      StubFilterWithNoSerializedFields();
      var initMethod = MutableMethodInfoObjectMother.Create (
          _deserializationCallbackType, parameterDeclarations: ParameterDeclaration.EmptyParameters, returnType: typeof (void));
      var method = _deserializationCallbackType.ExistingMutableMethods.Single (x => x.Name == "OnDeserialization");
      var oldBody = method.Body;

      _enabler.MakeSerializable (_deserializationCallbackType, initMethod);

      Assert.That (_deserializationCallbackType.AllMutableMethods.Count(), Is.EqualTo (1));
      var expectedBody = Expression.Block (
          typeof (void), oldBody, MethodCallExpression.Call (new ThisExpression (_deserializationCallbackType), initMethod));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, method.Body);
    }

    [Test]
    public void MakeSerializableType_SerializableWithDeserializationCallbackType ()
    {
      StubFilterWithNoSerializedFields();

      _enabler.MakeSerializable (_serializableInterfaceWithDeserializationCallbackType, initializationMethod: null);

      Assert.That (_serializableInterfaceWithDeserializationCallbackType.AddedInterfaces, Is.Empty);
      Assert.That (_serializableInterfaceWithDeserializationCallbackType.AddedMethods, Is.Empty);
    }

    [Test]
    public void MakeSerializableType_SerializableWithDeserializationCallbackType_WithInitializations ()
    {
      StubFilterWithNoSerializedFields();
      var initMethod = MutableMethodInfoObjectMother.Create (
          _serializableInterfaceWithDeserializationCallbackType,
          parameterDeclarations: ParameterDeclaration.EmptyParameters,
          returnType: typeof (void));
      var method = _serializableInterfaceWithDeserializationCallbackType.ExistingMutableMethods.Single (x => x.Name == "OnDeserialization");
      var oldBody = method.Body;

      _enabler.MakeSerializable (_serializableInterfaceWithDeserializationCallbackType, initMethod);

      var expectedBody = Expression.Block (
          typeof (void), oldBody, MethodCallExpression.Call (new ThisExpression (_serializableInterfaceWithDeserializationCallbackType), initMethod));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, method.Body);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "The underlying type implements ISerializable but does not define a deserialization constructor.")]
    public void MakeSerializable_ISerializable_SerializedFields_InaccessibleCtor ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExisting (typeof (SerializableInterfaceMissingCtorType));
      StubFilterWithSerializedFields (mutableType);

      _enabler.MakeSerializable (mutableType, _someInitializationMethod);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "The underlying type implements ISerializable but GetObjectData cannot be overridden. "
        + "Make sure that GetObjectData is implemented implicitly (not explicitly) and virtual.")]
    public void MakeSerializable_ISerializable_SerializedFields_CannotModifyGetObjectData ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExisting (typeof (ExplicitSerializableInterfaceType));
      StubFilterWithSerializedFields (mutableType);

      _enabler.MakeSerializable (mutableType, _someInitializationMethod);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "The underlying type implements ISerializable but GetObjectData cannot be overridden. "
        + "Make sure that GetObjectData is implemented implicitly (not explicitly) and virtual.")]
    public void MakeSerializable_ISerializable_SerializedFields_CannotModifyGetObjectDataInBase ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExisting (typeof (DerivedExplicitSerializableInterfaceType));
      StubFilterWithSerializedFields (mutableType);

      _enabler.MakeSerializable (mutableType, _someInitializationMethod);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "The underlying type implements IDeserializationCallback but OnDeserialization cannot be overridden. "
        + "Make sure that OnDeserialization is implemented implicitly (not explicitly) and virtual.")]
    public void MakeSerializable_IDeserializationCallback_CannotModifyGetObjectData ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExisting (typeof (ExplicitDeserializationCallbackType));
      StubFilterWithNoSerializedFields ();

      _enabler.MakeSerializable (mutableType, _someInitializationMethod);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "The underlying type implements IDeserializationCallback but OnDeserialization cannot be overridden. "
        + "Make sure that OnDeserialization is implemented implicitly (not explicitly) and virtual.")]
    public void MakeSerializable_IDeserializationCallback_CannotModifyGetObjectDataInBase ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExisting (typeof (DerivedExplicitDeserializationCallbackType));
      StubFilterWithNoSerializedFields ();

      _enabler.MakeSerializable (mutableType, _someInitializationMethod);
    }

    [Test]
    public void IsDeserializationConstructor ()
    {
      var ctor1 = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new SerializableInterfaceType (null, new StreamingContext()));
      var ctor2 = ReflectionObjectMother.GetSomeConstructor();

      Assert.That (_enabler.IsDeserializationConstructor (ctor1), Is.True);
      Assert.That (_enabler.IsDeserializationConstructor (ctor2), Is.False);
    }

    private void StubFilterWithNoSerializedFields ()
    {
      _serializedFieldHandlerMock.Stub (stub => stub.GetSerializedFields (Arg<IEnumerable<FieldInfo>>.Is.Anything)).Return (new FieldInfo[0]);
    }

    private void StubFilterWithSerializedFields (MutableType declaringType)
    {
      _serializedFieldHandlerMock
          .Stub (stub => stub.GetSerializedFields (Arg<IEnumerable<FieldInfo>>.Is.Anything))
          .Return (new[] { MutableFieldInfoObjectMother.Create (declaringType) });
    }

    class SomeType { }

    class SerializableInterfaceType : ISerializable
    {
      public SerializableInterfaceType (SerializationInfo info, StreamingContext context) { Dev.Null = info; Dev.Null = context; }
      public virtual void GetObjectData (SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    class SerializableType { }

    class DeserializationCallbackType : IDeserializationCallback
    {
      public virtual void OnDeserialization (object sender) { }
    }

    [Serializable]
    class SerializableWithDeserializationCallbackType : IDeserializationCallback
    {
      public virtual void OnDeserialization (object sender) { }
    }

    class SerializableInterfaceMissingCtorType : ISerializable
    {
      public virtual void GetObjectData (SerializationInfo info, StreamingContext context) { }
    }

    class ExplicitSerializableInterfaceType : ISerializable
    {
      void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context) { }
    }
    class DerivedExplicitSerializableInterfaceType : ExplicitSerializableInterfaceType { }

    class ExplicitDeserializationCallbackType : IDeserializationCallback
    {
      void IDeserializationCallback.OnDeserialization (object sender) { }
    }
    class DerivedExplicitDeserializationCallbackType : ExplicitDeserializationCallbackType { }
  }
}