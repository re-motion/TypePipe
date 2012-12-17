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
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class TypePipeCustomAttributeImplementationUtilityTest
  {
    private MemberInfo _member;

    [SetUp]
    public void SetUp ()
    {
      _member = NormalizingMemberInfoFromExpressionUtility.GetMember ((DomainType obj) => obj.Member (7));
    }

    [Test]
    public void GetCustomAttributes ()
    {
      Assert.That (TypePipeCustomAttributeImplementationUtility.GetCustomAttributes (_member, inherit: false), Is.Empty);
      var result = TypePipeCustomAttributeImplementationUtility.GetCustomAttributes (_member, inherit: true);

      Assert.That (result, Has.Length.EqualTo (1));
      var attribute = result.Single();
      Assert.That (attribute, Is.TypeOf<DerivedAttribute>());
    }

    [Test]
    public void GetCustomAttributes_NewInstance ()
    {
      var attribute1 = TypePipeCustomAttributeImplementationUtility.GetCustomAttributes (_member, true).Single();
      var attribute2 = TypePipeCustomAttributeImplementationUtility.GetCustomAttributes (_member, true).Single();

      Assert.That (attribute1, Is.Not.SameAs (attribute2));
    }

    [Test]
    public void GetCustomAttributes_Filter ()
    {
      Assert.That (TypePipeCustomAttributeImplementationUtility.GetCustomAttributes (_member, typeof (UnrelatedAttribute), true), Is.Empty);
      Assert.That (TypePipeCustomAttributeImplementationUtility.GetCustomAttributes (_member, typeof (DerivedAttribute), true), Has.Length.EqualTo (1));
      Assert.That (TypePipeCustomAttributeImplementationUtility.GetCustomAttributes (_member, typeof (BaseAttribute), true), Has.Length.EqualTo (1));
      Assert.That (TypePipeCustomAttributeImplementationUtility.GetCustomAttributes (_member, typeof (IBaseAttributeInterface), true), Has.Length.EqualTo (1));
    }

    [Test]
    public void GetCustomAttributes_ArrayType ()
    {
      // Standard reflection. Use as reference behavior.
      Assert.That (_member.GetCustomAttributes (false), Is.TypeOf (typeof (object[])));
      Assert.That (_member.GetCustomAttributes (typeof (BaseAttribute), false), Is.TypeOf (typeof (BaseAttribute[])));

      Assert.That (TypePipeCustomAttributeImplementationUtility.GetCustomAttributes (_member, true), Is.TypeOf (typeof (object[])));
      Assert.That (TypePipeCustomAttributeImplementationUtility.GetCustomAttributes (_member, typeof (BaseAttribute), true), Is.TypeOf (typeof (BaseAttribute[])));
    }

    [Test]
    public void IsDefined ()
    {
      Assert.That (TypePipeCustomAttributeImplementationUtility.IsDefined (_member, typeof (UnrelatedAttribute), true), Is.False);
      Assert.That (TypePipeCustomAttributeImplementationUtility.IsDefined (_member, typeof (DerivedAttribute), true), Is.True);
      Assert.That (TypePipeCustomAttributeImplementationUtility.IsDefined (_member, typeof (BaseAttribute), true), Is.True);
      Assert.That (TypePipeCustomAttributeImplementationUtility.IsDefined (_member, typeof (IBaseAttributeInterface), true), Is.True);
    }

    public class DomainTypeBase
    {
      [Derived]
      public virtual void Member (int arg) { Dev.Null = arg; }
    }

    public class DomainType : DomainTypeBase
    {
      public override void Member ([Derived] int arg) { Dev.Null = arg; }
    }

    interface IBaseAttributeInterface { }
    class BaseAttribute : Attribute, IBaseAttributeInterface { }
    class DerivedAttribute : BaseAttribute { }
    class UnrelatedAttribute : Attribute { }
  }
}