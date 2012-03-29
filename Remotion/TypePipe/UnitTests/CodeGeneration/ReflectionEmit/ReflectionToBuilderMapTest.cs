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
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class ReflectionToBuilderMapTest
  {
    private ReflectionToBuilderMap _reflectionToBuilderMap;

    private Type _someType;
    private ITypeBuilder _fakeTypeBuilder;

    private ConstructorInfo _someConstructorInfo;
    private IConstructorBuilder _fakeConstructorBuilder;
    
    private FieldInfo _someFieldInfo;
    private IFieldBuilder _fakeFieldBuilder;

    [SetUp]
    public void SetUp ()
    {
      _reflectionToBuilderMap = new ReflectionToBuilderMap();

      _someType = ReflectionObjectMother.GetSomeType();
      _fakeTypeBuilder = MockRepository.GenerateStub<ITypeBuilder>();

      _someConstructorInfo = ReflectionObjectMother.GetSomeDefaultConstructor();
      _fakeConstructorBuilder = MockRepository.GenerateStub<IConstructorBuilder>();

      _someFieldInfo = ReflectionObjectMother.GetSomeField ();
      _fakeFieldBuilder = MockRepository.GenerateStub<IFieldBuilder> ();
    }

    [Test]
    public void AddMapping_Type ()
    {
      _reflectionToBuilderMap.AddMapping (_someType, _fakeTypeBuilder);
      var result = _reflectionToBuilderMap.GetBuilder (_someType);

      Assert.That (result, Is.SameAs (_fakeTypeBuilder));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Type is already mapped.\r\nParameter name: mappedType")]
    public void AddMapping_Type_Twice ()
    {
      _reflectionToBuilderMap.AddMapping (_someType, _fakeTypeBuilder);
      _reflectionToBuilderMap.AddMapping (_someType, _fakeTypeBuilder);
    }

    [Test]
    public void AddMapping_ConstructorInfo ()
    {
      _reflectionToBuilderMap.AddMapping (_someConstructorInfo, _fakeConstructorBuilder);
      var result = _reflectionToBuilderMap.GetBuilder (_someConstructorInfo);

      Assert.That (result, Is.SameAs (_fakeConstructorBuilder));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "ConstructorInfo is already mapped.\r\nParameter name: mappedConstructorInfo")]
    public void AddMapping_ConstructorInfo_Twice ()
    {
      _reflectionToBuilderMap.AddMapping (_someConstructorInfo, _fakeConstructorBuilder);
      _reflectionToBuilderMap.AddMapping (_someConstructorInfo, _fakeConstructorBuilder);
    }

    [Test]
    public void AddMapping_FieldInfo ()
    {
      _reflectionToBuilderMap.AddMapping (_someFieldInfo, _fakeFieldBuilder);
      var result = _reflectionToBuilderMap.GetBuilder (_someFieldInfo);

      Assert.That (result, Is.SameAs (_fakeFieldBuilder));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "FieldInfo is already mapped.\r\nParameter name: mappedFieldInfo")]
    public void AddMapping_FieldInfo_Twice ()
    {
      _reflectionToBuilderMap.AddMapping (_someFieldInfo, _fakeFieldBuilder);
      _reflectionToBuilderMap.AddMapping (_someFieldInfo, _fakeFieldBuilder);
    }

    [Test]
    public void GetBuilder_Type_NoMapping ()
    {
      var result = _reflectionToBuilderMap.GetBuilder (_someType);
      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetBuilder_ConstructorInfo_NoMapping ()
    {
      var result = _reflectionToBuilderMap.GetBuilder (_someConstructorInfo);
      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetBuilder_FieldInfo_NoMapping ()
    {
      var result = _reflectionToBuilderMap.GetBuilder (_someFieldInfo);
      Assert.That (result, Is.Null);
    }
  }
}