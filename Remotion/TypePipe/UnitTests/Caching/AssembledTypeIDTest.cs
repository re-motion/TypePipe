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
  public class AssembledTypeIDTest
  {
    private AssembledTypeID _id1;
    private AssembledTypeID _id2;
    private AssembledTypeID _id3;
    private AssembledTypeID _id4;

    [SetUp]
    public void SetUp ()
    {
      _id1 = new AssembledTypeID (typeof (int), new object[] { 1, 2 });
      _id2 = new AssembledTypeID (typeof (string), new object[] { 1, 2 });
      _id3 = new AssembledTypeID (typeof (int), new object[] { 1, 3 });
      _id4 = new AssembledTypeID (typeof (int), new object[] { 1, 2 });
    }

    [Test]
    public void IsStruct_ForPerformance ()
    {
      Assert.That (typeof (AssembledTypeID).IsValueType, Is.True);
    }

    [Test]
    public void Equals ()
    {
      Assert.That (_id1, Is.Not.EqualTo (_id2));
      Assert.That (_id1, Is.Not.EqualTo (_id3));
      Assert.That (_id1, Is.EqualTo (_id4));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException))]
    public void Equals_Object ()
    {
      Dev.Null = _id1.Equals (null);
    }

    [Test]
    public new void GetHashCode ()
    {
      // TODO 5552: remove
      // Usually testing for different hash-codes is a bad idea, but our peformance depends on it.
      Assert.That (_id1.GetHashCode (), Is.Not.EqualTo (_id2.GetHashCode ()));
      Assert.That (_id1.GetHashCode (), Is.Not.EqualTo (_id3.GetHashCode ()));

      Assert.That (_id1.GetHashCode (), Is.EqualTo (_id4.GetHashCode ()));
    }
  }
}