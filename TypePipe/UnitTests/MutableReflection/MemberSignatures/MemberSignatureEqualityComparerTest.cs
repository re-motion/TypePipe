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
using Remotion.TypePipe.MutableReflection.MemberSignatures;
using Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures.TestDomain;

namespace Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures
{
  [TestFixture]
  public class MemberSignatureEqualityComparerTest
  {
    private MemberSignatureEqualityComparer _comparer;

    private ConstructorInfo _c1;
    private ConstructorInfo _c2;
    private ConstructorInfo _c3;

    private MethodInfo _m1;
    private MethodInfo _m2;
    private MethodInfo _m3;

    private PropertyInfo _p1;
    private PropertyInfo _p2;
    private PropertyInfo _p3;

    private EventInfo _e1;
    private EventInfo _e2;
    private EventInfo _e3;

    private FieldInfo _f1;
    private FieldInfo _f2;
    private FieldInfo _f3;


    [SetUp]
    public void SetUp ()
    {
      _comparer = new MemberSignatureEqualityComparer ();

      _c1 = typeof (ClassForSignatureComparisons).GetConstructor (Type.EmptyTypes);
      _c2 = typeof (ClassForSignatureComparisons2).GetConstructor (Type.EmptyTypes);
      _c3 = typeof (ClassForSignatureComparisons).GetConstructor (new[] { typeof (int) });

      _m1 = typeof (ClassForSignatureComparisons).GetMethod ("M1");
      _m2 = typeof (ClassForSignatureComparisons).GetMethod ("M2");
      _m3 = typeof (ClassForSignatureComparisons).GetMethod ("M3");

      _p1 = typeof (ClassForSignatureComparisons).GetProperty ("P1");
      _p2 = typeof (ClassForSignatureComparisons).GetProperty ("P2");
      _p3 = typeof (ClassForSignatureComparisons).GetProperty ("P3");

      _e1 = typeof (ClassForSignatureComparisons).GetEvent ("E1");
      _e2 = typeof (ClassForSignatureComparisons).GetEvent ("E2");
      _e3 = typeof (ClassForSignatureComparisons).GetEvent ("E3");

      _f1 = typeof (ClassForSignatureComparisons).GetField ("F1");
      _f2 = typeof (ClassForSignatureComparisons).GetField ("F2");
      _f3 = typeof (ClassForSignatureComparisons).GetField ("F3");
    }

    [Test]
    public void Equals_Constructors_True ()
    {
      Assert.That (_comparer.Equals (_c1, _c2), Is.True);
    }

    [Test]
    public void Equals_Constructors_False ()
    {
      Assert.That (_comparer.Equals (_c1, _c3), Is.False);
    }

    [Test]
    public void GetHashCode_Constructors_Equal ()
    {
      Assert.That (_comparer.GetHashCode (_c1), Is.EqualTo (_comparer.GetHashCode (_c2)));
    }

    [Test]
    public void Equals_Methods_True ()
    {
      Assert.That (_comparer.Equals (_m1, _m2), Is.True);
    }

    [Test]
    public void Equals_Methods_False ()
    {
      Assert.That (_comparer.Equals (_m1, _m3), Is.False);
    }

    [Test]
    public void GetHashCode_Methods_Equal ()
    {
      Assert.That (_comparer.GetHashCode (_m1), Is.EqualTo (_comparer.GetHashCode (_m2)));
    }

    [Test]
    public void Equals_Properties_True ()
    {
      Assert.That (_comparer.Equals (_p1, _p2), Is.True);
    }

    [Test]
    public void Equals_Properties_False ()
    {
      Assert.That (_comparer.Equals (_p1, _p3), Is.False);
    }

    [Test]
    public void GetHashCode_Properties_Equal ()
    {
      Assert.That (_comparer.GetHashCode (_p1), Is.EqualTo (_comparer.GetHashCode (_p2)));
    }

    [Test]
    public void Equals_Events_True ()
    {
      Assert.That (_comparer.Equals (_e1, _e2), Is.True);
    }

    [Test]
    public void Equals_Events_False ()
    {
      Assert.That (_comparer.Equals (_e1, _e3), Is.False);
    }

    [Test]
    public void GetHashCode_Events_Equal ()
    {
      Assert.That (_comparer.GetHashCode (_e1), Is.EqualTo (_comparer.GetHashCode (_e2)));
    }

    [Test]
    public void Equals_Fields_True ()
    {
      Assert.That (_comparer.Equals (_f1, _f2), Is.True);
    }

    [Test]
    public void Equals_Fields_False ()
    {
      Assert.That (_comparer.Equals (_f1, _f3), Is.False);
    }

    [Test]
    public void GetHashCode_Fields_Equal ()
    {
      Assert.That (_comparer.GetHashCode (_f1), Is.EqualTo (_comparer.GetHashCode (_f2)));
    }

    [Test]
    public void Equals_NonEqualMemberTypes ()
    {
      Assert.That (_comparer.Equals (_p1, _m1), Is.False);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException),
        ExpectedMessage = "MemberSignatureEqualityComparer does not support member type 'TypeInfo', "
        + "only constructors, methods, properties, events and fields are supported.")]
    public void Equals_InvalidMemberType ()
    {
      _comparer.Equals (_m1, typeof (object));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException),
        ExpectedMessage = "MemberSignatureEqualityComparer does not support member type 'TypeInfo', "
        + "only constructors, methods, properties, events and fields are supported.")]
    public void GetHashCode_InvalidMemberType ()
    {
      _comparer.GetHashCode (typeof (object));
    }
  }
}
