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
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.Expressions;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection.Implementation.MemberFactory;
using Moq;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation.MemberFactory
{
  [TestFixture]
  public class PropertyFactoryTest
  {
    private Mock<IMethodFactory> _methodFactoryMock;

    private PropertyFactory _factory;

    private MutableType _mutableType;

    [SetUp]
    public void SetUp ()
    {
      _methodFactoryMock = new Mock<IMethodFactory> (MockBehavior.Strict);

      _factory = new PropertyFactory (_methodFactoryMock.Object);

      _mutableType = MutableTypeObjectMother.Create();
    }

    [Test]
    public void CreateProperty_Providers ()
    {
      var name = "Property";
      var propertyType = ReflectionObjectMother.GetSomeType();
      var indexParameters = ParameterDeclarationObjectMother.CreateMultiple (2).ToList();
      var accessorAttributes = (MethodAttributes) 7;
      var setterParameters = indexParameters.Concat (new[] { ParameterDeclarationObjectMother.Create (propertyType, "value") }).ToList();
      Func<MethodBodyCreationContext, Expression> getBodyProvider = ctx => null;
      Func<MethodBodyCreationContext, Expression> setBodyProvider = ctx => null;

      var fakeGetMethod = MutableMethodInfoObjectMother.Create (returnType: propertyType, parameters: indexParameters);
      var fakeSetMethod = MutableMethodInfoObjectMother.Create (parameters: setterParameters);

      _methodFactoryMock
          .Setup (
              mock => mock.CreateMethod (
                  _mutableType,
                  "get_Property",
                  accessorAttributes | MethodAttributes.SpecialName,
                  GenericParameterDeclaration.None,
                  It.IsAny<Func<GenericParameterContext, Type>>(),
                  It.IsAny<Func<GenericParameterContext, IEnumerable<ParameterDeclaration>>>(),
                  getBodyProvider))
          .Callback (
              (
                  MutableType declaringType,
                  string nameArgument,
                  MethodAttributes attributes,
                  IEnumerable<GenericParameterDeclaration> genericParameters,
                  Func<GenericParameterContext, Type> returnTypeProvider,
                  Func<GenericParameterContext, IEnumerable<ParameterDeclaration>> parameterProvider,
                  Func<MethodBodyCreationContext, Expression> bodyProvider) =>
              {
                var returnType = returnTypeProvider (null);
                Assert.That (returnType, Is.SameAs (propertyType));

                var parameters = parameterProvider (null).ToList();
                Assert.That (parameters.Select (p => p.Type), Is.EqualTo (indexParameters.Select (p => p.Type)));
                Assert.That (parameters.Select (p => p.Name), Is.EqualTo (indexParameters.Select (p => p.Name)));
              })
          .Returns (fakeGetMethod);
      _methodFactoryMock
          .Setup (
              mock => mock.CreateMethod (
                  _mutableType,
                  "set_Property",
                  (accessorAttributes | MethodAttributes.SpecialName),
                  GenericParameterDeclaration.None,
                  It.IsAny<Func<GenericParameterContext, Type>>(),
                  It.IsAny<Func<GenericParameterContext, IEnumerable<ParameterDeclaration>>>(),
                  setBodyProvider))
          .Callback (
              (
                  MutableType declaringType,
                  string nameArgument,
                  MethodAttributes attributes,
                  IEnumerable<GenericParameterDeclaration> genericParameters,
                  Func<GenericParameterContext, Type> returnTypeProvider,
                  Func<GenericParameterContext, IEnumerable<ParameterDeclaration>> parameterProvider,
                  Func<MethodBodyCreationContext, Expression> bodyProvider) =>
              {
                var returnType = returnTypeProvider (null);
                Assert.That (returnType, Is.SameAs (typeof (void)));

                var parameters = parameterProvider (null).ToList();
                Assert.That (parameters.Select (p => p.Type), Is.EqualTo (setterParameters.Select (p => p.Type)));
                Assert.That (parameters.Select (p => p.Name), Is.EqualTo (setterParameters.Select (p => p.Name)));
              })
          .Returns (fakeSetMethod)
          .Verifiable();

      var result = _factory.CreateProperty (
          _mutableType, name, propertyType, indexParameters.AsOneTime(), accessorAttributes, getBodyProvider, setBodyProvider);

      _methodFactoryMock.Verify();
      Assert.That (result.DeclaringType, Is.SameAs (_mutableType));
      Assert.That (result.Name, Is.EqualTo (name));
      Assert.That (result.Attributes, Is.EqualTo (PropertyAttributes.None));
      Assert.That (result.PropertyType, Is.SameAs (propertyType));
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
          .Setup (
              stub => stub.CreateMethod (
                  It.IsAny<MutableType>(),
                  "get_Property",
                  It.IsAny<MethodAttributes>(),
                  It.IsAny<IEnumerable<GenericParameterDeclaration>>(),
                  It.IsAny<Func<GenericParameterContext, Type>>(),
                  It.IsAny<Func<GenericParameterContext, IEnumerable<ParameterDeclaration>>>(),
                  It.IsAny<Func<MethodBodyCreationContext, Expression>>()))
          .Returns (fakeGetMethod);

      var result = _factory.CreateProperty (
          _mutableType, "Property", type, ParameterDeclaration.None, 0, getBodyProvider: getBodyProvider, setBodyProvider: null);

      Assert.That (result.MutableSetMethod, Is.Null);
    }

    [Test]
    public void CreateProperty_Providers_WriteOnly ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      Func<MethodBodyCreationContext, Expression> setBodyProvider = ctx => ExpressionTreeObjectMother.GetSomeExpression (typeof (void));
      var fakeSetMethod = MutableMethodInfoObjectMother.Create (parameters: new[] { ParameterDeclarationObjectMother.Create (type) });
      _methodFactoryMock
          .Setup (
              stub => stub.CreateMethod (
                  It.IsAny<MutableType>(),
                  "set_Property",
                  It.IsAny<MethodAttributes>(),
                  It.IsAny<IEnumerable<GenericParameterDeclaration>>(),
                  It.IsAny<Func<GenericParameterContext, Type>>(),
                  It.IsAny<Func<GenericParameterContext, IEnumerable<ParameterDeclaration>>>(),
                  It.IsAny<Func<MethodBodyCreationContext, Expression>>()))
          .Returns (fakeSetMethod);

      var result = _factory.CreateProperty (
          _mutableType,
          "Property",
          type,
          ParameterDeclaration.None,
          0,
          getBodyProvider: null,
          setBodyProvider: setBodyProvider);

      Assert.That (result.MutableGetMethod, Is.Null);
    }

    [Test]
    public void CreateProperty_Providers_ThrowsForInvalidAccessorAttributes ()
    {
      var message = "The following MethodAttributes are not supported for property accessor methods: " +
                    "RequireSecObject.\r\nParameter name: accessorAttributes";
      Assert.That (() => CreateProperty (_mutableType, MethodAttributes.RequireSecObject), Throws.ArgumentException.With.Message.EqualTo (message));
    }

    [Test]
    public void CreateProperty_Providers_NoAccessorProviders ()
    {
      Assert.That (
          () => _factory.CreateProperty (
          _mutableType, "Property", typeof (int), ParameterDeclaration.None, 0, getBodyProvider: null, setBodyProvider: null),
          Throws.ArgumentException
              .With.Message.EqualTo ("At least one accessor body provider must be specified.\r\nParameter name: getBodyProvider"));
    }

    [Test]
    public void CreateProperty_Providers_ThrowsIfAlreadyExists ()
    {
      var factory = new PropertyFactory (new MethodFactory (new RelatedMethodFinder()));

      Func<MethodBodyCreationContext, Expression> setBodyProvider = ctx => Expression.Empty();
      var indexParameters = ParameterDeclarationObjectMother.CreateMultiple (2);
      var property = _mutableType.AddProperty ("Property", typeof (int), indexParameters, setBodyProvider: setBodyProvider);

      Assert.That (
          () => factory.CreateProperty (_mutableType, "OtherName", property.PropertyType, indexParameters, 0, null, setBodyProvider),
          Throws.Nothing);

      Assert.That (
          () => factory.CreateProperty (_mutableType, property.Name, typeof (string), indexParameters, 0, null, setBodyProvider),
          Throws.Nothing);

      Assert.That (
          () => factory.CreateProperty (
              _mutableType, property.Name, property.PropertyType, ParameterDeclarationObjectMother.CreateMultiple (3), 0, null, setBodyProvider),
          Throws.Nothing);

      Assert.That (
          () => factory.CreateProperty (_mutableType, property.Name, property.PropertyType, indexParameters, 0, null, setBodyProvider),
          Throws.InvalidOperationException.With.Message.EqualTo ("Property with equal name and signature already exists."));
    }

    [Test]
    public void CreateProperty_Accessors ()
    {
      var name = "Property";
      var attributes = (PropertyAttributes) 7;
      var type = ReflectionObjectMother.GetSomeType ();
      var getMethod = MutableMethodInfoObjectMother.Create (declaringType: _mutableType, returnType: type);
      var setMethod = MutableMethodInfoObjectMother.Create (
          declaringType: _mutableType, parameters: new[] { ParameterDeclarationObjectMother.Create (type) });

      var result = _factory.CreateProperty (_mutableType, name, attributes, getMethod, setMethod);

      Assert.That (result.DeclaringType, Is.SameAs (_mutableType));
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
      Assert.That (() => CreateProperty (_mutableType, PropertyAttributes.HasDefault), Throws.ArgumentException.With.Message.EqualTo (message));
      Assert.That (() => CreateProperty (_mutableType, PropertyAttributes.Reserved2), Throws.ArgumentException.With.Message.EqualTo (message));
      Assert.That (() => CreateProperty (_mutableType, PropertyAttributes.Reserved3), Throws.ArgumentException.With.Message.EqualTo (message));
      Assert.That (() => CreateProperty (_mutableType, PropertyAttributes.Reserved4), Throws.ArgumentException.With.Message.EqualTo (message));
    }

    [Test]
    public void CreateProperty_Accessors_NoAccessorProviders ()
    {
      Assert.That (
          () => _factory.CreateProperty (_mutableType, "Property", 0, getMethod: null, setMethod: null),
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "Property must have at least one accessor.\r\nParameter name: getMethod"));
    }

    [Test]
    public void CreateProperty_Accessors_ThrowsForDifferentStaticness ()
    {
      var getMethod = MutableMethodInfoObjectMother.Create (attributes: MethodAttributes.Static);
      var setMethod = MutableMethodInfoObjectMother.Create (attributes: 0 /*instance*/);
      Assert.That (
          () => _factory.CreateProperty (_mutableType, "Property", 0, getMethod, setMethod),
          Throws.ArgumentException
              .With.Message.EqualTo ("Accessor methods must be both either static or non-static.\r\nParameter name: getMethod"));
    }

    [Test]
    public void CreateProperty_Accessors_ThrowsForDifferentDeclaringType ()
    {
      var nonMatchingMethod = MutableMethodInfoObjectMother.Create ();

      var message = "{0} method is not declared on the current type.\r\nParameter name: {1}";
      Assert.That (
          () => _factory.CreateProperty (_mutableType, "Property", 0, nonMatchingMethod, null),
          Throws.ArgumentException.With.Message.EqualTo (string.Format (message, "Get", "getMethod")));
      Assert.That (
          () => _factory.CreateProperty (_mutableType, "Property", 0, null, nonMatchingMethod),
          Throws.ArgumentException.With.Message.EqualTo (string.Format (message, "Set", "setMethod")));
    }

    [Test]
    public void CreateProperty_Accessors_ThrowsForVoidGetMethod ()
    {
      var getMethod = MutableMethodInfoObjectMother.Create (declaringType: _mutableType, returnType: typeof (void));
      Assert.That (
          () => _factory.CreateProperty (_mutableType, "Property", 0, getMethod, null),
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "Get accessor must be a non-void method.\r\nParameter name: getMethod"));
    }

    [Test]
    public void CreateProperty_Accessors_ThrowsForNonVoidSetMethod ()
    {
      var setMethod = MutableMethodInfoObjectMother.Create (declaringType: _mutableType, returnType: typeof (int));
      Assert.That (
          () => _factory.CreateProperty (_mutableType, "Property", 0, null, setMethod),
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "Set accessor must have return type void.\r\nParameter name: setMethod"));
    }

    [Test]
    public void CreateProperty_Accessors_ThrowsForDifferentIndexParameters ()
    {
      var indexParameters = new[] { ParameterDeclarationObjectMother.Create (typeof (int)) };
      var valueParameter = ParameterDeclarationObjectMother.Create (typeof (string));
      var nonMatchingSetParameters = new[] { ParameterDeclarationObjectMother.Create (typeof (long)), valueParameter };
      var getMethod = MutableMethodInfoObjectMother.Create (declaringType: _mutableType, returnType: valueParameter.Type, parameters: indexParameters);
      var setMethod = MutableMethodInfoObjectMother.Create (declaringType: _mutableType, parameters: nonMatchingSetParameters);
      Assert.That (
          () => _factory.CreateProperty (_mutableType, "Property", 0, getMethod, setMethod),
          Throws.ArgumentException
              .With.Message.EqualTo ("Get and set accessor methods must have a matching signature.\r\nParameter name: setMethod"));
    }

    [Test]
    public void CreateProperty_Accessors_ThrowsIfAlreadyExists ()
    {
      var returnType = ReflectionObjectMother.GetSomeType ();
      var getMethod = MutableMethodInfoObjectMother.Create (declaringType: _mutableType, returnType: returnType);
      var property = _mutableType.AddProperty2 ("Property", getMethod: getMethod);

      Assert.That (() => _factory.CreateProperty (_mutableType, "OtherName", 0, getMethod, null), Throws.Nothing);

      var differentPropertyType = ReflectionObjectMother.GetSomeOtherType ();
      var getMethod2 = MutableMethodInfoObjectMother.Create (declaringType: _mutableType, returnType: differentPropertyType);
      Assert.That (() => _factory.CreateProperty (_mutableType, property.Name, 0, getMethod2, null), Throws.Nothing);

      var differentIndexParameters = ParameterDeclarationObjectMother.CreateMultiple (2);
      var getMethod3 = MutableMethodInfoObjectMother.Create (declaringType: _mutableType, returnType: returnType, parameters: differentIndexParameters);
      Assert.That (() => _factory.CreateProperty (_mutableType, property.Name, 0, getMethod3, null), Throws.Nothing);

      Assert.That (
          () => _factory.CreateProperty (_mutableType, property.Name, 0, getMethod, null),
          Throws.InvalidOperationException.With.Message.EqualTo ("Property with equal name and signature already exists."));
    }

    private MutablePropertyInfo CreateProperty (MutableType mutableType, MethodAttributes accessorAttributes)
    {
      return _factory.CreateProperty (
          mutableType, "dummy", typeof (int), ParameterDeclaration.None, accessorAttributes, ctx => Expression.Constant (7), null);
    }

    private MutablePropertyInfo CreateProperty (MutableType mutableType, PropertyAttributes attributes)
    {
      var getMethod = MutableMethodInfoObjectMother.Create (returnType: typeof (int));
      return _factory.CreateProperty (mutableType, "dummy", attributes, getMethod, null);
    }
  }
}