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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class MutableTypeMethodCollectionTest
  {
    private MutableType _declaringType;

    private MutableTypeMethodCollection _collection;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = MutableTypeObjectMother.CreateForExisting (typeof (DomainType));
      Func<MethodInfo, MutableMethodInfo> mutableMemberProvider = MutableMethodInfoObjectMother.CreateForExisting;

      _collection = new MutableTypeMethodCollection (_declaringType, typeof (DomainType).GetMethods(), mutableMemberProvider);
    }

    [Test]
    public void GetEnumerator_FiltersOverriddenBaseMembers ()
    {
      var overriddenMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.BaseMethod());
      Assert.That (_collection.ToArray (), Has.Member (overriddenMethod));
      
      var overridingMethod = MutableMethodInfoObjectMother.CreateForNew (_declaringType, baseMethod: overriddenMethod);
      _collection.Add (overridingMethod);

      var enumeratedMethods = _collection.ToArray();

      Assert.That (enumeratedMethods, Has.Member (overridingMethod));
      Assert.That (enumeratedMethods, Has.No.Member (overriddenMethod));
    }

    [Test]
    public void GetMutableMember_StandardMemberInfo_BaseDeclaringType ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.BaseMethod());

      var result = _collection.GetMutableMember (method);

      Assert.That (result, Is.Null);
    }

    public class DomainTypeBase
    {
      public virtual void BaseMethod () { }
    }

    public class DomainType : DomainTypeBase { }
  }
}