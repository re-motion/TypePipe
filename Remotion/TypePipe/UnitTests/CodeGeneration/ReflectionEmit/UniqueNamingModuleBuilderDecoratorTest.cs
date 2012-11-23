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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class UniqueNamingModuleBuilderDecoratorTest
  {
    private IModuleBuilder _moduleBuilderMock;
    private UniqueNamingModuleBuilderDecorator _decorator;

    [SetUp]
    public void SetUp ()
    {
      _moduleBuilderMock = MockRepository.GenerateStrictMock<IModuleBuilder>();
      _decorator = new UniqueNamingModuleBuilderDecorator (_moduleBuilderMock);
    }

    [Test]
    public void DefineType ()
    {
      var typeAttributes = TypeAttributes.SpecialName;
      var baseType = ReflectionObjectMother.GetSomeType();
      var fakeTypeBuilder = MockRepository.GenerateStub<ITypeBuilder>();
      _moduleBuilderMock.Expect (x => x.DefineType ("ClassName_Proxy1", typeAttributes, baseType)).Return (fakeTypeBuilder);

      var result = _decorator.DefineType ("ClassName", typeAttributes, baseType);

      _moduleBuilderMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeTypeBuilder));
    }

    [Test]
    public void DefineType_Uniqueness ()
    {
      var baseType = ReflectionObjectMother.GetSomeType ();

      _moduleBuilderMock.Expect (x => x.DefineType ("ClassName_Proxy1", 0, baseType)).Return (null);
      _moduleBuilderMock.Expect (x => x.DefineType ("ClassName_Proxy2", 0, baseType)).Return (null);
      _moduleBuilderMock.Expect (x => x.DefineType ("OtherClassName_Proxy3", 0, baseType)).Return (null);

      _decorator.DefineType ("ClassName", 0, baseType);
      _decorator.DefineType ("ClassName", 0, baseType);
      _decorator.DefineType ("OtherClassName", 0, baseType);

      _moduleBuilderMock.VerifyAllExpectations ();
    }

    [Test]
    public void SaveToDisk ()
    {
      var fakeResult = "abc";
      _moduleBuilderMock.Expect (mock => mock.SaveToDisk()).Return (fakeResult);

      var result = _decorator.SaveToDisk();

      _moduleBuilderMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (fakeResult));
    }
  }
}