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
using JetBrains.Annotations;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Implementation;

namespace Remotion.TypePipe.UnitTests.Implementation
{
  [TestFixture]
  public class ConstructorFinderTest
  {
    private ConstructorFinder _finder;

    private Type _requestedType;
    private Type _assembledType;

    [SetUp]
    public void SetUp ()
    {
      _finder = new ConstructorFinder();

      _requestedType = typeof (RequestedType);
      _assembledType = typeof (AssembledType);
    }

    [Test]
    public void GetConstructor ()
    {
      var parameterTypes = new[] { typeof (string), typeof (int) };

      var result = _finder.GetConstructor (_requestedType, parameterTypes, false, _assembledType);

      var expectedConstructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new AssembledType ("", 0));
      Assert.That (result, Is.EqualTo (expectedConstructor));
    }

    [Test]
    public void GetConstructor_NonPublicOnRequestedType_PublicOnAssembledType ()
    {
      var parameterTypes = Type.EmptyTypes;
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new RequestedType());
      Assert.That (constructor.IsPublic, Is.False);

      var result = _finder.GetConstructor (_requestedType, parameterTypes, true, _assembledType);

      var expectedConstructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new AssembledType());
      Assert.That (result, Is.EqualTo (expectedConstructor));
    }

    [Test]
    public void GetConstructor_NonPublicOnRequestedType_AssembledTypeIsRequestedType ()
    {
      var parameterTypes = Type.EmptyTypes;
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new RequestedType ());
      Assert.That (constructor.IsPublic, Is.False);

      var result = _finder.GetConstructor (_requestedType, parameterTypes, true, _requestedType);

      var expectedConstructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new RequestedType ());
      Assert.That (result, Is.EqualTo (expectedConstructor));
    }

    [Test]
    [ExpectedException (typeof (MissingMethodException), ExpectedMessage =
        "Type 'Remotion.TypePipe.UnitTests.Implementation.ConstructorFinderTest+RequestedType' does not contain a constructor with the following "
        + "signature: (IDisposable, String).")]
    public void GetConstructor_ThrowsForMissingMember ()
    {
      _finder.GetConstructor (_requestedType, new[] { typeof (IDisposable), typeof (string) }, false, _assembledType);
    }

    [Test]
    [ExpectedException (typeof (MissingMethodException), ExpectedMessage =
        "Type 'Remotion.TypePipe.UnitTests.Implementation.ConstructorFinderTest+RequestedType' contains a constructor with the required signature, "
        + "but it is not public (and the allowNonPublic flag is not set).")]
    public void GetConstructor_ThrowsForNonPublic ()
    {
      _finder.GetConstructor (_requestedType, Type.EmptyTypes, false, _assembledType);
    }

    [Test]
    public void GetConstructor_AbstractAssembledType ()
    {
      var parameterTypes = Type.EmptyTypes;

      Assert.That (
          () => _finder.GetConstructor (_requestedType, parameterTypes, true, typeof (AbstractAssembledType)),
          Throws.InvalidOperationException.With.Message.EqualTo (
              "The type 'Remotion.TypePipe.UnitTests.Implementation.ConstructorFinderTest+RequestedType' cannot be constructed because the "
              + "assembled type is abstract."));
    }

    private class RequestedType
    {
      [UsedImplicitly]
      public RequestedType (string s1, int i2)
      {
        Dev.Null = s1;
        Dev.Null = i2;
      }

      internal RequestedType () {}
    }

    class AssembledType : RequestedType
    {
      public AssembledType (string s1, int i2) : base (s1, i2) {}
      public AssembledType () {}
    }

    abstract class AbstractAssembledType : RequestedType {}
  }
}