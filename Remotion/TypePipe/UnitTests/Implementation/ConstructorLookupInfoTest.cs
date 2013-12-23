// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 

using System;
using System.Reflection;
using NUnit.Framework;
using Remotion.Reflection;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.UnitTests.Implementation.TestDomain;

namespace Remotion.TypePipe.UnitTests.Implementation
{
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
              "Cannot create an instance of 'Remotion.TypePipe.UnitTests.Implementation.TestDomain.AbstractClass' because it is an abstract type."));
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
              "Cannot create an instance of 'Remotion.TypePipe.UnitTests.Implementation.TestDomain.AbstractClass' because it is an abstract type."));
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
