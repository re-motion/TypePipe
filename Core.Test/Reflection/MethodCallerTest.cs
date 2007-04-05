using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Rubicon.Reflection;
using System.Reflection;

namespace Rubicon.Core.UnitTests.Reflection
{
  [TestFixture]
  public class MethodCallerTest
  {
    class TestClass
    {
      private readonly string _name;

      public TestClass (string name)
      {
        _name = name;
      }

      public string Say (string msg)
      {
        return msg + " " + _name;
      }
    }

    [Test]
    public void TestOpenDelegate ()
    {
      MethodInfo mi = typeof (TestClass).GetMethod ("Say");
      Func<TestClass, string, string> f = (Func<TestClass, string, string>) Delegate.CreateDelegate (typeof (Func<TestClass, string, string>), mi);

      TestClass foo = new TestClass ("foo");
      TestClass bar = new TestClass ("bar");

      TestClass testClass = null;
      TestClass TestClass = null;
      TestClass myTestClass = null;

      Assert.AreEqual ("Hi foo", f (foo, "Hi"));
      Assert.AreEqual ("Hi bar", f (bar, "Hi"));

      // Assert.AreEqual ("Hi foo", MethodCaller.Call<string> ("Say").With (foo, "Hi"));

      Func<TestClass, string, string> f2 = MethodCaller.GetMethod<TestClass, string> ("Say").With<TestClass,string> ();
      Assert.AreEqual ("Hi foo", f2 (foo, "Hi"));
      Assert.AreEqual ("Hi bar", f2 (bar, "Hi"));

      //Func<TestClass, string, string> f3 = MethodCaller.GetMethod<TestClass, string, string> ("Say", BindingFlags.Instance | BindingFlags.Public);
      //Assert.AreEqual ("Hi foo", f3 (foo, "Hi"));
      //Assert.AreEqual ("Hi bar", f3 (bar, "Hi"));

      Assert.AreEqual ("Hi foo", MethodCaller.Call<string> ("Say").With (foo, "Hi"));
      Assert.AreEqual ("Hi bar", MethodCaller.Call<string> ("Say").With (bar, "Hi"));
    }
  }


}
