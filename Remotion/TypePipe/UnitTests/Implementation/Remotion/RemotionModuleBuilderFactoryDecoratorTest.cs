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

using NUnit.Framework;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.Reflection.TypeDiscovery;
using Remotion.TypePipe.CodeGeneration.Implementation.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.Implementation.ReflectionEmit.Abstractions;
using Remotion.TypePipe.Implementation.Remotion;
using Remotion.TypePipe.MutableReflection;
using Rhino.Mocks;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.Implementation.Remotion
{
  [TestFixture]
  public class RemotionModuleBuilderFactoryDecoratorTest
  {
    private IModuleBuilderFactory _innerFactoryMock;

    private RemotionModuleBuilderFactoryDecorator _factory;

    [SetUp]
    public void SetUp ()
    {
      _innerFactoryMock = MockRepository.GenerateStrictMock<IModuleBuilderFactory>();

      _factory = new RemotionModuleBuilderFactoryDecorator (_innerFactoryMock);
    }

    [Test]
    public void CreateModuleBuilder ()
    {
      var assemblyName = "assembly name";
      var assemblyDirectoryOrNull = "directory";
      var strongNamed = BooleanObjectMother.GetRandomBoolean();
      var keyFilePathOrNull = "key file path";

      var moduleBuilderMock = MockRepository.GenerateStrictMock<IModuleBuilder>();
      var assemblyBuilderMock = MockRepository.GenerateStrictMock<IAssemblyBuilder>();
      _innerFactoryMock
          .Expect (mock => mock.CreateModuleBuilder (assemblyName, assemblyDirectoryOrNull, strongNamed, keyFilePathOrNull))
          .Return (moduleBuilderMock);
      moduleBuilderMock.Expect (mock => mock.AssemblyBuilder).Return (assemblyBuilderMock);
      assemblyBuilderMock
          .Expect (mock => mock.SetCustomAttribute (Arg<CustomAttributeDeclaration>.Is.Anything))
          .WhenCalled (
              mi =>
              {
                var declaration = (CustomAttributeDeclaration) mi.Arguments.Single();
                Assert.That (declaration.Type, Is.SameAs (typeof (NonApplicationAssemblyAttribute)));
                Assert.That (declaration.ConstructorArguments, Is.Empty);
              });

      var result = _factory.CreateModuleBuilder (assemblyName, assemblyDirectoryOrNull, strongNamed, keyFilePathOrNull);

      _innerFactoryMock.VerifyAllExpectations();
      moduleBuilderMock.VerifyAllExpectations();
      assemblyBuilderMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (moduleBuilderMock));
    }
  }
}