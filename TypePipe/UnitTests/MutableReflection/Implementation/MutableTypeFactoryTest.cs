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
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Development.UnitTesting.Expressions;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class MutableTypeFactoryTest
  {
    private MutableTypeFactory _factory;

    private Type _domainType;
    private Type _declaringType;

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
      var declaringType = MutableTypeObjectMother.Create();

      var result = _factory.CreateType (name, @namespace, attributes, baseType, declaringType);

      Assert.That (result.Name, Is.EqualTo (name));
      Assert.That (result.Namespace, Is.EqualTo (@namespace));
      Assert.That (result.Attributes, Is.EqualTo (attributes));
      Assert.That (result.BaseType, Is.SameAs (baseType));
      Assert.That (result.DeclaringType, Is.SameAs (declaringType));
    }

    [Test]
    public void CreateType_NamespaceCanBeNull ()
    {
      var result = _factory.CreateType ("Name", null, TypeAttributes.Class, typeof (object), null);

      Assert.That (result.Namespace, Is.Null);
    }

    [Test]
    public void CreateType_DeclaringTypeCanBeNull ()
    {
      var result = _factory.CreateType ("Name", "ns", TypeAttributes.Class, typeof (object), declaringType: null);

      Assert.That (result.DeclaringType, Is.Null);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Base type cannot be null.\r\nParameter name: baseType")]
    public void CreateType_BaseType_CannotBeNull ()
    {
      _factory.CreateType ("Name", "ns", TypeAttributes.Class, baseType: null, declaringType: null);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Base type must be null for interfaces.\r\nParameter name: baseType")]
    public void CreateType_BaseType_MustBeNullForInterfaces ()
    {
      _factory.CreateType ("Name", "ns", TypeAttributes.Interface, typeof (object), null);
    }

    [Test]
    public void CreateType_SpecialBaseType ()
    {
      CheckCreateType (typeof (Enum));
      CheckCreateType (typeof (Delegate));
      CheckCreateType (typeof (MulticastDelegate));
      CheckCreateType (typeof (ValueType));
      CheckCreateType (typeof (List<int>));
    }

    [Test]
    public void CreateType_ThrowsIfBaseTypeCannotBeSubclassed ()
    {
      CheckThrowsForInvalidBaseType (typeof (string));
      CheckThrowsForInvalidBaseType (typeof (int));
      CheckThrowsForInvalidBaseType (typeof (IDisposable));
      CheckThrowsForInvalidBaseType (typeof (ExpressionType));
      CheckThrowsForInvalidBaseType (typeof (List<>));
      CheckThrowsForInvalidBaseType (typeof (List<>).GetGenericArguments().Single());
      CheckThrowsForInvalidBaseType (typeof (int).MakeArrayType());
      CheckThrowsForInvalidBaseType (typeof (int).MakeByRefType());
      CheckThrowsForInvalidBaseType (typeof (int).MakePointerType());
      CheckThrowsForInvalidBaseType (typeof (TypeWithoutAccessibleConstructor));
    }

    [Test]
    public void CreateProxy ()
    {
      var result = _factory.CreateProxy (_domainType, ProxyKind.AssembledType);

      var type = result.Type;
      Assert.That (type.BaseType, Is.SameAs (_domainType));
      Assert.That (type.Name, Is.EqualTo (@"DomainType_AssembledTypeProxy_1"));
      Assert.That (type.Namespace, Is.EqualTo ("Remotion.TypePipe.UnitTests.MutableReflection.Implementation"));
      Assert.That (type.Attributes, Is.EqualTo (TypeAttributes.Public | TypeAttributes.BeforeFieldInit));
      Assert.That (type.DeclaringType, Is.Null);

      Assert.That (result, Is.TypeOf<ProxyTypeModificationTracker> ());
      Assert.That (result.As<ProxyTypeModificationTracker> ().ConstructorBodies, Is.EqualTo (type.AddedConstructors.Select (c => c.Body)));
    }

    [Test]
    public void CreateProxy_UsesProxyKindToDetermineName ()
    {
      var result = _factory.CreateProxy (_domainType, ProxyKind.AdditionalType);

      var type = result.Type;
      Assert.That (type.Name, Is.EqualTo (@"DomainType_AdditionalTypeProxy_1"));
    }

    [Test]
    public void CreateProxy_UniqueNames ()
    {
      var result1 = _factory.CreateProxy (_domainType, ProxyKind.AssembledType).Type;
      var result2 = _factory.CreateProxy (_domainType, ProxyKind.AssembledType).Type;

      Assert.That (result1.Name, Is.Not.EqualTo (result2.Name));
    }

    [Test]
    public void CreateProxy_Serializable ()
    {
      var result = _factory.CreateProxy (typeof (SerializableType), ProxyKind.AssembledType).Type;

      Assert.That (result.IsSerializable, Is.True);
    }

    [Test]
    public void CreateProxy_CopiesAccessibleInstanceConstructors_WithPublicVisibility ()
    {
      var result = _factory.CreateProxy (_domainType, ProxyKind.AssembledType).Type;

      Assert.That (result.AddedConstructors, Has.Count.EqualTo (1));
      var ctor = result.AddedConstructors.Single();
      Assert.That (ctor.IsStatic, Is.False);
      Assert.That (ctor.IsPublic, Is.True, "Changed from 'family or assembly'.");
      Assert.That (ctor.IsHideBySig, Is.True);

      var parameter = ctor.GetParameters().Single();
      CustomParameterInfoTest.CheckParameter (parameter, ctor, 0, "i", typeof (int).MakeByRefType(), ParameterAttributes.Out);

      var baseCtor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainType (out Dev<int>.Dummy));
      var expectedBody = Expression.Call (
          new ThisExpression (result), NonVirtualCallMethodInfoAdapter.Adapt (baseCtor), ctor.ParameterExpressions.Cast<Expression>());
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, ctor.Body);
    }

    private void CheckCreateType (Type validBaseType)
    {
      Assert.That (() => _factory.CreateType ("t", "ns", TypeAttributes.Class, validBaseType, null), Throws.Nothing);
    }

    private void CheckThrowsForInvalidBaseType (Type invalidBaseType)
    {
      var message = "Base type must not be sealed, an interface, an array, a byref type, a pointer, "
                    + "a generic parameter, contain generic parameters and must have an accessible constructor.\r\nParameter name: baseType";
      Assert.That (() => _factory.CreateType ("t", "ns", TypeAttributes.Class, invalidBaseType, null), Throws.ArgumentException.With.Message.EqualTo (message));
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