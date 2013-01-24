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
using NUnit.Framework;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.IntegrationTests.MutableReflection
{
  [TestFixture]
  public class IsAssignableFromTest
  {
    private ProxyType _proxyType;

    [SetUp]
    public void SetUp ()
    {
      _proxyType = ProxyTypeObjectMother.Create (typeof (DomainType), new UnderlyingTypeFactory());
    }

    [Test]
    public void UnderlyingSystemType ()
    {
      var result = _proxyType.UnderlyingSystemType;

      Assert.That (result.IsRuntimeType(), Is.True);
      Assert.That (_proxyType.UnderlyingSystemType, Is.SameAs (result));
      Assert.That (typeof (DomainType).IsAssignableFrom (_proxyType.UnderlyingSystemType), Is.True);
      Assert.That (typeof (IDomainInterface).IsAssignableFrom (_proxyType.UnderlyingSystemType), Is.True);

      _proxyType.AddInterface (typeof (IAddedInterface));
      Assert.That (_proxyType.UnderlyingSystemType, Is.Not.SameAs (result));
      Assert.That (typeof (IAddedInterface).IsAssignableFrom (_proxyType.UnderlyingSystemType), Is.True);
    }

    [Test]
    public void IsAssignableFrom ()
    {
      Assert.That (_proxyType.IsAssignableFrom (_proxyType), Is.True);
      Assert.That (typeof (object).IsAssignableFrom (_proxyType), Is.True);
      Assert.That (typeof (DomainType).IsAssignableFrom (_proxyType), Is.True);
      Assert.That (_proxyType.IsAssignableFrom (typeof (DomainType)), Is.False);
      Assert.That (typeof (IDomainInterface).IsAssignableFrom (_proxyType), Is.True);
      Assert.That (typeof (IAddedInterface).IsAssignableFrom (_proxyType), Is.False);
      Assert.That (typeof (UnrelatedType).IsAssignableFrom (_proxyType), Is.False);

      _proxyType.AddInterface (typeof (IAddedInterface));
      Assert.That (typeof (IAddedInterface).IsAssignableFrom (_proxyType), Is.True);
    }

    [Test]
    public void IsAssignableFromFast ()
    {
      Assert.That (_proxyType.IsAssignableFromFast (_proxyType), Is.True);
      Assert.That (typeof (object).IsAssignableFromFast (_proxyType), Is.True);
      Assert.That (typeof (DomainType).IsAssignableFromFast (_proxyType), Is.True);
      Assert.That (_proxyType.IsAssignableFromFast (typeof (DomainType)), Is.False);
      Assert.That (typeof (IDomainInterface).IsAssignableFromFast (_proxyType), Is.True);
      Assert.That (typeof (IAddedInterface).IsAssignableFromFast (_proxyType), Is.False);
      Assert.That (typeof (UnrelatedType).IsAssignableFromFast (_proxyType), Is.False);

      _proxyType.AddInterface (typeof (IAddedInterface));
      Assert.That (typeof (IAddedInterface).IsAssignableFromFast (_proxyType), Is.True);
    }

    [Test]
    public void IsSubclassOf ()
    {
      Assert.That (_proxyType.IsSubclassOf (typeof (object)), Is.True);
      Assert.That (_proxyType.IsSubclassOf (typeof (DomainType)), Is.True);
      Assert.That (_proxyType.IsSubclassOf (_proxyType), Is.False);
      Assert.That (typeof (DomainType).IsSubclassOf (_proxyType), Is.False);
      Assert.That (_proxyType.IsSubclassOf (typeof (IDomainInterface)), Is.False);
    }

    [Test]
    public void Equals_Type ()
    {
      var proxyType = ProxyTypeObjectMother.Create (typeof (DomainType));

      Assert.That (_proxyType.Equals (_proxyType), Is.True);
      Assert.That (_proxyType.Equals (proxyType), Is.False);
      // ReSharper disable CheckForReferenceEqualityInstead.1
      Assert.That (_proxyType.Equals (typeof (DomainType)), Is.False);
      // ReSharper restore CheckForReferenceEqualityInstead.1
      Assert.That (typeof (DomainType).Equals (_proxyType), Is.False);
    }

    [Test]
    public void Equals_Object ()
    {
      var proxyType = ProxyTypeObjectMother.Create (typeof (DomainType));

      Assert.That (_proxyType.Equals ((object) _proxyType), Is.True);
      Assert.That (_proxyType.Equals ((object) proxyType), Is.False);
      Assert.That (_proxyType.Equals ((object) typeof (DomainType)), Is.False);
      Assert.That (typeof (DomainType).Equals ((object) _proxyType), Is.False);
    }

    [Test]
    public new void GetHashCode ()
    {
      var proxyType = ProxyTypeObjectMother.Create (typeof (DomainType));

      var result = _proxyType.GetHashCode();

      Assert.That (_proxyType.GetHashCode(), Is.EqualTo (result));
      Assert.That (proxyType.GetHashCode(), Is.Not.EqualTo (result));

      _proxyType.AddInterface (typeof (IDisposable));
      Assert.That (_proxyType.GetHashCode(), Is.EqualTo (result), "Hash code must not change.");
    }

    public class DomainType : IDomainInterface { }
    public interface IDomainInterface { }
    public interface IAddedInterface { }
    public class UnrelatedType { }
  }
}