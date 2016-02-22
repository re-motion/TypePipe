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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class ParameterOnCustomMemberTest
  {
    private MemberInfo _declaringMember;
    private int _position;
    private string _name;
    private Type _type;
    private ParameterAttributes _attributes;

    private ParameterOnCustomMember _parameter;

    [SetUp]
    public void SetUp ()
    {
      _declaringMember = ReflectionObjectMother.GetSomeMember();
      _position = 7;
      _name = "Parameter name";
      _type = ReflectionObjectMother.GetSomeType();
      _attributes = (ParameterAttributes) 7;

      _parameter = new ParameterOnCustomMember (_declaringMember, _position, _name, _type, _attributes);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_parameter.Member, Is.SameAs (_declaringMember));
      Assert.That (_parameter.Position, Is.EqualTo (_position));
      Assert.That (_parameter.Name, Is.EqualTo (_name));
      Assert.That (_parameter.ParameterType, Is.SameAs (_type));
      Assert.That (_parameter.Attributes, Is.EqualTo (_attributes));
    }

    [Test]
    public void GetCustomAttributeData ()
    {
      Assert.That (_parameter.GetCustomAttributeData(), Is.Empty);
    }
  }
}