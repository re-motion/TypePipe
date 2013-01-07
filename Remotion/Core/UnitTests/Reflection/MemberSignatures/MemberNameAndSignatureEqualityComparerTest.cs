// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.Reflection.MemberSignatures;

namespace Remotion.UnitTests.Reflection.MemberSignatures
{
  [TestFixture]
  public class MemberNameAndSignatureEqualityComparerTest
  {
    private MemberNameAndSignatureEqualityComparer _comparer;

    private MemberInfo _member;
    private MemberInfo _memberDifferentSignature;
    private MemberInfo _memberDifferentName;
    private MemberInfo _memberSameNameAndSignature;

    [SetUp]
    public void SetUp ()
    {
      _comparer = new MemberNameAndSignatureEqualityComparer();

      _member = NormalizingMemberInfoFromExpressionUtility.GetMember (() => Member (""));
      _memberDifferentSignature = NormalizingMemberInfoFromExpressionUtility.GetMember (() => Member (7));
      _memberDifferentName = NormalizingMemberInfoFromExpressionUtility.GetMember (() => OtherMember (""));
      _memberSameNameAndSignature = NormalizingMemberInfoFromExpressionUtility.GetMember ((ISomeInterface obj) => obj.Member (""));
    }

    [Test]
    public void Equals ()
    {
      Assert.That (_comparer.Equals (_member, _memberDifferentSignature), Is.False);
      Assert.That (_comparer.Equals (_member, _memberDifferentName), Is.False);
      Assert.That (_comparer.Equals (_member, _memberSameNameAndSignature), Is.True);
    }

    [Test]
    public new void GetHashCode ()
    {
      Assert.That (_comparer.GetHashCode (_member), Is.EqualTo (_comparer.GetHashCode (_memberSameNameAndSignature)));
    }

    void Member (string s) { Dev.Null = s; }
    void Member (int l) { Dev.Null = l; }
    void OtherMember (string s) { Dev.Null = s; }

    interface ISomeInterface
    {
      void Member (string s);
    }
  }
}