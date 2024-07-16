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
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection.MemberSignatures;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class ArrayTypeBaseTest
  {
    private const BindingFlags c_all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    public static readonly Func<MethodBase, string> NameAndSignatureProvider =
        m => string.Format ("{0}({1}), {2}", m.Name, string.Join (",", m.GetParameters().Select (p => p.Name)), MethodSignature.Create (m));

    private CustomType _elementType;
    private int _rank;

    private ArrayTypeBase _type;

    private Type _realArrayTypeForComparison;

    [SetUp]
    public void SetUp ()
    {
      var customAttributes = new[] { CustomAttributeDeclarationObjectMother.Create () };
      _elementType = CustomTypeObjectMother.Create (name: "Abc", @namespace: "MyNs", customAttributeDatas: customAttributes);
      _rank = 2;

      _type = new TestableArrayTypeBase (_elementType, _rank);

      _realArrayTypeForComparison = typeof (string[]);
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

      Assert.That (_type.Equals ((object) null), Is.False);
      Assert.That (_type.Equals (new object()), Is.False);
      Assert.That (_type.Equals ((object) type1), Is.False);
      Assert.That (_type.Equals ((object) type2), Is.False);
      Assert.That (_type.Equals ((object) type3), Is.True);

      var multiDimensionalArrayOfRank1 = MultiDimensionalArrayTypeObjectMother.Create(_elementType, 1);
      var vector = VectorTypeObjectMother.Create(_elementType);
      Assert.That (multiDimensionalArrayOfRank1.Equals ((object) vector), Is.False);
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

      var multiDimensionalArrayOfRank1 = MultiDimensionalArrayTypeObjectMother.Create(_elementType, 1);
      var vector = VectorTypeObjectMother.Create(_elementType);
      Assert.That(multiDimensionalArrayOfRank1.Equals(vector), Is.False);
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
    public void GetAllNestedType ()
    {
      Assert.That (_realArrayTypeForComparison.GetNestedTypes (c_all), Is.Empty);

      Assert.That (_type.GetAllNestedTypes(), Is.Empty);
    }

    [Test]
    public void GetAllFields ()
    {
      Assert.That (_realArrayTypeForComparison.GetFields (c_all), Is.Empty);

      Assert.That (_type.GetAllFields().ToArray(), Is.Empty);
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

      var result = _type.GetAllMethods().Select (m => NameAndSignatureProvider (m)).ToArray();

      Assert.That (result, Is.EquivalentTo (expectedMethods));
      // TODO: An array type has different methods (fewer!) than it's Array base class.
      //Assert.That (result.Length, Is.EqualTo (_realArrayTypeForComparison.GetMethods (c_all).Length));
    }

    [Test]
    public void GetAllProperties ()
    {
      // TODO
      //var expectedProperties = _realArrayTypeForComparison.GetProperties (c_all);
      var expectedProperties = typeof (Array).GetProperties (c_all);

      var result = _type.GetAllProperties();

      Assert.That (result, Is.Not.Empty.And.EquivalentTo (expectedProperties));
    }

    [Test]
    public void GetAllEvents ()
    {
      Assert.That(_realArrayTypeForComparison.GetEvents(c_all), Is.Empty);

      Assert.That (_type.GetAllEvents(), Is.Empty);
    }
  }
}