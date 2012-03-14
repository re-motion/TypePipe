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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.BuilderAbstractions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class TypeModificationHandlerTest
  {
    private ITypeBuilder _subclassProxyBuilderMock;
    private TypeModificationHandler _handler;

    [SetUp]
    public void SetUp ()
    {
      _subclassProxyBuilderMock = MockRepository.GenerateMock<ITypeBuilder>();
      _handler = new TypeModificationHandler (_subclassProxyBuilderMock);
    }

    [Test]
    public void HandleAddedInterface ()
    {
      var addedInterface = ReflectionObjectMother.GetSomeInterfaceType();

      _handler.HandleAddedInterface (addedInterface);

      _subclassProxyBuilderMock.AssertWasCalled (mock => mock.AddInterfaceImplementation (addedInterface));
    }

    [Test]
    public void HandleAddedField ()
    {
      var addedField = MutableFieldInfoObjectMother.Create();

      _handler.HandleAddedField (addedField);

      _subclassProxyBuilderMock.AssertWasCalled (mock => mock.DefineField (addedField.Name, addedField.FieldType, addedField.Attributes));
    }
  }
}