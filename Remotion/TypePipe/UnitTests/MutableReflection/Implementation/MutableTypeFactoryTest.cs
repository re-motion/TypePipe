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
using System.Reflection;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.UnitTests.Expressions;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class MutableTypeFactoryTest
  {
    private MutableTypeFactory _factory;

    private Type _domainType;

    [SetUp]
    public void SetUp ()
    {
      _factory = new MutableTypeFactory();

      _domainType = typeof (DomainType);
    }

    [Test]
    public void CreateType ()
    {
      var name = "MyName";
      var @namespace = "MyNamespace";
      var attributes = (TypeAttributes) 7;
      var baseType = ReflectionObjectMother.GetSomeSubclassableType();

      var result = _factory.CreateType (name, @namespace, attributes, baseType);

      Assert.That (result.Name, Is.EqualTo (name));
      Assert.That (result.Namespace, Is.EqualTo (@namespace));
      Assert.That (result.Attributes, Is.EqualTo (attributes));
      Assert.That (result.BaseType, Is.SameAs (baseType));
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException), ExpectedMessage = "Value cannot be null.\r\nParameter name: baseType")]
    public void CreateType_Class_BaseTypeCannotBeNull ()
    {
      _factory.CreateType ("t", "ns", TypeAttributes.Class, baseType: null);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Interfaces cannot have a base type.\r\nParameter name: baseType")]
    public void CreateType_Interface_BaseTypeMustBeNull ()
    {
      _factory.CreateType ("i", "ns", TypeAttributes.Interface, baseType: ReflectionObjectMother.GetSomeSubclassableType());
    }

    [Test]
    public void CreateType_ThrowsIfClassAndBaseTypeCannotBeSubclassed ()
    {
      CheckThrowsForInvalidBaseType (typeof (string));
      CheckThrowsForInvalidBaseType (typeof (int));
      CheckThrowsForInvalidBaseType (typeof (ExpressionType));
      CheckThrowsForInvalidBaseType (typeof (Delegate));
      CheckThrowsForInvalidBaseType (typeof (MulticastDelegate));
      CheckThrowsForInvalidBaseType (typeof (List<>));
      CheckThrowsForInvalidBaseType (typeof (List<>).GetGenericArguments ().Single ());
      CheckThrowsForInvalidBaseType (typeof (int).MakeArrayType ());
      CheckThrowsForInvalidBaseType (typeof (int).MakeByRefType ());
      CheckThrowsForInvalidBaseType (typeof (int).MakePointerType());
      CheckThrowsForInvalidBaseType (typeof (TypeWithoutAccessibleConstructor));

      Assert.That (() => _factory.CreateType ("t", "ns", TypeAttributes.Class, typeof (List<int>)), Throws.Nothing);
    }

    [Test]
    public void CreateProxyType ()
    {
      var result = _factory.CreateProxyType (_domainType);

      Assert.That (result.BaseType, Is.SameAs (_domainType));
      Assert.That (result.Name, Is.EqualTo (@"DomainType_Proxy1"));
      Assert.That (result.Namespace, Is.EqualTo ("Remotion.TypePipe.UnitTests.MutableReflection.Implementation"));
      Assert.That (result.Attributes, Is.EqualTo (TypeAttributes.Public | TypeAttributes.BeforeFieldInit));
    }

    [Test]
    public void CreateProxyType_UniqueNames ()
    {
      var result1 = _factory.CreateProxyType (_domainType);
      var result2 = _factory.CreateProxyType (_domainType);

      Assert.That (result1.Name, Is.Not.EqualTo (result2.Name));
    }

    [Test]
    public void CreateProxyType_Serializable ()
    {
      var result = _factory.CreateProxyType (typeof (SerializableType));

      Assert.That (result.IsSerializable, Is.True);
    }

    [Test]
    public void CreateProxyType_CopiesAccessibleInstanceConstructors ()
    {
      var result = _factory.CreateProxyType (_domainType);

      Assert.That (result.AddedConstructors, Has.Count.EqualTo (1));

      var ctor = result.AddedConstructors.Single();
      Assert.That (ctor.IsStatic, Is.False);
      Assert.That (ctor.IsFamily, Is.True);
      Assert.That (ctor.IsAssembly, Is.False);

      var parameter = ctor.GetParameters().Single();
      CustomParameterInfoTest.CheckParameter (parameter, ctor, 0, "i", typeof (int).MakeByRefType(), ParameterAttributes.Out);

      var baseCtor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainType (out Dev<int>.Dummy));
      var expectedBody = Expression.Call (
          new ThisExpression (result), NonVirtualCallMethodInfoAdapter.Adapt (baseCtor), ctor.ParameterExpressions.Cast<Expression>());
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, ctor.Body);
    }

    private void CheckThrowsForInvalidBaseType (Type invalidBaseType)
    {
      var message = "Base type must not be sealed, an interface, a value type, an enum, a delegate, an array, a byref type, a pointer, "
                    + "a generic parameter, contain generic parameters and must have an accessible constructor.\r\nParameter name: baseType";
      Assert.That (() => _factory.CreateType ("t", "ns", TypeAttributes.Class, invalidBaseType), Throws.ArgumentException.With.Message.EqualTo (message));
    }

    public class DomainType
    {
      static DomainType() { }

      protected internal DomainType (out int i) { i = 7; }
      internal DomainType (string inaccessible) { Dev.Null = inaccessible; }
    }

    [Serializable]
    public class SerializableType { }

    public class TypeWithoutAccessibleConstructor
    {
      internal TypeWithoutAccessibleConstructor () { }
    }
  }
}