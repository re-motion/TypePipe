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
using Remotion.Utilities;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableTypeIntegrationTest
  {
    private MutableType _mutableType;

    private MutableMethodInfo _publicMethod;
    private MutableMethodInfo _publicMethodWithOverloadEmpty;
    private MutableMethodInfo _publicMethodWithOverloadInt;

    [SetUp]
    public void SetUp ()
    {
      _mutableType = MutableTypeObjectMother.CreateForExistingType (typeof (DomainType));

      _publicMethod = _mutableType.GetMutableMethod (MemberInfoFromExpressionUtility.GetMethod ((DomainType dt) => dt.PublicMethod (0)));
      _publicMethodWithOverloadEmpty = _mutableType.GetMutableMethod (MemberInfoFromExpressionUtility.GetMethod ((DomainType dt) => dt.PublicMethodWithOverload ()));
      _publicMethodWithOverloadInt = _mutableType.GetMutableMethod (MemberInfoFromExpressionUtility.GetMethod ((DomainType dt) => dt.PublicMethodWithOverload (1)));
    }

    [Test]
    public void GetMethod_Name ()
    {
      var positiveResult = _mutableType.GetMethod ("PublicMethod");
      Assert.That (positiveResult, Is.SameAs (_publicMethod));
    }

    [Test]
    public void GetMethod_Name_NonMatchingName ()
    {
      var negativeResult = _mutableType.GetMethod ("DoesNotExist");
      Assert.That (negativeResult, Is.Null);
    }

    [Test]
    public void GetMethod_NameAndBindingFlags ()
    {
      var positiveResult = _mutableType.GetMethod ("PublicMethod", BindingFlags.Public | BindingFlags.Instance);
      Assert.That (positiveResult, Is.SameAs (_publicMethod));
    }

    [Test]
    public void GetMethod_NameAndBindingFlags_NonMatchingName ()
    {
      var negativeResult = _mutableType.GetMethod ("DoesNotExist", BindingFlags.Public | BindingFlags.Instance);
      Assert.That (negativeResult, Is.Null);
    }

    [Test]
    public void GetMethod_NameAndBindingFlags_NonMatchingBindingFlags ()
    {
      var negativeResult = _mutableType.GetMethod ("PublicMethod", BindingFlags.NonPublic | BindingFlags.Instance);
      Assert.That (negativeResult, Is.Null);
    }

    [Test]
    public void GetMethod_NameAndBindingFlagsAndTypesAndCallingConvention ()
    {
      var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
      var binderMock = MockRepository.GenerateStrictMock<Binder> ();
      var callConvention = CallingConventions.Any;
      var types = new[] { typeof (int) };
      var parameterModifiers = new[] { new ParameterModifier (1) };

      var candidates = new[] { _publicMethodWithOverloadEmpty, _publicMethodWithOverloadInt };
      var fakeResult = ReflectionObjectMother.GetSomeMethod();
      binderMock
          .Expect (mock => mock.SelectMethod (bindingFlags, candidates, types, parameterModifiers))
          .Return (fakeResult);

      var positiveResult = _mutableType.GetMethod (
          "PublicMethodWithOverload",
          bindingFlags,
          binderMock,
          callConvention,
          types,
          parameterModifiers);

      binderMock.VerifyAllExpectations();

      Assert.That (positiveResult, Is.SameAs (fakeResult));
    }

    [Test]
    public void GetMethod_NameAndBindingFlagsAndTypesAndCallingConvention_NonMatchingBindingFlags ()
    {
      var negativeResultDueToBindingFlags = _mutableType.GetMethod (
          "PublicMethodWithOverload",
          BindingFlags.NonPublic | BindingFlags.Instance,
          Type.DefaultBinder,
          CallingConventions.Any,
          new[] { typeof (int) },
          null);
      Assert.That (negativeResultDueToBindingFlags, Is.Null);
    }

    [Test]
    [Ignore ("TODO 4836")]
    public void GetMethod_NameAndBindingFlagsAndTypesAndCallingConvention_NonMatchingCallingConvention ()
    {
      var negativeResultDueToCallingConvention = _mutableType.GetMethod (
          "PublicMethodWithOverload",
          BindingFlags.Public | BindingFlags.Instance,
          Type.DefaultBinder,
          CallingConventions.Standard,
          new[] { typeof (int) },
          null);
      Assert.That (negativeResultDueToCallingConvention, Is.Null);
    }

    [Test]
    public void GetMethod_NameAndBindingFlagsAndTypesAndCallingConvention_NonMatchingTypes ()
    {
      var negativeResultDueToTypes = _mutableType.GetMethod (
          "PublicMethodWithOverload",
          BindingFlags.Public | BindingFlags.Instance,
          Type.DefaultBinder,
          CallingConventions.Any,
          new[] { typeof (string) },
          null);
      Assert.That (negativeResultDueToTypes, Is.Null);
    }

    [Test]
    public void GetMethod_NameAndBindingFlagsAndTypes ()
    {
      var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
      var binderMock = MockRepository.GenerateStrictMock<Binder> ();
      var types = new[] { typeof (int) };
      var parameterModifiers = new[] { new ParameterModifier (1) };

      var candidates = new[] { _publicMethodWithOverloadEmpty, _publicMethodWithOverloadInt };
      var fakeResult = ReflectionObjectMother.GetSomeMethod ();
      binderMock
          .Expect (mock => mock.SelectMethod (bindingFlags, candidates, types, parameterModifiers))
          .Return (fakeResult);

      var positiveResult = _mutableType.GetMethod (
          "PublicMethodWithOverload",
          bindingFlags,
          binderMock,
          types,
          parameterModifiers);

      binderMock.VerifyAllExpectations ();

      Assert.That (positiveResult, Is.SameAs (fakeResult));
    }

    [Test]
    public void GetMethod_NameAndBindingFlagsAndTypes_NonMatchingBindingFlags ()
    {
      var negativeResultDueToBindingFlags = _mutableType.GetMethod (
          "PublicMethodWithOverload",
          BindingFlags.NonPublic | BindingFlags.Instance,
          Type.DefaultBinder,
          new[] { typeof (int) },
          null);
      Assert.That (negativeResultDueToBindingFlags, Is.Null);
    }

    [Test]
    public void GetMethod_NameAndBindingFlagsAndTypes_NonMatchingTypes ()
    {
      var negativeResultDueToTypes = _mutableType.GetMethod (
          "PublicMethodWithOverload",
          BindingFlags.Public | BindingFlags.Instance,
          Type.DefaultBinder,
          new[] { typeof (string) },
          null);
      Assert.That (negativeResultDueToTypes, Is.Null);
    }

    [Test]
    public void GetMethod_NameAndTypes ()
    {
      var result = _mutableType.GetMethod ("PublicMethodWithOverload", new[] { typeof (int) });
      Assert.That (result, Is.SameAs (_publicMethodWithOverloadInt));
    }

    [Test]
    public void GetMethod_NameAndTypes_NonMatchingName ()
    {
      var result = _mutableType.GetMethod ("DoesNotExist", new[] { typeof (int) });
      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetMethod_NameAndTypes_NonMatchingTypes ()
    {
      var result = _mutableType.GetMethod ("PublicMethodWithOverload", new[] { typeof (string) });
      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetMethod_NameAndTypesAndParameterModifiers ()
    {
      var result = _mutableType.GetMethod ("PublicMethodWithOverload", new[] { typeof (int) }, new[] { new ParameterModifier (1) });
      Assert.That (result, Is.SameAs (_publicMethodWithOverloadInt));
    }

    [Test]
    public void GetMethod_NameAndTypesAndParameterModifiers_NonMatchingName ()
    {
      var result = _mutableType.GetMethod ("DoesNotExist", new[] { typeof (int) }, new[] { new ParameterModifier (1) });
      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetMethod_NameAndTypesAndParameterModifiers_NonMatchingTypes ()
    {
      var result = _mutableType.GetMethod ("PublicMethodWithOverload", new[] { typeof (string) }, new[] { new ParameterModifier (1) });
      Assert.That (result, Is.Null);
    }

    public class DomainType
    {
      public void PublicMethod (int i) { }

      public void PublicMethodWithOverload () { }
      public void PublicMethodWithOverload (int i) { }
    }
  }
}