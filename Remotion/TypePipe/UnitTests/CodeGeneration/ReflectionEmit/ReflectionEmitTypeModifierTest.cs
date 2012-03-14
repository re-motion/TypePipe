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
using Remotion.FunctionalProgramming;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.BuilderAbstractions;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class ReflectionEmitTypeModifierTest
  {
    private IModuleBuilder _moduleBuilderMock;
    private ISubclassProxyNameProvider _subclassProxyNameProviderMock;
    private ITypeBuilder _typeBuilderMock;
    private ReflectionEmitTypeModifier _reflectionEmitTypeModifier;
    private ITypeInfo _typeInfoMock;
    private MutableType _mutableType;
    private Type _underlyingSystemType;

    [SetUp]
    public void SetUp ()
    {
      _moduleBuilderMock = MockRepository.GenerateStrictMock<IModuleBuilder>();
      _subclassProxyNameProviderMock = MockRepository.GenerateStrictMock<ISubclassProxyNameProvider> ();
      _typeBuilderMock = MockRepository.GenerateStrictMock<ITypeBuilder> ();
      _reflectionEmitTypeModifier = new ReflectionEmitTypeModifier (_moduleBuilderMock, _subclassProxyNameProviderMock);
      _underlyingSystemType = typeof (OriginalType);
      _typeInfoMock = MockRepository.GenerateStrictMock<ITypeInfo> ();
      _mutableType = MutableTypeObjectMother.Create (typeInfo: _typeInfoMock);
    }

    [Test]
    public void CreateMutableType ()
    {
      var mutableType = _reflectionEmitTypeModifier.CreateMutableType (_underlyingSystemType);

      Assert.That (mutableType.UnderlyingSystemType, Is.SameAs (_underlyingSystemType));
    }

    [Test]
    public void ApplyModifications_NoModifications ()
    {
      CheckApplyModifications (Type.EmptyTypes, tb => { });
    }

    [Test]
    public void ApplyModifications_Interfaces ()
    {
      _typeInfoMock.Expect (mock => mock.GetInterfaces ()).Return (Type.EmptyTypes);
      _mutableType.AddInterface (typeof (IDisposable));

      CheckApplyModifications (new[] { typeof (IDisposable) }, tb => { });
    }

    [Test]
    public void ApplyModifications_AddField ()
    {
      _typeInfoMock.Expect (mock => mock.GetFields (Arg<BindingFlags>.Is.Anything)).Return (new FieldInfo[0]);
      _mutableType.AddField (typeof (string), "_newField", FieldAttributes.Private);

      CheckApplyModifications (
          Type.EmptyTypes, 
          tb => _typeBuilderMock.Expect (mock => mock.DefineField ("_newField", typeof (string), FieldAttributes.Private)));
    }

    private void CheckApplyModifications (Type[] expectedInterfaces, Action<ITypeBuilder> typeBuilderExpectations)
    {
      var fakeResultType = typeof (string);

      _typeInfoMock.Expect (mock => mock.GetUnderlyingSystemType ()).Return (Maybe.ForValue (_underlyingSystemType));
      _subclassProxyNameProviderMock.Expect (mock => mock.GetSubclassProxyName (_underlyingSystemType)).Return ("foofoo");
      _moduleBuilderMock
          .Expect (mock => mock.DefineType ("foofoo", TypeAttributes.Public | TypeAttributes.BeforeFieldInit, _underlyingSystemType, expectedInterfaces))
          .Return (_typeBuilderMock);
      typeBuilderExpectations (_typeBuilderMock);
      _typeBuilderMock.Expect (mock => mock.CreateType()).Return (fakeResultType);

      var result = _reflectionEmitTypeModifier.ApplyModifications (_mutableType);

      _moduleBuilderMock.VerifyAllExpectations();
      _subclassProxyNameProviderMock.VerifyAllExpectations();
      _typeBuilderMock.VerifyAllExpectations();
      _typeInfoMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeResultType));
    }

    public class OriginalType
    {
    }
  }
}