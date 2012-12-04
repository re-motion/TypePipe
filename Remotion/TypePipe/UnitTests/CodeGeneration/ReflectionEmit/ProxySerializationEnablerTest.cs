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
using Remotion.Development.UnitTesting;
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

    private MutableType _someType;
    private MutableType _serializableInterfaceType;
    private MutableType _serializableType;
    private MutableType _deserializationCallbackType;
    private MutableType _serializableInterfaceWithDeserializationCallbackType;
    private MutableType _serializableInterfaceMissingCtorType;
    private MutableType _serializableInterfaceCannotModifyGetObjectDataType;

    private MethodInfo _someInitializationMethod;

    [SetUp]
    public void SetUp ()
    {
      _enabler = new ProxySerializationEnabler();

      _someType = MutableTypeObjectMother.CreateForExisting (typeof (SomeType));
      _serializableType = MutableTypeObjectMother.CreateForExisting (typeof (SerializableType));
      _serializableInterfaceType = MutableTypeObjectMother.CreateForExisting (typeof (SerializableInterfaceType));
      _deserializationCallbackType = MutableTypeObjectMother.CreateForExisting (typeof (DeserializationCallbackType));
      _serializableInterfaceWithDeserializationCallbackType =
          MutableTypeObjectMother.CreateForExisting (typeof (SerializableWithDeserializationCallbackType));
      _serializableInterfaceMissingCtorType = MutableTypeObjectMother.CreateForExisting (typeof (SerializableInterfaceMissingCtorType));
      _serializableInterfaceCannotModifyGetObjectDataType =
          MutableTypeObjectMother.CreateForExisting (typeof (SerializableInterfaceCannotModifyGetObjectDataType));

      _someInitializationMethod = ReflectionObjectMother.GetSomeInstanceMethod();
    }

    [Test]
    public void MakeSerializable_SerializableInterfaceType_AddedFields ()
    {
      var nonSerializedAttributeConstructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new NonSerializedAttribute ());
      var deserializationCtor = _serializableInterfaceType.AllMutableConstructors.Single();
      Assert.That (deserializationCtor.IsModified, Is.False);
      var oldCtorBody = deserializationCtor.Body;
      var field1 = _serializableInterfaceType.AddField ("field", typeof (int));
      var field2 = _serializableInterfaceType.AddField ("xxx", typeof (int));
      var field3 = _serializableInterfaceType.AddField ("xxx", typeof (string));
      _serializableInterfaceType.AddField ("staticField", typeof (double), FieldAttributes.Static);
      _serializableInterfaceType.AddField ("nonSerializedField", typeof (double))
                                .AddCustomAttribute (new CustomAttributeDeclaration (nonSerializedAttributeConstructor, new object[0]));

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
    public void MakeSerializable_SerializableInterfaceType ()
    {
      _enabler.MakeSerializable (_serializableInterfaceType, _someInitializationMethod);

      Assert.That (_serializableInterfaceType.AddedInterfaces, Is.Empty);
      Assert.That (_serializableInterfaceType.AllMutableMethods.Count(), Is.EqualTo (1));
      var method = _serializableInterfaceType.ExistingMutableMethods.Single();
      Assert.That (method.IsModified, Is.False);
    }

    [Test]
    public void MakeSerializable_SomeType_AddedFields ()
    {
      _someType.AddField ("abc", typeof (int));

      _enabler.MakeSerializable (_someType, _someInitializationMethod);

      Assert.That (_someType.AddedInterfaces, Is.Empty);
      Assert.That (_someType.AllMutableMethods, Is.Empty);
    }

    [Test]
    public void MakeSerializable_SomeType ()
    {
      _enabler.MakeSerializable (_someType, initializationMethod: null);

      Assert.That (_someType.AddedInterfaces, Is.Empty);
      Assert.That (_someType.AllMutableMethods, Is.Empty);
    }

    [Test]
    public void MakeSerializableType_SerializableType_WithInitializations ()
    {
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
      _enabler.MakeSerializable (_serializableInterfaceWithDeserializationCallbackType, initializationMethod: null);

      Assert.That (_serializableInterfaceWithDeserializationCallbackType.AddedInterfaces, Is.Empty);
      Assert.That (_serializableInterfaceWithDeserializationCallbackType.AddedMethods, Is.Empty);
    }

    [Test]
    public void MakeSerializableType_SerializableWithDeserializationCallbackType_WithInitializations ()
    {
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
        "The underlying type implements 'ISerializable' but does not define a deserialization constructor.")]
    public void MakeSerializable_SerializableInterfaceType_AddedFields_UnaccessibleCtor ()
    {
      _serializableInterfaceMissingCtorType.AddField ("field", typeof (int));
      _enabler.MakeSerializable (_serializableInterfaceMissingCtorType, _someInitializationMethod);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "The underlying type implements ISerializable but GetObjectData cannot be overridden. "
        + "Make sure that GetObjectData is implemented implicitly (not explicitly) and virtual.")]
    public void MakeSerializable_SerializableInterfaceType_AddedFields_CannotModifyGetObjectData ()
    {
      _serializableInterfaceCannotModifyGetObjectDataType.AddField ("field", typeof (int));
      _enabler.MakeSerializable (_serializableInterfaceCannotModifyGetObjectDataType, _someInitializationMethod);
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

    class SerializableInterfaceCannotModifyGetObjectDataType : ISerializable
    {
      void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context) { }
    }
  }
}