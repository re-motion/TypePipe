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
using Remotion.Collections;
using Remotion.Reflection.MemberSignatures;
using Remotion.TypePipe.MutableReflection.Implementation;
using Rhino.Mocks;
using Remotion.Development.UnitTesting;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class ArrayTypeTest
  {
    private const BindingFlags c_all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    private CustomType _elementType;

    private ArrayType _type;

    [SetUp]
    public void SetUp ()
    {
      var customAttributes = new[] { CustomAttributeDeclarationObjectMother.Create () };
      _elementType = CustomTypeObjectMother.Create (
          name: "Abc", @namespace: "MyNs", typeArguments: new[] { typeof (int) }, customAttributeDatas: customAttributes);

      var memberSelectorMock = MockRepository.GenerateStrictMock<IMemberSelector>();
      _type = new ArrayType (_elementType, 1, memberSelectorMock);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_type.Name, Is.EqualTo ("Abc[]"));
      Assert.That (_type.Namespace, Is.EqualTo ("MyNs"));
      Assert.That (_type.Attributes, Is.EqualTo (TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Serializable));
      Assert.That (_type.BaseType, Is.SameAs (typeof (Array)));
      Assert.That (_type.DeclaringType, Is.Null);
      Assert.That (_type.IsGenericType, Is.False);
      Assert.That (_type.IsGenericTypeDefinition, Is.False);
      Assert.That (_type.GetGenericArguments(), Is.Empty);
    }

    [Test]
    public void Initialization_Rank ()
    {
      var type = ArrayTypeObjectMother.Create (_elementType, 3);

      Assert.That (type.Name, Is.EqualTo ("Abc[,,]"));
    }

    [Test]
    public void GetElementType ()
    {
      Assert.That (_type.GetElementType (), Is.SameAs (_elementType));
    }

    // TODO 5409
    // t.GetArrayRank

    [Test]
    public void GetCustomAttributeData ()
    {
      Assert.That (_type.GetCustomAttributeData(), Is.Empty);
    }

    [Test]
    public void IsArrayImpl ()
    {
      Assert.That (_type.IsArray, Is.True);
    }

    [Ignore ("TODO 5409")]
    [Test]
    public void GetAllInterfaces ()
    {
      var result = _type.GetInterfaces();

      var expectedInterfaces =
          new[]
          {
              typeof (ICloneable), typeof (IList), typeof (ICollection), typeof (IEnumerable),
              typeof (IList<>).MakeGenericType (_elementType),
              typeof (ICollection<>).MakeGenericType (_elementType),
              typeof (IEnumerable<>).MakeGenericType (_elementType)
          };
      Assert.That (result, Is.EqualTo (expectedInterfaces));
    }

    [Ignore ("TODO 5409")]
    [Test]
    public void GetAllFields ()
    {
      Assert.That (_type.Invoke ("GetAllFields"), Is.Empty);
    }

    [Ignore ("TODO 5409")]
    [Test]
    public void GetAllMethods ()
    {
      var expectedBaseMethods = typeof (Array).GetMethods (c_all).Select (m => new { m.Name, Signature = MethodSignature.Create (m) });
      var expectedDeclaredMethods =
          new[]
          {
              new { Name = "Set", Signature = new MethodSignature (typeof (void), new[] { typeof (int), _elementType }, 0) },
              new { Name = "Address", Signature = new MethodSignature (_elementType.MakeByRefType(), new[] { typeof (int) }, 0) },
              new { Name = "Get", Signature = new MethodSignature (_elementType, new[] { typeof (int) }, 0) },
              new { Name = "ToString", Signature = new MethodSignature (typeof (string), Type.EmptyTypes, 0) },
              new { Name = "Equals", Signature = new MethodSignature (typeof (bool), new[] { typeof (object) }, 0) },
              new { Name = "GetHashCode", Signature = new MethodSignature (typeof (int), Type.EmptyTypes, 0) },
              new { Name = "GetType", Signature = new MethodSignature (typeof (Type), Type.EmptyTypes, 0) },
              new { Name = "Finalize", Signature = new MethodSignature (typeof (void), Type.EmptyTypes, 0) },
              new { Name = "MemberwiseClone", Signature = new MethodSignature (typeof (object), Type.EmptyTypes, 0) },
          };
      var expectedMethods = expectedDeclaredMethods.Concat (expectedBaseMethods);

      var result = _type.GetMethods (c_all).Select (m => new { m.Name, Signature = MethodSignature.Create (m) });

      Assert.That (result, Is.EquivalentTo (expectedMethods));
    }

    [Ignore ("TODO 5409")]
    [Test]
    public void GetAllEvents ()
    {
      Assert.That (_type.Invoke ("GetAllEvents"), Is.Empty);
    }
  }
}