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

namespace Remotion.TypePipe.UnitTests
{
  [TestFixture]
  public class CompoundCacheKeyTest
  {
    private Type _type;
    private Type _otherType;

    [SetUp]
    public void SetUp ()
    {
      _type = typeof (string);
      _otherType = typeof (int);
    }

    [Test]
    public void Equals_True ()
    {
      var key1 = CreateCompoundCacheKey (_type, "a", "b");
      var key2 = CreateCompoundCacheKey (_type, "a", "b");

      Assert.That (key1, Is.EqualTo (key2));
    }

    [Test]
    public void Equals_False_RequestedType ()
    {
      var key1 = CreateCompoundCacheKey (_type, "a", "b");
      var key2 = CreateCompoundCacheKey (_otherType, "a", "b");

      Assert.That (key1, Is.Not.EqualTo (key2));
    }

    [Test]
    public void Equals_False_CacheKeys ()
    {
      var key1 = CreateCompoundCacheKey (_type, "a", "b");
      var key2 = CreateCompoundCacheKey (_type, "a", "c");
      var key3 = CreateCompoundCacheKey (_type, "b", "a"); // Order matters!

      Assert.That (key1, Is.Not.EqualTo (key2));
      Assert.That (key1, Is.Not.EqualTo (key3));
    }

    [Test]
    public new void GetHashCode ()
    {
      var key1 = CreateCompoundCacheKey (_type, "a", "b");
      var key2 = CreateCompoundCacheKey (_type, "a", "b");

      Assert.That (key1.GetHashCode(), Is.EqualTo (key2.GetHashCode()));
    }

    [Test]
    public void GetHashCode_IsPreComputed ()
    {
      var keyPart = new MutableCacheKey();
      var key = new CompoundCacheKey (_type, new CacheKey[] { keyPart });

      Assert.That (keyPart.GetHashCode(), Is.Not.EqualTo (keyPart.GetHashCode()));
      Assert.That (key.GetHashCode(), Is.EqualTo (key.GetHashCode()));
    }

    private CompoundCacheKey CreateCompoundCacheKey (Type requestedType, params string[] cacheKeyContents)
    {
      var cacheKeys = cacheKeyContents.Select (k => new ContentCacheKey (k)).Cast<CacheKey>().ToArray();
      return new CompoundCacheKey (requestedType, cacheKeys);
    }

    private class MutableCacheKey : CacheKey
    {
      private int _counter;
      public override bool Equals (object other) { throw new NotImplementedException(); }
// ReSharper disable NonReadonlyFieldInGetHashCode
      public override int GetHashCode () { return ++_counter; }
// ReSharper restore NonReadonlyFieldInGetHashCode
    }
  }
}