// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
  public class MethodLookupInfoTest
  {
    [Test]
    public void GetInstanceMethodDelegate_WithExactMatchFromBase ()
    {
      MethodLookupInfo lookupInfo = new MethodLookupInfo ("InstanceMethod");
      var actual = (Func<TestClass, Base, Type>) lookupInfo.GetInstanceMethodDelegate (typeof (Func<TestClass, Base, Type>));

      TestClass instance = new TestClass ((Derived) null);
      Assert.That (actual (instance, null), Is.SameAs (typeof (Base)));
    }

    [Test]
    public void GetInstanceMethodDelegate_WithExactMatchFromDerived ()
    {
      MethodLookupInfo lookupInfo = new MethodLookupInfo ("InstanceMethod");
      var actual = (Func<TestClass, Derived, Type>) lookupInfo.GetInstanceMethodDelegate (typeof (Func<TestClass, Derived, Type>));

      TestClass instance = new TestClass ((Base) null);
      Assert.That (actual (instance, null), Is.SameAs (typeof (Derived)));
    }

    [Test]
    public void GetInstanceMethodDelegate_WithExactMatchFromDerivedDerived ()
    {
      MethodLookupInfo lookupInfo = new MethodLookupInfo ("InstanceMethod");
      var actual = (Func<TestClass, DerivedDerived, Type>) lookupInfo.GetInstanceMethodDelegate (typeof (Func<TestClass, DerivedDerived, Type>));

      TestClass instance = new TestClass ((Base) null);
      Assert.That (actual (instance, null), Is.SameAs (typeof (Derived)));
    }

    [Test]
    [Ignore ("TODO: Implement support for static methods.")]
    public void GetStaticMethodDelegate_WithExactMatchFromBase ()
    {
      MethodLookupInfo lookupInfo = new MethodLookupInfo ("StaticMethod");
      var actual = (Func<TestClass, Base, Type>) lookupInfo.GetInstanceMethodDelegate (typeof (Func<TestClass, Base, Type>));

      Assert.That (actual (null, null), Is.SameAs (typeof (Base)));
    }

  }
}