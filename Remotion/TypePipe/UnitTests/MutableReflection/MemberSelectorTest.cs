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
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;
using Rhino.Mocks;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MemberSelectorTest
  {
    private IBindingFlagsEvaluator _bindingFlagsEvaluatorMock;
    private MemberSelector _selector;

    [SetUp]
    public void SetUp ()
    {
      _bindingFlagsEvaluatorMock = MockRepository.GenerateStrictMock<IBindingFlagsEvaluator>();
      _selector = new MemberSelector (_bindingFlagsEvaluatorMock);
    }

    [Test]
    public void SelectFields ()
    {
      var candidates = new[] { GetField(dt => dt.Field1), GetField (dt => dt.Field2), GetField (dt => dt.Field3) };
      var bindingFlags = BindingFlags.NonPublic;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[0].Attributes, bindingFlags)).Return (true);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[1].Attributes, bindingFlags)).Return (false);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[2].Attributes, bindingFlags)).Return (true);

      var result = _selector.SelectFields (candidates, bindingFlags).ToArray();

      _bindingFlagsEvaluatorMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (new[] { candidates[0], candidates[2] }));
    }

    [Test]
    public void SelectMethods ()
    {
      var candidates = new[] { GetMethod (dt => dt.Method1()), GetMethod (dt => dt.Method2()), GetMethod (dt => dt.Method3()) };
      var bindingFlags = BindingFlags.NonPublic;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[0].Attributes, bindingFlags)).Return (true);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[1].Attributes, bindingFlags)).Return (false);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[2].Attributes, bindingFlags)).Return (true);

      var result = _selector.SelectMethods (candidates, bindingFlags).ToArray ();

      _bindingFlagsEvaluatorMock.VerifyAllExpectations ();
      Assert.That (result, Is.EqualTo (new[] { candidates[0], candidates[2] }));
    }

    [Test]
    public void SelectSingleMethod ()
    {
      var binderMock = MockRepository.GenerateStrictMock<Binder>();
      var bindingFlags = BindingFlags.NonPublic;
      var candidates = new[] { GetMethod (dt => dt.Method1 ()), GetMethod (dt => dt.Method2 ()), GetMethod (dt => dt.Method3 ()) };
      var typesOrNull = new[] { typeof (int), typeof (string) };
      var modifiersOrNull = new[] { new ParameterModifier (2) };

      var fakeResult = ReflectionObjectMother.GetSomeMethod ();
      binderMock
          .Expect (mock => mock.SelectMethod (bindingFlags, candidates, typesOrNull, modifiersOrNull))
          .Return (fakeResult);

      var result = _selector.SelectSingleMethod (binderMock, bindingFlags, candidates, typesOrNull, modifiersOrNull);

      binderMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void SelectSingleMethod_NoCandidates ()
    {
      var binderMock = MockRepository.GenerateStrictMock<Binder> ();
      var bindingFlags = BindingFlags.NonPublic;
      var candidates = new MethodBase[0];
      var typesOrNull = new[] { typeof (int), typeof (string) };
      var modifiersOrNull = new[] { new ParameterModifier (2) };

      var result = _selector.SelectSingleMethod (binderMock, bindingFlags, candidates, typesOrNull, modifiersOrNull);

      binderMock.AssertWasNotCalled (
          mock => mock.SelectMethod (
              Arg<BindingFlags>.Is.Anything, Arg<MethodBase[]>.Is.Anything, Arg<Type[]>.Is.Anything, Arg<ParameterModifier[]>.Is.Anything));

      Assert.That (result, Is.Null);
    }

    [Test]
    public void SelectSingleMethod_OneCandidate ()
    {
      var binderMock = MockRepository.GenerateStrictMock<Binder>();
      var bindingFlags = BindingFlags.NonPublic;
      var candidates = new[] { GetMethod (dt => dt.Method1 ()) };
      var typesOrNull = new[] { typeof (int), typeof (string) };
      var modifiersOrNull = new[] { new ParameterModifier (2) };
      
      var fakeResult = ReflectionObjectMother.GetSomeMethod();
      binderMock
          .Expect (mock => mock.SelectMethod (bindingFlags, candidates, typesOrNull, modifiersOrNull))
          .Return (fakeResult);

      var result = _selector.SelectSingleMethod (binderMock, bindingFlags, candidates, typesOrNull, modifiersOrNull);

      binderMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    [ExpectedException (typeof (AmbiguousMatchException))]
    public void SelectSingleMethod_TypesNull_MultipleCandidates ()
    {
      var binderMock = MockRepository.GenerateStrictMock<Binder> ();
      var bindingFlags = BindingFlags.NonPublic;
      var candidates = new[] { GetMethod (dt => dt.Method1 ()), GetMethod (dt => dt.Method2 ()), GetMethod (dt => dt.Method3 ()) };
      Type[] typesOrNull = null;
      ParameterModifier[] modifiersOrNull = null;

      _selector.SelectSingleMethod (binderMock, bindingFlags, candidates, typesOrNull, modifiersOrNull);
    }

    [Test]
    public void SelectSingleMethod_TypesNull_SingleCandidate ()
    {
      var binderMock = MockRepository.GenerateStrictMock<Binder> ();
      var bindingFlags = BindingFlags.NonPublic;
      var candidates = new[] { GetMethod (dt => dt.Method1 ()) };
      Type[] typesOrNull = null;
      ParameterModifier[] modifiersOrNull = null;

      var result = _selector.SelectSingleMethod (binderMock, bindingFlags, candidates, typesOrNull, modifiersOrNull);

      binderMock.AssertWasNotCalled (
          mock => mock.SelectMethod (
              Arg<BindingFlags>.Is.Anything, Arg<MethodBase[]>.Is.Anything, Arg<Type[]>.Is.Anything, Arg<ParameterModifier[]>.Is.Anything));
      Assert.That (result, Is.SameAs (candidates[0]));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Modifiers must not be specified if types are null.\r\nParameter name: modifiersOrNull")]
    public void SelectSingleMethod_TypesNull_ModifiersNotNull ()
    {
      var binderMock = MockRepository.GenerateStrictMock<Binder> ();
      var bindingFlags = BindingFlags.NonPublic;
      var candidates = new[] { GetMethod (dt => dt.Method1 ()) };
      Type[] typesOrNull = null;
      var modifiersOrNull = new[] { new ParameterModifier(2) };

      _selector.SelectSingleMethod (binderMock, bindingFlags, candidates, typesOrNull, modifiersOrNull);
    }

    private FieldInfo GetField<TR> (Expression<Func<DomainType, TR>> memberAccessExpression)
    {
      return MemberInfoFromExpressionUtility.GetField (memberAccessExpression);
    }

    private MethodBase GetMethod (Expression<Action<DomainType>> memberAccessExpression)
    {
      return MemberInfoFromExpressionUtility.GetMethod (memberAccessExpression);
    }

// ReSharper disable ClassNeverInstantiated.Global
    public class DomainType
// ReSharper restore ClassNeverInstantiated.Global
    {
      public readonly int Field1 = Dev<int>.Null;
      public readonly int Field2 = Dev<int>.Null;
      public readonly int Field3 = Dev<int>.Null;

      public void Method1 () { }
      public void Method2 () { }
      public void Method3 () { }
    }
  }
}