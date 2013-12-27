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
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.MemberSignatures;

namespace Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures
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