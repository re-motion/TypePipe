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
using Remotion.Utilities;
using Rhino.Mocks;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  [TestFixture]
  public class MethodOnTypeInstantiationTest
  {
    private TypeInstantiation _declaringType;
    private ITypeAdjuster _typeAdjusterMock;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = TypeInstantiationObjectMother.Create();
      _typeAdjusterMock = MockRepository.GenerateStrictMock<ITypeAdjuster> ();
    }

    [Test]
    public void Initialization ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => Method (7));

      var fakeReturnType = ReflectionObjectMother.GetSomeType();
      var fakeParameterType = ReflectionObjectMother.GetSomeOtherType();
      _typeAdjusterMock
          .Expect (mock => mock.SubstituteGenericParameters (method.ReturnType))
          .Return (fakeReturnType);
      _typeAdjusterMock
          .Expect (mock => mock.SubstituteGenericParameters (method.GetParameters().Single().ParameterType))
          .Return (fakeParameterType);
      
      var result = new MethodOnTypeInstantiation (_declaringType, _typeAdjusterMock, method);

      _typeAdjusterMock.VerifyAllExpectations();
      Assert.That (result.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (result.Name, Is.EqualTo (method.Name));
      Assert.That (result.Attributes, Is.EqualTo (method.Attributes));
      Assert.That (result.MethodOnGenericType, Is.SameAs (method));

      var returnParameter = result.ReturnParameter;
      Assertion.IsNotNull (returnParameter);
      Assert.That (returnParameter, Is.TypeOf<MemberParameterOnTypeInstantiation>());
      Assert.That (returnParameter.Member, Is.SameAs (result));
      Assert.That (returnParameter.As<MemberParameterOnTypeInstantiation>().MemberParameterOnGenericType, Is.EqualTo (method.ReturnParameter));

      var parameter = result.GetParameters().Single();
      Assert.That (parameter, Is.TypeOf<MemberParameterOnTypeInstantiation>());
      Assert.That (parameter.Member, Is.SameAs (result));
      Assert.That (parameter.As<MemberParameterOnTypeInstantiation>().MemberParameterOnGenericType, Is.EqualTo (method.GetParameters().Single()));
    }

    string Method (int i) { Dev.Null = i; return ""; }
  }
}