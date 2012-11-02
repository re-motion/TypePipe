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
using NUnit.Framework;
using Remotion.TypePipe.MutableReflection;
using Rhino.Mocks;
using System.Linq;
using Remotion.Development.UnitTesting.Enumerables;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MemberSelectorTest
  {
    private IBindingFlagsEvaluator _bindingFlagsEvaluatorMock;
    
    private MemberSelector _selector;

    private Type _someType;    


    [SetUp]
    public void SetUp ()
    {
      _bindingFlagsEvaluatorMock = MockRepository.GenerateStrictMock<IBindingFlagsEvaluator> ();

      _selector = new MemberSelector (_bindingFlagsEvaluatorMock);

      _someType = ReflectionObjectMother.GetSomeType();
    }

    [Test]
    public void SelectFields ()
    {
      var candidates = new[]
                       {
                           CreateFieldStub (fieldAttributes: FieldAttributes.Assembly),
                           CreateFieldStub (fieldAttributes: FieldAttributes.Family),
                           CreateFieldStub (fieldAttributes: FieldAttributes.FamORAssem)
                       };
      var bindingFlags = (BindingFlags) 1;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[0].Attributes, bindingFlags)).Return (true);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[1].Attributes, bindingFlags)).Return (false);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[2].Attributes, bindingFlags)).Return (true);

      var result = _selector.SelectFields (candidates, bindingFlags).ForceEnumeration();

      _bindingFlagsEvaluatorMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (new[] { candidates[0], candidates[2] }));
    }

    [Test]
    public void SelectMethods ()
    {
      var candidates = new[] { CreateMethodStub(), CreateMethodStub(), CreateMethodStub() };
      var bindingFlags = (BindingFlags) 1;
      var declaringType = ReflectionObjectMother.GetSomeType();

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[0].Attributes, bindingFlags)).Return (true);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[1].Attributes, bindingFlags)).Return (false);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[2].Attributes, bindingFlags)).Return (true);

      var result = _selector.SelectMethods (candidates, bindingFlags, declaringType).ForceEnumeration();

      _bindingFlagsEvaluatorMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (new[] { candidates[0], candidates[2] }));
    }

    [Test]
    public void SelectMethods_DeclaredOnly ()
    {
      var declaringType1 = typeof (string);
      var declaringType2 = typeof (int);
      var candidates = new[] { CreateMethodStub (declaringType: declaringType1), CreateMethodStub (declaringType: declaringType2) };
      var bindingFlags = (BindingFlags) 1 | BindingFlags.DeclaredOnly;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[0].Attributes, bindingFlags)).Return (true);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[1].Attributes, bindingFlags)).Return (true);

      var result = _selector.SelectMethods (candidates, bindingFlags, declaringType2).ForceEnumeration();

      _bindingFlagsEvaluatorMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (new[] { candidates[1] }));
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
      var bindingFlags = (BindingFlags) 1;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (fields[0].Attributes, bindingFlags)).Return (false);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (fields[2].Attributes, bindingFlags)).Return (true);

      var result = _selector.SelectSingleField (fields, bindingFlags, "field1");

      _bindingFlagsEvaluatorMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (fields[2]));
    }

    [Test]
    public void SelectSingleField_EmptyCandidates ()
    {
      var fields = new[] { CreateFieldStub ("field1"), CreateFieldStub ("wrong name") };
      var bindingFlags = (BindingFlags) 1;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (fields[0].Attributes, bindingFlags)).Return (false);

      var result = _selector.SelectSingleField (fields, bindingFlags, "field1");

      Assert.That (result, Is.Null);
    }

    [Test]
    [ExpectedException (typeof (AmbiguousMatchException), ExpectedMessage = "Ambiguous field name 'field1'.")]
    public void SelectSingleField_Ambiguous ()
    {
      var fields = new[] { CreateFieldStub ("field1", FieldAttributes.Assembly), CreateFieldStub ("field1", FieldAttributes.Public) };
      var bindingFlags = (BindingFlags) 1;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (fields[0].Attributes, bindingFlags)).Return (true);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (fields[1].Attributes, bindingFlags)).Return (true);

      _selector.SelectSingleField (fields, bindingFlags, "field1");
    }

    [Test]
    public void SelectSingleMethod ()
    {
      var methods =
          new[]
          {
              CreateMethodStub ("Method1", MethodAttributes.Assembly),
              CreateMethodStub ("This method is filtered because of its name", MethodAttributes.Family),
              CreateMethodStub ("Method1", MethodAttributes.Public)
          };
      var bindingFlags = (BindingFlags) 1;
      var typesOrNull = new[] { typeof (int), typeof (string) };
      var modifiersOrNull = new[] { new ParameterModifier (2) };

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (methods[0].Attributes, bindingFlags)).Return (false);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (methods[2].Attributes, bindingFlags)).Return (true);

      var binderMock = MockRepository.GenerateStrictMock<Binder>();
      var fakeResult = ReflectionObjectMother.GetSomeMethod();
      binderMock
          .Expect (mock => mock.SelectMethod (bindingFlags, new[] { methods[2] }, typesOrNull, modifiersOrNull))
          .Return (fakeResult);

      var result = _selector.SelectSingleMethod (methods, binderMock, bindingFlags, "Method1", _someType, typesOrNull, modifiersOrNull);

      _bindingFlagsEvaluatorMock.VerifyAllExpectations();
      binderMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void SelectSingleMethod_ForConstructors_NameIsNotConsidered ()
    {
      var constructors = new[] { CreateConstructorStub (MethodAttributes.Assembly), CreateConstructorStub (MethodAttributes.Family) };
      var bindingFlags = (BindingFlags) 1;
      var typesOrNull = new[] { typeof (int), typeof (string) };
      var modifiersOrNull = new[] { new ParameterModifier (2) };

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (constructors[0].Attributes, bindingFlags)).Return (false);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (constructors[1].Attributes, bindingFlags)).Return (true);

      var binderMock = MockRepository.GenerateStrictMock<Binder>();
      var fakeResult = ReflectionObjectMother.GetSomeConstructor();
      binderMock
          .Expect (mock => mock.SelectMethod (bindingFlags, new[] { constructors[1] }, typesOrNull, modifiersOrNull))
          .Return (fakeResult);

      var result = _selector.SelectSingleMethod (constructors, binderMock, bindingFlags, null, _someType, typesOrNull, modifiersOrNull);

      _bindingFlagsEvaluatorMock.VerifyAllExpectations ();
      binderMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void SelectSingleMethod_ZeroCandidates ()
    {
      var methods = new[] { CreateMethodStub ("Method1"), CreateMethodStub ("wrong name") };
      var binderStub = MockRepository.GenerateStub<Binder> ();
      var bindingFlags = (BindingFlags) 1;
      var typesOrNull = new[] { typeof (int), typeof (string) };
      var modifiersOrNull = new[] { new ParameterModifier (2) };

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (methods[0].Attributes, bindingFlags)).Return (false);

      var result = _selector.SelectSingleMethod (methods, binderStub, bindingFlags, "Method1", _someType, typesOrNull, modifiersOrNull);

      _bindingFlagsEvaluatorMock.VerifyAllExpectations();
      Assert.That (result, Is.Null);
    }

    [Test]
    [ExpectedException (typeof (AmbiguousMatchException), ExpectedMessage = "Ambiguous method name 'Method1'.")]
    public void SelectSingleMethod_TypesNull_Ambiguous ()
    {
      var methods = new[] { CreateMethodStub ("Method1"), CreateMethodStub ("Method1") };
      var bindingFlags = (BindingFlags) 1;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (methods[0].Attributes, bindingFlags)).Return (true);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (methods[1].Attributes, bindingFlags)).Return (true);

      var binderStub = MockRepository.GenerateStub<Binder>();
      _selector.SelectSingleMethod (methods, binderStub, bindingFlags, "Method1", _someType, typesOrNull: null, modifiersOrNull: null);
    }

    [Test]
    public void SelectSingleMethod_TypesNull_SingleCandidate ()
    {
      var methods = new[] { CreateMethodStub ("Method1") };
      var bindingFlags = (BindingFlags) 1;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (methods[0].Attributes, bindingFlags)).Return (true);

      var binderMock = MockRepository.GenerateStrictMock<Binder>();
      var result = _selector.SelectSingleMethod (methods, binderMock, bindingFlags, "Method1", _someType, typesOrNull: null, modifiersOrNull: null);

      binderMock.AssertWasNotCalled (
          mock => mock.SelectMethod (
              Arg<BindingFlags>.Is.Anything, Arg<MethodBase[]>.Is.Anything, Arg<Type[]>.Is.Anything, Arg<ParameterModifier[]>.Is.Anything));
      Assert.That (result, Is.SameAs (methods[0]));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Modifiers must not be specified if types are null.\r\nParameter name: modifiersOrNull")]
    public void SelectSingleMethod_TypesNull_ModifiersNotNull ()
    {
      var methods = new[] { CreateMethodStub() };
      var bindingFlags = (BindingFlags) 1;
      var modifiersOrNull = new[] { new ParameterModifier (2) };

      var binderStub = MockRepository.GenerateStub<Binder>();
      _selector.SelectSingleMethod (methods, binderStub, bindingFlags, "Whatever", _someType, null, modifiersOrNull);
    }

    private FieldInfo CreateFieldStub (string name = "Unspecified", FieldAttributes fieldAttributes = FieldAttributes.PrivateScope)
    {
      var fieldStub = MockRepository.GenerateStub<FieldInfo>();
      fieldStub.Stub (stub => stub.Name).Return (name);
      fieldStub.Stub (stub => stub.Attributes).Return (fieldAttributes);

      var x = fieldStub.Attributes;
      var y = fieldStub.Attributes;

      return fieldStub;
    }

    private ConstructorInfo CreateConstructorStub (MethodAttributes methodAttributes = MethodAttributes.PrivateScope)
    {
      var constructorStub = MockRepository.GenerateStub<ConstructorInfo>();
      constructorStub.Stub (stub => stub.Name).Repeat.Never();
      constructorStub.Stub (stub => stub.Attributes).Return (methodAttributes);
      return constructorStub;
    }

    private MethodInfo CreateMethodStub (
        string name = "Unspecified", MethodAttributes methodAttributes = MethodAttributes.PrivateScope, Type declaringType = null)
    {
      var methodStub = MockRepository.GenerateStub<MethodInfo>();
      methodStub.Stub (stub => stub.Name).Return (name);
      methodStub.Stub (stub => stub.Attributes).Return (methodAttributes);
      methodStub.Stub (stub => stub.DeclaringType).Return (declaringType);
      return methodStub;
    }
  }
}