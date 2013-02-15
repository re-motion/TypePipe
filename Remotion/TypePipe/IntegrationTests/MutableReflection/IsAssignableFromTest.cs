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
using System.Linq;
using NUnit.Framework;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.IntegrationTests.MutableReflection
{
  [TestFixture]
  public class IsAssignableFromTest
  {
    private ProxyType _proxyType;
    private ProxyType _proxyTypeWithUnderlyingType;

    [SetUp]
    public void SetUp ()
    {
      _proxyType = ProxyTypeObjectMother.Create (typeof (DomainType));
      _proxyTypeWithUnderlyingType = ProxyTypeObjectMother.Create (typeof (DomainType), new UnderlyingTypeFactory());
    }

    [Test]
    public void UnderlyingSystemType ()
    {
      var result = _proxyTypeWithUnderlyingType.UnderlyingSystemType;

      Assert.That (result.IsRuntimeType(), Is.True);
      Assert.That (_proxyTypeWithUnderlyingType.UnderlyingSystemType, Is.SameAs (result));
      Assert.That (typeof (DomainType).IsAssignableFrom (_proxyTypeWithUnderlyingType.UnderlyingSystemType), Is.True);
      Assert.That (typeof (IDomainInterface).IsAssignableFrom (_proxyTypeWithUnderlyingType.UnderlyingSystemType), Is.True);

      _proxyTypeWithUnderlyingType.AddInterface (typeof (IAddedInterface));
      Assert.That (_proxyTypeWithUnderlyingType.UnderlyingSystemType, Is.Not.SameAs (result));
      Assert.That (typeof (IAddedInterface).IsAssignableFrom (_proxyTypeWithUnderlyingType.UnderlyingSystemType), Is.True);
    }

    [Test]
    public void IsAssignableFrom ()
    {
      Assert.That (_proxyTypeWithUnderlyingType.IsAssignableFrom (_proxyTypeWithUnderlyingType), Is.True);
      Assert.That (typeof (object).IsAssignableFrom (_proxyTypeWithUnderlyingType), Is.True);
      Assert.That (typeof (DomainType).IsAssignableFrom (_proxyTypeWithUnderlyingType), Is.True);
      Assert.That (_proxyTypeWithUnderlyingType.IsAssignableFrom (typeof (DomainType)), Is.False);
      Assert.That (typeof (IDomainInterface).IsAssignableFrom (_proxyTypeWithUnderlyingType), Is.True);
      Assert.That (typeof (IAddedInterface).IsAssignableFrom (_proxyTypeWithUnderlyingType), Is.False);
      Assert.That (typeof (UnrelatedType).IsAssignableFrom (_proxyTypeWithUnderlyingType), Is.False);

      _proxyTypeWithUnderlyingType.AddInterface (typeof (IAddedInterface));
      Assert.That (typeof (IAddedInterface).IsAssignableFrom (_proxyTypeWithUnderlyingType), Is.True);
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
    public void IsAssignableFromFast_TypeInstantiations ()
    {
      var instantiation = typeof (GenericType<>).MakeTypePipeGenericType (_proxyType);
      var baseInstantiation = instantiation.BaseType;
      var ifcInstantiation = instantiation.GetInterfaces().Single();
      Assert.That (baseInstantiation, Is.TypeOf<TypeInstantiation>());
      Assert.That (ifcInstantiation, Is.TypeOf<TypeInstantiation> ());

      Assert.That (instantiation.IsAssignableFromFast (instantiation), Is.True);
      Assert.That (baseInstantiation.IsAssignableFromFast (instantiation), Is.True);
      Assert.That (ifcInstantiation.IsAssignableFromFast (instantiation), Is.True);
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

      Assert.That (_proxyTypeWithUnderlyingType.Equals (_proxyTypeWithUnderlyingType), Is.True);
      Assert.That (_proxyTypeWithUnderlyingType.Equals (proxyType), Is.False);
      // ReSharper disable CheckForReferenceEqualityInstead.1
      Assert.That (_proxyTypeWithUnderlyingType.Equals (typeof (DomainType)), Is.False);
      // ReSharper restore CheckForReferenceEqualityInstead.1
      Assert.That (typeof (DomainType).Equals (_proxyTypeWithUnderlyingType), Is.False);
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

    public interface IMyInterface<T> { }
    public class BaseType<T> { }
    public class GenericType<T> : BaseType<T>, IMyInterface<T> { }
  }
}