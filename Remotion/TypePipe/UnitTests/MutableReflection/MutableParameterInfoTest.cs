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
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Descriptors;
using Remotion.TypePipe.UnitTests.MutableReflection.Descriptors;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableParameterInfoTest
  {
    private MemberInfo _declaringMember;
    private ParameterDescriptor _descriptor;

    private MutableParameterInfo _parameter;

    [SetUp]
    public void SetUp ()
    {
      _declaringMember = ReflectionObjectMother.GetSomeMember();
      _descriptor = ParameterDescriptorObjectMother.Create();
      _parameter = new MutableParameterInfo (_declaringMember, _descriptor);
    }

    [Test]
    public void Initialization ()
    {
      var member = ReflectionObjectMother.GetSomeMember();

      var mutableParameter = new MutableParameterInfo (member, _descriptor);

      Assert.That (mutableParameter.Member, Is.SameAs (member));
    }

    [Test]
    public void UnderlyingSystemParameterInfo ()
    {
      var underlyingParameter = ReflectionObjectMother.GetSomeParameter();
      var parameter = MutableParameterInfoObjectMother.CreateForExisting (underlyingParameter: underlyingParameter);

      Assert.That (parameter.UnderlyingSystemParameterInfo, Is.SameAs (underlyingParameter));
    }

    [Test]
    public void UnderlyingSystemParameterInfo_ForNull ()
    {
      var parameter = MutableParameterInfoObjectMother.CreateForNew();

      Assert.That (parameter.UnderlyingSystemParameterInfo, Is.SameAs (parameter));
    }

    [Test]
    public void IsNew ()
    {
      var parameter1 = MutableParameterInfoObjectMother.CreateForExisting();
      var parameter2 = MutableParameterInfoObjectMother.CreateForNew();

      Assert.That (parameter1.IsNew, Is.False);
      Assert.That (parameter2.IsNew, Is.True);
    }

    [Test]
    public void IsModified ()
    {
      Assert.That (_parameter.IsModified, Is.False);
      _parameter.AddCustomAttribute (CustomAttributeDeclarationObjectMother.Create());

      Assert.That (_parameter.IsModified, Is.True);
    }

    [Test]
    public void ParameterType ()
    {
      Assert.That (_parameter.ParameterType, Is.SameAs (_descriptor.Type));
    }

    [Test]
    public void Name ()
    {
      Assert.That (_parameter.Name, Is.EqualTo (_descriptor.Name));
    }

    [Test]
    public void Position ()
    {
      Assert.That (_parameter.Position, Is.EqualTo (_descriptor.Position));
    }

    [Test]
    public void Attributes ()
    {
      Assert.That (_parameter.Attributes, Is.EqualTo (_descriptor.Attributes));
    }

    [Test]
    public void CanAddCustomAttributes ()
    {
      var parameter1 = MutableParameterInfoObjectMother.CreateForExisting();
      var parameter2 = MutableParameterInfoObjectMother.CreateForNew();

      Assert.That (parameter1.CanAddCustomAttributes, Is.False);
      Assert.That (parameter2.CanAddCustomAttributes, Is.True);
    }

    [Test]
    public new void ToString ()
    {
      var parameter = MutableParameterInfoObjectMother.Create (name: "param1", parameterType: typeof (int), attributes: ParameterAttributes.Out);

      Assert.That (parameter.ToString(), Is.EqualTo ("Int32 param1"));
    }

    [Test]
    public void ToDebugString ()
    {
      var declaringMemberName = _parameter.Member.Name;
      var parameterType = _parameter.ParameterType.Name;
      var parameterName = _parameter.Name;
      var expected = "MutableParameter = \"" + parameterType + " " + parameterName + "\", DeclaringMember = \"" + declaringMemberName + "\"";

      Assert.That (_parameter.ToDebugString(), Is.EqualTo (expected));
    }

    [Test]
    public void CustomAttributeMethods ()
    {
      var declaration = CustomAttributeDeclarationObjectMother.Create (typeof (ObsoleteAttribute));
      Assert.That (_parameter.CanAddCustomAttributes, Is.True);
      _parameter.AddCustomAttribute (declaration);

      Assert.That (_parameter.AddedCustomAttributes, Is.EqualTo (new[] { declaration }));

      Assert.That (_parameter.GetCustomAttributeData().Select (a => a.Type), Is.EquivalentTo (new[] { typeof (ObsoleteAttribute) }));

      Assert.That (_parameter.GetCustomAttributes (false).Single(), Is.TypeOf<ObsoleteAttribute>());
      Assert.That (_parameter.GetCustomAttributes (typeof (NonSerializedAttribute), false), Is.Empty);

      Assert.That (_parameter.IsDefined (typeof (ObsoleteAttribute), false), Is.True);
      Assert.That (_parameter.IsDefined (typeof (NonSerializedAttribute), false), Is.False);
    }
  }
}