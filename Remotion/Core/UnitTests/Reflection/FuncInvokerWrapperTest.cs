// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using Remotion.Development.UnitTesting;
using Remotion.Reflection;

namespace Remotion.UnitTests.Reflection
{
  [TestFixture]
  public class FuncInvokerWrapperTest
  {
    public class ClassWithCtors
    {
      public string Ctor;

      public ClassWithCtors (int one)
      {
        Dev.Null = one;
        Ctor = "one";
      }

      public ClassWithCtors (int one, string two)
      {
        Dev.Null = one;
        Dev.Null = two;
        Ctor = "two";
      }

      public ClassWithCtors (int one, string two, int three)
      {
        Dev.Null = one;
        Dev.Null = two;
        Dev.Null = three;
        Ctor = "three";
      }
    }

    [Test]
    public void AfterActionIsExecuted ()
    {
      bool afterActionCalled = false;

      FuncInvoker<ClassWithCtors> activator = TypesafeActivator.CreateInstance<ClassWithCtors> ();
      FuncInvokerWrapper<ClassWithCtors> wrapper = new FuncInvokerWrapper<ClassWithCtors> (activator, delegate (ClassWithCtors instance)
      {
        afterActionCalled = true;
        return instance;
      });
      
      ClassWithCtors one = wrapper.With (0);
      Assert.IsTrue (afterActionCalled);
      Assert.AreEqual ("one", one.Ctor);

      afterActionCalled = false;

      ClassWithCtors two = wrapper.With (0, "1");
      Assert.IsTrue (afterActionCalled);
      Assert.AreEqual ("two", two.Ctor);

      afterActionCalled = false;

      ClassWithCtors three = wrapper.With (0, "1", 2);
      Assert.IsTrue (afterActionCalled);
      Assert.AreEqual ("three", three.Ctor);

      afterActionCalled = false;

      ClassWithCtors threeInvoked1 = wrapper.Invoke (new object[] { 0, "1", 2 });
      Assert.IsTrue (afterActionCalled);
      Assert.AreEqual ("three", threeInvoked1.Ctor);

      afterActionCalled = false;

      ClassWithCtors threeInvoked2 = wrapper.Invoke (new Type[] {typeof (int), typeof (string), typeof (int)}, new object[] { 0, "1", 2 });
      Assert.IsTrue (afterActionCalled);
      Assert.AreEqual ("three", threeInvoked2.Ctor);
    }

    [Test]
    public void AfterActionCanReplaceResult ()
    {
      ClassWithCtors fixedInstance = new ClassWithCtors (0);

      FuncInvoker<ClassWithCtors> activator = TypesafeActivator.CreateInstance<ClassWithCtors> ();
      FuncInvokerWrapper<ClassWithCtors> wrapper = new FuncInvokerWrapper<ClassWithCtors> (activator, delegate (ClassWithCtors instance)
      {
        return fixedInstance;
      });

      ClassWithCtors one = wrapper.With (0);
      Assert.AreSame (fixedInstance, one);

      ClassWithCtors two = wrapper.With (0, "1");
      Assert.AreSame (fixedInstance, two);

      ClassWithCtors threeInvoked1 = wrapper.Invoke (new object[] { 0, "1", 2 });
      Assert.AreSame (fixedInstance, threeInvoked1);

      ClassWithCtors threeInvoked2 = wrapper.Invoke (new Type[] { typeof (int), typeof (string), typeof (int) }, new object[] { 0, "1", 2 });
      Assert.AreSame (fixedInstance, threeInvoked2);
    }
  }
}
