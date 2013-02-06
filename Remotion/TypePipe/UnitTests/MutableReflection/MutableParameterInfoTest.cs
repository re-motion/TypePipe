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
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableParameterInfoTest
  {
    public static void CheckParameter (
        ParameterInfo parameter,
        MemberInfo expectedMember,
        int expectedPosition,
        string expectedName,
        Type expectedType,
        ParameterAttributes expectedAttributes)
    {
      Assert.That (parameter.Member, Is.SameAs (expectedMember));
      Assert.That (parameter.Position, Is.EqualTo (expectedPosition));
      Assert.That (parameter.Name, Is.EqualTo (expectedName));
      Assert.That (parameter.ParameterType, Is.SameAs (expectedType));
      Assert.That (parameter.Attributes, Is.EqualTo (expectedAttributes));
    }

    private MutableParameterInfo _parameter;

    [SetUp]
    public void SetUp ()
    {
      _parameter = MutableParameterInfoObjectMother.Create();
    }

    [Test]
    public void Initialization ()
    {
      var member = ReflectionObjectMother.GetSomeMember();
      var position = 7;
      var name = "abc";
      var type = ReflectionObjectMother.GetSomeType();
      var attributes = (ParameterAttributes) 7;

      var parameter = new MutableParameterInfo (member, position, name, type, attributes);

      CheckParameter (parameter, member, position, name, type, attributes);
    }

    [Test]
    public void CustomAttributeMethods ()
    {
      var declaration = CustomAttributeDeclarationObjectMother.Create (typeof (ObsoleteAttribute));
      _parameter.AddCustomAttribute (declaration);

      Assert.That (_parameter.AddedCustomAttributes, Is.EqualTo (new[] { declaration }));
      Assert.That (_parameter.GetCustomAttributeData().Select (a => a.Type), Is.EquivalentTo (new[] { typeof (ObsoleteAttribute) }));
    }

    [Test]
    public void ToDebugString ()
    {
      // Node: ToDebugString is defined in CustomParameterInfo base class.
      var memberName = _parameter.Member.Name;
      var parameterType = _parameter.ParameterType.Name;
      var parameterName = _parameter.Name;
      var expected = "MutableParameter = \"" + parameterType + " " + parameterName + "\", Member = \"" + memberName + "\"";

      Assert.That (_parameter.ToDebugString(), Is.EqualTo (expected));
    }
  }
}