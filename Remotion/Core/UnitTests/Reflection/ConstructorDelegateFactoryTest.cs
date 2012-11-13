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
using Remotion.Reflection;
using Remotion.Utilities;

namespace Remotion.UnitTests.Reflection
{
  [TestFixture]
  public class ConstructorDelegateFactoryTest
  {
    private ConstructorDelegateFactory _factory;

    [SetUp]
    public void SetUp ()
    {
      _factory = new ConstructorDelegateFactory();
    }

    [Test]
    public void CreateConstructorCall ()
    {
      var constructor = MemberInfoFromExpressionUtility.GetConstructor (() => new DomainType ("", 7));

      var result =
          (Func<string, int, DomainType>)
          _factory.CreateConstructorCall (constructor, typeof (Func<string, int, DomainType>), typeof (DomainType));

      var instance = result ("abc", 7);
      Assert.That (instance.String, Is.EqualTo ("abc"));
      Assert.That (instance.Int, Is.EqualTo (7));
    }

    [Test]
    public void CreateConstructorCall_ValueType ()
    {
      var constructor = MemberInfoFromExpressionUtility.GetConstructor (() => new DomainValueType ("", 7));

      var result =
          (Func<string, int, DomainValueType>)
          _factory.CreateConstructorCall (constructor, typeof (Func<string, int, DomainValueType>), typeof (DomainValueType));

      var instance = result ("abc", 7);
      Assert.That (instance.String, Is.EqualTo ("abc"));
      Assert.That (instance.Int, Is.EqualTo (7));
    }

    [Test]
    public void CreateConstructorCall_ValueType_Boxing ()
    {
      var constructor = MemberInfoFromExpressionUtility.GetConstructor (() => new DomainValueType ("", 7));

      var result = (Func<string, int, object>) _factory.CreateConstructorCall (constructor, typeof (Func<string, int, object>), typeof (object));

      var instance = (DomainValueType) result ("abc", 7);
      Assert.That (instance.String, Is.EqualTo ("abc"));
      Assert.That (instance.Int, Is.EqualTo (7));
    }

    class DomainType
    {
      public readonly string String;
      public readonly int Int;

      public DomainType (string s1, int i2) { String = s1; Int = i2; }
    }

    struct DomainValueType
    {
      public readonly string String;
      public readonly int Int;

      public DomainValueType (string s1, int i2) { String = s1; Int = i2; }
    }
  }
}