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
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.Expressions;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection.Implementation.MemberFactory;
using Remotion.Utilities;
using Moq;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableTypeTest
  {
    private const BindingFlags c_all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    private Mock<IMemberSelector> _memberSelectorMock;
    private Mock<IInterfaceMappingComputer> _interfaceMappingComputerMock;
    private Mock<IMutableMemberFactory> _mutableMemberFactoryMock;

    private MutableType _mutableType;
    private MutableType _mutableTypeWithoutMocks;

    [SetUp]
    public void SetUp ()
    {
      _memberSelectorMock = new Mock<IMemberSelector> (MockBehavior.Strict);
      _interfaceMappingComputerMock = new Mock<IInterfaceMappingComputer> (MockBehavior.Strict);
      _mutableMemberFactoryMock = new Mock<IMutableMemberFactory> (MockBehavior.Strict);

      _mutableType = MutableTypeObjectMother.Create (
          name: "MyAbcType",
          baseType: typeof (DomainType),
          memberSelector: _memberSelectorMock.Object,
          interfaceMappingComputer: _interfaceMappingComputerMock.Object,
          mutableMemberFactory: _mutableMemberFactoryMock.Object);

      _mutableTypeWithoutMocks = MutableTypeObjectMother.Create (baseType: typeof (DomainType));
    }

    [Test]
    public void Initialization ()
    {
      var declaringType = MutableTypeObjectMother.Create (name: "DeclaringType");
      var baseType = ReflectionObjectMother.GetSomeSubclassableType();
      var name = "MyType";
      var @namespace = "MyNs";
      var attributes = (TypeAttributes) 7;

      var mutableType = new MutableType (
          declaringType, baseType, name, @namespace, attributes, _interfaceMappingComputerMock.Object, _mutableMemberFactoryMock.Object);

      Assert.That (mutableType.DeclaringType, Is.SameAs (declaringType));
      Assert.That (mutableType.MutableDeclaringType, Is.SameAs (declaringType));
      Assert.That (mutableType.BaseType, Is.SameAs (baseType));
      Assert.That (mutableType.Name, Is.EqualTo (name));
      Assert.That (mutableType.Namespace, Is.EqualTo (@namespace));
      Assert.That (mutableType.FullName, Is.EqualTo ("MyNs.DeclaringType+MyType"));
      Assert.That (mutableType.Attributes, Is.EqualTo (attributes));
      Assert.That (mutableType.IsGenericType, Is.False);
      Assert.That (mutableType.IsGenericTypeDefinition, Is.False);
      Assert.That (mutableType.GetGenericArguments(), Is.Empty);

      Assert.That (mutableType.AddedNestedTypes, Is.Empty);
      Assert.That (mutableType.AddedCustomAttributes, Is.Empty);
      Assert.That (mutableType.Initialization, Is.Not.Null);
      Assert.That (mutableType.AddedInterfaces, Is.Empty);
      Assert.That (mutableType.AddedFields, Is.Empty);
      Assert.That (mutableType.AddedConstructors, Is.Empty);
      Assert.That (mutableType.AddedMethods, Is.Empty);
      Assert.That (mutableType.AddedProperties, Is.Empty);
      Assert.That (mutableType.AddedEvents, Is.Empty);
    }

    [Test]
    public void Initialization_NullNamespace ()
    {
      var mutableType = MutableTypeObjectMother.Create (name: "MyType", @namespace: null);

      Assert.That (mutableType.Namespace, Is.Null);
      Assert.That (mutableType.FullName, Is.EqualTo ("MyType"));
    }

    [Test]
    public void Initialization_NullDeclaringType ()
    {
      var mutableType = MutableTypeObjectMother.Create (declaringType: null);

      Assert.That (mutableType.DeclaringType, Is.Null);
    }

    [Test]
    public void CustomAttributeMethods ()
    {
      var declaration = CustomAttributeDeclarationObjectMother.Create (typeof (ObsoleteAttribute));
      _mutableType.AddCustomAttribute (declaration);

      Assert.That (_mutableType.AddedCustomAttributes, Is.EqualTo (new[] { declaration }));
      Assert.That (_mutableType.GetCustomAttributeData().Select (a => a.Type), Is.EquivalentTo (new[] { typeof (ObsoleteAttribute) }));
    }

    [Test]
    public void GetAllNestedTypes ()
    {
      Assertion.IsTrue (typeof (DomainType).GetNestedTypes().Length > 0);
      var addedNestedType = _mutableTypeWithoutMocks.AddNestedType();

      Assert.That (_mutableTypeWithoutMocks.GetAllNestedTypes(), Is.EqualTo (new[] { addedNestedType }));
    }

    [Test]
    public void GetAllInterfaces ()
    {
      var baseInterfaces = typeof (DomainType).GetInterfaces();
      Assert.That (baseInterfaces, Is.Not.Empty);
      var addedInterface = ReflectionObjectMother.GetSomeInterfaceType();
      _mutableType.AddInterface (addedInterface);

      Assert.That (_mutableType.GetAllInterfaces(), Is.EquivalentTo (new[] { addedInterface }.Concat (baseInterfaces)));
    }

    [Test]
    public void GetAllInterfaces_Distinct ()
    {
      var baseInterface = typeof (DomainType).GetInterfaces().Single();
      _mutableType.AddInterface (baseInterface);

      Assert.That (_mutableType.GetInterfaces().Count (ifc => ifc == baseInterface), Is.EqualTo (1));
    }

    [Test]
    public void GetAllFields ()
    {
      var baseFields = typeof (DomainType).GetFields (c_all);
      Assert.That (baseFields, Is.Not.Empty);
      var addedField = _mutableTypeWithoutMocks.AddField();

      Assert.That (_mutableTypeWithoutMocks.GetAllFields(), Is.EquivalentTo (new[] { addedField }.Concat (baseFields)));
    }

    [Test]
    public void GetAllConstructors ()
    {
      var baseCtors = typeof (DomainType).GetConstructors (c_all);
      Assert.That (baseCtors, Is.Not.Empty);
      var addedCtor = _mutableTypeWithoutMocks.AddConstructor();

      Assert.That (_mutableTypeWithoutMocks.GetAllConstructors(), Is.EqualTo (new[] { addedCtor }));
    }

    [Test]
    public void GetAllConstructors_TypeInitializer ()
    {
      var addedTypeInitializer = _mutableTypeWithoutMocks.AddConstructor (MethodAttributes.Static);
      var addedCtor = _mutableTypeWithoutMocks.AddConstructor();

      Assert.That (_mutableTypeWithoutMocks.GetAllConstructors(), Is.EqualTo (new[] { addedCtor, addedTypeInitializer }));
    }

    [Test]
    public void GetAllMethods ()
    {
      var baseMethods = typeof (DomainType).GetMethods (c_all);
      var addedMethod = _mutableTypeWithoutMocks.AddMethod();

      var result = _mutableTypeWithoutMocks.GetAllMethods().ToArray();
      Assert.That (result, Is.EquivalentTo (new[] { addedMethod }.Concat (baseMethods)));
      Assert.That (result.Last(), Is.SameAs (addedMethod));
    }

    [Test]
    public void GetAllMethods_FiltersOverriddenMethods ()
    {
      var baseMethod = typeof (DomainType).GetMethod ("ToString");
      var fakeOverride = MutableMethodInfoObjectMother.Create (
          declaringType: _mutableType,
          name: baseMethod.Name,
          attributes: baseMethod.Attributes,
          parameters: ParameterDeclaration.None,
          baseMethod: baseMethod);
      _mutableMemberFactoryMock
          .Setup (
              mock => mock.CreateMethod (
                  It.IsAny<MutableType>(),
                  It.IsAny<string>(),
                  It.IsAny<MethodAttributes>(),
                  It.IsAny<IEnumerable<GenericParameterDeclaration>>(),
                  It.IsAny<Func<GenericParameterContext, Type>>(),
                  It.IsAny<Func<GenericParameterContext, IEnumerable<ParameterDeclaration>>>(),
                  It.IsAny<Func<MethodBodyCreationContext, Expression>>()))
          .Returns (fakeOverride)
          .Verifiable();
      _mutableType.AddMethod ("in", 0, typeof (int), ParameterDeclaration.None, ctx => null);

      var result = _mutableType.GetAllMethods().ToArray();

      _memberSelectorMock.Verify();
      Assert.That (result, Has.Member (fakeOverride));
      Assert.That (result, Has.No.Member (baseMethod));
      Assert.That (result, Has.No.Member (null));
      Assert.That (result.Last(), Is.SameAs (fakeOverride));
    }

    [Test]
    public void GetAllProperties ()
    {
      var baseProperties = typeof (DomainType).GetProperties (c_all);
      Assert.That (baseProperties, Is.Not.Empty);
      var addedProperty = _mutableTypeWithoutMocks.AddProperty();

      Assert.That (_mutableTypeWithoutMocks.GetAllProperties(), Is.EquivalentTo (new[] { addedProperty }.Concat (baseProperties)));
    }

    [Test]
    public void GetAllEvents ()
    {
      var baseEvents = typeof (DomainType).GetEvents (c_all);
      Assert.That (baseEvents, Is.Not.Empty);
      var addedEvent = _mutableTypeWithoutMocks.AddEvent();

      Assert.That (_mutableTypeWithoutMocks.GetAllEvents(), Is.EquivalentTo (new[] { addedEvent }.Concat (baseEvents)));
    }

    [Test]
    public void AddNestedType ()
    {
      Assert.That (_mutableType.AddedNestedTypes, Is.Empty);
      var typeName = "NestedType";
      var typeAttributes = TypeAttributes.NestedFamily;
      var baseType = ReflectionObjectMother.GetSomeType();
      var nestedTypeFake = MutableTypeObjectMother.Create();
      _mutableMemberFactoryMock.Setup (mock => mock.CreateNestedType (_mutableType, typeName, typeAttributes, baseType)).Returns (nestedTypeFake).Verifiable();

      var result = _mutableType.AddNestedType (typeName, typeAttributes, baseType);

      Assert.That (result, Is.SameAs (nestedTypeFake));
      _mutableMemberFactoryMock.Verify();
    }

    [Test]
    public void AddNestedType_WithNullBaseType ()
    {
      Assert.That (_mutableType.AddedNestedTypes, Is.Empty);
      var typeName = "NestedType";
      var typeAttributes = TypeAttributes.NestedFamily;
      var nestedTypeFake = MutableTypeObjectMother.Create ();
      _mutableMemberFactoryMock.Setup (mock => mock.CreateNestedType (_mutableType, typeName, typeAttributes, null)).Returns (nestedTypeFake).Verifiable();

      var result = _mutableType.AddNestedType (typeName, typeAttributes, baseType: null);

      Assert.That (result, Is.SameAs (nestedTypeFake));
      _mutableMemberFactoryMock.Verify();
    }

    [Test]
    public void AddTypeInitializer ()
    {
      Assert.That (_mutableType.MutableTypeInitializer, Is.Null);
      Func<ConstructorBodyCreationContext, Expression> bodyProvider = ctx => null;
      var typeInitializerFake = MutableConstructorInfoObjectMother.Create (attributes: MethodAttributes.Static);
      _mutableMemberFactoryMock
          .Setup (mock => mock.CreateConstructor (_mutableType, MethodAttributes.Private | MethodAttributes.Static, ParameterDeclaration.None, bodyProvider))
          .Returns (typeInitializerFake)
          .Verifiable();

      var result = _mutableType.AddTypeInitializer (bodyProvider);

      Assert.That (result, Is.SameAs (typeInitializerFake));
      Assert.That (_mutableType.AddedConstructors, Is.Empty);
      Assert.That (_mutableType.MutableTypeInitializer, Is.SameAs (typeInitializerFake));
    }

    [Test]
    public void AddInitialization ()
    {
      Func<InitializationBodyContext, Expression> initializationProvider = ctx => null;

      var fakeExpression = ExpressionTreeObjectMother.GetSomeExpression();
      _mutableMemberFactoryMock.Setup (mock => mock.CreateInitialization (_mutableType, initializationProvider)).Returns (fakeExpression).Verifiable();

      _mutableType.AddInitialization (initializationProvider);

      _mutableMemberFactoryMock.Verify();
      Assert.That (_mutableType.Initialization.Expressions, Is.EqualTo (new[] { fakeExpression }));
    }

    [Test]
    public void AddInterface ()
    {
      var baseInterface = typeof (DomainType).GetInterfaces().Single();
      var addedInterface = ReflectionObjectMother.GetSomeInterfaceType();

      _mutableType.AddInterface (addedInterface);
      Assert.That (_mutableType.AddedInterfaces, Is.EqualTo (new[] { addedInterface }));
      Assert.That (_mutableType.GetInterfaces(), Is.EqualTo (new[] { addedInterface, baseInterface }));

      _mutableType.AddInterface (baseInterface); // Base interface can be re-implemented.
      Assert.That (_mutableType.AddedInterfaces, Is.EqualTo (new[] { addedInterface, baseInterface }));
      Assert.That (_mutableType.GetInterfaces(), Is.EqualTo (new[] { addedInterface, baseInterface }));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Type must be an interface.\r\nParameter name: interfaceType")]
    public void AddInterface_ThrowsIfNotAnInterface ()
    {
      _mutableType.AddInterface (typeof (string));
    }

    [Test]
    public void AddInterface_AlreadyImplemented ()
    {
      var interfaceType = typeof (IDisposable);
      _mutableType.AddInterface (interfaceType);

      Assert.That (() => _mutableType.AddInterface (interfaceType, throwIfAlreadyImplemented: false), Throws.Nothing);
      Assert.That (
          () => _mutableType.AddInterface (interfaceType),
          Throws.TypeOf<ArgumentException>().With.Message.EqualTo ("Interface 'IDisposable' is already implemented.\r\nParameter name: interfaceType"));
    }

    [Test]
    public void AddField ()
    {
      var name = "_newField";
      var type = ReflectionObjectMother.GetSomeType();
      var attributes = (FieldAttributes) 7;
      var fakeField = MutableFieldInfoObjectMother.Create (_mutableType);
      _mutableMemberFactoryMock.Setup (mock => mock.CreateField (_mutableType, name, type, attributes)).Returns (fakeField).Verifiable();

      var result = _mutableType.AddField (name, attributes, type);

      _mutableMemberFactoryMock.Verify();
      Assert.That (result, Is.SameAs (fakeField));
      Assert.That (_mutableType.AddedFields, Is.EqualTo (new[] { result }));
    }

    [Test]
    public void AddConstructor ()
    {
      var attributes = (MethodAttributes) 7;
      var parameters = ParameterDeclarationObjectMother.CreateMultiple (2);
      Func<ConstructorBodyCreationContext, Expression> bodyProvider = ctx => null;
      var fakeConstructor = MutableConstructorInfoObjectMother.Create();
      _mutableMemberFactoryMock
          .Setup (mock => mock.CreateConstructor (_mutableType, attributes, parameters, bodyProvider))
          .Returns (fakeConstructor)
          .Verifiable();

      var result = _mutableType.AddConstructor (attributes, parameters, bodyProvider);

      _mutableMemberFactoryMock.Verify();
      Assert.That (result, Is.SameAs (fakeConstructor));
      Assert.That (_mutableType.AddedConstructors, Is.EqualTo (new[] { result }));
    }

    [Test]
    public void AddConstructor_Static ()
    {
      Assert.That (_mutableType.MutableTypeInitializer, Is.Null);
      var typeInitializerFake = MutableConstructorInfoObjectMother.Create (attributes: MethodAttributes.Static);
      _mutableMemberFactoryMock
          .Setup (
              stub => stub.CreateConstructor (
                  It.IsAny<MutableType>(),
                  It.IsAny<MethodAttributes>(),
                  It.IsAny<IEnumerable<ParameterDeclaration>>(),
                  It.IsAny<Func<ConstructorBodyCreationContext, Expression>>())).Returns (typeInitializerFake);

      var result = _mutableType.AddConstructor (0, ParameterDeclaration.None, ctx => Expression.Empty());

      Assert.That (result, Is.SameAs (typeInitializerFake));
      Assert.That (_mutableType.AddedConstructors, Is.Empty);
      Assert.That (_mutableType.MutableTypeInitializer, Is.SameAs (typeInitializerFake));
    }

    [Test]
    public void AddMethod ()
    {
      var name = "GenericMethod";
      var attributes = (MethodAttributes) 7;
      var genericParameterDeclarations = new[] { GenericParameterDeclarationObjectMother.Create() };
      Func<GenericParameterContext, Type> returnTypeProvider = ctx => null;
      Func<GenericParameterContext, IEnumerable<ParameterDeclaration>> parameterProvider = ctx => null;
      Func<MethodBodyCreationContext, Expression> bodyProvider = ctx => null;
      var fakeMethod = MutableMethodInfoObjectMother.Create();
      _mutableMemberFactoryMock
          .Setup (mock => mock.CreateMethod (_mutableType, name, attributes, genericParameterDeclarations, returnTypeProvider, parameterProvider, bodyProvider))
          .Returns (fakeMethod)
          .Verifiable();

      var result = _mutableType.AddMethod (name, attributes, genericParameterDeclarations, returnTypeProvider, parameterProvider, bodyProvider);

      _mutableMemberFactoryMock.Verify();
      Assert.That (result, Is.SameAs (fakeMethod));
      Assert.That (_mutableType.AddedMethods, Is.EqualTo (new[] { result }));
    }

    [Test]
    public void AddExplicitOverride ()
    {
      var method = ReflectionObjectMother.GetSomeMethod();
      Func<MethodBodyCreationContext, Expression> bodyProvider = ctx => null;
      var fakeMethod = MutableMethodInfoObjectMother.Create();
      _mutableMemberFactoryMock.Setup (mock => mock.CreateExplicitOverride (_mutableType, method, bodyProvider)).Returns (fakeMethod).Verifiable();

      var result = _mutableType.AddExplicitOverride (method, bodyProvider);

      Assert.That (result, Is.SameAs (fakeMethod));
      Assert.That (_mutableType.AddedMethods, Is.EqualTo (new[] { result }));
    }

    [Test]
    public void GetOrAddOverride_CreatesNewOverride ()
    {
      var baseMethod = ReflectionObjectMother.GetSomeMethod();
      var fakeOverride = MutableMethodInfoObjectMother.Create();
      var isNewlyCreated = true;
      _mutableMemberFactoryMock
          .Setup (
              mock => mock.GetOrCreateOverride (
                  _mutableType,
                  baseMethod,
                  out isNewlyCreated))
          .Returns (fakeOverride)
          .Verifiable();

      var result = _mutableType.GetOrAddOverride (baseMethod);

      Assert.That (result, Is.SameAs (fakeOverride));
      Assert.That (_mutableType.AddedMethods, Has.Member (result));
    }

    [Test]
    public void GetOrAddOverride_RetrievesExistingOverride ()
    {
      var baseMethod = ReflectionObjectMother.GetSomeMethod();
      var fakeOverride = MutableMethodInfoObjectMother.Create();
      var isNewlyCreated = false;
      _mutableMemberFactoryMock
          .Setup (mock => mock.GetOrCreateOverride (_mutableType, baseMethod, out isNewlyCreated))
          .Returns (fakeOverride)
          .Verifiable();

      var result = _mutableType.GetOrAddOverride (baseMethod);

      Assert.That (result, Is.SameAs (fakeOverride));
      Assert.That (_mutableType.AddedMethods, Has.No.Member (result));
    }

    [Test]
    public void GetOrAddImplementation_CreatesNewImplementation ()
    {
      var interfaceMethod = ReflectionObjectMother.GetSomeMethod();
      var fakeImplementation = MutableMethodInfoObjectMother.Create ();
      var isNewlyCreated = true;
      _mutableMemberFactoryMock
          .Setup (
              mock => mock.GetOrCreateImplementation (
                  _mutableType,
                  interfaceMethod,
                  out isNewlyCreated))
          .Returns (fakeImplementation)
          .Verifiable();

      var result = _mutableType.GetOrAddImplementation (interfaceMethod);

      Assert.That (result, Is.SameAs (fakeImplementation));
      Assert.That (_mutableType.AddedMethods, Has.Member (result));
    }

    [Test]
    public void GetOrAddImplementation_RetrievesExistingImplementation ()
    {
      var interfaceMethod = ReflectionObjectMother.GetSomeMethod();
      var fakeImplementation = MutableMethodInfoObjectMother.Create();
      var isNewlyCreated = false;
      _mutableMemberFactoryMock
          .Setup (
              mock => mock.GetOrCreateImplementation (
                  _mutableType,
                  interfaceMethod,
                  out isNewlyCreated))
          .Returns (fakeImplementation)
          .Verifiable();

      var result = _mutableType.GetOrAddImplementation (interfaceMethod);

      Assert.That (result, Is.SameAs (fakeImplementation));
      Assert.That (_mutableType.AddedMethods, Has.No.Member (result));
    }

    [Test]
    public void AddProperty_Simple ()
    {
      var name = "Property";
      var type = ReflectionObjectMother.GetSomeType();
      var indexParameters = ParameterDeclarationObjectMother.CreateMultiple (2);
      var accessorAttributes = (MethodAttributes) 7;
      Func<MethodBodyCreationContext, Expression> getBodyProvider = ctx => null;
      Func<MethodBodyCreationContext, Expression> setBodyProvider = ctx => null;
      var fakeProperty = MutablePropertyInfoObjectMother.CreateReadWrite();
      _mutableMemberFactoryMock
          .Setup (mock => mock.CreateProperty (_mutableType, name, type, indexParameters, accessorAttributes, getBodyProvider, setBodyProvider))
          .Returns (fakeProperty)
          .Verifiable();

      var result = _mutableType.AddProperty (name, type, indexParameters, accessorAttributes, getBodyProvider, setBodyProvider);

      _mutableMemberFactoryMock.Verify();
      Assert.That (result, Is.SameAs (fakeProperty));
      Assert.That (_mutableType.AddedProperties, Is.EqualTo (new[] { result }));
      Assert.That (_mutableType.AddedMethods, Is.EqualTo (new[] { result.MutableGetMethod, result.MutableSetMethod }));
      Assert.That (result.MutableGetMethod.Attributes, Is.EqualTo (accessorAttributes));
      Assert.That (result.MutableSetMethod.Attributes, Is.EqualTo (accessorAttributes));
    }

    [Test]
    public void AddProperty_Simple_ReadOnlyProperty ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      Func<MethodBodyCreationContext, Expression> getBodyProvider = ctx => null;
      var fakeGetMethod = MutableMethodInfoObjectMother.Create (returnType: type);
      var fakeProperty = MutablePropertyInfoObjectMother.Create (getMethod: fakeGetMethod);
      Assert.That (fakeProperty.MutableSetMethod, Is.Null);
      _mutableMemberFactoryMock
          .Setup (stub => stub.CreateProperty (_mutableType, "Property", type, ParameterDeclaration.None, MethodAttributes.Public, getBodyProvider, null))
          .Returns (fakeProperty);

      _mutableType.AddProperty ("Property", type, getBodyProvider: getBodyProvider);

      Assert.That (_mutableType.AddedMethods, Is.EqualTo (new[] { fakeGetMethod }));
    }

    [Test]
    public void AddProperty_Simple_WriteOnlyProperty ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      Func<MethodBodyCreationContext, Expression> setBodyProvider = ctx => null;
      var fakeSetMethod = MutableMethodInfoObjectMother.Create (parameters: new[] { ParameterDeclarationObjectMother.Create (type) });
      var fakeProperty = MutablePropertyInfoObjectMother.Create (setMethod: fakeSetMethod);
      Assert.That (fakeProperty.MutableGetMethod, Is.Null);
      _mutableMemberFactoryMock
          .Setup (stub => stub.CreateProperty (_mutableType, "Property", type, ParameterDeclaration.None, MethodAttributes.Public, null, setBodyProvider))
          .Returns (fakeProperty);

      _mutableType.AddProperty ("Property", type, setBodyProvider: setBodyProvider);

      Assert.That (_mutableType.AddedMethods, Is.EqualTo (new[] { fakeSetMethod }));
    }

    [Test]
    public void AddProperty_Complex ()
    {
      var name = "Property";
      var attributes = (PropertyAttributes) 7;
      var getMethod = MutableMethodInfoObjectMother.Create (attributes: MethodAttributes.Static);
      var setMethod = MutableMethodInfoObjectMother.Create (attributes: MethodAttributes.Static);
      var fakeProperty = MutablePropertyInfoObjectMother.Create();
      _mutableMemberFactoryMock
          .Setup (mock => mock.CreateProperty (_mutableType, name, attributes, getMethod, setMethod))
          .Returns (fakeProperty)
          .Verifiable();

      var result = _mutableType.AddProperty (name, attributes, getMethod, setMethod);

      _mutableMemberFactoryMock.Verify();
      Assert.That (result, Is.SameAs (fakeProperty));
      Assert.That (_mutableType.AddedProperties, Is.EqualTo (new[] { result }));
    }

    [Test]
    public void AddEvent_Simple ()
    {
      var name = "Event";
      var handlerType = ReflectionObjectMother.GetSomeType();
      var accessorAttributes = (MethodAttributes) 7;
      Func<MethodBodyCreationContext, Expression> addBodyProvider = ctx => null;
      Func<MethodBodyCreationContext, Expression> removeBodyProvider = ctx => null;
      Func<MethodBodyCreationContext, Expression> raiseBodyProvider = ctx => null;

      var fakeEvent = MutableEventInfoObjectMother.CreateWithAccessors (createRaiseMethod: true);
      _mutableMemberFactoryMock
          .Setup (
              mock => mock.CreateEvent (_mutableType, name, handlerType, accessorAttributes, addBodyProvider, removeBodyProvider, raiseBodyProvider))
          .Returns (fakeEvent)
          .Verifiable();

      var result = _mutableType.AddEvent (name, handlerType, accessorAttributes, addBodyProvider, removeBodyProvider, raiseBodyProvider);

      _mutableMemberFactoryMock.Verify();
      Assert.That (result, Is.SameAs (fakeEvent));
      Assert.That (_mutableType.AddedEvents, Is.EqualTo (new[] { result }));
      Assert.That (_mutableType.AddedMethods, Is.EqualTo (new[] { result.MutableAddMethod, result.MutableRemoveMethod, result.MutableRaiseMethod }));
    }

    [Test]
    public void AddEvent_Simple_NoRaiseBodyProvider ()
    {
      var handlerType = ReflectionObjectMother.GetSomeType();
      Func<MethodBodyCreationContext, Expression> addBodyProvider = ctx => null;
      Func<MethodBodyCreationContext, Expression> removeBodyProvider = ctx => null;

      var addRemoveParameters = new[] { new ParameterDeclaration (typeof (Func<int, string>), "handler") };
      var addMethod = MutableMethodInfoObjectMother.Create (returnType: typeof (void), parameters: addRemoveParameters);
      var removeMethod = MutableMethodInfoObjectMother.Create (returnType: typeof (void), parameters: addRemoveParameters);
      var fakeEvent = MutableEventInfoObjectMother.Create (addMethod: addMethod, removeMethod: removeMethod);
      Assert.That (fakeEvent.MutableRaiseMethod, Is.Null);
      _mutableMemberFactoryMock
          .Setup (
              stub => stub.CreateEvent (
                  It.IsAny<MutableType>(),
                  It.IsAny<string>(),
                  It.IsAny<Type>(),
                  It.IsAny<MethodAttributes>(),
                  It.IsAny<Func<MethodBodyCreationContext, Expression>>(),
                  It.IsAny<Func<MethodBodyCreationContext, Expression>>(),
                  It.IsAny<Func<MethodBodyCreationContext, Expression>>()))
        .Returns (fakeEvent);

      var result = _mutableType.AddEvent ("Event", handlerType, addBodyProvider: addBodyProvider, removeBodyProvider: removeBodyProvider);

      Assert.That (_mutableType.AddedMethods, Is.EqualTo (new[] { result.MutableAddMethod, result.MutableRemoveMethod }));
    }

    [Test]
    public void AddEvent_Complex ()
    {
      var eventAttributes = EventAttributes.SpecialName;
      var addMethod = MutableMethodInfoObjectMother.Create();
      var removeMethod = MutableMethodInfoObjectMother.Create();
      var raiseMethod = MutableMethodInfoObjectMother.Create();
      var fakeEvent = MutableEventInfoObjectMother.CreateWithAccessors();
      _mutableMemberFactoryMock
          .Setup (mock => mock.CreateEvent (_mutableType, "Event", eventAttributes, addMethod, removeMethod, raiseMethod))
          .Returns (fakeEvent)
          .Verifiable();

      var result = _mutableType.AddEvent ("Event", eventAttributes, addMethod, removeMethod, raiseMethod);

      _mutableMemberFactoryMock.Verify();
      Assert.That (result, Is.SameAs (fakeEvent));
      Assert.That (_mutableType.AddedEvents, Is.EqualTo (new[] { result }));
    }

    [Test]
    public void GetInterfaceMap ()
    {
      var interfaceType = typeof (IDomainInterface);
      var fakeResult = new InterfaceMapping { InterfaceType = ReflectionObjectMother.GetSomeType() };
      _memberSelectorMock
          .Setup (stub => stub.SelectMethods (It.IsAny<IEnumerable<MethodInfo>>(), It.IsAny<BindingFlags>(), It.IsAny<Type>()))
          .Returns (new MethodInfo[0]);
      _interfaceMappingComputerMock
          .Setup (mock => mock.ComputeMapping (_mutableType, typeof (DomainType).GetInterfaceMap, interfaceType, false))
          .Returns (fakeResult)
          .Verifiable();

      var result = _mutableType.GetInterfaceMap (interfaceType);

      _interfaceMappingComputerMock.Verify();
      Assert.That (result, Is.EqualTo (fakeResult), "Interface mapping is a struct, therefore we must use EqualTo and a non-empty struct.");
    }

    [Test]
    public void GetInterfaceMap_AllowPartialInterfaceMapping ()
    {
      var interfaceType = typeof (IDomainInterface);
      var allowPartial = BooleanObjectMother.GetRandomBoolean();
      var fakeResult = new InterfaceMapping { InterfaceType = ReflectionObjectMother.GetSomeType() };
      _memberSelectorMock.Setup (stub => stub.SelectMethods<MethodInfo> (It.IsAny<IEnumerable<MethodInfo>>(), It.IsAny<BindingFlags>(), It.IsAny<Type>())).Returns (new MethodInfo[0]);
      _interfaceMappingComputerMock
          .Setup (mock => mock.ComputeMapping (_mutableType, typeof (DomainType).GetInterfaceMap, interfaceType, allowPartial))
          .Returns (fakeResult)
          .Verifiable();

      var result = _mutableType.GetInterfaceMap (interfaceType, allowPartial);

      _interfaceMappingComputerMock.Verify();
      Assert.That (result, Is.EqualTo (fakeResult), "Interface mapping is a struct, therefore we must use EqualTo and a non-empty struct.");
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Method GetInterfaceMap is not supported by interface types.")]
    public void GetInterfaceMap_Interface_Throws ()
    {
      var mutableInterfaceType = MutableTypeObjectMother.CreateInterface();
      mutableInterfaceType.GetInterfaceMap (ReflectionObjectMother.GetSomeType());
    }

    [Test]
    public void GetAttributeFlagsImpl_Serializable ()
    {
      var proxyType = MutableTypeObjectMother.Create (memberSelector: _memberSelectorMock.Object);
      _memberSelectorMock
          .Setup (stub => stub.SelectMethods<MethodInfo> (It.IsAny<IEnumerable<MethodInfo>>(), It.IsAny<BindingFlags>(), It.IsAny<Type>()))
          .Returns (new MethodInfo[0]);
      Assert.That (proxyType.IsTypePipeSerializable(), Is.False);

      proxyType.AddCustomAttribute (CustomAttributeDeclarationObjectMother.Create (typeof (SerializableAttribute)));

      Assert.That (proxyType.IsTypePipeSerializable(), Is.True);
    }

    [Test]
    public void GetAttributeFlagsImpl_Abstract ()
    {
      var proxyType = MutableTypeObjectMother.Create (baseType: typeof (AbstractType));

      Assert.That (proxyType.IsAbstract, Is.True);
      Assert.That (proxyType.Attributes, Is.EqualTo (TypeAttributes.Public | TypeAttributes.BeforeFieldInit | TypeAttributes.Abstract));
    }

    [Test]
    public void GetAttributeFlagsImpl_AbstractCache_Updated_WhenAddingAbstractMethod_ToNonAbstractType ()
    {
      var proxyType = MutableTypeObjectMother.Create (baseType: typeof (DomainType));
      Assert.That (proxyType.IsAbstract, Is.False);

      proxyType.AddMethod ("NonAbstractMethod", returnType: typeof (void), bodyProvider: c => Expression.Default (typeof (void)));
      Assert.That (proxyType.IsAbstract, Is.False);

      proxyType.AddMethod ("AbstractMethod", attributes: MethodAttributes.Abstract | MethodAttributes.Virtual);
      Assert.That (proxyType.IsAbstract, Is.True);
    }

    [Test]
    public void GetAttributeFlagsImpl_AbstractCache_Updated_WhenSettingBody ()
    {
      var proxyType = MutableTypeObjectMother.Create (baseType: typeof (DomainType));
      Assert.That (proxyType.IsAbstract, Is.False);

      var nonAbstractMethod = proxyType.AddMethod ("NonAbstractMethod", returnType: typeof (void), bodyProvider: c => Expression.Default (typeof (void)));
      Assert.That (proxyType.IsAbstract, Is.False);

      var abstractMethod = proxyType.AddMethod ("AbstractMethod", attributes: MethodAttributes.Abstract | MethodAttributes.Virtual);
      Assert.That (proxyType.IsAbstract, Is.True);

      nonAbstractMethod.SetBody (c => ExpressionTreeObjectMother.GetSomeExpression (c.ReturnType));
      Assert.That (proxyType.IsAbstract, Is.True);

      abstractMethod.SetBody (b => Expression.Default (b.ReturnType));
      Assert.That (proxyType.IsAbstract, Is.False);
    }

    [Test]
    public void GetAttributeFlagsImpl_AbstractCache_Updated_WhenAddingNonAbstractMethod_ThatOverridesAbstractMethod ()
    {
      var proxyType = MutableTypeObjectMother.Create (baseType: typeof (AbstractTypeBase));
      Assert.That (proxyType.IsAbstract, Is.True);

      proxyType
          .AddMethod (
              "AbstractMethod1",
              MethodAttributes.Public | MethodAttributes.Virtual,
              MethodDeclaration.CreateEquivalent (NormalizingMemberInfoFromExpressionUtility.GetMethod ((AbstractTypeBase t) => t.AbstractMethod1 ())),
              bodyProvider: c => Expression.Default (c.ReturnType));
      Assert.That (proxyType.IsAbstract, Is.False);
    }

    [Test]
    public void GetAttributeFlagsImpl_AbstractCache_Updated_WhenAddingNonAbstractMethod_ThatExplicitlyOverridesAbstractMethod ()
    {
      var proxyType = MutableTypeObjectMother.Create (baseType: typeof (AbstractTypeBase));
      Assert.That (proxyType.IsAbstract, Is.True);

      proxyType
          .AddExplicitOverride (
              NormalizingMemberInfoFromExpressionUtility.GetMethod ((AbstractTypeBase t) => t.AbstractMethod1 ()),
              bodyProvider: c => Expression.Default (c.ReturnType));
      Assert.That (proxyType.IsAbstract, Is.False);
    }

    [Test]
    public void GetAttributeFlagsImpl_AbstractCache_Updated_WhenSettingBodyOfAbstractOverride ()
    {
      var proxyType = MutableTypeObjectMother.Create (baseType: typeof (AbstractTypeBase));
      Assert.That (proxyType.IsAbstract, Is.True);

      var addedOverride = proxyType.GetOrAddOverride (NormalizingMemberInfoFromExpressionUtility.GetMethod ((AbstractTypeBase t) => t.AbstractMethod1 ()));
      Assert.That (proxyType.IsAbstract, Is.True);

      addedOverride
          .SetBody(c => Expression.Default(c.ReturnType));
      Assert.That (proxyType.IsAbstract, Is.False);
    }

    [Test]
    public void GetAttributeFlagsImpl_AbstractCache_Updated_WhenAddingNonAbstractExplicitOverride ()
    {
      var proxyType = MutableTypeObjectMother.Create (baseType: typeof (AbstractTypeBase));
      Assert.That (proxyType.IsAbstract, Is.True);

      proxyType.AddExplicitOverride (
          NormalizingMemberInfoFromExpressionUtility.GetMethod ((AbstractTypeBase t) => t.AbstractMethod1 ()), 
          c => Expression.Default (c.ReturnType));
      Assert.That (proxyType.IsAbstract, Is.False);
    }

    [Test]
    public void GetAttributeFlagsImpl_AbstractCache_Updated_WhenAddingNonAbstractExplicitOverride_Later ()
    {
      var proxyType = MutableTypeObjectMother.Create (baseType: typeof (AbstractTypeBase));
      Assert.That (proxyType.IsAbstract, Is.True);

      var abstractBaseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((AbstractTypeBase t) => t.AbstractMethod1());
      var addedMethod = proxyType
          .AddMethod (
              "Override",
              MethodAttributes.Public | MethodAttributes.Virtual,
              MethodDeclaration.CreateEquivalent (abstractBaseMethod),
              bodyProvider: c => Expression.Default (c.ReturnType));
      Assert.That (proxyType.IsAbstract, Is.True);

      addedMethod.AddExplicitBaseDefinition (abstractBaseMethod);
      Assert.That (proxyType.IsAbstract, Is.False);
    }

    [Test]
    public void GetAttributeFlagsImpl_NonAbstract ()
    {
      var proxyType = MutableTypeObjectMother.Create (baseType: typeof (AbstractType));
      Assert.That (proxyType.IsAbstract, Is.True);

      var abstractMethodBaseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((AbstractTypeBase obj) => obj.AbstractMethod1());
      var abstractMethod1 = proxyType.GetMethod ("AbstractMethod1");
      var abstractMethod2 = proxyType.GetMethod ("AbstractMethod2");
      Assert.That (abstractMethod1, Is.Not.EqualTo (abstractMethodBaseDefinition));
      Assert.That (abstractMethod1.GetBaseDefinition(), Is.EqualTo (abstractMethodBaseDefinition));

      proxyType.AddExplicitOverride (abstractMethodBaseDefinition, ctx => Expression.Empty());
      proxyType.AddMethod (attributes: MethodAttributes.Virtual).AddExplicitBaseDefinition (abstractMethod2);

      Assert.That (proxyType.IsAbstract, Is.False);
      Assertion.IsNotNull (proxyType.BaseType);
      Assert.That (proxyType.BaseType.IsAbstract, Is.True);
      Assert.That (proxyType.Attributes & TypeAttributes.Abstract, Is.Not.EqualTo (TypeAttributes.Abstract));
    }

    [Test]
    public void GetAttributeFlagsImpl_Interface ()
    {
      var mutableInterfaceType = MutableTypeObjectMother.CreateInterface();
      Assert.That (mutableInterfaceType.Attributes.IsSet (TypeAttributes.Interface | TypeAttributes.Abstract), Is.True);
    }

    [Test]
    public void ToDebugString ()
    {
      // Note: ToDebugString() is implemented in CustomType base class.
      Assert.That (_mutableType.ToDebugString (), Is.EqualTo ("MutableType = \"MyAbcType\""));
    }

    public class DomainTypeBase
    {
      public int BaseField;

      public string ExplicitOverrideTarget (double d) { return "" + d; }
    }
    public interface IDomainInterface { }
    public class DomainType : DomainTypeBase, IDomainInterface
    {
      public class NestedType {}

      public int Field;

      public int Property { get; set; }

      public event EventHandler Event;

      public virtual string VirtualMethod () { return ""; }
      public void NonVirtualMethod () { }
    }

    public abstract class AbstractTypeBase
    {
      public abstract void AbstractMethod1 ();
    }

    public abstract class AbstractType : AbstractTypeBase
    {
      public override abstract void AbstractMethod1 ();
      public abstract void AbstractMethod2 ();
    }
  }
}