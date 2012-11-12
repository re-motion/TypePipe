// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 

using System;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.Reflection;
using Remotion.Utilities;

namespace Remotion.UnitTests.Reflection
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
        "Type Remotion.UnitTests.Reflection.ConstructorProviderTest+RequestedType does not contain a constructor with the following signature: "
        + "(IDisposable, String).")]
    public void GetConstructor_ThrowsForMissingMember ()
    {
      var generatedType = typeof (GeneratedType);
      var generatedParameterTypes = new[] { typeof (double) };

      _provider.GetConstructor (generatedType, generatedParameterTypes, false, typeof (RequestedType), new[] { typeof (IDisposable), typeof (string) });
    }

    [Test]
    [ExpectedException (typeof (MissingMethodException), ExpectedMessage =
        "Type Remotion.UnitTests.Reflection.ConstructorProviderTest+RequestedType contains a constructor with the required signature, but it is "
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