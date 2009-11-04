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
using System.Reflection;
using NUnit.Framework;
using Remotion.Reflection;
using Remotion.UnitTests.Reflection.TestDomain;
using Remotion.Utilities;

namespace Remotion.UnitTests.Reflection
{
  [TestFixture]
  public class TypesafeActivatorTest
  {
    [Test]
    [ExpectedException (typeof (MissingMethodException))]
    public void TestWithObjectNull ()
    {
      object o = null;
      TestClass testObject = TypesafeActivator.CreateInstance<TestClass> ().With (o);
      Assert.AreEqual (typeof (object), testObject.InvocationType);
    }

    [Test]
    public void TestWithANull ()
    {
      Base a = null;
      TestClass testObject = TypesafeActivator.CreateInstance<TestClass> ().With (a);
      Assert.AreEqual (typeof (Base), testObject.InvocationType);
    }

    [Test]
    public void TestWithBNull ()
    {
      Derived b = null;
      TestClass testObject = TypesafeActivator.CreateInstance<TestClass> ().With (b);
      Assert.AreEqual (typeof (Derived), testObject.InvocationType);
    }

    [Test]
    public void TestWithCNull ()
    {
      DerivedDerived c = null;
      TestClass testObject = TypesafeActivator.CreateInstance<TestClass> ().With (c);
      Assert.AreEqual (typeof (Derived), testObject.InvocationType);
    }

    [Test]
    public void TestWithUntypedANull ()
    {
      Base a = null;
      TestClass testObject = (TestClass) TypesafeActivator.CreateInstance (typeof (TestClass)).With (a);
      Assert.AreEqual (typeof (Base), testObject.InvocationType);
    }

    [Test]
    public void TestWithUntypedDerivedAndTMinimal ()
    {
      Base a = TypesafeActivator.CreateInstance<Base> (typeof (Derived)).With ();

      Assert.IsNotNull (a);
      Assert.AreEqual (typeof (Derived), a.GetType ());
    }

    [Test]
    public void TestWithUntypedDerivedAndTMinimalWithBindingFlags ()
    {
      Base a = TypesafeActivator.CreateInstance<Base> (typeof (Derived), BindingFlags.Public | BindingFlags.Instance).With ();

      Assert.IsNotNull (a);
      Assert.AreEqual (typeof (Derived), a.GetType ());
    }

    [Test]
    public void TestWithUntypedDerivedAndTMinimalWithFullSignature ()
    {
      Base a = TypesafeActivator.CreateInstance<Base> (typeof (Derived), BindingFlags.Public | BindingFlags.Instance, null, CallingConventions.Any, null).With ();

      Assert.IsNotNull (a);
      Assert.AreEqual (typeof (Derived), a.GetType ());
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void TestWithUntypedAndTMinimalThrowsOnIncompatibleTypes ()
    {
      TypesafeActivator.CreateInstance<Derived> (typeof (Base)).With ();
    }

    [Test]
    public void TestValueTypeDefaultCtor()
    {
      Struct @struct = TypesafeActivator.CreateInstance<Struct>().With();
      Assert.AreEqual (@struct.Value, 0);
    }

    [Test]
    public void TestValueTypeCustomCtor ()
    {
      Struct @struct = TypesafeActivator.CreateInstance<Struct> ().With (1);
      Assert.AreEqual (@struct.Value, 1);
    }
  }
}
