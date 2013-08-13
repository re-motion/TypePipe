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
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class ByRefTypeTest
  {
    private CustomType _elementType;

    private ByRefType _type;

    [SetUp]
    public void SetUp ()
    {
      var customAttributes = new[] { CustomAttributeDeclarationObjectMother.Create() };
      _elementType = CustomTypeObjectMother.Create (
          name: "Abc", @namespace: "MyNs", typeArguments: new[] { typeof (int) }, customAttributeDatas: customAttributes);

      var memberSelectorMock = MockRepository.GenerateStrictMock<IMemberSelector>();
      _type = new ByRefType (_elementType, memberSelectorMock);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_type.Name, Is.EqualTo ("Abc&"));
      Assert.That (_type.Namespace, Is.EqualTo ("MyNs"));
      Assert.That (_type.Attributes, Is.EqualTo (TypeAttributes.NotPublic));
      Assert.That (_type.BaseType, Is.Null);
      Assert.That (_type.DeclaringType, Is.Null);
      Assert.That (_type.IsGenericType, Is.False);
      Assert.That (_type.IsGenericTypeDefinition, Is.False);
      Assert.That (_type.GetGenericArguments(), Is.Empty);
    }

    [Test]
    public void GetElementType ()
    {
      Assert.That (_type.GetElementType(), Is.SameAs (_elementType));
    }

    [Test]
    public void GetCustomAttributeData ()
    {
      Assert.That (_type.GetCustomAttributeData(), Is.Empty);
    }

    [Test]
    public void IsByRef ()
    {
      Assert.That (_type.IsByRef, Is.True);
    }

    [Test]
    public void GetAllXXX ()
    {
      Assert.That (_type.GetAllNestedTypes(), Is.Empty);
      Assert.That (_type.GetAllInterfaces(), Is.Empty);
      Assert.That (_type.GetAllFields(), Is.Empty);
      Assert.That (_type.GetAllConstructors(), Is.Empty);
      Assert.That (_type.GetAllMethods(), Is.Empty);
      Assert.That (_type.GetAllProperties(), Is.Empty);
      Assert.That (_type.GetAllEvents(), Is.Empty);
    }

    [Test]
    public void UnsupportedMembers ()
    {
      UnsupportedMemberTestHelper.CheckMethod (() => _type.MakeByRefType(), "MakeByRefType");
      UnsupportedMemberTestHelper.CheckMethod (() => _type.MakeArrayType(), "MakeArrayType");
      UnsupportedMemberTestHelper.CheckMethod (() => _type.MakeArrayType (7), "MakeArrayType");
    }
  }
}