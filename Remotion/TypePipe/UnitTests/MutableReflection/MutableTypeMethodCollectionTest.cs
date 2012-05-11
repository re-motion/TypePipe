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
using Remotion.TypePipe.MutableReflection;
using System.Linq;
using Remotion.Utilities;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableTypeMethodCollectionTest
  {
    private MutableType _declaringType;

    private MutableTypeMethodCollection _collection;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = MutableTypeObjectMother.CreateForExistingType (typeof (DerivedType));
      Func<MethodInfo, MutableMethodInfo> mutableMemberProvider = mi => MutableMethodInfoObjectMother.CreateForExisting (_declaringType, mi);

      _collection = new MutableTypeMethodCollection (_declaringType, typeof (DerivedType).GetMethods (), mutableMemberProvider);
    }

    [Test]
    public void GetEnumerator_FiltersOverriddenBaseMembers ()
    {
      var overriddenMethod = MemberInfoFromExpressionUtility.GetMethod ((DerivedType obj) => obj.OverriddenMethod());
      Assert.That (_collection.ToArray (), Has.Member (overriddenMethod));
      
      var overridingMethod = MutableMethodInfoObjectMother.CreateForNew (_declaringType, baseMethod: overriddenMethod);
      _collection.Add (overridingMethod);

      var enumeratedMethods = _collection.ToArray();

      Assert.That (enumeratedMethods, Has.Member (overridingMethod));
      Assert.That (enumeratedMethods, Has.No.Member (overriddenMethod));
    }

    public class DomainType
    {
      public virtual void OverriddenMethod () { }
    }

    public class DerivedType : DomainType
    { 
    }
  }
}