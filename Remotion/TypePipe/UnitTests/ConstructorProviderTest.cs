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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.UnitTests
{
  [TestFixture]
  public class ConstructorProviderTest
  {
    private ConstructorProvider _provider;

    [SetUp]
    public void SetUp ()
    {
      _provider = new ConstructorProvider();
    }

    [Test]
    public void GetConstructor ()
    {
      var generatedType = typeof (GeneratedType);
      var generatedParameterTypes = new[] { typeof (string), typeof (int) };
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new GeneratedType ("", 0));

      var result = _provider.GetConstructor (generatedType, generatedParameterTypes, false, typeof (object), Type.EmptyTypes);

      Assert.That (result, Is.EqualTo (constructor));
    }

    [Test]
    public void GetConstructor_NonPublic ()
    {
      var generatedType = typeof (GeneratedType);
      var generatedParameterTypes = Type.EmptyTypes;
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new GeneratedType ());
      Assert.That (constructor.IsPublic, Is.False);

      var result = _provider.GetConstructor (generatedType, generatedParameterTypes, true, typeof (object), Type.EmptyTypes);

      Assert.That (result, Is.EqualTo (constructor));
    }

    [Test]
    [ExpectedException (typeof (MissingMethodException), ExpectedMessage =
        "Type Remotion.TypePipe.UnitTests.ConstructorProviderTest+RequestedType does not contain a constructor with the following signature: "
        + "(IDisposable, String).")]
    public void GetConstructor_ThrowsForMissingMember ()
    {
      var generatedType = typeof (GeneratedType);
      var generatedParameterTypes = new[] { typeof (double) };

      _provider.GetConstructor (generatedType, generatedParameterTypes, false, typeof (RequestedType), new[] { typeof (IDisposable), typeof (string) });
    }

    [Test]
    [ExpectedException (typeof (MissingMethodException), ExpectedMessage =
        "Type Remotion.TypePipe.UnitTests.ConstructorProviderTest+RequestedType contains a constructor with the required signature, but it is "
        + "not public (and the allowNonPublic flag is not set).")]
    public void GetConstructor_ThrowsForNonPublic ()
    {
      var generatedType = typeof (GeneratedType);
      var generatedParameterTypes = Type.EmptyTypes;

      _provider.GetConstructor (generatedType, generatedParameterTypes, false, typeof (RequestedType), Type.EmptyTypes);
    }

    [Test]
    public void CreateConstructorCall ()
    {
      var constructor = MemberInfoFromExpressionUtility.GetConstructor (() => new GeneratedType ("", 7));

      var result = (Func<string, int, GeneratedType>) _provider.CreateConstructorCall (constructor, typeof (Func<string, int, GeneratedType>));

      var instance = result ("abc", 7);
      Assert.That (instance.String, Is.EqualTo ("abc"));
      Assert.That (instance.Int, Is.EqualTo (7));
    }

    [Test]
    public void CreateConstructorCall_ValueType ()
    {
      var constructor = MemberInfoFromExpressionUtility.GetConstructor (() => new GeneratedValueType ("", 7));

      var result = (Func<string, int, GeneratedValueType>) _provider.CreateConstructorCall (constructor, typeof (Func<string, int, GeneratedValueType>));

      var instance = result ("abc", 7);
      Assert.That (instance.String, Is.EqualTo ("abc"));
      Assert.That (instance.Int, Is.EqualTo (7));
    }

    class RequestedType { }

    class GeneratedType
    {
      public readonly string String;
      public readonly int Int;

      public GeneratedType (string s1, int i2) { String = s1; Int = i2; }
      internal GeneratedType () { String = "non-public .ctor"; }
    }

    struct GeneratedValueType
    {
      public readonly string String;
      public readonly int Int;

      public GeneratedValueType (string s1, int i2) { String = s1; Int = i2; }
    }
  }
}