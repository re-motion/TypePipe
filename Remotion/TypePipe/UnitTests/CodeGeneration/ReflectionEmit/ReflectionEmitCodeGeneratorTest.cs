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
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.Configuration;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class ReflectionEmitCodeGeneratorTest
  {
    private IModuleBuilderFactory _moduleBuilderFactoryMock;
    private IConfigurationProvider _configurationProviderMock;

    private ReflectionEmitCodeGenerator _flusher;

    private IModuleBuilder _moduleBuilderMock;
    private IEmittableOperandProvider _emittableOperandProviderMock;

    [SetUp]
    public void SetUp ()
    {
      _moduleBuilderFactoryMock = MockRepository.GenerateStrictMock<IModuleBuilderFactory>();
      _configurationProviderMock = MockRepository.GenerateStrictMock<IConfigurationProvider>();

      _flusher = new ReflectionEmitCodeGenerator (_moduleBuilderFactoryMock, _configurationProviderMock);

      _moduleBuilderMock = MockRepository.GenerateStrictMock<IModuleBuilder>();
      _emittableOperandProviderMock = MockRepository.GenerateStrictMock<IEmittableOperandProvider>();
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_flusher.AssemblyDirectory, Is.Null);
      Assert.That (_flusher.AssemblyNamePattern, Is.EqualTo (@"TypePipe_GeneratedAssembly_{counter}"));
    }

    [Test]
    public void DebugInfoGenerator ()
    {
      var debugInfoGenerator = _flusher.DebugInfoGenerator;
      Assert.That (debugInfoGenerator.GetType().FullName, Is.EqualTo ("System.Runtime.CompilerServices.SymbolDocumentGenerator"));
      Assert.That (_flusher.DebugInfoGenerator, Is.SameAs (debugInfoGenerator));
    }

    [Test]
    public void SetAssemblyDirectory ()
    {
      _flusher.SetAssemblyDirectory ("Abc");
      Assert.That (_flusher.AssemblyDirectory, Is.EqualTo ("Abc"));

      _flusher.SetAssemblyDirectory (null);
      Assert.That (_flusher.AssemblyDirectory, Is.Null);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException),
        ExpectedMessage = "Cannot set assembly directory after a type has been defined (use FlushCodeToDisk() to start a new assembly).")]
    public void SetAssemblyDirectory_ThrowsForExistingCurrentModuleBuilder ()
    {
      DefineSomeType();
      _flusher.SetAssemblyDirectory ("Abc");
    }

    [Test]
    public void SetAssemblyNamePattern ()
    {
      _flusher.SetAssemblyNamePattern ("Def");
      Assert.That (_flusher.AssemblyNamePattern, Is.EqualTo ("Def"));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException),
        ExpectedMessage = "Cannot set assembly name pattern after a type has been defined (use FlushCodeToDisk() to start a new assembly).")]
    public void SetAssemblyNamePattern_ThrowsForExistingCurrentModuleBuilder ()
    {
      DefineSomeType();
      _flusher.SetAssemblyNamePattern ("Abc");
    }

    [Test]
    public void FlushCodeToDisk ()
    {
      DefineSomeType();
      var assemblyAttribute = CustomAttributeDeclarationObjectMother.Create();
      var fakeAssemblyPath = "fake path";
      var assemblyBuilderMock = MockRepository.GenerateStrictMock<IAssemblyBuilder>();
      _moduleBuilderMock.Expect (mock => mock.AssemblyBuilder).Return (assemblyBuilderMock);
      assemblyBuilderMock.Expect (mock => mock.SetCustomAttribute (assemblyAttribute));
      assemblyBuilderMock.Expect (mock => mock.SaveToDisk()).Return (fakeAssemblyPath);
      var previousDebugInfoGenerator = _flusher.DebugInfoGenerator;

      var result = _flusher.FlushCodeToDisk (new[] { assemblyAttribute });

      _moduleBuilderMock.VerifyAllExpectations();
      assemblyBuilderMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (fakeAssemblyPath));
      Assert.That (_flusher.DebugInfoGenerator, Is.Not.EqualTo (previousDebugInfoGenerator));
    }

    [Test]
    public void FlushCodeToDisk_NoTypeDefined ()
    {
      Assert.That (_flusher.FlushCodeToDisk (new CustomAttributeDeclaration[0]), Is.Null);
    }

    [Test]
    public void CreateEmittableOperandProvider ()
    {
      _configurationProviderMock.Expect (mock => mock.ForceStrongNaming).Return (false);

      var result = _flusher.CreateEmittableOperandProvider ();

      _configurationProviderMock.VerifyAllExpectations ();
      Assert.That (result, Is.TypeOf<EmittableOperandProvider> ());
    }

    [Test]
    public void CreateEmittableOperandProvider_StrongNaming ()
    {
      _configurationProviderMock.Expect (mock => mock.ForceStrongNaming).Return (true);

      var result = _flusher.CreateEmittableOperandProvider();

      _configurationProviderMock.VerifyAllExpectations();
      Assert.That (result, Is.TypeOf<StrongNameCheckingEmittableOperandProviderDecorator>());
      var strongNamingDecorator = (StrongNameCheckingEmittableOperandProviderDecorator) result;
      Assert.That (strongNamingDecorator.InnerEmittableOperandProvider, Is.TypeOf<EmittableOperandProvider>());
    }

    [Test]
    public void CreateEmittableOperandProvider_ConfigurationScopedViaModuleContext ()
    {
      _configurationProviderMock.Expect (mock => mock.ForceStrongNaming).Return (false).Repeat.Once();

      _flusher.CreateEmittableOperandProvider();
      _flusher.CreateEmittableOperandProvider();

      _configurationProviderMock.VerifyAllExpectations();
    }

    [Test]
    public void DefineType ()
    {
      var name = "DomainType";
      var attributes = (TypeAttributes) 7;
      var forceStrongNaming = BooleanObjectMother.GetRandomBoolean();
      var keyFilePath = "key file path";
      _flusher.SetAssemblyNamePattern ("custom assembly name pattern");

      _configurationProviderMock.Expect (mock => mock.ForceStrongNaming).Return (forceStrongNaming);
      _configurationProviderMock.Expect (mock => mock.KeyFilePath).Return (keyFilePath);
      _moduleBuilderFactoryMock
          .Expect (mock => mock.CreateModuleBuilder ("custom assembly name pattern", null, forceStrongNaming, keyFilePath))
          .Return (_moduleBuilderMock);

      var fakeTypeBuilder1 = MockRepository.GenerateStub<ITypeBuilder>();
      var fakeTypeBuilder2 = MockRepository.GenerateStub<ITypeBuilder>();
      _moduleBuilderMock.Expect (mock => mock.DefineType (name, attributes)).Return (fakeTypeBuilder1);
      _moduleBuilderMock.Expect (mock => mock.DefineType ("OtherType", 0)).Return (fakeTypeBuilder2);

      var result1 = _flusher.DefineType (name, attributes, _emittableOperandProviderMock);
      var result2 = _flusher.DefineType ("OtherType", 0, _emittableOperandProviderMock);

      _moduleBuilderFactoryMock.VerifyAllExpectations();
      _moduleBuilderMock.VerifyAllExpectations();
      _configurationProviderMock.VerifyAllExpectations();

      Assert.That (result1.As<TypeBuilderDecorator>().DecoratedTypeBuilder, Is.SameAs (fakeTypeBuilder1));
      Assert.That (result2.As<TypeBuilderDecorator>().DecoratedTypeBuilder, Is.SameAs (fakeTypeBuilder2));
      Assert.That (PrivateInvoke.GetNonPublicField (result1, "EmittableOperandProvider"), Is.SameAs (_emittableOperandProviderMock));
      Assert.That (PrivateInvoke.GetNonPublicField (result2, "EmittableOperandProvider"), Is.SameAs (_emittableOperandProviderMock));
    }

    private void DefineSomeType ()
    {
      _configurationProviderMock.Stub (stub => stub.ForceStrongNaming).Return (false);
      _configurationProviderMock.Stub (stub => stub.KeyFilePath).Return (null);
      _moduleBuilderFactoryMock.Stub (stub => stub.CreateModuleBuilder (null, null, false, null)).IgnoreArguments().Return (_moduleBuilderMock);
      _moduleBuilderMock.Stub (stub => stub.DefineType (null, 0)).IgnoreArguments();

      _flusher.DefineType ("SomeType", 0, _emittableOperandProviderMock);
    }
  }
}