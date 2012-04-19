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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.MutableReflection;
using Remotion.FunctionalProgramming;
using Rhino.Mocks;
using Remotion.Development.UnitTesting.Enumerables;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableMemberCollectionTest
  {
    private MutableType _declaringType;
    private MethodInfo _excludedExistingMember;
    private MethodInfo[] _existingMembers;
    private MutableMemberCollection<MethodInfo, MutableMethodInfo> _collection;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = MutableTypeObjectMother.CreateForExistingType(typeof(object));
      var allExistingMembers = _declaringType.UnderlyingSystemType.GetMethods();
      _excludedExistingMember = allExistingMembers.First();
      _existingMembers = allExistingMembers.Skip (1).ToArray();
      Func<MethodInfo, MutableMethodInfo> mutableMemberProvider = mi => MutableMethodInfoObjectMother.CreateForExisting (_declaringType, mi);

      _collection = new MutableMemberCollection<MethodInfo, MutableMethodInfo> (_declaringType, _existingMembers.AsOneTime(), mutableMemberProvider);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_collection.Added, Is.Empty);
      Assert.That (_collection.Existing.Select (mutableMember => mutableMember.UnderlyingSystemMethodInfo), Is.EquivalentTo (_existingMembers));
    }

    [Test]
    public void GetEnumerator ()
    {
      var mutableMember = CreateMutableMember ();
      _collection.Add (mutableMember);
      Assert.That (_collection.Existing, Is.Not.Empty);
      Assert.That (_collection.Added, Has.Count.EqualTo (1));

      IEnumerable<MutableMethodInfo> enumerable = _collection;

      Assert.That (enumerable, Is.EqualTo (_collection.Existing.Concat (mutableMember)));
    }

    [Test]
    public void GetMutableMember_MutableMethodInfo ()
    {
      var mutableMember = CreateMutableMember();
      _collection.Add (mutableMember);

      var result = _collection.GetMutableMember(mutableMember);

      Assert.That (result, Is.SameAs (mutableMember));
    }

    [Test]
    public void GetMutableMember_StandardMemberInfo ()
    {
      var standardMember = _existingMembers.First();
      Assert.That (standardMember, Is.Not.AssignableTo<MutableMethodInfo> ());

      var result = _collection.GetMutableMember(standardMember);

      var expectedMutableMember = _collection.Existing.Single (mutableMember => mutableMember.UnderlyingSystemMethodInfo == standardMember);
      Assert.That (result, Is.SameAs (expectedMutableMember));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "The given MethodInfo cannot be modified.")]
    public void GetMutableMember_StandardMemberInfo_NoMatch ()
    {
      Assert.That (_excludedExistingMember, Is.Not.AssignableTo<MutableMethodInfo> ());
      Dev.Null = _collection.GetMutableMember(_excludedExistingMember);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "MethodInfo is declared by a different type: 'System.String'.\r\nParameter name: member")]
    public void GetMutableMember_NonEquivalentDeclaringType ()
    {
      var memberStub = MockRepository.GenerateStub<MethodInfo> ();
      memberStub.Stub (stub => stub.DeclaringType).Return (typeof (string));

      Dev.Null = _collection.GetMutableMember(memberStub);
    }

    [Test]
    public void Add ()
    {
      var mutableMember = CreateMutableMember ();

      _collection.Add (mutableMember);

      Assert.That (_collection.Added, Is.EqualTo (new[] { mutableMember }));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "MethodInfo is declared by a different type: 'Remotion.TypePipe.UnitTests.MutableReflection.MutableMemberCollectionTest'.\r\n"
        + "Parameter name: mutableMember")]
    public void Add_NonEquivalentDeclaringType ()
    {
      var declaringType = MutableTypeObjectMother.CreateForExistingType (typeof (MutableMemberCollectionTest));
      Assert.That (_declaringType.IsEquivalentTo (declaringType), Is.False);
      var mutableMember = MutableMethodInfoObjectMother.CreateForExisting (declaringType);

      _collection.Add (mutableMember);
    }

    private MutableMethodInfo CreateMutableMember ()
    {
      return MutableMethodInfoObjectMother.Create (declaringType: _declaringType);
    }
  }
}