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
using Remotion.Collections;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.Serialization.Implementation;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class ProxySerializationEnablerTest
  {
    private ISerializableFieldFinder _serializableFieldFinderMock;
    
    private ProxySerializationEnabler _enabler;

    private ProxyType _someType;
    private ProxyType _serializableInterfaceType;
    private ProxyType _serializableType;
    private ProxyType _deserializationCallbackType;
    private ProxyType _serializableInterfaceWithDeserializationCallbackType;

    private MethodInfo _someInitializationMethod;

    [SetUp]
    public void SetUp ()
    {
      _serializableFieldFinderMock = MockRepository.GenerateStrictMock<ISerializableFieldFinder>();

      _enabler = new ProxySerializationEnabler (_serializableFieldFinderMock);

      _someType = ProxyTypeObjectMother.Create (
          typeof (SomeType),
          memberSelector: null,
          relatedMethodFinder: null,
          interfaceMappingComputer: null,
          mutableMemberFactory: null);
      _serializableType = ProxyTypeObjectMother.Create (
          typeof (SerializableType),
          memberSelector: null,
          relatedMethodFinder: null,
          interfaceMappingComputer: null,
          mutableMemberFactory: null);
      _serializableInterfaceType = ProxyTypeObjectMother.Create (
          typeof (SerializableInterfaceType),
          memberSelector: null,
          relatedMethodFinder: null,
          interfaceMappingComputer: null,
          mutableMemberFactory: null);
      _deserializationCallbackType = ProxyTypeObjectMother.Create (
          typeof (DeserializationCallbackType),
          memberSelector: null,
          relatedMethodFinder: null,
          interfaceMappingComputer: null,
          mutableMemberFactory: null);
      _serializableInterfaceWithDeserializationCallbackType = ProxyTypeObjectMother.Create (
          typeof (SerializableWithDeserializationCallbackType),
          memberSelector: null,
          relatedMethodFinder: null,
          interfaceMappingComputer: null,
          mutableMemberFactory: null);

      _someInitializationMethod = ReflectionObjectMother.GetSomeInstanceMethod();
    }

    [Test]
    public void MakeSerializable_SerializableInterfaceType ()
    {
      StubFilterWithNoSerializedFields();

      _enabler.MakeSerializable (_serializableInterfaceType, _someInitializationMethod);

      Assert.That (_serializableInterfaceType.AddedInterfaces, Is.Empty);
    }

    [Test]
    public void MakeSerializable_SerializableInterfaceType_SerializedFields ()
    {
      var getObjectDataMethod = _serializableInterfaceType.GetMethod ("GetObjectData");
      var deserializationCtor = _serializableInterfaceType.AddedConstructors.Single();
      var dummyField = _serializableInterfaceType.AddField ("input field", typeof (int));

      var fakeFieldType = ReflectionObjectMother.GetSomeType();
      FieldInfo fakeField = MutableFieldInfoObjectMother.Create (_serializableInterfaceType, type: fakeFieldType);
      var fakeMapping = new[] { Tuple.Create ("fake key", fakeField) };
      _serializableFieldFinderMock
          .Expect (mock => mock.GetSerializableFieldMapping (Arg<IEnumerable<FieldInfo>>.List.Equal (new[] { dummyField })))
          .Return (fakeMapping);

      _enabler.MakeSerializable (_serializableInterfaceType, _someInitializationMethod);

      _serializableFieldFinderMock.VerifyAllExpectations();
      Assert.That (_serializableInterfaceType.AddedInterfaces, Is.Empty);
      Assert.That (_serializableInterfaceType.AddedMethods, Has.Count.EqualTo (1));

      var method = _serializableInterfaceType.AddedMethods.Single();
      var expectedMethodBody = Expression.Block (
          typeof (void),
          Expression.Call (new NonVirtualCallMethodInfoAdapter (getObjectDataMethod), method.ParameterExpressions.Cast<Expression>()),
          Expression.Call (
              method.ParameterExpressions[0],
              "AddValue",
              Type.EmptyTypes,
              Expression.Constant ("fake key"),
              Expression.Field (new ThisExpression (_serializableInterfaceType), fakeField)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedMethodBody, method.Body);

      var getValue = NormalizingMemberInfoFromExpressionUtility.GetMethod ((SerializationInfo obj) => obj.GetValue ("", null));
      var expectedCtorBody = Expression.Block (
          typeof (void),
          Expression.Call (new NonVirtualCallMethodInfoAdapter (new ConstructorAsMethodInfoAdapter (deserializationCtor))),
          Expression.Assign (
              Expression.Field (new ThisExpression (_serializableInterfaceType), fakeField),
              Expression.Convert (
                  Expression.Call (
                      deserializationCtor.ParameterExpressions[0], getValue, Expression.Constant ("fake key"), Expression.Constant (fakeFieldType)),
                  fakeFieldType)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedCtorBody, deserializationCtor.Body);
    }

    [Test]
    public void MakeSerializable_SomeType_SerializedFields ()
    {
      StubFilterWithSerializedFields (_someType);

      _enabler.MakeSerializable (_someType, _someInitializationMethod);

      Assert.That (_someType.AddedInterfaces, Is.Empty);
      Assert.That (_someType.AddedMethods, Is.Empty);
    }

    [Test]
    public void MakeSerializable_SomeType ()
    {
      StubFilterWithNoSerializedFields();

      _enabler.MakeSerializable (_someType, initializationMethod: null);

      Assert.That (_someType.AddedInterfaces, Is.Empty);
      Assert.That (_someType.AddedMethods, Is.Empty);
    }

    [Test]
    public void MakeSerializableType_SerializableType_WithInitializations ()
    {
      StubFilterWithNoSerializedFields();
      var initMethod = MutableMethodInfoObjectMother.Create (
          _serializableType,
          parameters: ParameterDeclaration.EmptyParameters,
          returnParameter: ParameterDeclaration.CreateReturnParameter (typeof (void)));

      _enabler.MakeSerializable (_serializableType, initMethod);

      Assert.That (_serializableType.AddedInterfaces, Is.EqualTo (new[] { typeof (IDeserializationCallback) }));
      Assert.That (_serializableType.AddedMethods, Has.Count.EqualTo (1));

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
          _deserializationCallbackType,
          parameters: ParameterDeclaration.EmptyParameters,
          returnParameter: ParameterDeclaration.CreateReturnParameter (typeof (void)));
      var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DeserializationCallbackType obj) => obj.OnDeserialization (null));

      _enabler.MakeSerializable (_deserializationCallbackType, initMethod);

      Assert.That (_deserializationCallbackType.AddedMethods, Has.Count.EqualTo (1));
      var method = _deserializationCallbackType.AddedMethods.Single();
      var expectedBody = Expression.Block (
          typeof (void),
          Expression.Call (new NonVirtualCallMethodInfoAdapter (baseMethod), method.ParameterExpressions.Cast<Expression>()),
          MethodCallExpression.Call (new ThisExpression (_deserializationCallbackType), initMethod));
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
          parameters: ParameterDeclaration.EmptyParameters,
          returnParameter: ParameterDeclaration.CreateReturnParameter (typeof (void)));
      var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((SerializableWithDeserializationCallbackType obj) => obj.OnDeserialization (null));

      _enabler.MakeSerializable (_serializableInterfaceWithDeserializationCallbackType, initMethod);

      Assert.That (_serializableInterfaceWithDeserializationCallbackType.AddedMethods, Has.Count.EqualTo (1));
      var method = _serializableInterfaceWithDeserializationCallbackType.AddedMethods.Single();
      var expectedBody = Expression.Block (
          typeof (void),
          Expression.Call (new NonVirtualCallMethodInfoAdapter (baseMethod), method.ParameterExpressions.Cast<Expression>()),
          MethodCallExpression.Call (new ThisExpression (_serializableInterfaceWithDeserializationCallbackType), initMethod));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, method.Body);
    }

    [Test]
    public void MakeSerializable_ISerializable_SerializedFields_MissingCtor ()
    {
      var proxyType = ProxyTypeObjectMother.Create (
          typeof (SerializableInterfaceMissingCtorType),
          memberSelector: null,
          relatedMethodFinder: null,
          interfaceMappingComputer: null,
          mutableMemberFactory: null);
      StubFilterWithSerializedFields (proxyType);
      var oldCtorBody = proxyType.AddedConstructors.Single().Body;

      _enabler.MakeSerializable (proxyType, _someInitializationMethod);

      Assert.That (proxyType.AddedConstructors.Single().Body, Is.SameAs (oldCtorBody));
      Assert.That (proxyType.AddedMethods, Is.Empty);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "The underlying type implements ISerializable but GetObjectData cannot be overridden. "
        + "Make sure that GetObjectData is implemented implicitly (not explicitly) and virtual.")]
    public void MakeSerializable_ISerializable_SerializedFields_CannotModifyGetObjectData ()
    {
      var proxyType = ProxyTypeObjectMother.Create (
          typeof (ExplicitSerializableInterfaceType),
          memberSelector: null,
          relatedMethodFinder: null,
          interfaceMappingComputer: null,
          mutableMemberFactory: null);
      StubFilterWithSerializedFields (proxyType);

      _enabler.MakeSerializable (proxyType, _someInitializationMethod);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "The underlying type implements ISerializable but GetObjectData cannot be overridden. "
        + "Make sure that GetObjectData is implemented implicitly (not explicitly) and virtual.")]
    public void MakeSerializable_ISerializable_SerializedFields_CannotModifyGetObjectDataInBase ()
    {
      var proxyType = ProxyTypeObjectMother.Create (
          typeof (DerivedExplicitSerializableInterfaceType),
          memberSelector: null,
          relatedMethodFinder: null,
          interfaceMappingComputer: null,
          mutableMemberFactory: null);
      StubFilterWithSerializedFields (proxyType);

      _enabler.MakeSerializable (proxyType, _someInitializationMethod);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "The underlying type implements IDeserializationCallback but OnDeserialization cannot be overridden. "
        + "Make sure that OnDeserialization is implemented implicitly (not explicitly) and virtual.")]
    public void MakeSerializable_IDeserializationCallback_CannotModifyGetObjectData ()
    {
      var proxyType = ProxyTypeObjectMother.Create (
          typeof (ExplicitDeserializationCallbackType),
          memberSelector: null,
          relatedMethodFinder: null,
          interfaceMappingComputer: null,
          mutableMemberFactory: null);
      StubFilterWithNoSerializedFields ();

      _enabler.MakeSerializable (proxyType, _someInitializationMethod);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "The underlying type implements IDeserializationCallback but OnDeserialization cannot be overridden. "
        + "Make sure that OnDeserialization is implemented implicitly (not explicitly) and virtual.")]
    public void MakeSerializable_IDeserializationCallback_CannotModifyGetObjectDataInBase ()
    {
      var proxyType = ProxyTypeObjectMother.Create (
          typeof (DerivedExplicitDeserializationCallbackType),
          memberSelector: null,
          relatedMethodFinder: null,
          interfaceMappingComputer: null,
          mutableMemberFactory: null);
      StubFilterWithNoSerializedFields ();

      _enabler.MakeSerializable (proxyType, _someInitializationMethod);
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
      _serializableFieldFinderMock
          .Stub (stub => stub.GetSerializableFieldMapping (Arg<IEnumerable<FieldInfo>>.Is.Anything))
          .Return (new Tuple<string, FieldInfo>[0]);
    }

    private void StubFilterWithSerializedFields (ProxyType declaringType)
    {
      _serializableFieldFinderMock
          .Stub (stub => stub.GetSerializableFieldMapping (Arg<IEnumerable<FieldInfo>>.Is.Anything))
          .Return (new[] { Tuple.Create<string, FieldInfo> ("someField", MutableFieldInfoObjectMother.Create (declaringType)) });
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
      public ExplicitSerializableInterfaceType (SerializationInfo info, StreamingContext context) { Dev.Null = info; Dev.Null = context; }
      void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context) { }
    }
    class DerivedExplicitSerializableInterfaceType : ExplicitSerializableInterfaceType
    {
      public DerivedExplicitSerializableInterfaceType (SerializationInfo info, StreamingContext context) : base (info, context) { }
    }

    class ExplicitDeserializationCallbackType : IDeserializationCallback
    {
      void IDeserializationCallback.OnDeserialization (object sender) { }
    }
    class DerivedExplicitDeserializationCallbackType : ExplicitDeserializationCallbackType { }
  }
}