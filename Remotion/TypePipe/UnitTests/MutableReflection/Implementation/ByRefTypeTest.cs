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
  public class ByRefTypeTest
  {
    private CustomType _elementType;

    private ByRefType _byRefType;

    [SetUp]
    public void SetUp ()
    {
      _elementType = CustomTypeObjectMother.Create (name: "Abc", @namespace: "MyNs", fullName: "Full", typeArguments: new[] { typeof (int) });

      _byRefType = ByRefTypeObjectMother.Create (_elementType);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_byRefType.Name, Is.EqualTo ("Abc&"));
      Assert.That (_byRefType.Namespace, Is.EqualTo ("MyNs"));
      Assert.That (_byRefType.FullName, Is.EqualTo ("Full&"));
      Assert.That (_byRefType.Attributes, Is.EqualTo (TypeAttributes.NotPublic));
      Assert.That (_byRefType.IsGenericType, Is.False);
      Assert.That (_byRefType.IsGenericTypeDefinition, Is.False);
      Assert.That (_byRefType.GetGenericArguments(), Is.Empty);
    }

    [Test]
    public void IsByRef ()
    {
      Assert.That (_byRefType.IsByRef, Is.True);
    }

    [Test]
    public void GetElementType ()
    {
      Assert.That (_byRefType.GetElementType(), Is.SameAs (_elementType));
    }

    [Test]
    public void GetAllXXX ()
    {
      Assert.That (_byRefType.Invoke ("GetAllInterfaces"), Is.Empty);
      Assert.That (_byRefType.Invoke ("GetAllFields"), Is.Empty);
      Assert.That (_byRefType.Invoke ("GetAllConstructors"), Is.Empty);
      Assert.That (_byRefType.Invoke ("GetAllMethods"), Is.Empty);
      Assert.That (_byRefType.Invoke ("GetAllProperties"), Is.Empty);
      Assert.That (_byRefType.Invoke ("GetAllEvents"), Is.Empty);
    }

    [Test]
    public void UnsupportedMembers ()
    {
      UnsupportedMemberTestHelper.CheckMethod (() => _byRefType.MakeByRefType(), "MakeByRefType");
      UnsupportedMemberTestHelper.CheckMethod (() => _byRefType.MakeArrayType(), "MakeArrayType");
    }
  }
}