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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.Reflection.MemberSignatures;
using Remotion.Text;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Rhino.Mocks;
using Remotion.Development.UnitTesting;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class MultiDimensionalArrayTypeTest
  {
    private const BindingFlags c_all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    private static readonly Func<MethodBase, string> s_nameAndSignatureProvider =
        m => string.Format ("{0}({1}), {2}", m.Name, SeparatedStringBuilder.Build (",", m.GetParameters(), p => p.Name), MethodSignature.Create (m));

    private CustomType _elementType;
    private int _rank;

    private MultiDimensionalArrayType _type;

    [SetUp]
    public void SetUp ()
    {
      var customAttributes = new[] { CustomAttributeDeclarationObjectMother.Create () };
      _elementType = CustomTypeObjectMother.Create (name: "Abc", @namespace: "MyNs", customAttributeDatas: customAttributes);
      _rank = 2;
      var memberSelectorMock = MockRepository.GenerateStrictMock<IMemberSelector>();

      _type = new MultiDimensionalArrayType (_elementType, _rank, memberSelectorMock);
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
      var type = ArrayTypeObjectMother.Create (elementType);

      Assert.That (type.GetGenericArguments (), Is.Empty);
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
    public void IsArrayImpl ()
    {
      Assert.That (_type.IsArray, Is.True);
    }

    [Test]
    public void GetAllInterfaces ()
    {
      var result = _type.GetInterfaces();

      var expectedInterfaces =
          new[]
          {
              typeof (ICloneable), typeof (IList), typeof (ICollection), typeof (IEnumerable),
              typeof (IList<>).MakeTypePipeGenericType (_elementType),
              typeof (ICollection<>).MakeTypePipeGenericType (_elementType),
              typeof (IEnumerable<>).MakeTypePipeGenericType (_elementType)
          };
      Assert.That (result, Is.EquivalentTo (expectedInterfaces));
    }

    [Test]
    public void GetAllFields ()
    {
      Assert.That (_type.Invoke ("GetAllFields"), Is.Empty);
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

      var result = _type.Invoke<IEnumerable<ConstructorInfo>> ("GetAllConstructors").Select (c => s_nameAndSignatureProvider (c));

      Assert.That (result, Is.EqualTo (expectedConstructors));
    }

    [Test]
    public void GetAllMethods ()
    {
      var expectedBaseMethods = typeof (Array).GetMethods (c_all).Select (s_nameAndSignatureProvider);
      var expectedDeclaredMethods =
          new[]
          {
              "Address(index0,index1), MyNs.Abc&(System.Int32,System.Int32)",
              "Get(index0,index1), MyNs.Abc(System.Int32,System.Int32)",
              "Set(index0,index1,value), System.Void(System.Int32,System.Int32,MyNs.Abc)"
          };
      var expectedMethods = expectedDeclaredMethods.Concat (expectedBaseMethods);
      
      var result = _type.Invoke<IEnumerable<MethodInfo>> ("GetAllMethods").Select (m => s_nameAndSignatureProvider (m));

      Assert.That (result, Is.EquivalentTo (expectedMethods));
    }
    
    [Test]
    public void GetAllProperties ()
    {
      Assert.That (_type.Invoke ("GetAllProperties"), Is.Empty);
    }

    [Test]
    public void GetAllEvents ()
    {
      Assert.That (_type.Invoke ("GetAllEvents"), Is.Empty);
    }
  }
}