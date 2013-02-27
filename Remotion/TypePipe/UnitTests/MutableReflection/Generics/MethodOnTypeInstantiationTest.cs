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
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.UnitTests.MutableReflection.Implementation;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  [TestFixture]
  public class MethodOnTypeInstantiationTest
  {
    private TypeInstantiation _declaringType;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = TypeInstantiationObjectMother.Create();
    }

    [Test]
    public void Initialization ()
    {
      var parameter = CustomParameterInfoObjectMother.Create();
      var method = CustomMethodInfoObjectMother.Create (_declaringType, parameters: new[] { parameter });

      var instantiation = new MethodOnTypeInstantiation (_declaringType, method);

      Assert.That (instantiation.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (instantiation.Name, Is.EqualTo (method.Name));
      Assert.That (instantiation.Attributes, Is.EqualTo (method.Attributes));
      Assert.That (instantiation.MethodOnGenericType, Is.SameAs (method));

      var returnParameter = instantiation.ReturnParameter;
      Assertion.IsNotNull (returnParameter);
      Assert.That (returnParameter, Is.TypeOf<MemberParameterOnInstantiation>());
      Assert.That (returnParameter.Member, Is.SameAs (instantiation));
      Assert.That (returnParameter.As<MemberParameterOnInstantiation>().MemberParameterOnGenericDefinition, Is.SameAs (method.ReturnParameter));

      var memberParameter = instantiation.GetParameters().Single();
      Assert.That (memberParameter, Is.TypeOf<MemberParameterOnInstantiation>());
      Assert.That (memberParameter.Member, Is.SameAs (instantiation));
      Assert.That (memberParameter.As<MemberParameterOnInstantiation>().MemberParameterOnGenericDefinition, Is.SameAs (parameter));
    }

    [Test]
    public void GetCustomAttributeData ()
    {
      var customAttributes = new[] { CustomAttributeDeclarationObjectMother.Create() };
      var method = CustomMethodInfoObjectMother.Create (customAttributes: customAttributes);
      var methodInstantiation = new MethodOnTypeInstantiation (_declaringType, method);

      Assert.That (methodInstantiation.GetCustomAttributeData(), Is.EqualTo (customAttributes));
    }
  }
}