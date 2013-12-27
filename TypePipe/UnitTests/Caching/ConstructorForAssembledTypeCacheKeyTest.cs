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
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.Caching;

namespace Remotion.TypePipe.UnitTests.Caching
{
  [TestFixture]
  public class ConstructorForAssembledTypeCacheKeyTest
  {
    private ConstructorForAssembledTypeCacheKey _key1;
    private ConstructorForAssembledTypeCacheKey _key2;
    private ConstructorForAssembledTypeCacheKey _key3;
    private ConstructorForAssembledTypeCacheKey _key4;
    private ConstructorForAssembledTypeCacheKey _key5;

    [SetUp]
    public void SetUp ()
    {
      _key1 = new ConstructorForAssembledTypeCacheKey (typeof (int), typeof (Action), true);
      _key2 = new ConstructorForAssembledTypeCacheKey (typeof (Random), typeof (Action), true);
      _key3 = new ConstructorForAssembledTypeCacheKey (typeof (int), typeof (Func<int>), true);
      _key4 = new ConstructorForAssembledTypeCacheKey (typeof (int), typeof (Action), false);
      _key5 = new ConstructorForAssembledTypeCacheKey (typeof (int), typeof (Action), true);
    }

    [Test]
    public void IsStruct_ForPerformance ()
    {
      Assert.That (typeof (ConstructorForAssembledTypeCacheKey).IsValueType, Is.True);
    }

    [Test]
    public void Equals ()
    {
      Assert.That (_key1, Is.Not.EqualTo (_key2));
      Assert.That (_key1, Is.Not.EqualTo (_key3));
      Assert.That (_key1, Is.Not.EqualTo (_key4));
      Assert.That (_key1, Is.EqualTo (_key5));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException))]
    public void Equals_Object ()
    {
      Dev.Null = _key1.Equals (new object());
    }

    [Test]
    public new void GetHashCode ()
    {
      Assert.That (_key1.GetHashCode(), Is.EqualTo (_key5.GetHashCode()));
    }
  }
}