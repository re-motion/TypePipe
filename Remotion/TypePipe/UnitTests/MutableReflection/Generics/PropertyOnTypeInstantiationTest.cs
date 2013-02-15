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
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.UnitTests.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  [TestFixture]
  public class PropertyOnTypeInstantiationTest
  {
    private TypeInstantiation _declaringType;
    private PropertyInfo _originalProperty;
    private MethodOnTypeInstantiation _getMethod;
    private MethodOnTypeInstantiation _setMethod;
    private CustomParameterInfo _indexParameter;

    private PropertyOnTypeInstantiation _property;

    [SetUp]
    public void SetUp ()
    {
      _indexParameter = CustomParameterInfoObjectMother.Create();

      _declaringType = TypeInstantiationObjectMother.Create();
      _getMethod = MethodOnTypeInstantiationObjectMother.Create (_declaringType, typeof (GenericType<>).GetMethod ("get_Item"));
      _setMethod = MethodOnTypeInstantiationObjectMother.Create (_declaringType, typeof (GenericType<>).GetMethod ("set_Item"));
      _originalProperty = CustomPropertyInfoObjectMother.Create (
          indexParameters: new[] { _indexParameter }, getMethod: _getMethod, setMethod: _setMethod);

      _property = new PropertyOnTypeInstantiation (_declaringType, _originalProperty, _getMethod, _setMethod);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_property.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (_property.PropertyOnGenericType, Is.SameAs (_originalProperty));
      Assert.That (_property.PropertyType, Is.EqualTo (_originalProperty.PropertyType));
      Assert.That (_property.Name, Is.EqualTo (_originalProperty.Name));
      Assert.That (_property.Attributes, Is.EqualTo (_originalProperty.Attributes));
      Assert.That (_property.GetGetMethod(), Is.SameAs (_getMethod));
      Assert.That (_property.GetSetMethod(), Is.SameAs (_setMethod));
    }

    [Test]
    public void GetIndexParameters ()
    {
      var originalParameter = _originalProperty.GetIndexParameters().Single();
      var parameter = _property.GetIndexParameters().Single();

      Assert.That (parameter, Is.TypeOf<MemberParameterOnTypeInstantiation>());
      Assert.That (parameter.Name, Is.EqualTo (originalParameter.Name));
      Assert.That (parameter.Member, Is.SameAs (_property));
      Assert.That (parameter, Is.TypeOf<MemberParameterOnTypeInstantiation>());
      Assert.That (parameter.As<MemberParameterOnTypeInstantiation>().MemberParameterOnGenericType, Is.SameAs (originalParameter));
    }

    [Test]
    public void GetCustomAttributeData ()
    {
      var customAttributes = new[] { CustomAttributeDeclarationObjectMother.Create() };
      var property = CustomPropertyInfoObjectMother.Create (customAttributes: customAttributes);
      var propertyInstantiation = new PropertyOnTypeInstantiation (_declaringType, property, _getMethod, _setMethod);

      Assert.That (propertyInstantiation.GetCustomAttributeData(), Is.EqualTo (customAttributes));
    }

    public class GenericType<T>
    {
      public T this [int index]
      {
        get { return default (T); }
        set { Dev.Null = value; }
      }
    }
  }
}