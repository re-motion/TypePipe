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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class TypeModifierTest
  {
    [Test]
    public void ApplyModifications ()
    {
      var mockRepository = new MockRepository();
      var handlerFactoryMock = mockRepository.StrictMock<ISubclassProxyBuilderFactory>();

      var descriptor = UnderlyingTypeDescriptorObjectMother.Create ();
      var mutableTypePartialMock = mockRepository.PartialMock<MutableType> (
          descriptor,
          new MemberSelector (new BindingFlagsEvaluator()),
          new RelatedMethodFinder());

      var builderMock = mockRepository.StrictMock<ISubclassProxyBuilder>();
      var fakeType = ReflectionObjectMother.GetSomeType ();

      using (mockRepository.Ordered ())
      {
        handlerFactoryMock.Expect (mock => mock.CreateBuilder (mutableTypePartialMock)).Return (builderMock);
        mutableTypePartialMock.Expect (mock => mock.Accept ((IMutableTypeUnmodifiedMutableMemberHandler) builderMock));
        mutableTypePartialMock.Expect (mock => mock.Accept ((IMutableTypeModificationHandler) builderMock));
        builderMock.Expect (mock => mock.Build()).Return (fakeType);
      }

      mockRepository.ReplayAll();
      
      var typeModifier = new TypeModifier (handlerFactoryMock);
      var result = typeModifier.ApplyModifications (mutableTypePartialMock);

      mockRepository.VerifyAll ();

      Assert.That (result, Is.SameAs (fakeType));
    }
  }
}