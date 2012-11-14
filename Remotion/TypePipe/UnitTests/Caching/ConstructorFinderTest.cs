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
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;

namespace Remotion.TypePipe.UnitTests.Caching
{
  [TestFixture]
  public class ConstructorFinderTest
  {
    private ConstructorFinder _finder;

    [SetUp]
    public void SetUp ()
    {
      _finder = new ConstructorFinder();      
    }

    [Test]
    public void GetConstructor ()
    {
      var generatedType = typeof (GeneratedType);
      var generatedParameterTypes = new[] { typeof (string), typeof (int) };
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new GeneratedType ("", 0));

      var result = _finder.GetConstructor (generatedType, generatedParameterTypes, false, typeof (object), Type.EmptyTypes);

      Assert.That (result, Is.EqualTo (constructor));
    }

    [Test]
    public void GetConstructor_NonPublic ()
    {
      var generatedType = typeof (GeneratedType);
      var generatedParameterTypes = Type.EmptyTypes;
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new GeneratedType ());
      Assert.That (constructor.IsPublic, Is.False);

      var result = _finder.GetConstructor (generatedType, generatedParameterTypes, true, typeof (object), Type.EmptyTypes);

      Assert.That (result, Is.EqualTo (constructor));
    }

    [Test]
    [ExpectedException (typeof (MissingMethodException), ExpectedMessage =
        "Type Remotion.TypePipe.UnitTests.Caching.ConstructorFinderTest+RequestedType does not contain a constructor with the following signature: "
        + "(IDisposable, String).")]
    public void GetConstructor_ThrowsForMissingMember ()
    {
      var generatedType = typeof (GeneratedType);
      var generatedParameterTypes = new[] { typeof (double) };

      _finder.GetConstructor (generatedType, generatedParameterTypes, false, typeof (RequestedType), new[] { typeof (IDisposable), typeof (string) });
    }

    [Test]
    [ExpectedException (typeof (MissingMethodException), ExpectedMessage =
        "Type Remotion.TypePipe.UnitTests.Caching.ConstructorFinderTest+RequestedType contains a constructor with the required signature, but "
        + "it is not public (and the allowNonPublic flag is not set).")]
    public void GetConstructor_ThrowsForNonPublic ()
    {
      var generatedType = typeof (GeneratedType);
      var generatedParameterTypes = Type.EmptyTypes;

      _finder.GetConstructor (generatedType, generatedParameterTypes, false, typeof (RequestedType), Type.EmptyTypes);
    }

    class RequestedType { }

    class GeneratedType
    {
      public GeneratedType (string s1, int i2)
      {
        Dev.Null = s1;
        Dev.Null = i2;
      }

      internal GeneratedType () { }
    }
  }
}