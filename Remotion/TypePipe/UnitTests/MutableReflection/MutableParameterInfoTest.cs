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
    private MemberInfo _declaringMember;
    private UnderlyingParameterInfoDescriptor _descriptor;
    private MutableParameterInfo _mutableParameter;

    [SetUp]
    public void SetUp ()
    {
      _declaringMember = ReflectionObjectMother.GetSomeMember();
      _descriptor = UnderlyingParameterInfoDescriptorObjectMother.CreateForNew();
      _mutableParameter = new MutableParameterInfo (_declaringMember, 0, _descriptor);
    }

    [Test]
    public void Initialization ()
    {
      var member = ReflectionObjectMother.GetSomeMember ();
      var position = 4711;

      var mutableParameter = new MutableParameterInfo (member, position, _descriptor);

      Assert.That (mutableParameter.Member, Is.SameAs (member));
      Assert.That (mutableParameter.Position, Is.EqualTo (position));
    }

    [Test]
    public void UnderlyingSystemParameterInfo ()
    {
      var descriptor = UnderlyingParameterInfoDescriptorObjectMother.CreateForExisting();
      Assert.That (descriptor.UnderlyingSystemInfo, Is.Not.Null);

      var mutableParameter = Create (descriptor);

      Assert.That (mutableParameter.UnderlyingSystemParameterInfo, Is.SameAs (descriptor.UnderlyingSystemInfo));
    }

    [Test]
    public void UnderlyingSystemParameterInfo_ForNull ()
    {
      var descriptor = UnderlyingParameterInfoDescriptorObjectMother.CreateForNew ();
      Assert.That (descriptor.UnderlyingSystemInfo, Is.Null);

      var mutableParameter = Create (descriptor);

      Assert.That (mutableParameter.UnderlyingSystemParameterInfo, Is.SameAs (mutableParameter));
    }

    [Test]
    public void ParameterType ()
    {
      Assert.That (_mutableParameter.ParameterType, Is.SameAs (_descriptor.Type));
    }

    [Test]
    public void Name ()
    {
      Assert.That (_mutableParameter.Name, Is.SameAs (_descriptor.Name));
    }

    [Test]
    public void Attributes ()
    {
      Assert.That (_mutableParameter.Attributes, Is.EqualTo (_descriptor.Attributes));
    }

    [Test]
    public void GetCustomAttributeData ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((MutableParameterInfoTest obj) => obj.Method (""));
      var parameter = method.GetParameters().Single();
      var mutableParameter = MutableParameterInfoObjectMother.CreateForExisting (parameter);

      var result = mutableParameter.GetCustomAttributeData ();

      Assert.That (result.Select (a => a.Type), Is.EquivalentTo (new[] { typeof (AbcAttribute) }));
    }

    [Test]
    public void GetCustomAttributeData_Lazy ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((MutableParameterInfoTest obj) => obj.Method (""));
      var parameter = method.GetParameters ().Single ();
      var mutableParameter = MutableParameterInfoObjectMother.CreateForExisting (parameter);

      var result1 = mutableParameter.GetCustomAttributeData ();
      var result2 = mutableParameter.GetCustomAttributeData ();

      Assert.That (result1, Is.SameAs (result2));
    }

    private MutableParameterInfo Create (UnderlyingParameterInfoDescriptor descriptor)
    {
      return new MutableParameterInfo (ReflectionObjectMother.GetSomeMember (), 0, descriptor);
    }

// ReSharper disable UnusedParameter.Local
    private void Method ([Abc] string parameter) { }
// ReSharper restore UnusedParameter.Local

    public class AbcAttribute : Attribute { }
  }
}