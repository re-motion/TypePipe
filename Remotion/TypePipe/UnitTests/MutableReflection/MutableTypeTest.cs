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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.Utilities;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableTypeTest
  {
    private IMemberSelector _memberSelectorMock;
    private IRelatedMethodFinder _relatedMethodFinderMock;
    private IInterfaceMappingComputer _interfaceMappingComputerMock;
    private IMutableMemberFactory _mutableMemberFactoryMock;

    private MutableType _mutableType;

    [SetUp]
    public void SetUp ()
    {
      _memberSelectorMock = MockRepository.GenerateStrictMock<IMemberSelector>();
      _relatedMethodFinderMock = MockRepository.GenerateStrictMock<IRelatedMethodFinder>();
      _interfaceMappingComputerMock = MockRepository.GenerateStrictMock<IInterfaceMappingComputer>();
      _mutableMemberFactoryMock = MockRepository.GenerateStrictMock<IMutableMemberFactory>();

      _mutableType = MutableTypeObjectMother.Create (
          memberSelector: _memberSelectorMock,
          relatedMethodFinder: _relatedMethodFinderMock,
          interfaceMappingComputer: _interfaceMappingComputerMock,
          mutableMemberFactory: _mutableMemberFactoryMock);
    }

    [Test]
    public void Initialization ()
    {
      var baseType = ReflectionObjectMother.GetSomeSubclassableType();
      var name = "abc";
      var @namespace = "def";
      var fullname = "hij";
      var attributes = (TypeAttributes) 7;

      var mutableType = new MutableType (
          baseType, name, @namespace, fullname, attributes, _memberSelectorMock, _interfaceMappingComputerMock, _mutableMemberFactoryMock);

      Assert.That (mutableType.UnderlyingSystemType, Is.SameAs (mutableType));
      Assert.That (mutableType.DeclaringType, Is.Null);
      Assert.That (mutableType.BaseType, Is.SameAs (baseType));
      Assert.That (mutableType.Name, Is.EqualTo (name));
      Assert.That (mutableType.Namespace, Is.EqualTo (@namespace));
      Assert.That (mutableType.FullName, Is.EqualTo (fullname));
      Assert.That (mutableType.Attributes, Is.EqualTo (attributes));

      Assert.That (mutableType.TypeInitializations, Is.Empty);
      Assert.That (mutableType.InstanceInitializations, Is.Empty);
      Assert.That (mutableType.AddedInterfaces, Is.Empty);
      Assert.That (mutableType.AddedFields, Is.Empty);
      Assert.That (mutableType.AddedConstructors, Is.Empty);
      Assert.That (mutableType.AddedMethods, Is.Empty);
    }

    [Test]
    public void CustomAttributeMethods ()
    {
      var declaration = CustomAttributeDeclarationObjectMother.Create (typeof (ObsoleteAttribute));
      _mutableType.AddCustomAttribute (declaration);

      Assert.That (_mutableType.AddedCustomAttributes, Is.EqualTo (new[] { declaration }));

      Assert.That (
          _mutableType.GetCustomAttributeData().Select (a => a.Type), Is.EquivalentTo (new[] { typeof (ObsoleteAttribute), typeof (AbcAttribute) }));
    }

    [Test]
    public void AddCustomAttribute_Serializable ()
    {
      _memberSelectorMock.Stub (stub => stub.SelectMethods<MethodInfo> (null, 0, null)).IgnoreArguments().Return (new MethodInfo[0]);
      Assert.That (_mutableType.IsSerializable, Is.False);

      _mutableType.AddCustomAttribute (CustomAttributeDeclarationObjectMother.Create (typeof (SerializableAttribute)));

      Assert.That (_mutableType.IsSerializable, Is.True);
    }

    [Test]
    public void IsAssignableTo ()
    {
      Assert.That (_mutableType.IsAssignableTo (_mutableType), Is.True);

      var underlyingSystemType = _mutableType.UnderlyingSystemType;
      Assert.That (underlyingSystemType, Is.Not.SameAs (_mutableType));
      Assert.That (_mutableType.IsAssignableTo (underlyingSystemType), Is.True);

      Assert.That (_mutableType.BaseType, Is.SameAs (typeof (DomainTypeBase)));
      Assert.That (_mutableType.IsAssignableTo (typeof (DomainTypeBase)), Is.True);

      Assertion.IsNotNull (_mutableType.BaseType); // For ReSharper...
      Assert.That (_mutableType.BaseType.BaseType, Is.SameAs (typeof (object)));
      Assert.That (_mutableType.IsAssignableTo (typeof (object)), Is.True);

      Assert.That (underlyingSystemType.GetInterfaces(), Has.Member (typeof (IDomainInterface)));
      Assert.That (_mutableType.IsAssignableTo (typeof (IDomainInterface)), Is.True);

      Assert.That (_mutableType.GetInterfaces(), Has.No.Member (typeof (IDisposable)));
      Assert.That (_mutableType.IsAssignableTo (typeof (IDisposable)), Is.False);
      _mutableType.AddInterface (typeof (IDisposable));
      Assert.That (_mutableType.IsAssignableTo (typeof (IDisposable)), Is.True);

      Assert.That (_mutableType.IsAssignableTo (typeof (UnrelatedType)), Is.False);
    }

    [Test]
    public void AddTypeInitialization ()
    {
      Func<InitializationBodyContext, Expression> initializationProvider = ctx => null;

      var fakeExpression = ExpressionTreeObjectMother.GetSomeExpression();
      _mutableMemberFactoryMock.Expect (mock => mock.CreateInitialization (_mutableType, true, initializationProvider)).Return (fakeExpression);

      _mutableType.AddTypeInitialization (initializationProvider);

      _mutableMemberFactoryMock.VerifyAllExpectations();
      Assert.That (_mutableType.TypeInitializations, Is.EqualTo (new[] { fakeExpression }));
    }

    [Test]
    public void AddInstanceInitialization ()
    {
      Func<InitializationBodyContext, Expression> initializationProvider = ctx => null;

      var fakeExpression = ExpressionTreeObjectMother.GetSomeExpression();
      _mutableMemberFactoryMock.Expect (mock => mock.CreateInitialization (_mutableType, false, initializationProvider)).Return (fakeExpression);

      _mutableType.AddInstanceInitialization (initializationProvider);

      _mutableMemberFactoryMock.VerifyAllExpectations();
      Assert.That (_mutableType.InstanceInitializations, Is.EqualTo (new[] { fakeExpression }));
    }

    [Test]
    public void AddInterface ()
    {
      Assert.That (_descriptor.Interfaces, Has.Count.EqualTo (1));
      var existingInterface = _descriptor.Interfaces.Single();
      var addedInterface = ReflectionObjectMother.GetSomeInterfaceType();

      _mutableType.AddInterface (addedInterface);

      Assert.That (_mutableType.AddedInterfaces, Is.EqualTo (new[] { addedInterface }));
      Assert.That (_mutableType.ExistingInterfaces, Is.EqualTo (new[] { existingInterface }));
      Assert.That (_mutableType.GetInterfaces(), Is.EqualTo (new[] { existingInterface, addedInterface }));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Type must be an interface.\r\nParameter name: interfaceType")]
    public void AddInterface_ThrowsIfNotAnInterface ()
    {
      _mutableType.AddInterface (typeof (string));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Interface 'IDomainInterface' is already implemented.\r\nParameter name: interfaceType")]
    public void AddInterface_ThrowsIfAlreadyImplemented ()
    {
      var existingInterface = _descriptor.Interfaces.First();
      _mutableType.AddInterface (existingInterface);
    }

    [Test]
    public void AddField ()
    {
      var name = "_newField";
      var type = ReflectionObjectMother.GetSomeType();
      var attributes = (FieldAttributes) 7;
      var fakeField = MutableFieldInfoObjectMother.Create (_mutableType);
      _mutableMemberFactoryMock.Expect (mock => mock.CreateField (_mutableType, name, type, attributes)).Return (fakeField);

      var result = _mutableType.AddField (name, type, attributes);

      _mutableMemberFactoryMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (fakeField));
      Assert.That (_mutableType.AddedFields, Is.EqualTo (new[] { result }));
    }

    [Test]
    public void GetMutableField ()
    {
      var existingField = _descriptor.Fields.Single (m => m.Name == "Field");
      Assert.That (existingField, Is.Not.AssignableTo<MutableFieldInfo>());

      var result = _mutableType.GetMutableField (existingField);

      //Assert.That (result.UnderlyingSystemFieldInfo, Is.SameAs (existingField));
      Assert.That (_mutableType.ExistingMutableFields, Has.Member (result));
    }

    [Test]
    public void AddConstructor ()
    {
      var attributes = (MethodAttributes) 7;
      var parameterDeclarations = ParameterDeclarationObjectMother.CreateMultiple (2);
      Func<ConstructorBodyCreationContext, Expression> bodyProvider = ctx => null;
      var constructorFake = MutableConstructorInfoObjectMother.Create (_mutableType);
      _mutableMemberFactoryMock
          .Expect (mock => mock.CreateConstructor (_mutableType, attributes, parameterDeclarations, bodyProvider))
          .Return (constructorFake);

      var result = _mutableType.AddConstructor (attributes, parameterDeclarations, bodyProvider);

      _mutableMemberFactoryMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (constructorFake));
      Assert.That (_mutableType.AddedConstructors, Is.EqualTo (new[] { result }));
    }

    [Test]
    public void GetMutableConstructor ()
    {
      var existingCtor = _descriptor.Constructors.Single();
      Assert.That (existingCtor, Is.Not.AssignableTo<MutableConstructorInfo>());

      var result = _mutableType.GetMutableConstructor (existingCtor);

      //Assert.That (result.UnderlyingSystemConstructorInfo, Is.SameAs (existingCtor));
      Assert.That (_mutableType.ExistingMutableConstructors, Has.Member (result));
    }

    [Test]
    public void AddMethod ()
    {
      var name = "Method";
      var attributes = MethodAttributes.Public;
      var returnType = ReflectionObjectMother.GetSomeType();
      var parameterDeclarations = ParameterDeclarationObjectMother.CreateMultiple (2);
      Func<MethodBodyCreationContext, Expression> bodyProvider = ctx => null;
      var fakeMethod = MutableMethodInfoObjectMother.Create(_mutableType);
      _mutableMemberFactoryMock
          .Expect (
              mock => mock.CreateMethod (
                  Arg.Is (_mutableType),
                  Arg.Is (name),
                  Arg.Is (attributes),
                  Arg.Is (returnType),
                  Arg.Is (parameterDeclarations),
                  Arg.Is (bodyProvider)))
          .Return (fakeMethod);

      var result = _mutableType.AddMethod (name, attributes, returnType, parameterDeclarations, bodyProvider);

      _mutableMemberFactoryMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeMethod));
      Assert.That (_mutableType.AddedMethods, Is.EqualTo (new[] { result }));
    }

    [Test]
    public void AddAbstractMethod ()
    {
      var name = "AbstractMethod";
      var attributes = MethodAttributes.Public;
      var returnType = ReflectionObjectMother.GetSomeType();
      var parameterDeclarations = ParameterDeclarationObjectMother.CreateMultiple (2);
      var expectedAttributes = MethodAttributes.Public | MethodAttributes.Abstract | MethodAttributes.Virtual;
      var fakeMethod = MutableMethodInfoObjectMother.Create (_mutableType);
      _mutableMemberFactoryMock
          .Expect (mock => mock.CreateMethod (_mutableType, name, expectedAttributes, returnType, parameterDeclarations, null))
          .Return (fakeMethod);

      var result = _mutableType.AddAbstractMethod (name, attributes, returnType, parameterDeclarations);

      Assert.That (result, Is.SameAs (fakeMethod));
      Assert.That (_mutableType.AddedMethods, Is.EqualTo (new[] { result }));
    }

    [Test]
    public void AddExplicitOverride ()
    {
      var method = ReflectionObjectMother.GetSomeMethod();
      Func<MethodBodyCreationContext, Expression> bodyProvider = ctx => null;
      var fakeMethod = MutableMethodInfoObjectMother.Create (_mutableType);
      _mutableMemberFactoryMock.Expect (mock => mock.CreateExplicitOverride (_mutableType, method, bodyProvider)).Return (fakeMethod);

      var result = _mutableType.AddExplicitOverride (method, bodyProvider);

      Assert.That (result, Is.SameAs (fakeMethod));
      Assert.That (_mutableType.AddedMethods, Is.EqualTo (new[] { result }));
    }

    [Test]
    public void GetOrAddMutableMethod_ExistingMethod_UsesMemberCollection ()
    {
      var existingMethod = _descriptor.Methods.Single (m => m.Name == "VirtualMethod");
      Assert.That (existingMethod, Is.Not.AssignableTo<MutableMethodInfo>());

      var result = _mutableType.GetOrAddOverride (existingMethod);

      //Assert.That (result.UnderlyingSystemMethodInfo, Is.SameAs (existingMethod));
      Assert.That (_mutableType.ExistingMutableMethods, Has.Member (result));
    }

    [Test]
    public void GetOrAddMutableMethod_BaseMethod_CreatesNewOverride ()
    {
      var baseMethod = _descriptor.Methods.Single (m => m.Name == "ToString");
      Assert.That (baseMethod, Is.Not.AssignableTo<MutableMethodInfo>());
      var fakeOverride = MutableMethodInfoObjectMother.Create (_mutableType);
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
    public void GetOrAddMutableMethod_BaseMethod_RetrievesExistingOverride ()
    {
      var baseMethod = _descriptor.Methods.Single (m => m.Name == "ToString");
      Assert.That (baseMethod, Is.Not.AssignableTo<MutableMethodInfo> ());
      var fakeOverride = MutableMethodInfoObjectMother.Create (_mutableType);
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
    public void GetInterfaceMap ()
    {
      var interfaceType = typeof (IDomainInterface);
      var fakeResult = new InterfaceMapping { InterfaceType = ReflectionObjectMother.GetSomeType() };
      _interfaceMappingComputerMock
          .Expect (mock => mock.ComputeMapping (_mutableType, _descriptor.InterfaceMappingProvider, GetAllMethods (_mutableType), interfaceType, false))
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
      _interfaceMappingComputerMock
          .Expect (
              mock =>
              mock.ComputeMapping (_mutableType, _descriptor.InterfaceMappingProvider, GetAllMethods (_mutableType), interfaceType, allowPartial))
          .Return (fakeResult);

      var result = _mutableType.GetInterfaceMap (interfaceType, allowPartial);

      _interfaceMappingComputerMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (fakeResult), "Interface mapping is a struct, therefore we must use EqualTo and a non-empty struct.");
    }

    [Test]
    public void GetAttributeFlagsImpl_Abstract ()
    {
      var allMethods = GetAllMethods (_mutableType);
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
      var fakeMethods = new[] { ReflectionObjectMother.GetSomeAbstractMethod() };
      _memberSelectorMock.Expect (mock => mock.SelectMethods (allMethods, bindingFlags, _mutableType)).Return (fakeMethods).Repeat.Times (2);

      Assert.That (_mutableType.IsAbstract, Is.True);
      Assert.That (_mutableType.Attributes, Is.EqualTo (_descriptor.Attributes | TypeAttributes.Abstract));
    }

    [Test]
    public void GetAttributeFlagsImpl_NonAbstract ()
    {
      var descriptor = TypeDescriptorObjectMother.Create (typeof (AbstractType));
      var mutableType = MutableTypeObjectMother.Create (descriptor, _memberSelectorMock);

      var abstractMethodBaseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((AbstractTypeBase obj) => obj.AbstractMethod1());
      var abstractMethod1 = mutableType.ExistingMutableMethods.Single (m => m.Name == "AbstractMethod1");
      var abstractMethod2 = mutableType.ExistingMutableMethods.Single (m => m.Name == "AbstractMethod2");
      Assert.That (abstractMethod1, Is.Not.EqualTo (abstractMethodBaseDefinition));
      Assert.That (abstractMethod1.GetBaseDefinition(), Is.EqualTo (abstractMethodBaseDefinition));

      mutableType.AddExplicitOverride (abstractMethodBaseDefinition, ctx => Expression.Empty());
      mutableType.ExistingMutableMethods.Single (m => m.Name == "ExistingMethod").AddExplicitBaseDefinition (abstractMethod2);

      var allMethods = GetAllMethods (mutableType);
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
      _memberSelectorMock.Expect (mock => mock.SelectMethods (allMethods, bindingFlags, mutableType)).Return (allMethods).Repeat.Times (2);

      Assert.That (mutableType.IsAbstract, Is.False);
      Assert.That (mutableType.UnderlyingSystemType.IsAbstract, Is.True);
      Assert.That (mutableType.Attributes, Is.EqualTo (descriptor.Attributes & ~TypeAttributes.Abstract));
    }

    [Test]
    public void GetAllInterfaces ()
    {
      Assert.That (_descriptor.Interfaces, Has.Count.EqualTo (1));
      var existingInterface = _descriptor.Interfaces.Single ();
      var addedInterface = ReflectionObjectMother.GetSomeInterfaceType ();
      _mutableType.AddInterface (addedInterface);

      var result = PrivateInvoke.InvokeNonPublicMethod (_mutableType, "GetAllInterfaces");

      Assert.That (result, Is.EqualTo (new[] { existingInterface, addedInterface }));
    }

    [Test]
    public void GetAllFields ()
    {
      AddField (_mutableType, "added");
      var allFields = GetAllFields (_mutableType);
      Assert.That (allFields.AddedMembers, Is.Not.Empty);
      Assert.That (allFields.ExistingDeclaredMembers, Is.Not.Empty);
      Assert.That (allFields.ExistingBaseMembers, Is.Not.Empty);

      var result = PrivateInvoke.InvokeNonPublicMethod (_mutableType, "GetAllFields");

      Assert.That (result, Is.SameAs (allFields));
    }

    [Test]
    public void GetAllConstructors ()
    {
      AddConstructor (_mutableType, ParameterDeclarationObjectMother.Create ());
      var allConstructors = GetAllConstructors (_mutableType);
      Assert.That (allConstructors.AddedMembers, Is.Not.Empty);
      Assert.That (allConstructors.ExistingDeclaredMembers, Is.Not.Empty);

      var result = PrivateInvoke.InvokeNonPublicMethod (_mutableType, "GetAllConstructors");

      Assert.That (result, Is.SameAs (allConstructors));
    }

    [Test]
    public void GetAllMethods ()
    {
      AddMethod (_mutableType, "Added");
      var allMethods = GetAllMethods (_mutableType);
      Assert.That (allMethods.AddedMembers, Is.Not.Empty);
      Assert.That (allMethods.ExistingDeclaredMembers, Is.Not.Empty);
      Assert.That (allMethods.ExistingBaseMembers, Is.Not.Empty);

      var result = PrivateInvoke.InvokeNonPublicMethod (_mutableType, "GetAllMethods");

      Assert.That (result, Is.EqualTo (allMethods));
    }

    [Test]
    public void GetConstructors_CallBase ()
    {
      var fakeCtors = new[] { ReflectionObjectMother.GetSomeConstructor() };
      _memberSelectorMock
          .Expect (mock => mock.SelectMethods (GetAllConstructors (_mutableType), BindingFlags.Default, _mutableType))
          .Return (fakeCtors);

      var result = _mutableType.GetConstructors (BindingFlags.Default);

      Assert.That (result, Is.EqualTo (fakeCtors));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException),
        ExpectedMessage = "Type initializers (static constructors) cannot be modified via this API, use MutableType.AddTypeInitialization instead.")]
    public void GetConstructors_ThrowsIfStatic ()
    {
      Dev.Null = _mutableType.GetConstructors (BindingFlags.Static);
    }

    [Test]
    public void GetConstructorImpl ()
    {
      var fakeCtor = ReflectionObjectMother.GetSomeConstructor();
      _memberSelectorMock
          .Expect (
              mock =>
              mock.SelectSingleMethod (GetAllConstructors (_mutableType), Type.DefaultBinder, BindingFlags.Default, null, _mutableType, null, null))
          .Return (fakeCtor);

      var result = PrivateInvoke.InvokeNonPublicMethod (
          _mutableType, "GetConstructorImpl", BindingFlags.Default, null, CallingConventions.Any, null, null);

      Assert.That (result, Is.SameAs (fakeCtor));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException),
        ExpectedMessage = "Type initializers (static constructors) cannot be modified via this API, use MutableType.AddTypeInitialization instead.")]
    public void GetConstructorImpl_ThrowsIfStatic ()
    {
      PrivateInvoke.InvokeNonPublicMethod (_mutableType, "GetConstructorImpl", BindingFlags.Static, null, CallingConventions.Any, null, null);
    }

    [Test]
    public void GetMethods_FiltersOverriddenMethods ()
    {
      var baseMethod = _descriptor.Methods.Single (m => m.Name == "ToString");
      var fakeOverride = MutableMethodInfoObjectMother.Create (
          declaringType: _mutableType,
          name: baseMethod.Name,
          attributes: baseMethod.Attributes,
          parameters: ParameterDeclaration.EmptyParameters,
          baseMethod: baseMethod);
      _mutableMemberFactoryMock
          .Expect (mock => mock.CreateMethod (null, null, 0, null, null, null))
          .IgnoreArguments()
          .Return (fakeOverride);
      _mutableType.AddMethod ("in", 0, typeof (int), ParameterDeclaration.EmptyParameters, ctx => null);

      var result = PrivateInvoke.InvokeNonPublicMethod (_mutableType, "GetAllMethods");

      _memberSelectorMock.VerifyAllExpectations();
      Assert.That (result, Has.Member (fakeOverride));
      Assert.That (result, Has.No.Member (baseMethod));
    }

    [Test]
    public void GetMutableMember_InvalidDeclaringType ()
    {
      var field = NormalizingMemberInfoFromExpressionUtility.GetField (() => Type.EmptyTypes);
      var ctor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new string(new char[0]));

      Assert.That (
          () => _mutableType.GetMutableField (field),
          Throws.ArgumentException.With.Message.EqualTo (
              "The given field is declared by a different type: 'System.Type'.\r\nParameter name: field"));
      Assert.That (
          () => _mutableType.GetMutableConstructor (ctor),
          Throws.ArgumentException.With.Message.EqualTo (
              "The given constructor is declared by a different type: 'System.String'.\r\nParameter name: constructor"));
    }

    [Test]
    public void GetMutableMember_NoMapping ()
    {
      var fieldStub = MockRepository.GenerateStub<FieldInfo> ();
      fieldStub.Stub (stub => stub.DeclaringType).Return (_mutableType);
      var ctorStub = MockRepository.GenerateStub<ConstructorInfo> ();
      ctorStub.Stub (stub => stub.DeclaringType).Return (_mutableType);

      Assert.That (
          () => _mutableType.GetMutableField (fieldStub),
          Throws.TypeOf<NotSupportedException>().With.Message.EqualTo ("The given field cannot be modified."));
      Assert.That (
          () => _mutableType.GetMutableConstructor (ctorStub),
          Throws.TypeOf<NotSupportedException> ().With.Message.EqualTo ("The given constructor cannot be modified."));
    }

    [Test]
    public new void ToString ()
    {
      // Note: ToString() is implemented in CustomType base class.
      Assert.That (_mutableType.ToString(), Is.EqualTo ("DomainType"));
    }

    [Test]
    public void ToDebugString ()
    {
      // Note: ToDebugString() is implemented in CustomType base class.
      Assert.That (_mutableType.ToDebugString(), Is.EqualTo ("MutableType = \"DomainType\""));
    }

    private MutableFieldInfo AddField (MutableType mutableType, string name)
    {
      Assertion.IsTrue (mutableType == _mutableType, "Consider adding a parameter for _mutableMemberFactoryMock");

      var fakeField = MutableFieldInfoObjectMother.Create (mutableType, name: name);
      _mutableMemberFactoryMock.Stub (stub => stub.CreateField (null, "", null, 0)).IgnoreArguments().Return (fakeField).Repeat.Once();

      return mutableType.AddField ("x", typeof (int));
    }

    private MutableConstructorInfo AddConstructor (MutableType mutableType, params ParameterDeclaration[] parameterDeclarations)
    {
      Assertion.IsTrue (mutableType == _mutableType, "Consider adding a parameter for _mutableMemberFactoryMock");

      var fakeCtor = MutableConstructorInfoObjectMother.CreateForNewWithParameters (mutableType, parameterDeclarations);
      _mutableMemberFactoryMock.Stub (stub => stub.CreateConstructor (null, 0, null, null)).IgnoreArguments().Return (fakeCtor).Repeat.Once();

      return mutableType.AddConstructor (0, ParameterDeclaration.EmptyParameters, ctx => null);
    }

    private MutableMethodInfo AddMethod (MutableType mutableType, string name, params ParameterDeclaration[] parameterDeclarations)
    {
      Assertion.IsTrue (mutableType == _mutableType, "Consider adding a parameter for _mutableMemberFactoryMock");

      var fakeMethod = MutableMethodInfoObjectMother.Create (mutableType, name, parameters: parameterDeclarations);
      _mutableMemberFactoryMock
          .Stub (stub => stub.CreateMethod (null, "", 0, null, null, null)).IgnoreArguments()
          .Return (fakeMethod)
          .Repeat.Once();

      return mutableType.AddMethod ("x", 0, typeof (int), ParameterDeclaration.EmptyParameters, null);
    }

    private MutableTypeMemberCollection<FieldInfo, MutableFieldInfo> GetAllFields (MutableType mutableType)
    {
      return (MutableTypeMemberCollection<FieldInfo, MutableFieldInfo>) PrivateInvoke.GetNonPublicField (mutableType, "_fields");
    }

    private MutableTypeMethodCollection GetAllMethods (MutableType mutableType)
    {
      return (MutableTypeMethodCollection) PrivateInvoke.GetNonPublicField (mutableType, "_methods");
    }

    private MutableTypeMemberCollection<ConstructorInfo, MutableConstructorInfo> GetAllConstructors (MutableType mutableType)
    {
      return (MutableTypeMemberCollection<ConstructorInfo, MutableConstructorInfo>) PrivateInvoke.GetNonPublicField (mutableType, "_constructors");
    }

    public class DomainTypeBase
    {
      public int BaseField;

      public string ExplicitOverrideTarget (double d) { return ""; }
    }

    [Abc]
    public class DomainType : DomainTypeBase, IDomainInterface
    {
      public int Field;

      public virtual string VirtualMethod () { return ""; }

      public void NonVirtualMethod () { }
    }

    public interface IDomainInterface { }

    private class UnrelatedType { }

    public class AbcAttribute : Attribute { }

    abstract class AbstractTypeBase
    {
      public abstract void AbstractMethod1 ();
    }

    abstract class AbstractType : AbstractTypeBase
    {
      public override abstract void AbstractMethod1 ();
      public abstract void AbstractMethod2 ();

      public virtual void ExistingMethod () { }
    }
  }
}