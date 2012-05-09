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
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class EmittableOperandProviderTest
  {
    private EmittableOperandProvider _provider;

    private Type _someType;
    private FieldInfo _someFieldInfo;
    private ConstructorInfo _someConstructorInfo;
    private MethodInfo _someMethodInfo;

    [SetUp]
    public void SetUp ()
    {
      _provider = new EmittableOperandProvider();

      _someType = ReflectionObjectMother.GetSomeType();
      _someFieldInfo = ReflectionObjectMother.GetSomeField ();
      _someConstructorInfo = ReflectionObjectMother.GetSomeDefaultConstructor();
      _someMethodInfo = ReflectionObjectMother.GetSomeMethod ();
    }

    [Test]
    public void AddMapping ()
    {
      CheckAddMapping (_provider.AddMapping, _provider.GetEmittableType, _someType);
      CheckAddMapping (_provider.AddMapping, _provider.GetEmittableField, _someFieldInfo);
      CheckAddMapping (_provider.AddMapping, _provider.GetEmittableConstructor, _someConstructorInfo);
      CheckAddMapping (_provider.AddMapping, _provider.GetEmittableMethod, _someMethodInfo);
    }

    [Test]
    public void AddMapping_Twice ()
    {
      CheckAddMappingTwiceThrows (_provider.AddMapping, _someType, "Type is already mapped.\r\nParameter name: mappedType");
      CheckAddMappingTwiceThrows (_provider.AddMapping, _someFieldInfo, "FieldInfo is already mapped.\r\nParameter name: mappedFieldInfo");
      CheckAddMappingTwiceThrows (_provider.AddMapping, _someConstructorInfo, "ConstructorInfo is already mapped.\r\nParameter name: mappedConstructorInfo");
      CheckAddMappingTwiceThrows (_provider.AddMapping, _someMethodInfo, "MethodInfo is already mapped.\r\nParameter name: mappedMethodInfo");
    }
    
    [Test]
    public void GetEmittableOperand_NoMapping ()
    {
      CheckGetEmitableOperandWithNoMapping (_provider.GetEmittableType, _someType);
      CheckGetEmitableOperandWithNoMapping (_provider.GetEmittableField, _someFieldInfo);
      CheckGetEmitableOperandWithNoMapping (_provider.GetEmittableConstructor, _someConstructorInfo);
      CheckGetEmitableOperandWithNoMapping (_provider.GetEmittableMethod, _someMethodInfo);
    }

    private void CheckAddMapping<T> (Action<T, T> addMappingMethod, Func<T, T> getEmittableOperandMethod, T mappedObject)
        where T : MemberInfo
    {
      var fakeOperand = MockRepository.GenerateStub<T>();
     
      addMappingMethod (mappedObject, fakeOperand);

      var result = getEmittableOperandMethod (mappedObject);
      Assert.That (result, Is.SameAs (fakeOperand));
    }

    private void CheckAddMappingTwiceThrows<T> (Action<T, T> addMappingMethod, T mappedObject, string expectedMessage)
        where T: class
    {
      addMappingMethod (mappedObject, MockRepository.GenerateStub<T> ());

      Assert.That (
          () => addMappingMethod (mappedObject, MockRepository.GenerateStub<T> ()),
          Throws.ArgumentException.With.Message.EqualTo (expectedMessage));
    }

    private void CheckGetEmitableOperandWithNoMapping<T> (Func<T, T> getEmittableOperandMethod, T mappedObject)
    {
      var result = getEmittableOperandMethod (mappedObject);

      Assert.That (result, Is.Not.Null);
      Assert.That (result, Is.SameAs (mappedObject));
    }
  }
}