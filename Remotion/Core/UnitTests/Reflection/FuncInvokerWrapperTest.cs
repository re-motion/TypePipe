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
      Assert.That (afterActionCalled, Is.True);
      Assert.That (one.Ctor, Is.EqualTo ("one"));

      afterActionCalled = false;

      ClassWithCtors two = wrapper.With (0, "1");
      Assert.That (afterActionCalled, Is.True);
      Assert.That (two.Ctor, Is.EqualTo ("two"));

      afterActionCalled = false;

      ClassWithCtors three = wrapper.With (0, "1", 2);
      Assert.That (afterActionCalled, Is.True);
      Assert.That (three.Ctor, Is.EqualTo ("three"));

      afterActionCalled = false;

      ClassWithCtors threeInvoked1 = wrapper.Invoke (new object[] { 0, "1", 2 });
      Assert.That (afterActionCalled, Is.True);
      Assert.That (threeInvoked1.Ctor, Is.EqualTo ("three"));

      afterActionCalled = false;

      ClassWithCtors threeInvoked2 = wrapper.Invoke (new Type[] {typeof (int), typeof (string), typeof (int)}, new object[] { 0, "1", 2 });
      Assert.That (afterActionCalled, Is.True);
      Assert.That (threeInvoked2.Ctor, Is.EqualTo ("three"));
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
      Assert.That (one, Is.SameAs (fixedInstance));

      ClassWithCtors two = wrapper.With (0, "1");
      Assert.That (two, Is.SameAs (fixedInstance));

      ClassWithCtors threeInvoked1 = wrapper.Invoke (new object[] { 0, "1", 2 });
      Assert.That (threeInvoked1, Is.SameAs (fixedInstance));

      ClassWithCtors threeInvoked2 = wrapper.Invoke (new Type[] { typeof (int), typeof (string), typeof (int) }, new object[] { 0, "1", 2 });
      Assert.That (threeInvoked2, Is.SameAs (fixedInstance));
    }
  }
}
