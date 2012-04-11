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
using MemberFilter = Remotion.TypePipe.MutableReflection.ReflectionEmit.MemberFilter;

namespace Remotion.TypePipe.UnitTests.MutableReflection.ReflectionEmit
{
  [TestFixture]
  public class MemberFilterTest
  {
    private const BindingFlags c_allButStaticMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    private const BindingFlags c_allMembers = c_allButStaticMembers | BindingFlags.Static;

    private MemberFilter _memberFilter;

    [SetUp]
    public void SetUp ()
    {
      _memberFilter = new MemberFilter();
    }

    [Test]
    public void FilterFields ()
    {
      var allFields = typeof (TestDomain).GetFields (c_allMembers);
      Assert.That (
          GetMemberNames (allFields),
          Is.EquivalentTo (new[] { "PublicField", "ProtectedOrInternalField", "ProtectedField", "InternalField", "_privateField" }));

      var filteredFields = _memberFilter.FilterFields(allFields);

      Assert.That (
          GetMemberNames (filteredFields.Cast<MemberInfo> ()),
          Is.EquivalentTo (new[] { "PublicField", "ProtectedOrInternalField", "ProtectedField" }));
    }

    [Test]
    public void FilterConstructors ()
    {
      var constructors = typeof (TestDomain).GetConstructors (c_allButStaticMembers);
      Assert.That (
          GetCtorSignatures (constructors),
          Is.EquivalentTo (new[] { ".ctor()", ".ctor(Int32)", ".ctor(System.String)", ".ctor(Double)", ".ctor(Int64)" }));

      var filteredConstructors = _memberFilter.FilterConstructors (constructors);

      Assert.That (GetCtorSignatures (filteredConstructors), Is.EquivalentTo (new[] { ".ctor()", ".ctor(Int32)", ".ctor(System.String)" }));
    }

    [Test]
    public void FilterMethods ()
    {
      var allMethods = typeof (TestDomain).GetMethods (c_allMembers);
      // Unfortunately, NUnit doesn't support Is.SupersetOf
      Assert.That (
          new[] { "PublicMethod", "ProtectedOrInternalMethod", "ProtectedMethod", "InternalMethod", "PrivateMethod" },
          Is.SubsetOf (GetMemberNames (allMethods)));

      var filteredMethods = _memberFilter.FilterMethods (allMethods);

      Assert.That (
          new[] { "PublicMethod", "ProtectedOrInternalMethod", "ProtectedMethod" },
          Is.SubsetOf (GetMemberNames (filteredMethods.Cast<MemberInfo>())));
    }

    private IEnumerable<string> GetMemberNames (IEnumerable<MemberInfo> memberInfo)
    {
      return memberInfo.Select (m => m.Name);
    }

    private IEnumerable<string> GetCtorSignatures (IEnumerable<ConstructorInfo> ctorInfos)
    {
      return ctorInfos.Select (ctor => ctor.ToString().Replace ("Void ", ""));
    }

    public class TestDomain
    {
      public int PublicField;
      protected internal int ProtectedOrInternalField;
      protected int ProtectedField;
      internal int InternalField = 0;
      private int _privateField = 0;

      public TestDomain () { Dev.Null = _privateField; }
      protected internal TestDomain (int i) { Dev.Null = i; }
      protected TestDomain (string s) { Dev.Null = s; }
      internal TestDomain (double d) { Dev.Null = d; }
// ReSharper disable UnusedMember.Local
      private TestDomain (long l) { Dev.Null = l; }
// ReSharper restore UnusedMember.Local

      public void PublicMethod () { }
      protected internal void ProtectedOrInternalMethod () { }
      protected void ProtectedMethod () { }
      internal void InternalMethod () { }
// ReSharper disable UnusedMember.Local
      private void PrivateMethod () { }
// ReSharper restore UnusedMember.Local
    }
  }
}