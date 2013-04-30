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
using Remotion.TypePipe.Caching;

namespace Remotion.TypePipe.UnitTests.Caching
{
  [TestFixture]
  public class CompoundIdentifierEqualityComparerTest
  {
    private CompoundIdentifierEqualityComparer _comparer;

    [SetUp]
    public void SetUp ()
    {
      _comparer = new CompoundIdentifierEqualityComparer();
    }

    [Test]
    public void Equals_True ()
    {
      var key1 = new object[] { 1, 2 };
      var key2 = new object[] { 1, 2 };
      var key3 = new object[] { 1, null, 3 };
      var key4 = new object[] { 1, null, 3 };

      Assert.That (_comparer.Equals (key1, key2), Is.True);
      Assert.That (_comparer.Equals (key3, key4), Is.True);
    }

    [Test]
    public void Equals_False ()
    {
      var key1 = new object[] { 1, 2 }; // Value types.

      var key2 = new object[] { 1, 3 };
      var key3 = new object[] { 1, null };
      var key4 = new object[] { 2, 1 }; // Order matters!

      var key5 = new[] { "", new object() }; // Reference types.
      var key6 = new[] { "", new object() };

      Assert.That (_comparer.Equals (key1, key2), Is.False);
      Assert.That (_comparer.Equals (key1, key3), Is.False);
      Assert.That (_comparer.Equals (key3, key1), Is.False); // Null on other side.
      Assert.That (_comparer.Equals (key1, key4), Is.False);
      Assert.That (_comparer.Equals (key5, key6), Is.False);
    }

    [Test]
    public new void GetHashCode ()
    {
      var key1 = new object[] { 1, 2 };
      var key2 = new object[] { 1, 2 };

      Assert.That (_comparer.GetHashCode (key1), Is.EqualTo (_comparer.GetHashCode (key2)));
    }
  }
}