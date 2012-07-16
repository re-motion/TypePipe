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
using Remotion.Development.UnitTesting.Reflection;
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
    private MutableType _declaredType;
    
    private MemberSelector _selector;

    [SetUp]
    public void SetUp ()
    {
      _declaredType = MutableTypeObjectMother.CreateForExistingType (typeof (DomainType));
      _bindingFlagsEvaluatorMock = MockRepository.GenerateStrictMock<IBindingFlagsEvaluator> ();

      _selector = new MemberSelector (_bindingFlagsEvaluatorMock);
    }

    [Test]
    public void SelectMethods ()
    {
      var candidates = new[] { GetMethod (dt => dt.Method1 ()), GetMethod (dt => dt.Method2 ()), GetMethod (dt => dt.Method3 ()) };
      var bindingFlags = BindingFlags.NonPublic;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[0].Attributes, bindingFlags)).Return (true);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[1].Attributes, bindingFlags)).Return (false);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[2].Attributes, bindingFlags)).Return (true);

      var result = _selector.SelectMethods (candidates, bindingFlags, _declaredType).ToArray ();

      _bindingFlagsEvaluatorMock.VerifyAllExpectations ();
      Assert.That (result, Is.EqualTo (new[] { candidates[0], candidates[2] }));
    }

    [Test]
    public void SelectMethods_DeclaredOnly ()
    {
      var candidates = new[] { GetBaseMethod (dtb => dtb.MethodInvolvedInShadowing()), GetMethod (dt => dt.MethodInvolvedInShadowing()) };
      var bindingFlags = BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[0].Attributes, bindingFlags)).Return (true);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[1].Attributes, bindingFlags)).Return (true);

      var result = _selector.SelectMethods (candidates, bindingFlags, _declaredType).ToArray();

      _bindingFlagsEvaluatorMock.VerifyAllExpectations ();
      Assert.That (result, Is.EqualTo (new[] { candidates[1] }));
    }

    [Test]
    public void SelectFields ()
    {
      var candidates = new[] { GetField (dt => dt.Field1), GetField (dt => dt.Field2), GetField (dt => dt.Field3) };
      var bindingFlags = BindingFlags.NonPublic;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[0].Attributes, bindingFlags)).Return (true);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[1].Attributes, bindingFlags)).Return (false);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[2].Attributes, bindingFlags)).Return (true);

      var result = _selector.SelectFields (candidates, bindingFlags).ToArray ();

      _bindingFlagsEvaluatorMock.VerifyAllExpectations ();
      Assert.That (result, Is.EqualTo (new[] { candidates[0], candidates[2] }));
    }

    [Test]
    public void SelectSingleMethod ()
    {
      var binderMock = MockRepository.GenerateStrictMock<Binder> ();
      var bindingFlags = BindingFlags.NonPublic;
      var methods =
          new[]
          {
              CreateMethodStub ("Method1", MethodAttributes.Assembly),
              CreateMethodStub ("This method is filtered because of its name", MethodAttributes.Family),
              CreateMethodStub ("Method1", MethodAttributes.Public)
          };
      var typesOrNull = new[] { typeof (int), typeof (string) };
      var modifiersOrNull = new[] { new ParameterModifier (2) };

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (methods[0].Attributes, bindingFlags)).Return (false);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (methods[2].Attributes, bindingFlags)).Return (true);

      var fakeResult = ReflectionObjectMother.GetSomeMethod ();
      binderMock
          .Expect (mock => mock.SelectMethod (bindingFlags, new[] { methods[2] }, typesOrNull, modifiersOrNull))
          .Return (fakeResult);

      var result = _selector.SelectSingleMethod (methods, binderMock, bindingFlags, "Method1", _declaredType, typesOrNull, modifiersOrNull);

      _bindingFlagsEvaluatorMock.VerifyAllExpectations();
      binderMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void SelectSingleMethod_ZeroCandidates ()
    {
      var binderMock = MockRepository.GenerateStrictMock<Binder> ();
      var bindingFlags = BindingFlags.NonPublic;
      var methods =
          new[]
          {
              CreateMethodStub ("Method1", MethodAttributes.Assembly),
              CreateMethodStub ("This method is filtered because of its name", MethodAttributes.Family),
              CreateMethodStub ("Method1", MethodAttributes.Public)
          };
      var typesOrNull = new[] { typeof (int), typeof (string) };
      var modifiersOrNull = new[] { new ParameterModifier (2) };

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (methods[0].Attributes, bindingFlags)).Return (false);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (methods[2].Attributes, bindingFlags)).Return (false);

      var result = _selector.SelectSingleMethod (methods, binderMock, bindingFlags, "Method1", _declaredType, typesOrNull, modifiersOrNull);

      _bindingFlagsEvaluatorMock.VerifyAllExpectations ();
      Assert.That (result, Is.Null);
    }

    [Test]
    public void SelectSingleMethod_TypesNull_ZeroCandidates ()
    {
      var binderMock = MockRepository.GenerateStrictMock<Binder> ();
      var bindingFlags = BindingFlags.NonPublic;
      var methods =
          new[]
          {
              CreateMethodStub ("Method1", MethodAttributes.Assembly),
              CreateMethodStub ("This method is filtered because of its name", MethodAttributes.Family),
              CreateMethodStub ("Method1", MethodAttributes.Public)
          };
      Type[] typesOrNull = null;
      ParameterModifier[] modifiersOrNull = null;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (methods[0].Attributes, bindingFlags)).Return (false);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (methods[2].Attributes, bindingFlags)).Return (false);

      var result = _selector.SelectSingleMethod (methods, binderMock, bindingFlags, "Method1", _declaredType, typesOrNull, modifiersOrNull);

      Assert.That (result, Is.Null);
    }

    [Test]
    [ExpectedException (typeof (AmbiguousMatchException), ExpectedMessage = "Ambiguous method name 'Method1'.")]
    public void SelectSingleMethod_TypesNull_Ambiguous ()
    {
      var binderMock = MockRepository.GenerateStrictMock<Binder> ();
      var bindingFlags = BindingFlags.NonPublic;
      var methods =
          new[]
          {
              CreateMethodStub ("Method1", MethodAttributes.Assembly),
              CreateMethodStub ("Method1", MethodAttributes.Public)
          };
      Type[] typesOrNull = null;
      ParameterModifier[] modifiersOrNull = null;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (methods[0].Attributes, bindingFlags)).Return (true);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (methods[1].Attributes, bindingFlags)).Return (true);

      _selector.SelectSingleMethod (methods, binderMock, bindingFlags, "Method1", _declaredType, typesOrNull, modifiersOrNull);
    }

    [Test]
    public void SelectSingleMethod_TypesNull_SingleCandidate ()
    {
      var binderMock = MockRepository.GenerateStrictMock<Binder> ();
      var bindingFlags = BindingFlags.NonPublic;
      var methods =
          new[]
          {
              CreateMethodStub ("Method1", MethodAttributes.Assembly),
              CreateMethodStub ("This method is filtered because of its name", MethodAttributes.Family),
              CreateMethodStub ("Method1", MethodAttributes.Public)
          };
      Type[] typesOrNull = null;
      ParameterModifier[] modifiersOrNull = null;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (methods[0].Attributes, bindingFlags)).Return (false);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (methods[2].Attributes, bindingFlags)).Return (true);

      var result = _selector.SelectSingleMethod (methods, binderMock, bindingFlags, "Method1", _declaredType, typesOrNull, modifiersOrNull);

      binderMock.AssertWasNotCalled (
          mock => mock.SelectMethod (
              Arg<BindingFlags>.Is.Anything, Arg<MethodBase[]>.Is.Anything, Arg<Type[]>.Is.Anything, Arg<ParameterModifier[]>.Is.Anything));
      Assert.That (result, Is.SameAs (methods[2]));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Modifiers must not be specified if types are null.\r\nParameter name: modifiersOrNull")]
    public void SelectSingleMethod_TypesNull_ModifiersNotNull ()
    {
      var binderMock = MockRepository.GenerateStrictMock<Binder> ();
      var bindingFlags = BindingFlags.NonPublic;
      var methods = new[] { CreateMethodStub() };
      Type[] typesOrNull = null;
      var modifiersOrNull = new[] { new ParameterModifier (2) };

      _selector.SelectSingleMethod (methods, binderMock, bindingFlags, "Whatever", _declaredType, typesOrNull, modifiersOrNull);
    }

    [Test]
    public void SelectSingleField ()
    {
      var fields =
          new[]
          {
              CreateFieldStub ("field1", FieldAttributes.Assembly), 
              CreateFieldStub ("this field is removed because of its name", FieldAttributes.Family),
              CreateFieldStub ("field1", FieldAttributes.Public)
          };
      
      var bindingFlags = BindingFlags.NonPublic;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (fields[0].Attributes, bindingFlags)).Return (false);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (fields[2].Attributes, bindingFlags)).Return (true);

      var result = _selector.SelectSingleField (fields, bindingFlags, "field1");

      _bindingFlagsEvaluatorMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (fields[2]));
    }

    [Test]
    public void SelectSingleField_EmptyCandidates ()
    {
      var fields = new[] { CreateFieldStub ("field1", FieldAttributes.Assembly), CreateFieldStub ("field1", FieldAttributes.Public) };
      var bindingFlags = BindingFlags.NonPublic;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (fields[0].Attributes, bindingFlags)).Return (false);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (fields[1].Attributes, bindingFlags)).Return (false);

      var result = _selector.SelectSingleField (fields, bindingFlags, "field1");

      Assert.That (result, Is.Null);
    }

    [Test]
    [ExpectedException (typeof (AmbiguousMatchException), ExpectedMessage = "Ambiguous field name 'field1'.")]
    public void SelectSingleField_Ambiguous ()
    {
      var fields = new[] { CreateFieldStub ("field1", FieldAttributes.Assembly), CreateFieldStub ("field1", FieldAttributes.Public) };
      var bindingFlags = BindingFlags.NonPublic;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (fields[0].Attributes, bindingFlags)).Return (true);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (fields[1].Attributes, bindingFlags)).Return (true);

      _selector.SelectSingleField (fields, bindingFlags, "field1");
    }

    private FieldInfo CreateFieldStub (string name, FieldAttributes fieldAttributes)
    {
      var fieldStub = MockRepository.GenerateStub<FieldInfo>();
      fieldStub.Stub (stub => stub.Name).Return (name);
      fieldStub.Stub (stub => stub.Attributes).Return (fieldAttributes);
      return fieldStub;
    }

    private MethodInfo CreateMethodStub (string name = "Unspecified", MethodAttributes methodAttributes = MethodAttributes.PrivateScope)
    {
      var methodStub = MockRepository.GenerateStub<MethodInfo> ();
      methodStub.Stub (stub => stub.Name).Return (name);
      methodStub.Stub (stub => stub.Attributes).Return (methodAttributes);
      return methodStub;
    }

    private FieldInfo GetField<TR> (Expression<Func<DomainType, TR>> memberAccessExpression)
    {
      return NormalizingMemberInfoFromExpressionUtility.GetField (memberAccessExpression);
    }

    private MethodBase GetMethod (Expression<Action<DomainType>> memberAccessExpression)
    {
      return NormalizingMemberInfoFromExpressionUtility.GetMethod (memberAccessExpression);
    }

    private MethodBase GetBaseMethod (Expression<Action<DomainTypeBase>> memberAccessExpression)
    {
      return NormalizingMemberInfoFromExpressionUtility.GetMethod (memberAccessExpression);
    }

    private class DomainTypeBase
    {
      public void MethodInvolvedInShadowing () { }
    }

    private class DomainType : DomainTypeBase
    {
      public readonly int Field1 = Dev<int>.Null;
      public readonly int Field2 = Dev<int>.Null;
      public readonly int Field3 = Dev<int>.Null;

      public void Method1 () { }
      public void Method2 () { }
      public void Method3 () { }

      public new void MethodInvolvedInShadowing () { }
    }
  }
}