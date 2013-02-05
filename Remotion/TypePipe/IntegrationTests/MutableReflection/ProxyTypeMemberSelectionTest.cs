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
using JetBrains.Annotations;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Rhino.Mocks;
using System.Linq;

namespace Remotion.TypePipe.IntegrationTests.MutableReflection
{
  [TestFixture]
  public class ProxyTypeMemberSelectionTest
  {
    private ProxyType _proxyType;

    private FieldInfo _publicField;
    private MethodInfo _publicMethod;
    private MethodInfo _publicMethodWithOverloadEmpty;
    private MethodInfo _publicMethodWithOverloadInt;
    private PropertyInfo _publicProperty;
    private PropertyInfo _publicPropertyWithIndexParameter;
    

    [SetUp]
    public void SetUp ()
    {
      _proxyType = ProxyTypeObjectMother.Create (typeof (DomainType));

      _publicField = NormalizingMemberInfoFromExpressionUtility.GetField ((DomainType obj) => obj.PublicField);
      _publicMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.PublicMethod (0));
      _publicMethodWithOverloadEmpty = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.PublicMethodWithOverload());
      _publicMethodWithOverloadInt = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.PublicMethodWithOverload (1));
      _publicProperty = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DomainType obj) => obj.PublicProperty);
      _publicPropertyWithIndexParameter = typeof (DomainType).GetProperty ("Item");
    }

    [Test]
    public void TypeInitializer ()
    {
      Assert.That (_proxyType.TypeInitializer, Is.Null);

      _proxyType.AddTypeInitializer (ctx => Expression.Empty());
      Assert.That (_proxyType.TypeInitializer, Is.Not.Null);
    }

    [Test]
    public void GetField_Name ()
    {
      var result = _proxyType.GetField ("PublicField");
      Assert.That (result, Is.SameAs (_publicField));
    }

    [Test]
    public void GetField_Name_NonMatchingName ()
    {
      var result = _proxyType.GetField ("DoesNotExist");
      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetField_NameAndBindingFlags ()
    {
      var result = _proxyType.GetField ("PublicField", BindingFlags.Public | BindingFlags.Instance);
      Assert.That (result, Is.SameAs (_publicField));
    }

    [Test]
    public void GetField_NameAndBindingFlags_NonMatchingName ()
    {
      var result = _proxyType.GetField ("DoesNotExist", BindingFlags.Public | BindingFlags.Instance);
      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetField_NameAndBindingFlags_NonMatchingBindingFlags ()
    {
      var result = _proxyType.GetField ("PublicField", BindingFlags.NonPublic | BindingFlags.Instance);
      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetConstructor_Types ()
    {
      var result = _proxyType.GetConstructor (new[] { typeof (int) });

      var expectedCtor =
          _proxyType.AddedConstructors.Single (c => c.MutableParameters.Select (p => p.ParameterType).SequenceEqual (new[] { typeof (int) }));
      Assert.That (result, Is.SameAs (expectedCtor));
    }

    [Test]
    public void GetConstructor_Types_NonMatchingTypes ()
    {
      var result = _proxyType.GetConstructor (new[] { typeof (string) });
      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetConstructor_BindingFlagsAndTypesAndCallingConvention ()
    {
      var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
      var binderMock = MockRepository.GenerateStrictMock<Binder> ();
      var callConvention = CallingConventions.Any;
      var types = new[] { typeof (int) };
      var parameterModifiers = new[] { new ParameterModifier (1) };

      var candidates = _proxyType.AddedConstructors.Cast<MethodBase>().ToArray();
      Assert.That (candidates, Is.Not.Empty);
      var fakeResult = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new object ());
      binderMock.Expect (mock => mock.SelectMethod (bindingFlags, candidates, types, parameterModifiers)).Return (fakeResult);

      var result = _proxyType.GetConstructor (bindingFlags, binderMock, callConvention, types, parameterModifiers);

      binderMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void GetConstructor_BindingFlagsAndTypesAndCallingConvention_NonMatchingBindingFlags ()
    {
      var negativeResultDueToBindingFlags = _proxyType.GetConstructor (
          BindingFlags.NonPublic | BindingFlags.Instance,
          Type.DefaultBinder,
          CallingConventions.Any,
          new[] { typeof (int) },
          null);
      Assert.That (negativeResultDueToBindingFlags, Is.Null);
    }

    [Test]
    [Ignore ("TODO 4836")]
    public void GetConstructor_BindingFlagsAndTypesAndCallingConvention_NonMatchingCallingConvention ()
    {
      var negativeResultDueToCallingConvention = _proxyType.GetConstructor (
          BindingFlags.Public | BindingFlags.Instance,
          Type.DefaultBinder,
          CallingConventions.Standard,
          new[] { typeof (int) },
          null);
      Assert.That (negativeResultDueToCallingConvention, Is.Null);
    }

    [Test]
    public void GetConstructor_BindingFlagsAndTypesAndCallingConvention_NonMatchingTypes ()
    {
      var negativeResultDueToTypes = _proxyType.GetConstructor (
          BindingFlags.Public | BindingFlags.Instance,
          Type.DefaultBinder,
          CallingConventions.Any,
          new[] { typeof (string) },
          null);
      Assert.That (negativeResultDueToTypes, Is.Null);
    }

    [Test]
    public void GetConstructor_BindingFlagsAndTypes ()
    {
      var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
      var binderMock = MockRepository.GenerateStrictMock<Binder> ();
      var types = new[] { typeof (int) };
      var parameterModifiers = new[] { new ParameterModifier (1) };

      var candidates = _proxyType.AddedConstructors.Cast<MethodBase>().ToArray();
      Assert.That (candidates, Is.Not.Empty);
      var fakeResult = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new object());
      binderMock.Expect (mock => mock.SelectMethod (bindingFlags, candidates, types, parameterModifiers)).Return (fakeResult);

      var result = _proxyType.GetConstructor (bindingFlags, binderMock, types, parameterModifiers);

      binderMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void GetConstructor_BindingFlagsAndTypes_NonMatchingBindingFlags ()
    {
      var negativeResultDueToBindingFlags = _proxyType.GetConstructor (
          BindingFlags.NonPublic | BindingFlags.Instance,
          Type.DefaultBinder,
          new[] { typeof (int) },
          null);
      Assert.That (negativeResultDueToBindingFlags, Is.Null);
    }

    [Test]
    public void GetConstructor_BindingFlagsAndTypes_NonMatchingTypes ()
    {
      var negativeResultDueToTypes = _proxyType.GetConstructor (
          BindingFlags.Public | BindingFlags.Instance,
          Type.DefaultBinder,
          new[] { typeof (string) },
          null);
      Assert.That (negativeResultDueToTypes, Is.Null);
    }

    [Test]
    public void GetMethod_Name ()
    {
      var result = _proxyType.GetMethod ("PublicMethod");
      Assert.That (result, Is.SameAs (_publicMethod));
    }

    [Test]
    public void GetMethod_Name_NonMatchingName ()
    {
      var result = _proxyType.GetMethod ("DoesNotExist");
      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetMethod_NameAndBindingFlags ()
    {
      var result = _proxyType.GetMethod ("PublicMethod", BindingFlags.Public | BindingFlags.Instance);
      Assert.That (result, Is.SameAs (_publicMethod));
    }

    [Test]
    public void GetMethod_NameAndBindingFlags_NonMatchingName ()
    {
      var result = _proxyType.GetMethod ("DoesNotExist", BindingFlags.Public | BindingFlags.Instance);
      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetMethod_NameAndBindingFlags_NonMatchingBindingFlags ()
    {
      var result = _proxyType.GetMethod ("PublicMethod", BindingFlags.NonPublic | BindingFlags.Instance);
      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetMethod_NameAndTypes ()
    {
      var result = _proxyType.GetMethod ("PublicMethodWithOverload", new[] { typeof (int) });
      Assert.That (result, Is.SameAs (_publicMethodWithOverloadInt));
    }

    [Test]
    public void GetMethod_NameAndTypes_NonMatchingName ()
    {
      var result = _proxyType.GetMethod ("DoesNotExist", new[] { typeof (int) });
      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetMethod_NameAndTypes_NonMatchingTypes ()
    {
      var result = _proxyType.GetMethod ("PublicMethodWithOverload", new[] { typeof (string) });
      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetMethod_NameAndTypesAndParameterModifiers ()
    {
      var result = _proxyType.GetMethod ("PublicMethodWithOverload", new[] { typeof (int) }, new[] { new ParameterModifier (1) });
      Assert.That (result, Is.SameAs (_publicMethodWithOverloadInt));
    }

    [Test]
    public void GetMethod_NameAndTypesAndParameterModifiers_NonMatchingName ()
    {
      var result = _proxyType.GetMethod ("DoesNotExist", new[] { typeof (int) }, new[] { new ParameterModifier (1) });
      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetMethod_NameAndTypesAndParameterModifiers_NonMatchingTypes ()
    {
      var result = _proxyType.GetMethod ("PublicMethodWithOverload", new[] { typeof (string) }, new[] { new ParameterModifier (1) });
      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetMethod_NameAndBindingFlagsAndTypesAndCallingConvention ()
    {
      var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
      var binderMock = MockRepository.GenerateStrictMock<Binder>();
      var callConvention = CallingConventions.Any;
      var types = new[] { typeof (int) };
      var parameterModifiers = new[] { new ParameterModifier (1) };

      var candidates = new MethodBase[] { _publicMethodWithOverloadEmpty, _publicMethodWithOverloadInt };
      var fakeResult = NormalizingMemberInfoFromExpressionUtility.GetMethod ((object obj) => obj.ToString());
      binderMock.Expect (mock => mock.SelectMethod (bindingFlags, candidates, types, parameterModifiers)).Return (fakeResult);

      var result = _proxyType.GetMethod ("PublicMethodWithOverload", bindingFlags, binderMock, callConvention, types, parameterModifiers);

      binderMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void GetMethod_NameAndBindingFlagsAndTypes ()
    {
      var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
      var binderMock = MockRepository.GenerateStrictMock<Binder> ();
      var types = new[] { typeof (int) };
      var parameterModifiers = new[] { new ParameterModifier (1) };

      var candidates = new MethodBase[] { _publicMethodWithOverloadEmpty, _publicMethodWithOverloadInt };
      var fakeResult = NormalizingMemberInfoFromExpressionUtility.GetMethod ((object obj) => obj.ToString());
      binderMock.Expect (mock => mock.SelectMethod (bindingFlags, candidates, types, parameterModifiers)).Return (fakeResult);

      var result = _proxyType.GetMethod ("PublicMethodWithOverload", bindingFlags, binderMock, types, parameterModifiers);

      binderMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void GetMethod_NameAndBindingFlagsAndTypes_NonMatchingName ()
    {
      var negativeResultDueToBindingFlags = _proxyType.GetMethod (
          "DoesNotExist",
          BindingFlags.Public | BindingFlags.Instance,
          Type.DefaultBinder,
          new[] { typeof (int) },
          null);
      Assert.That (negativeResultDueToBindingFlags, Is.Null);
    }

    [Test]
    public void GetMethod_NameAndBindingFlagsAndTypes_NonMatchingBindingFlags ()
    {
      var negativeResultDueToBindingFlags = _proxyType.GetMethod (
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
      var negativeResultDueToTypes = _proxyType.GetMethod (
          "PublicMethodWithOverload",
          BindingFlags.Public | BindingFlags.Instance,
          Type.DefaultBinder,
          new[] { typeof (string) },
          null);
      Assert.That (negativeResultDueToTypes, Is.Null);
    }

    [Test]
    public void GetMethod_NameAndBindingFlagsAndTypesAndCallingConvention_NonMatchingName ()
    {
      var negativeResultDueToBindingFlags = _proxyType.GetMethod (
          "DoesNotExist",
          BindingFlags.Public | BindingFlags.Instance,
          Type.DefaultBinder,
          CallingConventions.Any,
          new[] { typeof (int) },
          null);
      Assert.That (negativeResultDueToBindingFlags, Is.Null);
    }

    [Test]
    public void GetMethod_NameAndBindingFlagsAndTypesAndCallingConvention_NonMatchingBindingFlags ()
    {
      var negativeResultDueToBindingFlags = _proxyType.GetMethod (
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
      var negativeResultDueToCallingConvention = _proxyType.GetMethod (
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
      var negativeResultDueToTypes = _proxyType.GetMethod (
          "PublicMethodWithOverload",
          BindingFlags.Public | BindingFlags.Instance,
          Type.DefaultBinder,
          CallingConventions.Any,
          new[] { typeof (string) },
          null);
      Assert.That (negativeResultDueToTypes, Is.Null);
    }

    [Test]
    public void GetProperty_Name ()
    {
      var result = _proxyType.GetProperty ("PublicProperty");
      Assert.That (result, Is.SameAs (_publicProperty));
    }

    [Test]
    public void GetProperty_Name_NonMatchingName ()
    {
      var result = _proxyType.GetProperty ("DoesNotExist");
      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetProperty_NameAndBindingFlags ()
    {
      var result = _proxyType.GetProperty ("PublicProperty", BindingFlags.Public | BindingFlags.Instance);
      Assert.That (result, Is.SameAs (_publicProperty));
    }

    [Test]
    public void GetProperty_NameAndBindingFlags_NonMatchingName ()
    {
      var result = _proxyType.GetProperty ("DoesNotExist", BindingFlags.Public | BindingFlags.Instance);
      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetProperty_NameAndBindingFlags_NonMatchingBindingFlags ()
    {
      var result = _proxyType.GetProperty ("PublicProperty", BindingFlags.NonPublic | BindingFlags.Instance);
      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetProperty_NameAndBindingFlagsAndBinderAndReturnTypeAndTypes ()
    {
      var result = _proxyType.GetProperty (
          "Item", BindingFlags.Public | BindingFlags.Instance, Type.DefaultBinder, typeof (int), new[] { typeof (int) }, modifiers: null);
      Assert.That (result, Is.SameAs (_publicPropertyWithIndexParameter));
    }

    [Test]
    public void GetProperty_NameAndBindingFlagsAndBinderAndReturnTypeAndTypes_NonMatchingName ()
    {
      var result = _proxyType.GetProperty (
          "DoesNotExist", BindingFlags.Public | BindingFlags.Instance, Type.DefaultBinder, typeof (int), new[] { typeof (int) }, modifiers: null);
      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetProperty_NameAndBindingFlagsAndBinderAndReturnTypeAndTypes_NonBindingFlags ()
    {
      var result = _proxyType.GetProperty (
          "Item", BindingFlags.NonPublic | BindingFlags.Instance, Type.DefaultBinder, typeof (int), new[] { typeof (int) }, modifiers: null);
      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetProperty_NameAndBindingFlagsAndBinderAndReturnTypeAndTypes_NonReturnType ()
    {
      var result = _proxyType.GetProperty (
          "Item", BindingFlags.Public | BindingFlags.Instance, Type.DefaultBinder, typeof (string), new[] { typeof (int) }, modifiers: null);
      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetProperty_NameAndBindingFlagsAndBinderAndReturnTypeAndTypes_NonTypes ()
    {
      var result = _proxyType.GetProperty (
          "Item", BindingFlags.Public | BindingFlags.Instance, Type.DefaultBinder, typeof (int), new[] { typeof (string) }, modifiers: null);
      Assert.That (result, Is.Null);
    }

    public class DomainType
    {
      [UsedImplicitly]
      public int PublicField;

      public DomainType () { }
      public DomainType (int i) { Dev.Null = i; }

      public void PublicMethod (int i) { Dev.Null = i; }
      public void PublicMethodWithOverload () { }
      public void PublicMethodWithOverload (int i) { Dev.Null = i; }

      public int PublicProperty { get; set; }
      public int this[int i] { get { return i; } set { Dev.Null = value; } }
    }
  }
}