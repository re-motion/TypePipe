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
using System.Linq;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection.Generics;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection.Generics;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  [TestFixture]
  public class FieldOnTypeInstantiationTest
  {
    private TypeInstantiation _declaringType;
    private Type _typeParameter;
    private Type _typeArgument;

    [SetUp]
    public void SetUp ()
    {
      _typeParameter = typeof (GenericType<>).GetGenericArguments().Single();
      _typeArgument = ReflectionObjectMother.GetSomeType();
      _declaringType = TypeInstantiationObjectMother.Create (typeof (GenericType<>), new[] { _typeArgument });
    }

    [Test]
    public void Initialization ()
    {
      var field = CustomFieldInfoObjectMother.Create (type: _typeParameter);

      var result = new FieldOnTypeInstantiation (_declaringType, field);

      Assert.That (result.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (result.Name, Is.EqualTo (field.Name));
      Assert.That (result.Attributes, Is.EqualTo (field.Attributes));
      Assert.That (result.FieldType, Is.SameAs (_typeArgument));
      Assert.That (result.FieldOnGenericType, Is.SameAs (field));
    }

    [Test]
    public void GetCustomAttributeData ()
    {
      var customAttributes = new[] { CustomAttributeDeclarationObjectMother.Create() };
      var field = CustomFieldInfoObjectMother.Create (customAttributes: customAttributes);
      var fieldInstantiation = new FieldOnTypeInstantiation (_declaringType, field);

      Assert.That (fieldInstantiation.GetCustomAttributeData(), Is.EqualTo (customAttributes));
    }

    class GenericType<T> {}
  }
}