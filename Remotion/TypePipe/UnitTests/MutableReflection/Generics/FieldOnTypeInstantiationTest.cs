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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Generics;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  [TestFixture]
  public class FieldOnTypeInstantiationTest
  {
    private TypeInstantiation _declaringType;
    private ITypeAdjuster _typeAdjuster;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = TypeInstantiationObjectMother.Create();
      _typeAdjuster = MockRepository.GenerateStrictMock<ITypeAdjuster>();
    }

    [Test]
    public void Initialization ()
    {
      var field = ReflectionObjectMother.GetSomeField();
      var fakeType = ReflectionObjectMother.GetSomeOtherType();
      _typeAdjuster.Expect (mock => mock.SubstituteGenericParameters (field.FieldType)).Return (fakeType);

      var result = new FieldOnTypeInstantiation (_declaringType, _typeAdjuster, field);

      _typeAdjuster.VerifyAllExpectations();
      Assert.That (result.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (result.Name, Is.EqualTo (field.Name));
      Assert.That (result.Attributes, Is.EqualTo (field.Attributes));
      Assert.That (result.FieldType, Is.SameAs (fakeType));
    }
  }
}