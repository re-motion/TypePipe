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
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class RelatedEventFinderTest
  {
    private RelatedEventFinder _finder;

    [SetUp]
    public void SetUp ()
    {
      _finder = new RelatedEventFinder ();
    }

    [Test]
    public void GetBaseEvent_NoBaseMethod ()
    {
      var @event = typeof (BaseBaseType).GetEvent ("OverridingEvent");

      var result = _finder.GetBaseEvent (@event);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetBaseEvent_Overridden ()
    {
      var @event = typeof (DomainType).GetEvent ("OverridingEvent");

      var result = _finder.GetBaseEvent(@event);

      var expected = typeof (BaseType).GetEvent ("OverridingEvent");
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetBaseEvent_Shadowed ()
    {
      var @event = typeof (DomainType).GetEvent ("ShadowingEvent");

      var result = _finder.GetBaseEvent (@event);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetBaseEvent_SkippingMiddleClass ()
    {
      var @event = typeof (DomainType).GetEvent ("OverridingEventSkippingMiddleClass");

      var result = _finder.GetBaseEvent (@event);

      var expected = typeof (BaseBaseType).GetEvent ("OverridingEventSkippingMiddleClass");
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetBaseEvent_NonPublic ()
    {
      var @event = typeof (DomainType).GetEvent ("NonPublicOverridingEvent", BindingFlags.Instance | BindingFlags.NonPublic);

      var result = _finder.GetBaseEvent (@event);

      var expected = typeof (BaseType).GetEvent ("NonPublicOverridingEvent", BindingFlags.Instance | BindingFlags.NonPublic);
      Assert.That (result, Is.EqualTo (expected));
    }

    public class BaseBaseType
    {
      public virtual event EventHandler OverridingEvent { add { Dev.Null = value; } remove { Dev.Null = value; } }
      public virtual event EventHandler OverridingEventSkippingMiddleClass { add { Dev.Null = value; } remove { Dev.Null = value; } }
    }

    public class BaseType : BaseBaseType
    {
      public override event EventHandler OverridingEvent { add { Dev.Null = value; } remove { Dev.Null = value; } }
      public virtual event EventHandler ShadowingEvent { add { Dev.Null = value; } remove { Dev.Null = value; } }
      
      internal virtual event EventHandler NonPublicOverridingEvent { add { Dev.Null = value; } remove { Dev.Null = value; } }
    }

    public class DomainType : BaseType
    {
      public override event EventHandler OverridingEvent { add { Dev.Null = value; } remove { Dev.Null = value; } }
      public new virtual event EventHandler ShadowingEvent { add { Dev.Null = value; } remove { Dev.Null = value; } }
      public override event EventHandler OverridingEventSkippingMiddleClass { add { Dev.Null = value; } remove { Dev.Null = value; } }

      internal override event EventHandler NonPublicOverridingEvent { add { Dev.Null = value; } remove { Dev.Null = value; } }
    }
  }
}