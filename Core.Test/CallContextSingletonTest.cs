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
using System.Runtime.Remoting.Messaging;
using NUnit.Framework;
using Remotion.Development.UnitTesting;

namespace Remotion.UnitTests
{
  [TestFixture]
  public class CallContextSingletonTest
  {
    [SetUp]
    public void SetUp ()
    {
      CallContext.SetData ("test", null);
      CallContext.SetData ("test1", null);
      CallContext.SetData ("test2", null);
    }

    [TearDown]
    public void TearDown ()
    {
      CallContext.SetData ("test", null);
      CallContext.SetData ("test1", null);
      CallContext.SetData ("test2", null);
    }

    [Test]
    public void UsesCallContext_WithGivenKey ()
    {
      object instance = new object();
      CallContextSingleton<object> singleton = new CallContextSingleton<object> ("test", delegate { return null; });
      singleton.SetCurrent (instance);

      Assert.AreSame (instance, CallContext.GetData ("test"));
    }

    [Test]
    public void SingleInstance_CreatedOnDemand ()
    {
      object instance = null;
      CallContextSingleton<object> singleton = new CallContextSingleton<object> ("test", delegate { return (instance = new object ()); });

      Assert.IsNull (instance);
      object current = singleton.Current;
      Assert.IsNotNull (current);
      Assert.IsNotNull (instance);
      Assert.AreSame (instance, current);

      Assert.AreSame (current, singleton.Current);
      Assert.AreSame (current, singleton.Current);
      Assert.AreSame (current, singleton.Current);
    }

    [Test]
    public void DifferentSingletons ()
    {
      object instance1 = new object ();
      object instance2 = new object ();

      CallContextSingleton<object> singleton1 = new CallContextSingleton<object> ("test1", delegate { return instance1; });
      CallContextSingleton<object> singleton2 = new CallContextSingleton<object> ("test2", delegate { return instance2; });

      Assert.AreSame (instance1, singleton1.Current);
      Assert.AreSame (instance2, singleton2.Current);
      Assert.AreSame (instance1, singleton1.Current);
      Assert.AreSame (instance2, singleton2.Current);
    }

    [Test]
    public void HasCurrent ()
    {
      CallContextSingleton<object> singleton = new CallContextSingleton<object> ("test", delegate { return ( new object ()); });
      Assert.IsFalse (singleton.HasCurrent);
      Dev.Null = singleton.Current;
      Assert.IsTrue (singleton.HasCurrent);
      singleton.SetCurrent (null);
      Assert.IsFalse (singleton.HasCurrent);
    }

    [Test]
    public void SetCurrent ()
    {
      object instance = new object();
      CallContextSingleton<object> singleton = new CallContextSingleton<object> ("test", delegate { return new object (); });
      Assert.AreNotSame (instance, singleton.Current);
      singleton.SetCurrent (instance);
      Assert.AreSame (instance, singleton.Current);
      singleton.SetCurrent (new object());
      Assert.AreNotSame (instance, singleton.Current);
    }
  }
}
