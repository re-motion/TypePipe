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
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class SubclassFilterUtilityTest
  {
    private const BindingFlags c_instanceMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    private const BindingFlags c_allMembers = c_instanceMembers | BindingFlags.Static;

    [Test]
    public void IsSubclassable ()
    {
      Assert.That (SubclassFilterUtility.IsSubclassable (typeof (string)), Is.False);
      Assert.That (SubclassFilterUtility.IsSubclassable (typeof (IDisposable)), Is.False);
      Assert.That (SubclassFilterUtility.IsSubclassable (typeof (TypeWithoutVisibleCtor)), Is.False);
      Assert.That (SubclassFilterUtility.IsSubclassable (typeof (object)), Is.True);
    }

    [Test]
    public void IsVisibleFromSubclass_FieldInfo ()
    {
      var allFields = typeof (TestDomain).GetFields (c_allMembers);
      Assert.That (
          GetMemberNames (allFields),
          Is.EquivalentTo (new[] { "PublicField", "ProtectedOrInternalField", "ProtectedField", "InternalField", "_privateField" }));

      var filteredFields = allFields.Where (SubclassFilterUtility.IsVisibleFromSubclass);

      Assert.That (
          GetMemberNames (filteredFields.Cast<MemberInfo> ()),
          Is.EquivalentTo (new[] { "PublicField", "ProtectedOrInternalField", "ProtectedField" }));
    }

    [Test]
    public void IsVisibleFromSubclass_ConstructorInfo ()
    {
      var instanceConstructors = typeof (TestDomain).GetConstructors (c_instanceMembers);
      Assert.That (
          GetCtorSignatures (instanceConstructors),
          Is.EquivalentTo (new[] { ".ctor()", ".ctor(Int32)", ".ctor(System.String)", ".ctor(Double)", ".ctor(Int64)" }));

      var filteredConstructors = instanceConstructors.Where (SubclassFilterUtility.IsVisibleFromSubclass);

      Assert.That (GetCtorSignatures (filteredConstructors), Is.EquivalentTo (new[] { ".ctor()", ".ctor(Int32)", ".ctor(System.String)" }));
    }

    [Test]
    public void IsVisibleFromSubclass_MethodInfo ()
    {
      var allMethods = typeof (TestDomain).GetMethods (c_allMembers);
      // Unfortunately, NUnit doesn't support Is.SupersetOf
      Assert.That (
          new[] { "PublicMethod", "ProtectedOrInternalMethod", "ProtectedMethod", "InternalMethod", "PrivateMethod" },
          Is.SubsetOf (GetMemberNames (allMethods)));

      var filteredMethods = allMethods.Where (SubclassFilterUtility.IsVisibleFromSubclass);

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

    public class TypeWithoutVisibleCtor
    {
      internal TypeWithoutVisibleCtor () {}
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