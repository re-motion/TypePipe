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

namespace Remotion.TypePipe.IntegrationTests.MutableReflection
{
  [TestFixture]
  public class IsAssignableFromTest
  {
    private MutableType _mutableType;

    [SetUp]
    public void SetUp ()
    {
      _mutableType = MutableTypeObjectMother.Create (typeof (DomainType));
    }

    [Test]
    public void IsAssignableFromFast ()
    {
      Assert.That (_mutableType.IsTypePipeAssignableFrom (_mutableType), Is.True);
      Assert.That (typeof (object).IsTypePipeAssignableFrom (_mutableType), Is.True);
      Assert.That (typeof (DomainType).IsTypePipeAssignableFrom (_mutableType), Is.True);
      Assert.That (_mutableType.IsTypePipeAssignableFrom (typeof (DomainType)), Is.False);
      Assert.That (typeof (IDomainInterface).IsTypePipeAssignableFrom (_mutableType), Is.True);
      Assert.That (typeof (IAddedInterface).IsTypePipeAssignableFrom (_mutableType), Is.False);
      Assert.That (typeof (UnrelatedType).IsTypePipeAssignableFrom (_mutableType), Is.False);

      _mutableType.AddInterface (typeof (IAddedInterface));
      Assert.That (typeof (IAddedInterface).IsTypePipeAssignableFrom (_mutableType), Is.True);
    }

    [Test]
    public void IsAssignableFromFast_TypeInstantiations ()
    {
      var instantiation = typeof (GenericType<>).MakeTypePipeGenericType (_mutableType);
      var baseInstantiation = instantiation.BaseType;
      var ifcInstantiation = instantiation.GetInterfaces().Single();
      Assert.That (baseInstantiation, Is.TypeOf<TypeInstantiation>());
      Assert.That (ifcInstantiation, Is.TypeOf<TypeInstantiation> ());

      Assert.That (instantiation.IsTypePipeAssignableFrom (instantiation), Is.True);
      Assert.That (baseInstantiation.IsTypePipeAssignableFrom (instantiation), Is.True);
      Assert.That (ifcInstantiation.IsTypePipeAssignableFrom (instantiation), Is.True);
    }

    [Test]
    public void IsSubclassOf ()
    {
      Assert.That (_mutableType.IsSubclassOf (typeof (object)), Is.True);
      Assert.That (_mutableType.IsSubclassOf (typeof (DomainType)), Is.True);
      Assert.That (_mutableType.IsSubclassOf (_mutableType), Is.False);
      Assert.That (typeof (DomainType).IsSubclassOf (_mutableType), Is.False);
      Assert.That (_mutableType.IsSubclassOf (typeof (IDomainInterface)), Is.False);
    }

    [Test]
    public void Equals_Type ()
    {
      var proxyType = MutableTypeObjectMother.Create (typeof (DomainType));

      Assert.That (_mutableType.Equals (_mutableType), Is.True);
      Assert.That (_mutableType.Equals (proxyType), Is.False);
      // ReSharper disable CheckForReferenceEqualityInstead.1
      Assert.That (_mutableType.Equals (typeof (DomainType)), Is.False);
      // ReSharper restore CheckForReferenceEqualityInstead.1
      Assert.That (() => typeof (DomainType).Equals (_mutableType), Throws.TypeOf<NotSupportedException>());
    }

    [Test]
    public void Equals_Object ()
    {
      var proxyType = MutableTypeObjectMother.Create (typeof (DomainType));

      Assert.That (_mutableType.Equals ((object) _mutableType), Is.True);
      Assert.That (_mutableType.Equals ((object) proxyType), Is.False);
      Assert.That (_mutableType.Equals ((object) typeof (DomainType)), Is.False);
      Assert.That (typeof (DomainType).Equals ((object) _mutableType), Is.False);
    }

    [Test]
    public new void GetHashCode ()
    {
      var proxyType = MutableTypeObjectMother.Create (typeof (DomainType));

      var result = _mutableType.GetHashCode();

      Assert.That (_mutableType.GetHashCode(), Is.EqualTo (result));
      Assert.That (proxyType.GetHashCode(), Is.Not.EqualTo (result));

      _mutableType.AddInterface (typeof (IDisposable));
      Assert.That (_mutableType.GetHashCode(), Is.EqualTo (result), "Hash code must not change.");
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