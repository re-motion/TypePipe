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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.UnitTests.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  [TestFixture]
  public class MemberParameterOnTypeInstantiationTest
  {
    private Type _typeParameter;
    private Type _typeArgument;
    private TypeInstantiation _declaringType;
    private MemberInfo _declaringMember;

    [SetUp]
    public void SetUp ()
    {
      _typeParameter = typeof (GenericType<>).GetGenericArguments().Single();
      _typeArgument = ReflectionObjectMother.GetSomeType();
      _declaringType = TypeInstantiationObjectMother.Create (typeof (GenericType<>), new[] { _typeArgument });
      _declaringMember = MethodOnTypeInstantiationObjectMother.Create (_declaringType);
    }

    [Test]
    public void Initialization ()
    {
      var parameter = CustomParameterInfoObjectMother.Create (_declaringMember, type: _typeParameter);

      var result = new MemberParameterOnTypeInstantiation (_declaringMember, parameter);

      Assert.That (result.Member, Is.SameAs (_declaringMember));
      Assert.That (result.Position, Is.EqualTo (parameter.Position));
      Assert.That (result.Name, Is.EqualTo (parameter.Name));
      Assert.That (result.Attributes, Is.EqualTo (parameter.Attributes));
      Assert.That (result.ParameterType, Is.SameAs (_typeArgument));
      Assert.That (result.MemberParameterOnGenericType, Is.SameAs (parameter));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "MemberParameterOnTypeInstantiation can only created with members of TypeInstantiation.\r\nParameter name: declaringMember")]
    public void Initialization_NonTypeInstantiationMember ()
    {
      var member = ReflectionObjectMother.GetSomeMember();
      var parameter = ReflectionObjectMother.GetSomeParameter();

      Dev.Null = new MemberParameterOnTypeInstantiation (member, parameter);
    }

    [Test]
    public void GetCustomAttributeData ()
    {
      var customAttributes = new[] { CustomAttributeDeclarationObjectMother.Create() };
      var member = MethodOnTypeInstantiationObjectMother.Create();
      var parameter = CustomParameterInfoObjectMother.Create (member, customAttributes: customAttributes);

      var parameterInstantiation = new MemberParameterOnTypeInstantiation (member, parameter);

      Assert.That (parameterInstantiation.GetCustomAttributeData(), Is.EqualTo (customAttributes));
    }

    class GenericType<T> { }
  }
}