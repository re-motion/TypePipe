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
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class TypeModifierTest
  {
    private MockRepository _mockRepository;
    private ISubclassProxyBuilderFactory _subclassProxyBuilderFactoryMock;

    private TypeModifier _typeModifier;

    [SetUp]
    public void SetUp ()
    {
      _mockRepository = new MockRepository();
      _subclassProxyBuilderFactoryMock = _mockRepository.StrictMock<ISubclassProxyBuilderFactory>();

      _typeModifier = new TypeModifier (_subclassProxyBuilderFactoryMock);
    }

    [Test]
    public void CodeGenerator ()
    {
      var fakeCodeGenerator = _mockRepository.Stub<ICodeGenerator>();
      _subclassProxyBuilderFactoryMock.Expect (mock => mock.CodeGenerator).Return (fakeCodeGenerator);
      _mockRepository.ReplayAll();

      Assert.That (_typeModifier.CodeGenerator, Is.SameAs (fakeCodeGenerator));
    }

    [Test]
    public void ApplyModifications ()
    {
      var descriptor = TypeDescriptorObjectMother.Create();
      var memberSelector = new MemberSelector (new BindingFlagsEvaluator());
      var relatedMethodFinder = new RelatedMethodFinder();
      var mutableMemberFactory = new MutableMemberFactory (memberSelector, relatedMethodFinder);
      var mutableTypePartialMock = _mockRepository.PartialMock<MutableType> (descriptor, memberSelector, relatedMethodFinder, mutableMemberFactory);

      var builderMock = _mockRepository.StrictMock<ISubclassProxyBuilder>();
      var fakeType = ReflectionObjectMother.GetSomeType();

      using (_mockRepository.Ordered())
      {
        _subclassProxyBuilderFactoryMock.Expect (mock => mock.CreateBuilder (mutableTypePartialMock)).Return (builderMock);
        mutableTypePartialMock.Expect (mock => mock.Accept ((IMutableTypeModificationHandler) builderMock));
        mutableTypePartialMock.Expect (mock => mock.Accept ((IMutableTypeUnmodifiedMutableMemberHandler) builderMock));
        builderMock.Expect (mock => mock.Build()).Return (fakeType);
      }
      _mockRepository.ReplayAll();

      var result = _typeModifier.ApplyModifications (mutableTypePartialMock);

      _mockRepository.VerifyAll();
      Assert.That (result, Is.SameAs (fakeType));
    }
  }
}