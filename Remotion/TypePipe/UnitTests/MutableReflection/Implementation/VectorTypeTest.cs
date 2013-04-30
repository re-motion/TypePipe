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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class VectorTypeTest
  {
    private CustomType _elementType;

    private VectorType _type;

    [SetUp]
    public void SetUp ()
    {
      _elementType = CustomTypeObjectMother.Create();
      var memberSelectorMock = MockRepository.GenerateStrictMock<IMemberSelector>();

      _type = new VectorType (_elementType, memberSelectorMock);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_type.GetElementType(), Is.SameAs (_elementType));
      Assert.That (_type.GetArrayRank(), Is.EqualTo (1));
    }

    [Test]
    public void GetAllInterfaces ()
    {
      var result = _type.GetInterfaces ();

      var expectedInterfaces =
          new[]
          {
              typeof (ICloneable), typeof (IList), typeof (ICollection), typeof (IEnumerable),
              typeof (IStructuralComparable),
              typeof (IStructuralEquatable),
              typeof (IList<>).MakeTypePipeGenericType (_elementType),
              typeof (ICollection<>).MakeTypePipeGenericType (_elementType),
              typeof (IEnumerable<>).MakeTypePipeGenericType (_elementType)
          };
      Assert.That (result, Is.EquivalentTo (expectedInterfaces));
    }

    [Test]
    public void GetAllConstructors ()
    {
      var expectedConstructors = new[] { ".ctor(length), System.Void(System.Int32)" };

      var result = _type.Invoke<IEnumerable<ConstructorInfo>> ("GetAllConstructors").Select (c => ArrayTypeBaseTest.NameAndSignatureProvider (c));

      Assert.That (result, Is.EqualTo (expectedConstructors));
    }
  }
}