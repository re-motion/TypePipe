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
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection.Generics;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection.Implementation;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.UnitTests.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  [TestFixture]
  public class MemberParameterOnInstantiationTest
  {
    private Type _typeArgument;
    
    private Type _genericTypeParameter;
    private TypeInstantiation _declaringType;
    private MemberInfo _memberOnTypeInstantiation;

    private Type _genericMethodParameter;
    private MethodInstantiation _methodInstantiation;


    [SetUp]
    public void SetUp ()
    {
      _typeArgument = ReflectionObjectMother.GetSomeType ();

      var genericTypeDefinition = typeof (GenericType<>);
      _genericTypeParameter = genericTypeDefinition.GetGenericArguments().Single();
      _declaringType = TypeInstantiationObjectMother.Create (genericTypeDefinition, new[] { _typeArgument });
      _memberOnTypeInstantiation = MethodOnTypeInstantiationObjectMother.Create (_declaringType);

      var genericMethodDefinition = NormalizingMemberInfoFromExpressionUtility.GetGenericMethodDefinition (() => GenericMethod<Dev.T>());
      _genericMethodParameter = genericMethodDefinition.GetGenericArguments().Single();
      _methodInstantiation = MethodInstantiationObjectMother.Create (genericMethodDefinition, typeArguments: new[] { _typeArgument });
    }

    [Test]
    public void Initialization_OnTypeInstantiation ()
    {
      var parameter = CustomParameterInfoObjectMother.Create (type: _genericTypeParameter);

      var result = new MemberParameterOnInstantiation (_memberOnTypeInstantiation, parameter);

      Assert.That (result.Member, Is.SameAs (_memberOnTypeInstantiation));
      Assert.That (result.MemberParameterOnGenericDefinition, Is.SameAs (parameter));
      Assert.That (result.Position, Is.EqualTo (parameter.Position));
      Assert.That (result.Name, Is.EqualTo (parameter.Name));
      Assert.That (result.Attributes, Is.EqualTo (parameter.Attributes));
      Assert.That (result.ParameterType, Is.SameAs (_typeArgument));
    }

    [Test]
    public void Initialization_OnMethodInstantiation ()
    {
      var parameter = CustomParameterInfoObjectMother.Create (type: _genericMethodParameter);

      var result = new MemberParameterOnInstantiation (_methodInstantiation, parameter);

      Assert.That (result.Member, Is.SameAs (_methodInstantiation));
      Assert.That (result.MemberParameterOnGenericDefinition, Is.SameAs (parameter));
      Assert.That (result.ParameterType, Is.SameAs (_typeArgument));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException),
        ExpectedMessage = "MemberParameterOnInstantiation can only represent parameters of members on TypeInstantiation or parameters of "
                          + "MethodInstantiation instances.\r\nParameter name: declaringMember")]
    public void Initialization_NonTypeInstantiationMember ()
    {
      var member = ReflectionObjectMother.GetSomeMember();
      var parameter = ReflectionObjectMother.GetSomeParameter();

      Dev.Null = new MemberParameterOnInstantiation (member, parameter);
    }

    [Test]
    public void GetCustomAttributeData ()
    {
      var customAttributes = new[] { CustomAttributeDeclarationObjectMother.Create() };
      var member = MethodOnTypeInstantiationObjectMother.Create();
      var parameter = CustomParameterInfoObjectMother.Create (member, customAttributes: customAttributes);

      var parameterInstantiation = new MemberParameterOnInstantiation (member, parameter);

      Assert.That (parameterInstantiation.GetCustomAttributeData(), Is.EqualTo (customAttributes));
    }

    class GenericType<T> { }
    public void GenericMethod<T> () { }
  }
}