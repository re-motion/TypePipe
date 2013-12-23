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
using System.Reflection;
using NUnit.Framework;
using Remotion.Reflection;
using Remotion.UnitTests.Reflection.TestDomain;

namespace Remotion.UnitTests.Reflection
{
  [Obsolete]
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

    [Test]
    public void GetDelegate_WithAbstractType_Throws ()
    {
      var info = new ConstructorLookupInfo (typeof (AbstractClass), BindingFlags.NonPublic | BindingFlags.Instance);
      Assert.That (
          () => info.GetDelegate (typeof (Func<AbstractClass>)),
          Throws.InvalidOperationException.With.Message.EqualTo (
              "Cannot create an instance of 'Remotion.UnitTests.Reflection.TestDomain.AbstractClass' because it is an abstract type."));
    }

    [Test]
    public void GetDelegate_ImplicitConversion ()
    {
      ConstructorLookupInfo lookupInfo = new ConstructorLookupInfo (typeof (TestClass));
      var actual = (Func<Base, object>) lookupInfo.GetDelegate (typeof (Func<Base, object>));

      var instance = actual (null);
      Assert.That (instance, Is.TypeOf<TestClass>());
    }

    [Test]
    public void GetDelegate_ValueType_DefaultCtor ()
    {
      var info = new ConstructorLookupInfo (typeof (int));
      var actual = (Func<int>) info.GetDelegate (typeof (Func<int>));
      var instance = actual();

      Assert.That (instance, Is.EqualTo (new int()));
    }

    [Test]
    public void GetDelegate_ValueType_DefaultCtor_Boxing ()
    {
      var info = new ConstructorLookupInfo (typeof (int));
      var actual = (Func<object>) info.GetDelegate (typeof (Func<object>));
      var instance = actual ();

      Assert.That (instance, Is.EqualTo (new int ()));
    }

    [Test]
    public void GetDelegate_ValueType_DefaultCtor_BoxingInterface ()
    {
      var info = new ConstructorLookupInfo (typeof (int));
      var actual = (Func<IComparable>) info.GetDelegate (typeof (Func<IComparable>));
      var instance = actual ();

      Assert.That (instance, Is.EqualTo (new int ()));
    }

    [Test]
    public void GetDelegate_ValueType_NonDefaultCtor ()
    {
      var info = new ConstructorLookupInfo (typeof (DateTime));
      var actual = (Func<int, int, int, DateTime>) info.GetDelegate (typeof (Func<int, int, int, DateTime>));
      var instance = actual (2012, 01, 02);

      Assert.That (instance, Is.EqualTo (new DateTime (2012, 01, 02)));
    }

    [Test]
    public void GetDelegate_ValueType_NonDefaultCtor_Boxing ()
    {
      var info = new ConstructorLookupInfo (typeof (DateTime));
      var actual = (Func<int, int, int, object>) info.GetDelegate (typeof (Func<int, int, int, object>));
      var instance = actual (2012, 01, 02);

      Assert.That (instance, Is.EqualTo (new DateTime (2012, 01, 02)));
    }

    [Test]
    public void GetDelegate_ValueType_NonDefaultCtor_BoxingInterface ()
    {
      var info = new ConstructorLookupInfo (typeof (DateTime));
      var actual = (Func<int, int, int, IComparable>) info.GetDelegate (typeof (Func<int, int, int, IComparable>));
      var instance = actual (2012, 01, 02);

      Assert.That (instance, Is.EqualTo (new DateTime (2012, 01, 02)));
    }

    [Test]
    public void DynamicInvoke ()
    {
      var info = new ConstructorLookupInfo (typeof (AbstractClass), BindingFlags.NonPublic | BindingFlags.Instance);
      Assert.That (
          () => info.DynamicInvoke (new Type[0], new object[0]),
          Throws.InvalidOperationException.With.Message.EqualTo (
              "Cannot create an instance of 'Remotion.UnitTests.Reflection.TestDomain.AbstractClass' because it is an abstract type."));
    }

    [Test]
    public void DynamicInvoke_ValueType_DefaultCtor ()
    {
      var info = new ConstructorLookupInfo (typeof (int));
      
      var instance = info.DynamicInvoke (new Type[0], new object[0]);

      Assert.That (instance, Is.EqualTo (new int ()));
    }

    [Test]
    public void DynamicInvoke_ValueType_NonDefaultCtor ()
    {
      var info = new ConstructorLookupInfo (typeof (DateTime));

      var instance = info.DynamicInvoke (new[] { typeof (int), typeof (int), typeof (int) }, new object[] { 2012, 01, 02 });

      Assert.That (instance, Is.EqualTo (new DateTime (2012, 01, 02)));
    }
  }
}
