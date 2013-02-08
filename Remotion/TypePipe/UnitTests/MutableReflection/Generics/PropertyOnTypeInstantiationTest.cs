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
using JetBrains.Annotations;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Generics;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  [TestFixture]
  public class PropertyOnTypeInstantiationTest
  {
    private TypeInstantiation _declaringType;
    private PropertyInfo _originalProperty;
    private MethodOnTypeInstantiation _getMethod;
    private MethodOnTypeInstantiation _setMethod;

    private PropertyOnTypeInstantiation _property;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = TypeInstantiationObjectMother.Create();
      _originalProperty = GetType().GetProperty ("Item");
      _getMethod = MethodOnTypeInstantiationObjectMother.Create (_declaringType, GetType().GetMethod ("get_Item"));
      _setMethod = MethodOnTypeInstantiationObjectMother.Create (_declaringType, GetType().GetMethod ("set_Item"));

      _property = new PropertyOnTypeInstantiation (_declaringType, _originalProperty, _getMethod, _setMethod);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_property.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (_property.Name, Is.EqualTo (_originalProperty.Name));
      Assert.That (_property.Attributes, Is.EqualTo (_originalProperty.Attributes));
      Assert.That (_property.GetGetMethod(), Is.SameAs (_getMethod));
      Assert.That (_property.GetSetMethod(), Is.SameAs (_setMethod));
    }

    [Test]
    [Ignore]
    public void GetIndexParameters ()
    {
      var originalParameter = _originalProperty.GetIndexParameters().Single();
      var parameter = _property.GetIndexParameters().Single();

      Assert.That (parameter, Is.TypeOf<MemberParameterOnTypeInstantiation>());
      Assert.That (parameter.Name, Is.EqualTo (originalParameter.Name));
      Assert.That (parameter.Member, Is.SameAs (_property));
    }

    [UsedImplicitly]
    public string this [int index]
    {
      get { return ""; }
      set { }
    }
  }
}