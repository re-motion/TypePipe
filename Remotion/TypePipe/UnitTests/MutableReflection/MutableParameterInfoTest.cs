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
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableParameterInfoTest
  {
    private MemberInfo _declaringMember;
    private ParameterDescriptor _descriptor;
    private MutableParameterInfo _mutableParameter;

    private MutableParameterInfo _mutableParameterWithAttribute;
    private bool _randomInherit;

    [SetUp]
    public void SetUp ()
    {
      _declaringMember = ReflectionObjectMother.GetSomeMember();
      _descriptor = ParameterDescriptorObjectMother.CreateForNew();
      _mutableParameter = new MutableParameterInfo (_declaringMember, _descriptor);

      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((MutableParameterInfoTest obj) => obj.Method (""));
      var parameter = method.GetParameters().Single();
      _mutableParameterWithAttribute = MutableParameterInfoObjectMother.CreateForExisting (originalParameter: parameter);
      _randomInherit = BooleanObjectMother.GetRandomBoolean ();
    }

    [Test]
    public void Initialization ()
    {
      var member = ReflectionObjectMother.GetSomeMember ();

      var mutableParameter = new MutableParameterInfo (member, _descriptor);

      Assert.That (mutableParameter.Member, Is.SameAs (member));
    }

    [Test]
    public void UnderlyingSystemParameterInfo ()
    {
      var descriptor = ParameterDescriptorObjectMother.CreateForExisting();
      Assert.That (descriptor.UnderlyingSystemInfo, Is.Not.Null);

      var mutableParameter = Create (descriptor);

      Assert.That (mutableParameter.UnderlyingSystemParameterInfo, Is.SameAs (descriptor.UnderlyingSystemInfo));
    }

    [Test]
    public void UnderlyingSystemParameterInfo_ForNull ()
    {
      var descriptor = ParameterDescriptorObjectMother.CreateForNew ();
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
      Assert.That (_mutableParameter.Name, Is.EqualTo (_descriptor.Name));
    }

    [Test]
    public void Position ()
    {
      Assert.That (_mutableParameter.Position, Is.EqualTo (_descriptor.Position));
    }

    [Test]
    public void Attributes ()
    {
      Assert.That (_mutableParameter.Attributes, Is.EqualTo (_descriptor.Attributes));
    }

    [Test]
    public void GetCustomAttributeData ()
    {
      var result = _mutableParameterWithAttribute.GetCustomAttributeData ();

      Assert.That (result.Select (a => a.Constructor.DeclaringType), Is.EquivalentTo (new[] { typeof (DerivedAttribute) }));
      Assert.That (result, Is.SameAs (_mutableParameterWithAttribute.GetCustomAttributeData ()), "should be cached");
    }

    [Test]
    public void GetCustomAttributes ()
    {
      var result = _mutableParameterWithAttribute.GetCustomAttributes (_randomInherit);

      Assert.That (result, Has.Length.EqualTo (1));
      var attribute = result.Single ();
      Assert.That (attribute, Is.TypeOf<DerivedAttribute> ());
      Assert.That (_mutableParameterWithAttribute.GetCustomAttributes (_randomInherit).Single (), Is.Not.SameAs (attribute), "new instance");
    }

    [Test]
    public void GetCustomAttributes_Filter ()
    {
      Assert.That (_mutableParameterWithAttribute.GetCustomAttributes (typeof (UnrelatedAttribute), _randomInherit), Is.Empty);
      Assert.That (_mutableParameterWithAttribute.GetCustomAttributes (typeof (BaseAttribute), _randomInherit), Has.Length.EqualTo (1));
    }

    [Test]
    public void IsDefined ()
    {
      Assert.That (_mutableParameterWithAttribute.IsDefined (typeof (UnrelatedAttribute), _randomInherit), Is.False);
      Assert.That (_mutableParameterWithAttribute.IsDefined (typeof (BaseAttribute), _randomInherit), Is.True);
    }

    private MutableParameterInfo Create (ParameterDescriptor descriptor)
    {
      return new MutableParameterInfo (ReflectionObjectMother.GetSomeMember (), descriptor);
    }

// ReSharper disable UnusedParameter.Local
    private void Method ([Derived] string parameter) { }
// ReSharper restore UnusedParameter.Local

    class BaseAttribute : Attribute { }
    class DerivedAttribute : BaseAttribute { }
    class UnrelatedAttribute : Attribute { }
  }
}