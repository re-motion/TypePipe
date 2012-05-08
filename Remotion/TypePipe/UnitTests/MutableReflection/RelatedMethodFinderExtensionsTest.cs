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
using NUnit.Framework;
using Remotion.Reflection.MemberSignatures;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class RelatedMethodFinderExtensionsTest
  {
    private IRelatedMethodFinder _finderMock;

    [SetUp]
    public void SetUp ()
    {
      _finderMock = MockRepository.GenerateStrictMock<IRelatedMethodFinder>();
    }

    [Test]
    public void GetBaseMethod_MethodInfo_ModifiedDerivedTypeMethod ()
    {
      var method = MemberInfoFromExpressionUtility.GetMethodBaseDefinition ((ModifiedDomainType obj) => obj.ModifiedTypeMethod ());

      var result = RelatedMethodFinderExtensions.GetBaseMethod (_finderMock, method);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetBaseMethod_MethodInfo_OverridingMethod ()
    {
      var method = typeof (ModifiedDomainType).GetMethod ("OverridingMethod");

      var fakeResult = ReflectionObjectMother.GetSomeMethod();
      _finderMock.Expect (mock => mock.GetBaseMethod ("OverridingMethod", MethodSignature.Create (method), typeof (DomainType))).Return (fakeResult);

      var result = RelatedMethodFinderExtensions.GetBaseMethod (_finderMock, method);

      _finderMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (fakeResult));
    }

    private class DomainType
    {
      public virtual void OverridingMethod () { }
    }

    private class ModifiedDomainType : DomainType
    {
      public virtual void ModifiedTypeMethod () { }
      public override void OverridingMethod () { }
    }
  }
}