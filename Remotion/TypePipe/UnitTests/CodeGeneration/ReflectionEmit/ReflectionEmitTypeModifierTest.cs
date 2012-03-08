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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.BuilderAbstractions;
using Remotion.TypePipe.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class ReflectionEmitTypeModifierTest
  {
    private IModuleBuilder _moduleBuilderMock;
    private ISubclassProxyNameProvider _subclassProxyNameProviderMock;
    private ReflectionEmitTypeModifier _reflectionEmitTypeModifier;
    private ITypeBuilder _typeBuilderMock;

    [SetUp]
    public void SetUp ()
    {
      _moduleBuilderMock = MockRepository.GenerateStrictMock<IModuleBuilder>();
      _subclassProxyNameProviderMock = MockRepository.GenerateStrictMock<ISubclassProxyNameProvider> ();
      _reflectionEmitTypeModifier = new ReflectionEmitTypeModifier (_moduleBuilderMock, _subclassProxyNameProviderMock);
      _typeBuilderMock = MockRepository.GenerateStrictMock<ITypeBuilder> ();
    }

    [TearDown]
    public void TearDown ()
    {
      _moduleBuilderMock.VerifyAllExpectations ();
      _subclassProxyNameProviderMock.VerifyAllExpectations ();
      _typeBuilderMock.VerifyAllExpectations ();
    }

    [Test]
    public void CreateModifiedType ()
    {
      var originalType = typeof (object);

      var modifiedType =_reflectionEmitTypeModifier.CreateModifiedType (originalType);

      Assert.That (modifiedType.OriginalType, Is.SameAs (originalType));
    }

    [Test]
    public void ApplyModifications_NoModifications ()
    {
      var modifiedType = new ModifiedType (typeof (object));
      var fakeResultType = typeof (string);

      _subclassProxyNameProviderMock.Expect (mock => mock.GetSubclassProxyName (modifiedType)).Return ("foofoo");
      
      var typeBuilderMock = MockRepository.GenerateStrictMock<ITypeBuilder>();
      _moduleBuilderMock
          .Expect (mock => mock.DefineType ("foofoo", TypeAttributes.Public | TypeAttributes.BeforeFieldInit, typeof (object), Type.EmptyTypes))
          .Return (typeBuilderMock);
      typeBuilderMock.Expect (mock => mock.CreateType()).Return (fakeResultType);
      
      var result = _reflectionEmitTypeModifier.ApplyModifications (modifiedType);

      Assert.That (result, Is.SameAs (fakeResultType));
    }

    [Test]
    public void ApplyModifications_Interfaces ()
    {
      var modifiedType = new ModifiedType (typeof (object));
      modifiedType.AddInterface (typeof (IDisposable));

      var fakeResultType = typeof (string);

      _subclassProxyNameProviderMock.Expect (mock => mock.GetSubclassProxyName (modifiedType)).Return ("foofoo");
      _moduleBuilderMock
          .Expect (
              mock =>
              mock.DefineType ("foofoo", TypeAttributes.Public | TypeAttributes.BeforeFieldInit, typeof (object), new[] { typeof (IDisposable) }))
          .Return (_typeBuilderMock);
      _typeBuilderMock.Expect (mock => mock.CreateType ()).Return (fakeResultType);

      var result = _reflectionEmitTypeModifier.ApplyModifications (modifiedType);

      Assert.That (result, Is.SameAs (fakeResultType));
    }

    [Test]
    public void ApplyModifications_AddField ()
    {
      var modifiedType = new ModifiedType (typeof (object));
      modifiedType.AddField ("_newField", typeof (string), FieldAttributes.Private);

      var fakeResultType = typeof (string);

      _subclassProxyNameProviderMock.Expect (mock => mock.GetSubclassProxyName (modifiedType)).Return ("foofoo");
      _moduleBuilderMock
          .Expect (
              mock =>
              mock.DefineType ("foofoo", TypeAttributes.Public | TypeAttributes.BeforeFieldInit, typeof (object), new Type[0]))
          .Return (_typeBuilderMock);
      _typeBuilderMock.Expect (mock => mock.DefineField ("_newField", typeof (string), FieldAttributes.Private));
      _typeBuilderMock.Expect (mock => mock.CreateType ()).Return (fakeResultType);

      var result = _reflectionEmitTypeModifier.ApplyModifications (modifiedType);

      Assert.That (result, Is.SameAs (fakeResultType));
    }
  }
}