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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Collections;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class InitializationBuilderTest
  {
    private InitializationBuilder _builder;

    private IProxySerializationEnabler _proxySerializationEnablerMock;
    private ProxyType _proxyType;

    [SetUp]
    public void SetUp ()
    {
      _builder = new InitializationBuilder();

      _proxySerializationEnablerMock = MockRepository.GenerateStrictMock<IProxySerializationEnabler>();
      _proxyType = ProxyTypeObjectMother.Create();
    }

    [Test]
    public void CreateInstanceInitializationMembers ()
    {
      var initExpression = ExpressionTreeObjectMother.GetSomeExpression ();
      _proxyType.AddInitialization (ctx => initExpression);

      var result = _builder.CreateInitializationMembers (_proxyType);

      var counter = (MutableFieldInfo) result.Item1;
      var initMethod = (MutableMethodInfo) result.Item2;

      // Interface added.
      Assert.That (_proxyType.AddedInterfaces, Is.EqualTo (new[] { typeof (IInitializableObject) }));

      // Field added.
      Assert.That (_proxyType.AddedFields, Is.EqualTo (new[] { counter }));
      Assert.That (counter.Name, Is.EqualTo ("<tp>_ctorRunCounter"));
      Assert.That (counter.FieldType, Is.SameAs (typeof (int)));
      Assert.That (counter.AddedCustomAttributes.Single().Type, Is.SameAs (typeof (NonSerializedAttribute)));

      // Initialization method added.
      var methodAttributes = MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.HideBySig;
      Assert.That (_proxyType.AddedMethods, Is.EqualTo (new[] { initMethod }));
      Assert.That (initMethod.DeclaringType, Is.SameAs (_proxyType));
      Assert.That (initMethod.Name, Is.EqualTo ("Remotion.TypePipe.Caching.IInitializableObject.Initialize"));
      Assert.That (initMethod.Attributes, Is.EqualTo (methodAttributes));
      Assert.That (initMethod.ReturnType, Is.SameAs (typeof (void)));
      Assert.That (initMethod.GetParameters (), Is.Empty);
      Assert.That (initMethod.Body.Type, Is.SameAs (typeof (void)));
      Assert.That (initMethod.Body, Is.InstanceOf<BlockExpression> ());
      var blockExpression = (BlockExpression) initMethod.Body;
      Assert.That (blockExpression.Expressions, Is.EqualTo (new[] { initExpression }));
      var interfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IInitializableObject obj) => obj.Initialize ());
      Assert.That (initMethod.AddedExplicitBaseDefinitions, Is.EqualTo (new[] { interfaceMethod }));
    }

    [Test]
    public void CreateInstanceInitializationMembers_Empty ()
    {
      var result = _builder.CreateInitializationMembers (_proxyType);

      Assert.That (result, Is.Null);
      Assert.That (_proxyType.AddedInterfaces, Is.Empty);
      Assert.That (_proxyType.AddedFields, Is.Empty);
      Assert.That (_proxyType.AddedMethods, Is.Empty);
    }

    [Test]
    public void WireConstructorWithInitialization ()
    {
      var counter = MutableFieldInfoObjectMother.Create (_proxyType, type: typeof (int));
      var initMethod = MutableMethodInfoObjectMother.Create (_proxyType);
      var initializationMembers = Tuple.Create<FieldInfo, MethodInfo> (counter, initMethod);
      var constructor = MutableConstructorInfoObjectMother.Create();
      var oldBody = constructor.Body;

      _proxySerializationEnablerMock.Expect (mock => mock.IsDeserializationConstructor (constructor)).Return (false);

      _builder.WireConstructorWithInitialization (constructor, initializationMembers, _proxySerializationEnablerMock);

      _proxySerializationEnablerMock.VerifyAllExpectations();
      var expectedBody = Expression.Block (
          Expression.Assign (
              Expression.Field (new ThisExpression (_proxyType), counter),
              Expression.Add (Expression.Field (new ThisExpression (_proxyType), counter), Expression.Constant (1))),
          oldBody,
          Expression.Assign (
              Expression.Field (new ThisExpression (_proxyType), counter),
              Expression.Subtract (Expression.Field (new ThisExpression (_proxyType), counter), Expression.Constant (1))),
          Expression.IfThen (
              Expression.Equal (Expression.Field (new ThisExpression (_proxyType), counter), Expression.Constant (0)),
              Expression.Call (new ThisExpression (_proxyType), initMethod)));

      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, constructor.Body);
    }

    [Test]
    public void WireConstructorWithInitialization_Null ()
    {
      var constructor = MutableConstructorInfoObjectMother.Create();
      var oldBody = constructor.Body;

      _builder.WireConstructorWithInitialization (constructor, initializationMembers: null, proxySerializationEnabler:_proxySerializationEnablerMock);

      _proxySerializationEnablerMock.AssertWasNotCalled (mock => mock.IsDeserializationConstructor (constructor));
      Assert.That (constructor.Body, Is.SameAs (oldBody));
    }

    [Test]
    public void WireConstructorWithInitialization_DeserializationConstructor ()
    {
      var constructor = MutableConstructorInfoObjectMother.Create();
      var oldBody = constructor.Body;
      var initializationMembers = new Tuple<FieldInfo, MethodInfo> (null, null);
      _proxySerializationEnablerMock.Expect (x => x.IsDeserializationConstructor (constructor)).Return (true);
      
      _builder.WireConstructorWithInitialization (constructor, initializationMembers, _proxySerializationEnablerMock);

      _proxySerializationEnablerMock.VerifyAllExpectations();
      Assert.That (constructor.Body, Is.SameAs (oldBody));
    }
  }
}