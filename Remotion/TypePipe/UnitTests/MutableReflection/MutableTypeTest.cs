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
using Remotion.Development.TypePipe.UnitTesting.Expressions;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection.Implementation.MemberFactory;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.Utilities;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableTypeTest
  {
    private const BindingFlags c_all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    private IMemberSelector _memberSelectorMock;
    private IInterfaceMappingComputer _interfaceMappingComputerMock;
    private IMutableMemberFactory _mutableMemberFactoryMock;

    private MutableType _mutableType;
    private MutableType _mutableTypeWithoutMocks;
    private MutableType _mutableInterfaceType;

    [SetUp]
    public void SetUp ()
    {
      _memberSelectorMock = MockRepository.GenerateStrictMock<IMemberSelector>();
      _interfaceMappingComputerMock = MockRepository.GenerateStrictMock<IInterfaceMappingComputer>();
      _mutableMemberFactoryMock = MockRepository.GenerateStrictMock<IMutableMemberFactory>();

      _mutableType = MutableTypeObjectMother.Create (
          name: "MyAbcType",
          baseType: typeof (DomainType),
          memberSelector: _memberSelectorMock,
          interfaceMappingComputer: _interfaceMappingComputerMock,
          mutableMemberFactory: _mutableMemberFactoryMock);

      _mutableTypeWithoutMocks = MutableTypeObjectMother.Create (baseType: typeof (DomainType));

      _mutableInterfaceType = MutableTypeObjectMother.CreateInterface();
    }

    [Test]
    public void Initialization ()
    {
      var baseType = ReflectionObjectMother.GetSomeSubclassableType();
      var name = "MyType";
      var @namespace = "MyNs";
      var attributes = (TypeAttributes) 7;

      var proxyType = new MutableType (
          _memberSelectorMock, baseType, name, @namespace, attributes, _interfaceMappingComputerMock, _mutableMemberFactoryMock);

      Assert.That (proxyType.DeclaringType, Is.Null);
      Assert.That (proxyType.MutableDeclaringType, Is.Null);
      Assert.That (proxyType.BaseType, Is.SameAs (baseType));
      Assert.That (proxyType.Name, Is.EqualTo (name));
      Assert.That (proxyType.Namespace, Is.EqualTo (@namespace));
      Assert.That (proxyType.FullName, Is.EqualTo ("MyNs.MyType"));
      _memberSelectorMock.Stub (mock => mock.SelectMethods<MethodInfo> (null, 0, null)).IgnoreArguments().Return (new MethodInfo[0]);
      Assert.That (proxyType.Attributes, Is.EqualTo (attributes));
      Assert.That (proxyType.IsGenericType, Is.False);
      Assert.That (proxyType.IsGenericTypeDefinition, Is.False);
      Assert.That (proxyType.GetGenericArguments(), Is.Empty);

      Assert.That (proxyType.AddedCustomAttributes, Is.Empty);
      Assert.That (proxyType.Initializations, Is.Empty);
      Assert.That (proxyType.AddedInterfaces, Is.Empty);
      Assert.That (proxyType.AddedFields, Is.Empty);
      Assert.That (proxyType.AddedConstructors, Is.Empty);
      Assert.That (proxyType.AddedMethods, Is.Empty);
      Assert.That (proxyType.AddedProperties, Is.Empty);
      Assert.That (proxyType.AddedEvents, Is.Empty);
    }

    [Test]
    public void Initialization_NullNamespace ()
    {
      var proxyType = MutableTypeObjectMother.Create (name: "MyType", @namespace: null);

      Assert.That (proxyType.Namespace, Is.Null);
      Assert.That (proxyType.FullName, Is.EqualTo ("MyType"));
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
    public void AddTypeInitializer ()
    {
      Assert.That (_mutableType.MutableTypeInitializer, Is.Null);
      Func<ConstructorBodyCreationContext, Expression> bodyProvider = ctx => null;
      var typeInitializerFake = MutableConstructorInfoObjectMother.Create (attributes: MethodAttributes.Static);
      _mutableMemberFactoryMock
          .Expect (
              mock => mock.CreateConstructor (
                  _mutableType, MethodAttributes.Private | MethodAttributes.Static, ParameterDeclaration.None, bodyProvider))
          .Return (typeInitializerFake);

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
      _mutableMemberFactoryMock.Expect (mock => mock.CreateInitialization (_mutableType, initializationProvider)).Return (fakeExpression);

      _mutableType.AddInitialization (initializationProvider);

      _mutableMemberFactoryMock.VerifyAllExpectations();
      Assert.That (_mutableType.Initializations, Is.EqualTo (new[] { fakeExpression }));
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
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Interface 'IDisposable' is already implemented.\r\nParameter name: interfaceType")]
    public void AddInterface_ThrowsIfAlreadyImplemented ()
    {
      _mutableType.AddInterface (typeof (IDisposable));
      _mutableType.AddInterface (typeof (IDisposable));
    }

    [Test]
    public void AddField ()
    {
      var name = "_newField";
      var type = ReflectionObjectMother.GetSomeType();
      var attributes = (FieldAttributes) 7;
      var fakeField = MutableFieldInfoObjectMother.Create (_mutableType);
      _mutableMemberFactoryMock.Expect (mock => mock.CreateField (_mutableType, name, type, attributes)).Return (fakeField);

      var result = _mutableType.AddField (name, attributes, type);

      _mutableMemberFactoryMock.VerifyAllExpectations();
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
          .Expect (mock => mock.CreateConstructor (_mutableType, attributes, parameters, bodyProvider))
          .Return (fakeConstructor);

      var result = _mutableType.AddConstructor (attributes, parameters, bodyProvider);

      _mutableMemberFactoryMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeConstructor));
      Assert.That (_mutableType.AddedConstructors, Is.EqualTo (new[] { result }));
    }

    [Test]
    public void AddConstructor_Static ()
    {
      Assert.That (_mutableType.MutableTypeInitializer, Is.Null);
      var typeInitializerFake = MutableConstructorInfoObjectMother.Create (attributes: MethodAttributes.Static);
      _mutableMemberFactoryMock.Stub (stub => stub.CreateConstructor (null, 0, null, null)).IgnoreArguments().Return (typeInitializerFake);

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
          .Expect (
              mock =>
              mock.CreateMethod (_mutableType, name, attributes, genericParameterDeclarations, returnTypeProvider, parameterProvider, bodyProvider))
          .Return (fakeMethod);

      var result = _mutableType.AddMethod (name, attributes, genericParameterDeclarations, returnTypeProvider, parameterProvider, bodyProvider);

      _mutableMemberFactoryMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeMethod));
      Assert.That (_mutableType.AddedMethods, Is.EqualTo (new[] { result }));
    }

    [Test]
    public void AddExplicitOverride ()
    {
      var method = ReflectionObjectMother.GetSomeMethod();
      Func<MethodBodyCreationContext, Expression> bodyProvider = ctx => null;
      var fakeMethod = MutableMethodInfoObjectMother.Create();
      _mutableMemberFactoryMock.Expect (mock => mock.CreateExplicitOverride (_mutableType, method, bodyProvider)).Return (fakeMethod);

      var result = _mutableType.AddExplicitOverride (method, bodyProvider);

      Assert.That (result, Is.SameAs (fakeMethod));
      Assert.That (_mutableType.AddedMethods, Is.EqualTo (new[] { result }));
    }

    [Test]
    public void GetOrAddOverride_CreatesNewOverride ()
    {
      var baseMethod = typeof (DomainType).GetMethod ("ToString");
      var fakeOverride = MutableMethodInfoObjectMother.Create();
      _mutableMemberFactoryMock
          .Expect (
              mock => mock.GetOrCreateOverride (
                  Arg.Is (_mutableType),
                  Arg.Is (baseMethod),
                  out Arg<bool>.Out (true).Dummy))
          .Return (fakeOverride);

      var result = _mutableType.GetOrAddOverride (baseMethod);

      Assert.That (result, Is.SameAs (fakeOverride));
      Assert.That (_mutableType.AddedMethods, Has.Member (result));
    }

    [Test]
    public void GetOrAddOverride_RetrievesExistingOverride ()
    {
      var baseMethod = typeof (DomainType).GetMethod ("ToString");
      var fakeOverride = MutableMethodInfoObjectMother.Create();
      _mutableMemberFactoryMock
          .Expect (
              mock => mock.GetOrCreateOverride (
                  Arg.Is (_mutableType),
                  Arg.Is (baseMethod),
                  out Arg<bool>.Out (false).Dummy))
          .Return (fakeOverride);

      var result = _mutableType.GetOrAddOverride (baseMethod);

      Assert.That (result, Is.SameAs (fakeOverride));
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
          .Expect (mock => mock.CreateProperty (_mutableType, name, type, indexParameters, accessorAttributes, getBodyProvider, setBodyProvider))
          .Return (fakeProperty);

      var result = _mutableType.AddProperty (name, type, indexParameters, accessorAttributes, getBodyProvider, setBodyProvider);

      _mutableMemberFactoryMock.VerifyAllExpectations();
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
          .Stub (stub => stub.CreateProperty (_mutableType, "Property", type, ParameterDeclaration.None, MethodAttributes.Public, getBodyProvider, null))
          .Return (fakeProperty);

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
        .Stub (stub => stub.CreateProperty (_mutableType, "Property", type, ParameterDeclaration.None, MethodAttributes.Public, null, setBodyProvider))
        .Return (fakeProperty);

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
          .Expect (mock => mock.CreateProperty (_mutableType, name, attributes, getMethod, setMethod))
          .Return (fakeProperty);

      var result = _mutableType.AddProperty (name, attributes, getMethod, setMethod);

      _mutableMemberFactoryMock.VerifyAllExpectations();
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
          .Expect (
              mock => mock.CreateEvent (_mutableType, name, handlerType, accessorAttributes, addBodyProvider, removeBodyProvider, raiseBodyProvider))
          .Return (fakeEvent);

      var result = _mutableType.AddEvent (name, handlerType, accessorAttributes, addBodyProvider, removeBodyProvider, raiseBodyProvider);

      _mutableMemberFactoryMock.VerifyAllExpectations();
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
        .Stub (stub => stub.CreateEvent (null, null, null, 0, null, null, null))
        .IgnoreArguments()
        .Return (fakeEvent);

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
          .Expect (mock => mock.CreateEvent (_mutableType, "Event", eventAttributes, addMethod, removeMethod, raiseMethod))
          .Return (fakeEvent);

      var result = _mutableType.AddEvent ("Event", eventAttributes, addMethod, removeMethod, raiseMethod);

      _mutableMemberFactoryMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeEvent));
      Assert.That (_mutableType.AddedEvents, Is.EqualTo (new[] { result }));
    }

    [Test]
    public void GetInterfaceMap ()
    {
      var interfaceType = typeof (IDomainInterface);
      var fakeResult = new InterfaceMapping { InterfaceType = ReflectionObjectMother.GetSomeType() };
      _memberSelectorMock.Stub (stub => stub.SelectMethods<MethodInfo> (null, 0, null)).IgnoreArguments().Return (new MethodInfo[0]);
      _interfaceMappingComputerMock
          .Expect (mock => mock.ComputeMapping (_mutableType, typeof (DomainType).GetInterfaceMap, interfaceType, false))
          .Return (fakeResult);

      var result = _mutableType.GetInterfaceMap (interfaceType);

      _interfaceMappingComputerMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (fakeResult), "Interface mapping is a struct, therefore we must use EqualTo and a non-empty struct.");
    }

    [Test]
    public void GetInterfaceMap_AllowPartialInterfaceMapping ()
    {
      var interfaceType = typeof (IDomainInterface);
      var allowPartial = BooleanObjectMother.GetRandomBoolean();
      var fakeResult = new InterfaceMapping { InterfaceType = ReflectionObjectMother.GetSomeType() };
      _memberSelectorMock.Stub (stub => stub.SelectMethods<MethodInfo> (null, 0, null)).IgnoreArguments().Return (new MethodInfo[0]);
      _interfaceMappingComputerMock
          .Expect (mock => mock.ComputeMapping (_mutableType, typeof (DomainType).GetInterfaceMap, interfaceType, allowPartial))
          .Return (fakeResult);

      var result = _mutableType.GetInterfaceMap (interfaceType, allowPartial);

      _interfaceMappingComputerMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (fakeResult), "Interface mapping is a struct, therefore we must use EqualTo and a non-empty struct.");
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Method GetInterfaceMap is not supported by interface types.")]
    public void GetInterfaceMap_Interface_Throws ()
    {
      _mutableInterfaceType.GetInterfaceMap (ReflectionObjectMother.GetSomeType());
    }

    [Test]
    public void GetAttributeFlagsImpl_Serializable ()
    {
      var proxyType = MutableTypeObjectMother.Create (memberSelector: _memberSelectorMock);
      _memberSelectorMock.Stub (stub => stub.SelectMethods<MethodInfo> (null, 0, null)).IgnoreArguments().Return (new MethodInfo[0]);
      Assert.That (proxyType.IsTypePipeSerializable(), Is.False);

      proxyType.AddCustomAttribute (CustomAttributeDeclarationObjectMother.Create (typeof (SerializableAttribute)));

      Assert.That (proxyType.IsTypePipeSerializable(), Is.True);
    }

    [Test]
    public void GetAttributeFlagsImpl_Abstract ()
    {
      var allMethods = GetAllMethods (_mutableType);
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
      var fakeMethods = new[] { ReflectionObjectMother.GetSomeAbstractMethod() };
      _memberSelectorMock
          .Expect (mock => mock.SelectMethods (Arg<IEnumerable<MethodInfo>>.List.Equal (allMethods), Arg.Is (bindingFlags), Arg.Is ((_mutableType))))
          .Return (fakeMethods).Repeat.Times (2);

      Assert.That (_mutableType.IsAbstract, Is.True);
      Assert.That (_mutableType.Attributes, Is.EqualTo (TypeAttributes.Public | TypeAttributes.BeforeFieldInit | TypeAttributes.Abstract));
      _memberSelectorMock.VerifyAllExpectations();
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
      Assert.That (_mutableInterfaceType.Attributes.IsSet (TypeAttributes.Interface | TypeAttributes.Abstract), Is.True);
    }

    [Test]
    public void GetAllInterfaces ()
    {
      var baseInterfaces = typeof (DomainType).GetInterfaces();
      Assert.That (baseInterfaces, Is.Not.Empty);
      var addedInterface = ReflectionObjectMother.GetSomeInterfaceType();
      _mutableType.AddInterface (addedInterface);

      var result = PrivateInvoke.InvokeNonPublicMethod (_mutableType, "GetAllInterfaces");

      Assert.That (result, Is.EquivalentTo (new[] { addedInterface }.Concat (baseInterfaces)));
    }

    [Test]
    public void GetAllInterfaces_Distinct ()
    {
      var baseInterface = typeof (DomainType).GetInterfaces().Single();
      _mutableType.AddInterface (baseInterface);

      Assert.That (_mutableType.GetInterfaces().Count (ifc => ifc == baseInterface), Is.EqualTo (1));
    }

    [Test]
    public void GetAllInterfaces_Interface ()
    {
      var addedInterface = ReflectionObjectMother.GetSomeInterfaceType();
      _mutableInterfaceType.AddInterface (addedInterface);

      var result = _mutableInterfaceType.Invoke ("GetAllInterfaces");

      Assert.That (result, Is.EqualTo (new[] { addedInterface }));
    }

    [Test]
    public void GetAllFields ()
    {
      var baseFields = typeof (DomainType).GetFields (c_all);
      Assert.That (baseFields, Is.Not.Empty);
      var addedField = _mutableTypeWithoutMocks.AddField();

      var result = PrivateInvoke.InvokeNonPublicMethod (_mutableTypeWithoutMocks, "GetAllFields");

      Assert.That (result, Is.EquivalentTo (new[] { addedField }.Concat (baseFields)));
    }

    [Test]
    public void GetAllFields_Interface ()
    {
      var addedField = _mutableInterfaceType.AddField ();

      var result = _mutableInterfaceType.Invoke ("GetAllFields");

      Assert.That (result, Is.EqualTo (new[] { addedField }));
    }

    [Test]
    public void GetAllConstructors ()
    {
      var baseCtors = typeof (DomainType).GetConstructors (c_all);
      Assert.That (baseCtors, Is.Not.Empty);
      var addedCtor = _mutableTypeWithoutMocks.AddConstructor();

      var result = PrivateInvoke.InvokeNonPublicMethod (_mutableTypeWithoutMocks, "GetAllConstructors");

      Assert.That (result, Is.EqualTo (new[] { addedCtor }));
    }

    [Test]
    public void GetAllConstructors_TypeInitializer ()
    {
      var addedTypeInitializer = _mutableTypeWithoutMocks.AddConstructor (MethodAttributes.Static);
      var addedCtor = _mutableTypeWithoutMocks.AddConstructor();

      var result = PrivateInvoke.InvokeNonPublicMethod (_mutableTypeWithoutMocks, "GetAllConstructors");

      Assert.That (result, Is.EqualTo (new[] { addedCtor, addedTypeInitializer }));
    }

    [Test]
    public void GetAllMethods ()
    {
      var baseMethods = typeof (DomainType).GetMethods (c_all);
      var addedMethod = _mutableTypeWithoutMocks.AddMethod();

      var result = PrivateInvoke.InvokeNonPublicMethod (_mutableTypeWithoutMocks, "GetAllMethods");

      Assert.That (result, Is.EquivalentTo (new[] { addedMethod }.Concat (baseMethods)));
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
          .Expect (mock => mock.CreateMethod (null, null, 0, null, null, null, null))
          .IgnoreArguments()
          .Return (fakeOverride);
      _mutableType.AddMethod ("in", 0, typeof (int), ParameterDeclaration.None, ctx => null);

      var result = PrivateInvoke.InvokeNonPublicMethod (_mutableType, "GetAllMethods");

      _memberSelectorMock.VerifyAllExpectations();
      Assert.That (result, Has.Member (fakeOverride));
      Assert.That (result, Has.No.Member (baseMethod));
    }

    [Test]
    public void GetAllMethods_Interface ()
    {
      var addedMethod = _mutableInterfaceType.AddMethod();

      var result = _mutableInterfaceType.Invoke ("GetAllMethods");

      Assert.That (result, Is.EqualTo (new[] { addedMethod }));
    }
    
    [Test]
    public void GetAllProperties ()
    {
      var baseProperties = typeof (DomainType).GetProperties (c_all);
      Assert.That (baseProperties, Is.Not.Empty);
      var addedProperty = _mutableTypeWithoutMocks.AddProperty();

      var result = PrivateInvoke.InvokeNonPublicMethod (_mutableTypeWithoutMocks, "GetAllProperties");

      Assert.That (result, Is.EquivalentTo (new[] { addedProperty }.Concat (baseProperties)));
    }

    [Test]
    public void GetAllProperties_Interface ()
    {
      var addedProperty = _mutableInterfaceType.AddProperty ();

      var result = _mutableInterfaceType.Invoke ("GetAllProperties");

      Assert.That (result, Is.EqualTo (new[] { addedProperty }));
    }

    [Test]
    public void GetAllEvents ()
    {
      var baseEvents = typeof (DomainType).GetEvents (c_all);
      Assert.That (baseEvents, Is.Not.Empty);
      var addedEvent = _mutableTypeWithoutMocks.AddEvent();

      var result = PrivateInvoke.InvokeNonPublicMethod (_mutableTypeWithoutMocks, "GetAllEvents");

      Assert.That (result, Is.EquivalentTo (new[] { addedEvent }.Concat (baseEvents)));
    }

    [Test]
    public void GetAllEvents_Interface ()
    {
      var addedEvent = _mutableInterfaceType.AddEvent ();

      var result = _mutableInterfaceType.Invoke ("GetAllEvents");

      Assert.That (result, Is.EqualTo (new[] { addedEvent }));
    }

    [Test]
    public void ToDebugString ()
    {
      // Note: ToDebugString() is implemented in CustomType base class.
      Assert.That (_mutableType.ToDebugString (), Is.EqualTo ("MutableType = \"MyAbcType\""));
    }

    private IEnumerable<MethodInfo> GetAllMethods (MutableType mutableType)
    {
      return (IEnumerable<MethodInfo>) PrivateInvoke.InvokeNonPublicMethod (mutableType, "GetAllMethods");
    }

    public class DomainTypeBase
    {
      public int BaseField;

      public string ExplicitOverrideTarget (double d) { return "" + d; }
    }
    public interface IDomainInterface { }
    public class DomainType : DomainTypeBase, IDomainInterface
    {
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