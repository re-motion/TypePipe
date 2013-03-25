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
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class ConstructorOnCustomTypeTest
  {
    private CustomType _declaringType;
    private MethodAttributes _attributes;
    private ParameterDeclaration[] _parameters;

    private ConstructorOnCustomType _constructor;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = CustomTypeObjectMother.Create();
      _attributes = (MethodAttributes) 7 | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
      _parameters = ParameterDeclarationObjectMother.CreateMultiple (2);

      _constructor = new ConstructorOnCustomType (_declaringType, _attributes, _parameters);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_constructor.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (_constructor.Attributes, Is.EqualTo (_attributes));
      var parameters = _constructor.GetParameters();
      Assert.That (parameters, Has.Length.EqualTo (2));
      CustomParameterInfoTest.CheckParameter (parameters[0], _constructor, 0, _parameters[0].Name, _parameters[0].Type, _parameters[0].Attributes);
      CustomParameterInfoTest.CheckParameter (parameters[1], _constructor, 1, _parameters[1].Name, _parameters[1].Type, _parameters[1].Attributes);
    }

    [Test]
    public void GetCustomAttributeData ()
    {
      Assert.That (_constructor.GetCustomAttributeData(), Is.Empty);
    }
  }
}