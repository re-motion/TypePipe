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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection.Implementation.MemberFactory;
using Remotion.TypePipe.UnitTests.Expressions;
using Rhino.Mocks;
using Remotion.FunctionalProgramming;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation.MemberFactory
{
  [TestFixture]
  public class PropertyFactoryTest
  {
    private IMethodFactory _methodFactoryMock;

    private PropertyFactory _factory;

    private ProxyType _proxyType;

    [SetUp]
    public void SetUp ()
    {
      _methodFactoryMock = MockRepository.GenerateStrictMock<IMethodFactory>();

      _factory = new PropertyFactory (_methodFactoryMock);

      _proxyType = ProxyTypeObjectMother.Create();
    }

    [Test]
    public void CreateProperty_Providers ()
    {
      var name = "Property";
      var type = ReflectionObjectMother.GetSomeType();
      var indexParameters = ParameterDeclarationObjectMother.CreateMultiple (2).ToList();
      var accessorAttributes = (MethodAttributes) 7;
      var setterParameters = indexParameters.Concat (ParameterDeclarationObjectMother.Create (type, "value")).ToList();
      Func<MethodBodyCreationContext, Expression> getBodyProvider = ctx => null;
      Func<MethodBodyCreationContext, Expression> setBodyProvider = ctx => null;

      var fakeGetMethod = MutableMethodInfoObjectMother.Create (returnType: type, parameters: indexParameters);
      var fakeSetMethod = MutableMethodInfoObjectMother.Create (parameters: setterParameters);

      _methodFactoryMock
          .Expect (
              mock => mock.CreateMethod (
                  Arg.Is (_proxyType),
                  Arg.Is ("get_Property"),
                  Arg.Is (accessorAttributes | MethodAttributes.SpecialName),
                  Arg.Is (type),
                  Arg<IEnumerable<ParameterDeclaration>>.Is.Anything,
                  Arg.Is (getBodyProvider)))
          .WhenCalled (
              mi =>
              {
                var parameters = ((IEnumerable<ParameterDeclaration>) mi.Arguments[4]).ToList();
                Assert.That (parameters.Select (p => p.Type), Is.EqualTo (indexParameters.Select (p => p.Type)));
                Assert.That (parameters.Select (p => p.Name), Is.EqualTo (indexParameters.Select (p => p.Name)));
              })
          .Return (fakeGetMethod);
      _methodFactoryMock
          .Expect (
              mock => mock.CreateMethod (
                  Arg.Is (_proxyType),
                  Arg.Is ("set_Property"),
                  Arg.Is (accessorAttributes | MethodAttributes.SpecialName),
                  Arg.Is (typeof (void)),
                  Arg<IEnumerable<ParameterDeclaration>>.Is.Anything,
                  Arg.Is (setBodyProvider)))
          .WhenCalled (
              mi =>
              {
                var parameters = ((IEnumerable<ParameterDeclaration>) mi.Arguments[4]).ToList();
                Assert.That (parameters.Select (p => p.Type), Is.EqualTo (setterParameters.Select (p => p.Type)));
                Assert.That (parameters.Select (p => p.Name), Is.EqualTo (setterParameters.Select (p => p.Name)));
              })
          .Return (fakeSetMethod);

      var result = _factory.CreateProperty (_proxyType, name, type, indexParameters.AsOneTime(), accessorAttributes, getBodyProvider, setBodyProvider);

      _methodFactoryMock.VerifyAllExpectations();
      Assert.That (result.DeclaringType, Is.SameAs (_proxyType));
      Assert.That (result.Name, Is.EqualTo (name));
      Assert.That (result.Attributes, Is.EqualTo (PropertyAttributes.None));
      Assert.That (result.PropertyType, Is.SameAs (type));
      Assert.That (result.MutableGetMethod, Is.SameAs (fakeGetMethod));
      Assert.That (result.MutableSetMethod, Is.SameAs (fakeSetMethod));
    }

    [Test]
    public void CreateProperty_Providers_ReadOnly ()
    {
      var type = ReflectionObjectMother.GetSomeType ();
      Func<MethodBodyCreationContext, Expression> getBodyProvider = ctx => ExpressionTreeObjectMother.GetSomeExpression (type);
      var fakeGetMethod = MutableMethodInfoObjectMother.Create (returnType: type);
      _methodFactoryMock
          .Stub (stub => stub.CreateMethod (null, null, 0, null, null, null)).IgnoreArguments ()
          .WhenCalled (mi => Assert.That (mi.Arguments[1], Is.EqualTo ("get_Property")))
          .Return (fakeGetMethod);

      var result = _factory.CreateProperty (
          _proxyType, "Property", type, ParameterDeclaration.None, 0, getBodyProvider: getBodyProvider, setBodyProvider: null);

      Assert.That (result.MutableSetMethod, Is.Null);
    }

    [Test]
    public void CreateProperty_Providers_WriteOnly ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      Func<MethodBodyCreationContext, Expression> setBodyProvider = ctx => ExpressionTreeObjectMother.GetSomeExpression (typeof (void));
      var fakeSetMethod = MutableMethodInfoObjectMother.Create (parameters: new[] { ParameterDeclarationObjectMother.Create (type) });
      _methodFactoryMock
          .Stub (stub => stub.CreateMethod (null, null, 0, null, null, null)).IgnoreArguments()
          .WhenCalled (mi => Assert.That (mi.Arguments[1], Is.EqualTo ("set_Property")))
          .Return (fakeSetMethod);

      var result = _factory.CreateProperty (
          _proxyType, "Property", type, ParameterDeclaration.None, 0, getBodyProvider: null, setBodyProvider: setBodyProvider);

      Assert.That (result.MutableGetMethod, Is.Null);
    }

    [Test]
    public void CreateProperty_Providers_ThrowsForInvalidAccessorAttributes ()
    {
      var message = "The following MethodAttributes are not supported for property accessor methods: " +
                    "RequireSecObject.\r\nParameter name: accessorAttributes";
      Assert.That (() => CreateProperty (_proxyType, MethodAttributes.RequireSecObject), Throws.ArgumentException.With.Message.EqualTo (message));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "At least one accessor body provider must be specified.\r\nParameter name: getBodyProvider")]
    public void CreateProperty_Providers_NoAccessorProviders ()
    {
      _factory.CreateProperty (
          _proxyType, "Property", typeof (int), ParameterDeclaration.None, 0, getBodyProvider: null, setBodyProvider: null);
    }

    [Test]
    public void CreateProperty_Providers_ThrowsIfAlreadyExists ()
    {
      var factory = new PropertyFactory (new MethodFactory (new RelatedMethodFinder()));

      Func<MethodBodyCreationContext, Expression> setBodyProvider = ctx => Expression.Empty();
      var indexParameters = ParameterDeclarationObjectMother.CreateMultiple (2);
      var property = _proxyType.AddProperty ("Property", typeof (int), indexParameters, setBodyProvider: setBodyProvider);

      Assert.That (
          () => factory.CreateProperty (_proxyType, "OtherName", property.PropertyType, indexParameters, 0, null, setBodyProvider),
          Throws.Nothing);

      Assert.That (
          () => factory.CreateProperty (_proxyType, property.Name, typeof (string), indexParameters, 0, null, setBodyProvider),
          Throws.Nothing);

      Assert.That (
          () => factory.CreateProperty (
              _proxyType, property.Name, property.PropertyType, ParameterDeclarationObjectMother.CreateMultiple (3), 0, null, setBodyProvider),
          Throws.Nothing);

      Assert.That (
          () => factory.CreateProperty (_proxyType, property.Name, property.PropertyType, indexParameters, 0, null, setBodyProvider),
          Throws.InvalidOperationException.With.Message.EqualTo ("Property with equal name and signature already exists."));
    }

    [Test]
    public void CreateProperty_Accessors ()
    {
      var name = "Property";
      var attributes = (PropertyAttributes) 7;
      var type = ReflectionObjectMother.GetSomeType ();
      var getMethod = MutableMethodInfoObjectMother.Create (declaringType: _proxyType, returnType: type);
      var setMethod = MutableMethodInfoObjectMother.Create (
          declaringType: _proxyType, parameters: new[] { ParameterDeclarationObjectMother.Create (type) });

      var result = _factory.CreateProperty (_proxyType, name, attributes, getMethod, setMethod);

      Assert.That (result.DeclaringType, Is.SameAs (_proxyType));
      Assert.That (result.Name, Is.EqualTo (name));
      Assert.That (result.Attributes, Is.EqualTo (attributes));
      Assert.That (result.MutableGetMethod, Is.SameAs (getMethod));
      Assert.That (result.MutableSetMethod, Is.SameAs (setMethod));
    }

    [Test]
    public void CreateProperty_Accessors_ThrowsForInvalidPropertyAttributes ()
    {
      var message = "The following PropertyAttributes are not supported for properties: " +
                    "HasDefault, Reserved2, Reserved3, Reserved4.\r\nParameter name: attributes";
      Assert.That (() => CreateProperty (_proxyType, PropertyAttributes.HasDefault), Throws.ArgumentException.With.Message.EqualTo (message));
      Assert.That (() => CreateProperty (_proxyType, PropertyAttributes.Reserved2), Throws.ArgumentException.With.Message.EqualTo (message));
      Assert.That (() => CreateProperty (_proxyType, PropertyAttributes.Reserved3), Throws.ArgumentException.With.Message.EqualTo (message));
      Assert.That (() => CreateProperty (_proxyType, PropertyAttributes.Reserved4), Throws.ArgumentException.With.Message.EqualTo (message));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Property must have at least one accessor.\r\nParameter name: getMethod")]
    public void CreateProperty_Accessors_NoAccessorProviders ()
    {
      _factory.CreateProperty (_proxyType, "Property", 0, getMethod: null, setMethod: null);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Accessor methods must be both either static or non-static.\r\nParameter name: getMethod")]
    public void CreateProperty_Accessors_ThrowsForDifferentStaticness ()
    {
      var getMethod = MutableMethodInfoObjectMother.Create (attributes: MethodAttributes.Static);
      var setMethod = MutableMethodInfoObjectMother.Create (attributes: 0 /*instance*/);

      _factory.CreateProperty (_proxyType, "Property", 0, getMethod, setMethod);
    }

    [Test]
    public void CreateProperty_Accessors_ThrowsForDifferentDeclaringType ()
    {
      var nonMatchingMethod = MutableMethodInfoObjectMother.Create ();

      var message = "{0} method is not declared on the current type.\r\nParameter name: {1}";
      Assert.That (
          () => _factory.CreateProperty (_proxyType, "Property", 0, nonMatchingMethod, null),
          Throws.ArgumentException.With.Message.EqualTo (string.Format (message, "Get", "getMethod")));
      Assert.That (
          () => _factory.CreateProperty (_proxyType, "Property", 0, null, nonMatchingMethod),
          Throws.ArgumentException.With.Message.EqualTo (string.Format (message, "Set", "setMethod")));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Get accessor must be a non-void method.\r\nParameter name: getMethod")]
    public void CreateProperty_Accessors_ThrowsForVoidGetMethod ()
    {
      var getMethod = MutableMethodInfoObjectMother.Create (declaringType: _proxyType, returnType: typeof (void));
      _factory.CreateProperty (_proxyType, "Property", 0, getMethod, null);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Set accessor must have return type void.\r\nParameter name: setMethod")]
    public void CreateProperty_Accessors_ThrowsForNonVoidSetMethod ()
    {
      var setMethod = MutableMethodInfoObjectMother.Create (declaringType: _proxyType, returnType: typeof (int));
      _factory.CreateProperty (_proxyType, "Property", 0, null, setMethod);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException),
        ExpectedMessage = "Get and set accessor methods must have a matching signature.\r\nParameter name: setMethod")]
    public void CreateProperty_Accessors_ThrowsForDifferentIndexParameters ()
    {
      var indexParameters = new[] { ParameterDeclarationObjectMother.Create (typeof (int)) };
      var valueParameter = ParameterDeclarationObjectMother.Create (typeof (string));
      var nonMatchingSetParameters = new[] { ParameterDeclarationObjectMother.Create (typeof (long)) }.Concat (valueParameter);
      var getMethod = MutableMethodInfoObjectMother.Create (declaringType: _proxyType, returnType: valueParameter.Type, parameters: indexParameters);
      var setMethod = MutableMethodInfoObjectMother.Create (declaringType: _proxyType, parameters: nonMatchingSetParameters);

      _factory.CreateProperty (_proxyType, "Property", 0, getMethod, setMethod);
    }

    [Test]
    public void CreateProperty_Accessors_ThrowsIfAlreadyExists ()
    {
      var returnType = ReflectionObjectMother.GetSomeType ();
      var getMethod = MutableMethodInfoObjectMother.Create (declaringType: _proxyType, returnType: returnType);
      var property = _proxyType.AddProperty2 ("Property", getMethod: getMethod);

      Assert.That (() => _factory.CreateProperty (_proxyType, "OtherName", 0, getMethod, null), Throws.Nothing);

      var differentPropertyType = ReflectionObjectMother.GetSomeOtherType ();
      var getMethod2 = MutableMethodInfoObjectMother.Create (declaringType: _proxyType, returnType: differentPropertyType);
      Assert.That (() => _factory.CreateProperty (_proxyType, property.Name, 0, getMethod2, null), Throws.Nothing);

      var differentIndexParameters = ParameterDeclarationObjectMother.CreateMultiple (2);
      var getMethod3 = MutableMethodInfoObjectMother.Create (declaringType: _proxyType, returnType: returnType, parameters: differentIndexParameters);
      Assert.That (() => _factory.CreateProperty (_proxyType, property.Name, 0, getMethod3, null), Throws.Nothing);

      Assert.That (
          () => _factory.CreateProperty (_proxyType, property.Name, 0, getMethod, null),
          Throws.InvalidOperationException.With.Message.EqualTo ("Property with equal name and signature already exists."));
    }

    private MutablePropertyInfo CreateProperty (ProxyType proxyType, MethodAttributes accessorAttributes)
    {
      return _factory.CreateProperty (
          proxyType, "dummy", typeof (int), ParameterDeclaration.None, accessorAttributes, ctx => Expression.Constant (7), null);
    }

    private MutablePropertyInfo CreateProperty (ProxyType proxyType, PropertyAttributes attributes)
    {
      var getMethod = MutableMethodInfoObjectMother.Create (returnType: typeof (int));
      return _factory.CreateProperty (proxyType, "dummy", attributes, getMethod, null);
    }
  }
}