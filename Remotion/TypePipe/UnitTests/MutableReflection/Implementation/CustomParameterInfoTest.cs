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
using Remotion.Development.UnitTesting.Reflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class CustomParameterInfoTest
  {
    private MemberInfo _member;
    private int _position;
    private string _name;
    private Type _type;
    private ParameterAttributes _attributes;

    private TestableCustomParameterInfo _parameter;

    [SetUp]
    public void SetUp ()
    {
      _member = ReflectionObjectMother.GetSomeMember();
      _position = 7;
      _name = "abc";
      _type = ReflectionObjectMother.GetSomeType();
      _attributes = (ParameterAttributes) 7;

      _parameter = new TestableCustomParameterInfo (_member, _position, _name, _type, _attributes);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_parameter.Member, Is.SameAs (_member));
      Assert.That (_parameter.Position, Is.EqualTo (_position));
      Assert.That (_parameter.Name, Is.EqualTo (_name));
      Assert.That (_parameter.ParameterType, Is.SameAs (_type));
      Assert.That (_parameter.Attributes, Is.EqualTo (_attributes));
    }

    [Test]
    public void Initialization_NullName ()
    {
      var member = ReflectionObjectMother.GetSomeMember();
      var type = ReflectionObjectMother.GetSomeType();

      var parameter = new TestableCustomParameterInfo (member, 7, null, type, (ParameterAttributes) 7);

      Assert.That (parameter.Name, Is.Null);
    }

    [Test]
    public void CustomAttributeMethods ()
    {
      _parameter.CustomAttributeDatas = new[] { CustomAttributeDeclarationObjectMother.Create (typeof (ObsoleteAttribute)) };

      Assert.That (_parameter.GetCustomAttributes (false).Select (a => a.GetType()), Is.EqualTo (new[] { typeof (ObsoleteAttribute) }));
      Assert.That (_parameter.GetCustomAttributes (typeof (NonSerializedAttribute), false), Is.Empty);

      Assert.That (_parameter.IsDefined (typeof (ObsoleteAttribute), false), Is.True);
      Assert.That (_parameter.IsDefined (typeof (NonSerializedAttribute), false), Is.False);
    }

    [Test]
    public new void ToString ()
    {
      var parameter = MutableParameterInfoObjectMother.Create (name: "param1", type: typeof (int), attributes: ParameterAttributes.Out);

      Assert.That (parameter.ToString (), Is.EqualTo ("Int32 param1"));
    }

    [Test]
    public void ToDebugString ()
    {
      var memberName = _parameter.Member.Name;
      var parameterType = _parameter.ParameterType.Name;
      var parameterName = _parameter.Name;
      var expected = "TestableCustomParameter = \"" + parameterType + " " + parameterName + "\", Member = \"" + memberName + "\"";

      Assert.That (_parameter.ToDebugString (), Is.EqualTo (expected));
    }
  }
}