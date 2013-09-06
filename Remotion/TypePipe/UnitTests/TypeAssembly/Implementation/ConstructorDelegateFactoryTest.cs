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
using Remotion.Reflection;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.TypeAssembly.Implementation
{
  [TestFixture]
  public class ConstructorDelegateFactoryTest
  {
    private IConstructorFinder _constructorFinderMock;
    private IDelegateFactory _delegateFactoryMock;

    private ConstructorDelegateFactory _factory;

    [SetUp]
    public void SetUp ()
    {
      _constructorFinderMock = MockRepository.GenerateStrictMock<IConstructorFinder>();
      _delegateFactoryMock = MockRepository.GenerateStrictMock<IDelegateFactory>();

      _factory = new ConstructorDelegateFactory (_constructorFinderMock, _delegateFactoryMock);
    }

    [Test]
    public void CreateConstructorCall ()
    {
      var requestedType = ReflectionObjectMother.GetSomeType();
      var delegateType = ReflectionObjectMother.GetSomeDelegateType();
      var allowNonPublic = BooleanObjectMother.GetRandomBoolean();
      var fakeSignature = Tuple.Create (new[] { ReflectionObjectMother.GetSomeType() }, ReflectionObjectMother.GetSomeType());
      var fakeConstructor = ReflectionObjectMother.GetSomeConstructor();
      var assembledConstructorCall = (Action) (() => { });
      var assembledType = ReflectionObjectMother.GetSomeOtherType();

      _delegateFactoryMock.Expect (mock => mock.GetSignature (delegateType)).Return (fakeSignature);
      _constructorFinderMock
          .Expect (mock => mock.GetConstructor (requestedType, fakeSignature.Item1, allowNonPublic, assembledType))
          .Return (fakeConstructor);
      _delegateFactoryMock
          .Expect (mock => mock.CreateConstructorCall (fakeConstructor, delegateType))
          .Return (assembledConstructorCall);

      var result = _factory.CreateConstructorCall (requestedType, assembledType, delegateType, allowNonPublic);

      _delegateFactoryMock.VerifyAllExpectations();
      _constructorFinderMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (assembledConstructorCall));
    }
  }
}