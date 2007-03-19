using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Rubicon.Reflection;
using NUnit.Framework;

namespace Rubicon.Core.UnitTests.Reflection
{
  [TestFixture]
  public class TypesafeActivatorTest
  {
    public class A { }
    public class B: A { }
    public class C: B { }

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
  }
}
