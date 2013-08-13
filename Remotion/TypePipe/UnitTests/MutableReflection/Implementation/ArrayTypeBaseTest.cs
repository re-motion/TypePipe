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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection.Implementation;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.Reflection.MemberSignatures;
using Remotion.Text;
using Remotion.TypePipe.MutableReflection.Implementation;
using Rhino.Mocks;
using Remotion.Development.UnitTesting;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class ArrayTypeBaseTest
  {
    private const BindingFlags c_all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    public static readonly Func<MethodBase, string> NameAndSignatureProvider =
        m => string.Format ("{0}({1}), {2}", m.Name, SeparatedStringBuilder.Build (",", m.GetParameters(), p => p.Name), MethodSignature.Create (m));

    private CustomType _elementType;
    private int _rank;

    private ArrayTypeBase _type;

    [SetUp]
    public void SetUp ()
    {
      var customAttributes = new[] { CustomAttributeDeclarationObjectMother.Create () };
      _elementType = CustomTypeObjectMother.Create (name: "Abc", @namespace: "MyNs", customAttributeDatas: customAttributes);
      _rank = 2;
      var memberSelectorMock = MockRepository.GenerateStrictMock<IMemberSelector>();

      _type = new TestableArrayTypeBase (_elementType, _rank, memberSelectorMock);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_type.Name, Is.EqualTo ("Abc[,]"));
      Assert.That (_type.Namespace, Is.EqualTo ("MyNs"));
      Assert.That (_type.Attributes, Is.EqualTo (TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Serializable));
      Assert.That (_type.BaseType, Is.SameAs (typeof (Array)));
      Assert.That (_type.DeclaringType, Is.Null);
      Assert.That (_type.IsGenericType, Is.False);
      Assert.That (_type.IsGenericTypeDefinition, Is.False);
    }

    [Test]
    public void Initialization_TypeArguments ()
    {
      var elementType = CustomTypeObjectMother.Create (typeArguments: new[] { ReflectionObjectMother.GetSomeType() });
      var type = ArrayTypeBaseObjectMother.Create (elementType);

      Assert.That (type.GetGenericArguments(), Is.Empty);
    }

    [Test]
    public void GetElementType ()
    {
      Assert.That (_type.GetElementType (), Is.SameAs (_elementType));
    }

    [Test]
    public void GetArrayRank ()
    {
      var result = _type.GetArrayRank();

      Assert.That (result, Is.EqualTo (_rank));
    }

    [Test]
    public void GetCustomAttributeData ()
    {
      Assert.That (_type.GetCustomAttributeData(), Is.Empty);
    }

    [Test]
    public void Equals_Object ()
    {
      var type1 = ArrayTypeBaseObjectMother.Create (CustomTypeObjectMother.Create(), _rank);
      var type2 = ArrayTypeBaseObjectMother.Create (_elementType, 3);
      var type3 = ArrayTypeBaseObjectMother.Create (_elementType, _rank);

      // ReSharper disable CheckForReferenceEqualityInstead.1
      Assert.That (_type.Equals ((object) null), Is.False);
      // ReSharper restore CheckForReferenceEqualityInstead.1
      Assert.That (_type.Equals (new object()), Is.False);
      Assert.That (_type.Equals ((object) type1), Is.False);
      Assert.That (_type.Equals ((object) type2), Is.False);
      Assert.That (_type.Equals ((object) type3), Is.True);
    }

    [Test]
    public void Equals_Type ()
    {
      var type1 = ArrayTypeBaseObjectMother.Create (CustomTypeObjectMother.Create(), _rank);
      var type2 = ArrayTypeBaseObjectMother.Create (_elementType, 3);
      var type3 = ArrayTypeBaseObjectMother.Create (_elementType, _rank);

      // ReSharper disable CheckForReferenceEqualityInstead.1
      Assert.That (_type.Equals (null), Is.False);
      // ReSharper restore CheckForReferenceEqualityInstead.1
      Assert.That (_type.Equals (type1), Is.False);
      Assert.That (_type.Equals (type2), Is.False);
      Assert.That (_type.Equals (type3), Is.True);
    }

    [Test]
    public new void GetHashCode ()
    {
      var type = ArrayTypeBaseObjectMother.Create (_elementType, _rank);
      Assert.That (_type.GetHashCode(), Is.EqualTo (type.GetHashCode()));
    }

    [Test]
    public void IsArrayImpl ()
    {
      Assert.That (_type.IsArray, Is.True);
    }

    [Test]
    public void GetAllFields ()
    {
      var expectedFields = typeof (Array).GetFields (c_all);

      var result = _type.Invoke<IEnumerable<FieldInfo>> ("GetAllFields");

      Assert.That (result, Is.Not.Empty.And.EquivalentTo (expectedFields));
    }

    [Test]
    public void GetAllMethods ()
    {
      var expectedBaseMethods = typeof (Array).GetMethods (c_all).Select (NameAndSignatureProvider);
      var expectedDeclaredMethods =
          new[]
          {
              "Address(index0,index1), MyNs.Abc&(System.Int32,System.Int32)",
              "Get(index0,index1), MyNs.Abc(System.Int32,System.Int32)",
              "Set(index0,index1,value), System.Void(System.Int32,System.Int32,MyNs.Abc)"
          };
      var expectedMethods = expectedDeclaredMethods.Concat (expectedBaseMethods);
      
      var result = _type.Invoke<IEnumerable<MethodInfo>> ("GetAllMethods").Select (m => NameAndSignatureProvider (m));

      Assert.That (result, Is.EquivalentTo (expectedMethods));
    }

    [Test]
    public void GetAllProperties ()
    {
      var expectedProperties = typeof (Array).GetProperties (c_all);

      var result = _type.Invoke<IEnumerable<PropertyInfo>> ("GetAllProperties");

      Assert.That (result, Is.Not.Empty.And.EquivalentTo (expectedProperties));
    }

    [Test]
    public void GetAllEvents ()
    {
      Assert.That (_type.Invoke ("GetAllEvents"), Is.Empty);
    }
  }
}