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
using System.Runtime.Remoting.Messaging;
using NUnit.Framework;
using Remotion.Context;
using Remotion.Development.UnitTesting;

namespace Remotion.UnitTests
{
  [TestFixture]
  public class SafeContextSingletonTest
  {
    [SetUp]
    public void SetUp ()
    {
      SafeContext.Instance.FreeData ("test");
      SafeContext.Instance.FreeData ("test1");
      SafeContext.Instance.FreeData ("test2");
    }

    [TearDown]
    public void TearDown ()
    {
      SafeContext.Instance.FreeData ("test");
      SafeContext.Instance.FreeData ("test1");
      SafeContext.Instance.FreeData ("test2");
    }

    [Test]
    public void UsesSafeContext_WithGivenKey ()
    {
      object instance = new object();
      SafeContextSingleton<object> singleton = new SafeContextSingleton<object> ("test", delegate { return null; });
      singleton.SetCurrent (instance);

      Assert.AreSame (instance, SafeContext.Instance.GetData ("test"));
    }

    [Test]
    public void SingleInstance_CreatedOnDemand ()
    {
      object instance = null;
      SafeContextSingleton<object> singleton = new SafeContextSingleton<object> ("test", delegate { return (instance = new object ()); });

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

      SafeContextSingleton<object> singleton1 = new SafeContextSingleton<object> ("test1", delegate { return instance1; });
      SafeContextSingleton<object> singleton2 = new SafeContextSingleton<object> ("test2", delegate { return instance2; });

      Assert.AreSame (instance1, singleton1.Current);
      Assert.AreSame (instance2, singleton2.Current);
      Assert.AreSame (instance1, singleton1.Current);
      Assert.AreSame (instance2, singleton2.Current);
    }

    [Test]
    public void HasCurrent ()
    {
      SafeContextSingleton<object> singleton = new SafeContextSingleton<object> ("test", delegate { return ( new object ()); });
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
      SafeContextSingleton<object> singleton = new SafeContextSingleton<object> ("test", delegate { return new object (); });
      Assert.AreNotSame (instance, singleton.Current);
      singleton.SetCurrent (instance);
      Assert.AreSame (instance, singleton.Current);
      singleton.SetCurrent (new object());
      Assert.AreNotSame (instance, singleton.Current);
    }
  }
}
