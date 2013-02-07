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
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Generics;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  [TestFixture]
  public class ConstructorOnTypeInstantiationTest
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
      var ctor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainType (7));
      var fakeParameter = ReflectionObjectMother.GetSomeParameter ();
      object backReference = null;
      _parameterAdjuster
          .Expect (mock => mock.SubstituteGenericParameters (Arg<MemberInfo>.Is.Anything, Arg.Is (ctor.GetParameters().Single())))
          .Return (fakeParameter)
          .WhenCalled (mi => backReference = mi.Arguments[0]);
      var result = new ConstructorOnTypeInstantiation (_declaringType, _parameterAdjuster, ctor);

      _parameterAdjuster.VerifyAllExpectations ();
      Assert.That (backReference, Is.SameAs (result));

      Assert.That (result.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (result.Attributes, Is.EqualTo (ctor.Attributes));
      Assert.That (result.ConstructorOnGenericType, Is.SameAs (ctor));

      Assert.That (result.GetParameters(), Is.EqualTo (new[] { fakeParameter }));
    }

    private class DomainType
    {
      public DomainType (int i) { Dev.Null = i; }
    }
  }
}