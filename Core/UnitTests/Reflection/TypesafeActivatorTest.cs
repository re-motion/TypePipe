/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Reflection;
using NUnit.Framework;
using Remotion.Reflection;
using Remotion.Utilities;

namespace Remotion.UnitTests.Reflection
{
  [TestFixture]
  public class TypesafeActivatorTest
  {
    public class A { }
    public class B: A { }
    public class C: B { }

    public struct V
    {
      public V (int value)
      {
        Value = value;
      }

      public int Value; 
    }

    public class TestClass
    {
      public readonly Type InvocationType;

      public TestClass (A a)
      {
        InvocationType = typeof (A);
      }

      public TestClass (B b)
      {
        InvocationType = typeof (B);
      }
    }

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
      A a = null;
      TestClass testObject = TypesafeActivator.CreateInstance<TestClass> ().With (a);
      Assert.AreEqual (typeof (A), testObject.InvocationType);
    }

    [Test]
    public void TestWithBNull ()
    {
      B b = null;
      TestClass testObject = TypesafeActivator.CreateInstance<TestClass> ().With (b);
      Assert.AreEqual (typeof (B), testObject.InvocationType);
    }

    [Test]
    public void TestWithCNull ()
    {
      C c = null;
      TestClass testObject = TypesafeActivator.CreateInstance<TestClass> ().With (c);
      Assert.AreEqual (typeof (B), testObject.InvocationType);
    }

    [Test]
    public void TestWithUntypedANull ()
    {
      A a = null;
      TestClass testObject = (TestClass) TypesafeActivator.CreateInstance (typeof (TestClass)).With (a);
      Assert.AreEqual (typeof (A), testObject.InvocationType);
    }

    [Test]
    public void TestWithUntypedDerivedAndTMinimal ()
    {
      A a = TypesafeActivator.CreateInstance<A> (typeof (B)).With ();

      Assert.IsNotNull (a);
      Assert.AreEqual (typeof (B), a.GetType ());
    }

    [Test]
    public void TestWithUntypedDerivedAndTMinimalWithBindingFlags ()
    {
      A a = TypesafeActivator.CreateInstance<A> (typeof (B), BindingFlags.Public | BindingFlags.Instance).With ();

      Assert.IsNotNull (a);
      Assert.AreEqual (typeof (B), a.GetType ());
    }

    [Test]
    public void TestWithUntypedDerivedAndTMinimalWithFullSignature ()
    {
      A a = TypesafeActivator.CreateInstance<A> (typeof (B), BindingFlags.Public | BindingFlags.Instance, null, CallingConventions.Any, null).With ();

      Assert.IsNotNull (a);
      Assert.AreEqual (typeof (B), a.GetType ());
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void TestWithUntypedAndTMinimalThrowsOnIncompatibleTypes ()
    {
      TypesafeActivator.CreateInstance<B> (typeof (A)).With ();
    }

    [Test]
    public void TestValueTypeDefaultCtor()
    {
      V v = TypesafeActivator.CreateInstance<V>().With();
      Assert.AreEqual (v.Value, 0);
    }

    [Test]
    public void TestValueTypeCustomCtor ()
    {
      V v = TypesafeActivator.CreateInstance<V> ().With (1);
      Assert.AreEqual (v.Value, 1);
    }
  }
}
