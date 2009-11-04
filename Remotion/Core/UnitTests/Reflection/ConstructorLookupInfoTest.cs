// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using NUnit.Framework.SyntaxHelpers;
using Remotion.Reflection;
using Remotion.UnitTests.Reflection.TestDomain;

namespace Remotion.UnitTests.Reflection
{
  [TestFixture]
  public class ConstructorLookupInfoTest
  {
    [Test]
    public void GetDelegate_WithExactMatchFromBase ()
    {
      ConstructorLookupInfo lookupInfo = new ConstructorLookupInfo (typeof (TestClass));
      var actual = (Func<Base, TestClass>) lookupInfo.GetDelegate (typeof (Func<Base, TestClass>));

      TestClass instance = actual (null);
      Assert.That (instance.InvocationType, Is.SameAs (typeof (Base)));
    }

    [Test]
    public void GetDelegate_WithExactMatchFromDerived ()
    {
      ConstructorLookupInfo lookupInfo = new ConstructorLookupInfo (typeof (TestClass));
      var actual = (Func<Derived, TestClass>) lookupInfo.GetDelegate (typeof (Func<Derived, TestClass>));

      TestClass instance = actual (null);
      Assert.That (instance.InvocationType, Is.SameAs (typeof (Derived)));
    }

    [Test]
    public void GetDelegate_WithExactMatchFromDerivedDerived ()
    {
      ConstructorLookupInfo lookupInfo = new ConstructorLookupInfo (typeof (TestClass));
      var actual = (Func<DerivedDerived, TestClass>) lookupInfo.GetDelegate (typeof (Func<DerivedDerived, TestClass>));

      TestClass instance = actual (null);
      Assert.That (instance.InvocationType, Is.SameAs (typeof (Derived)));
    }
  }
}
