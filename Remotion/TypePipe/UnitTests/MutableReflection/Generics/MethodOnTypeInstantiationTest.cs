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
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Generics;
using Rhino.Mocks;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  [TestFixture]
  public class MethodOnTypeInstantiationTest
  {
    private TypeInstantiation _declaringType;
    private IParameterAdjuster _parameterAdjuster;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = TypeInstantiationObjectMother.Create();
      _parameterAdjuster = MockRepository.GenerateStrictMock<IParameterAdjuster>();
    }

    [Test]
    public void Initialization ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => Method (7));

      var fakeReturnParameter = ReflectionObjectMother.GetSomeParameter();
      var fakeParameter = ReflectionObjectMother.GetSomeParameter();
      object backReference = null;
      _parameterAdjuster
          .Expect (mock => mock.SubstituteGenericParameters (Arg<MemberInfo>.Is.Anything, Arg.Is (method.ReturnParameter)))
          .Return (fakeReturnParameter)
          .WhenCalled (mi => backReference = mi.Arguments[0]);
      _parameterAdjuster
          .Expect (mock => mock.SubstituteGenericParameters (Arg<MemberInfo>.Is.Anything, Arg.Is (method.GetParameters().Single())))
          .Return (fakeParameter)
          .WhenCalled (mi => Assert.That (mi.Arguments[0], Is.Not.Null.And.SameAs (backReference)));

      var result = new MethodOnTypeInstantiation (_declaringType, _parameterAdjuster, method);

      _parameterAdjuster.VerifyAllExpectations();
      Assert.That (backReference, Is.SameAs (result));

      Assert.That (result.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (result.Name, Is.EqualTo (method.Name));
      Assert.That (result.Attributes, Is.EqualTo (method.Attributes));
      Assert.That (result.MethodOnGenericType, Is.SameAs (method));

      Assert.That (result.ReturnParameter, Is.SameAs (fakeReturnParameter));
      Assert.That (result.GetParameters(), Is.EqualTo (new[] { fakeParameter }));
    }

    string Method (int i) { Dev.Null = i; return ""; }
  }
}