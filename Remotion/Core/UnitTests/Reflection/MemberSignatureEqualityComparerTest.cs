// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Reflection;
using Remotion.Reflection.SignatureStringBuilding;
using Remotion.UnitTests.Reflection.TestDomain;

namespace Remotion.UnitTests.Reflection
{
  [TestFixture]
  public class MemberSignatureEqualityComparerTest
  {
    private MemberSignatureEqualityComparer _comparer;

    private MethodInfo _m1;
    private MethodInfo _m2;
    private MethodInfo _m3;

    private PropertyInfo _p1;
    private PropertyInfo _p2;
    private PropertyInfo _p3;

    private EventInfo _e1;
    private EventInfo _e2;
    private EventInfo _e3;

    [SetUp]
    public void SetUp ()
    {
      _comparer = new MemberSignatureEqualityComparer ();

      _m1 = typeof (ClassForSignatureComparisons).GetMethod ("M1");
      _m2 = typeof (ClassForSignatureComparisons).GetMethod ("M2");
      _m3 = typeof (ClassForSignatureComparisons).GetMethod ("M3");

      _p1 = typeof (ClassForSignatureComparisons).GetProperty ("P1");
      _p2 = typeof (ClassForSignatureComparisons).GetProperty ("P2");
      _p3 = typeof (ClassForSignatureComparisons).GetProperty ("P3");

      _e1 = typeof (ClassForSignatureComparisons).GetEvent ("E1");
      _e2 = typeof (ClassForSignatureComparisons).GetEvent ("E2");
      _e3 = typeof (ClassForSignatureComparisons).GetEvent ("E3");
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
    public void Equals_NonEqualMemberTypes ()
    {
      Assert.That (new PropertySignatureStringBuilder ().BuildSignatureString (_p1), 
          Is.EqualTo (new MethodSignatureStringBuilder ().BuildSignatureString (_m1)));
      
      Assert.That (_comparer.Equals (_p1, _m1), Is.False);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException),
        ExpectedMessage = "MemberSignatureEqualityComparer does not support member type 'TypeInfo', only methods, properties, and events are supported.")]
    public void Equals_InvalidMemberType ()
    {
      _comparer.Equals (_m1, typeof (object));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException),
        ExpectedMessage = "MemberSignatureEqualityComparer does not support member type 'TypeInfo', only methods, properties, and events are supported.")]
    public void GetHashCode_InvalidMemberType ()
    {
      _comparer.GetHashCode (typeof (object));
    }
  }
}
