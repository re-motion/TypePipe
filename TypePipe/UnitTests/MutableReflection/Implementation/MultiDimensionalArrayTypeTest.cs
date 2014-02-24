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
using System.Collections;
using System.Linq;
using NUnit.Framework;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class MultiDimensionalArrayTypeTest
  {
    private int _rank;
    private CustomType _elementType;

    private MultiDimensionalArrayType _type;

    [SetUp]
    public void SetUp ()
    {
      _elementType = CustomTypeObjectMother.Create();
      _rank = 2;

      _type = new MultiDimensionalArrayType (_elementType, _rank);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_type.GetElementType(), Is.SameAs (_elementType));
      Assert.That (_type.GetArrayRank(), Is.EqualTo (2));
    }

    [Test]
    public void GetAllInterfaces ()
    {
      var expectedInterfaces =
          new[]
          {
              typeof (ICloneable), typeof (IList), typeof (ICollection), typeof (IEnumerable), typeof (IStructuralComparable),
              typeof (IStructuralEquatable)
          };

      var result = _type.GetInterfaces();

      Assert.That (result, Is.EquivalentTo (expectedInterfaces));
    }

    [Test]
    public void GetAllConstructors ()
    {
      var expectedConstructors =
          new[]
          {
              ".ctor(length0,length1), System.Void(System.Int32,System.Int32)",
              ".ctor(lowerBound0,length0,lowerBound1,length1), System.Void(System.Int32,System.Int32,System.Int32,System.Int32)",
          };

      var result = _type.GetAllConstructors().Select (c => ArrayTypeBaseTest.NameAndSignatureProvider (c));

      Assert.That (result, Is.EqualTo (expectedConstructors));
    }
  }
}