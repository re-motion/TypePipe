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
using NUnit.Framework;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.Implementation;
using Remotion.Utilities;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.TypeAssembly.Implementation
{
  [TestFixture]
  public class ConstructorDelegateFactoryTest
  {
    private IConstructorFinder _constructorFinderMock;

    private ConstructorDelegateFactory _factory;

    [SetUp]
    public void SetUp ()
    {
      _constructorFinderMock = MockRepository.GenerateStrictMock<IConstructorFinder>();

      _factory = new ConstructorDelegateFactory (_constructorFinderMock);
    }

    [Test]
    public void CreateConstructorCall ()
    {
      var requestedType = ReflectionObjectMother.GetSomeType();
      var delegateType = typeof (Func<string, int, DomainType>);
      var allowNonPublic = BooleanObjectMother.GetRandomBoolean();
      var assembledType = ReflectionObjectMother.GetSomeOtherType();

      _constructorFinderMock
          .Expect (mock => mock.GetConstructor (requestedType, new[] { typeof (string), typeof (int) }, allowNonPublic, assembledType))
          .Return (MemberInfoFromExpressionUtility.GetConstructor (() => new DomainType ("", 7)));

      var result = (Func<string, int, DomainType>) _factory.CreateConstructorCall (requestedType, assembledType, delegateType, allowNonPublic);

      var instance = result ("abc", 7);
      Assert.That (instance.String, Is.EqualTo ("abc"));
      Assert.That (instance.Int, Is.EqualTo (7));
    }

    [Test]
    public void CreateConstructorCall_ValueType ()
    {
      var requestedType = ReflectionObjectMother.GetSomeType();
      var delegateType = typeof (Func<string, int, DomainValueType>);
      var allowNonPublic = BooleanObjectMother.GetRandomBoolean();
      var assembledType = ReflectionObjectMother.GetSomeOtherType();

      _constructorFinderMock
          .Expect (mock => mock.GetConstructor (requestedType, new[] { typeof (string), typeof (int) }, allowNonPublic, assembledType))
          .Return (MemberInfoFromExpressionUtility.GetConstructor (() => new DomainValueType ("", 7)));

      var result = (Func<string, int, DomainValueType>) _factory.CreateConstructorCall (requestedType, assembledType, delegateType, allowNonPublic);

      var instance = result ("abc", 7);
      Assert.That (instance.String, Is.EqualTo ("abc"));
      Assert.That (instance.Int, Is.EqualTo (7));
    }

    [Test]
    public void CreateConstructorCall_ValueType_Boxing ()
    {
      var requestedType = ReflectionObjectMother.GetSomeType();
      var delegateType = typeof (Func<string, int, object>);
      var allowNonPublic = BooleanObjectMother.GetRandomBoolean();
      var assembledType = ReflectionObjectMother.GetSomeOtherType();

      _constructorFinderMock
          .Expect (mock => mock.GetConstructor (requestedType, new[] { typeof (string), typeof (int) }, allowNonPublic, assembledType))
          .Return (MemberInfoFromExpressionUtility.GetConstructor (() => new DomainValueType ("", 7)));

      var result = (Func<string, int, object>) _factory.CreateConstructorCall (requestedType, assembledType, delegateType, allowNonPublic);

      var instance = (DomainValueType) result ("abc", 7);
      Assert.That (instance.String, Is.EqualTo ("abc"));
      Assert.That (instance.Int, Is.EqualTo (7));
    }

    private class DomainType
    {
      public readonly string String;
      public readonly int Int;

      public DomainType (string s1, int i2)
      {
        String = s1;
        Int = i2;
      }
    }

    private struct DomainValueType
    {
      public readonly string String;
      public readonly int Int;

      public DomainValueType (string s1, int i2)
      {
        String = s1;
        Int = i2;
      }
    }
  }
}