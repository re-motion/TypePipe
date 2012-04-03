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
using Remotion.Development.UnitTesting;
using Remotion.FunctionalProgramming;
using Remotion.TypePipe.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableTypeTest
  {
    private static readonly IEnumerable<ParameterDeclaration> s_emptyParamDecls = Enumerable.Empty<ParameterDeclaration>();

    private UnderlyingTypeDescriptor _descriptor;
    private IEqualityComparer<MemberInfo> _memberInfoEqualityComparerStub;
    private IBindingFlagsEvaluator _bindingFlagsEvaluatorMock;

    private MutableType _mutableType;
    
    [SetUp]
    public void SetUp ()
    {
      _descriptor = UnderlyingTypeDescriptorObjectMother.Create(originalType: typeof (DomainClass));
      _memberInfoEqualityComparerStub = MockRepository.GenerateStub<IEqualityComparer<MemberInfo>>();
      _bindingFlagsEvaluatorMock = MockRepository.GenerateMock<IBindingFlagsEvaluator>();

      _mutableType = new MutableType (_descriptor, _memberInfoEqualityComparerStub, _bindingFlagsEvaluatorMock);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_mutableType.AddedInterfaces, Is.Empty);
      Assert.That (_mutableType.AddedFields, Is.Empty);
      Assert.That (_mutableType.AddedConstructors, Is.Empty);
    }

    [Test]
    public void Initialization_WithInterfaces ()
    {
      Assert.That (_descriptor.Interfaces, Is.Not.Empty);

      Assert.That (_mutableType.ExistingInterfaces, Is.EqualTo (_descriptor.Interfaces));
    }

    [Test]
    public void Initialization_WithFields ()
    {
      Assert.That (_descriptor.Fields, Is.Not.Empty);

      Assert.That (_mutableType.ExistingFields, Is.EqualTo (_descriptor.Fields));
    }

    [Test]
    public void Initialization_WithConstructors ()
    {
      var ctors = _descriptor.Constructors;
      Assert.That (ctors, Is.Not.Empty.And.Count.EqualTo (1));
      var expectedCtor = ctors.Single ();

      Assert.That (_mutableType.ExistingConstructors, Has.Count.EqualTo (1));
      var mutableCtor = _mutableType.ExistingConstructors.Single();

      Assert.That (mutableCtor.UnderlyingSystemConstructorInfo, Is.EqualTo (expectedCtor));
      Assert.That (mutableCtor.DeclaringType, Is.SameAs (_mutableType));
    }

    [Test]
    public void UnderlyingSystemType ()
    {
      Assert.That (_descriptor.UnderlyingSystemType, Is.Not.Null);

      Assert.That (_mutableType.UnderlyingSystemType, Is.SameAs (_descriptor.UnderlyingSystemType));
    }

    [Test]
    public void IsNewType ()
    {
      Assert.That (_mutableType.IsNewType, Is.False);
    }

    [Test]
    public void Assembly ()
    {
      Assert.That (_mutableType.Assembly, Is.Null);
    }

    [Test]
    public void BaseType ()
    {
      Assert.That (_descriptor.BaseType, Is.Not.Null);

      Assert.That (_mutableType.BaseType, Is.SameAs (_descriptor.BaseType));
    }

    [Test]
    public void Name ()
    {
      Assert.That (_descriptor.Name, Is.Not.Null.And.Not.Empty);

      Assert.That (_mutableType.Name, Is.EqualTo (_descriptor.Name));
    }

    [Test]
    public void Namespace ()
    {
      Assert.That (_descriptor.Namespace, Is.Not.Null.And.Not.Empty);

      Assert.That (_mutableType.Namespace, Is.EqualTo (_descriptor.Namespace));
    }

    [Test]
    public void FullName ()
    {
      Assert.That (_descriptor.FullName, Is.Not.Null.And.Not.Empty);

      Assert.That (_mutableType.FullName, Is.EqualTo (_descriptor.FullName));
    }

    [Test]
    public new void ToString ()
    {
      Assert.That (_descriptor.StringRepresentation, Is.Not.Null.And.Not.Empty);

      Assert.That (_mutableType.ToString(), Is.EqualTo (_descriptor.StringRepresentation));
    }

    [Test]
    public void ToDebugString ()
    {
      Assert.That (_descriptor.StringRepresentation, Is.Not.Null.And.Not.Empty);

      Assert.That (_mutableType.ToDebugString(), Is.EqualTo ("MutableType = \"" + _descriptor.Name + "\""));
    }

    [Test]
    public void IsEquivalentTo_Type_False ()
    {
      var type = ReflectionObjectMother.GetSomeDifferentType();

      Assert.That (_mutableType.IsEquivalentTo (type), Is.False);
    }

    [Test]
    public void IsEquivalentTo_MutableType_True ()
    {
      var mutableType = _mutableType;

      Assert.That (_mutableType.IsEquivalentTo (mutableType), Is.True);
    }

    [Test]
    public void IsEquivalentTo_MutableType_False ()
    {
      var mutableType = MutableTypeObjectMother.Create();

      Assert.That (_mutableType.IsEquivalentTo(mutableType), Is.False);
    }

    [Test]
    public void AddInterface ()
    {
      var interface1 = ReflectionObjectMother.GetSomeInterfaceType();
      var interface2 = ReflectionObjectMother.GetSomeDifferentInterfaceType();

      _mutableType.AddInterface (interface1);
      _mutableType.AddInterface (interface2);

      Assert.That (_mutableType.AddedInterfaces, Is.EqualTo (new[] { interface1, interface2 }));
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
    public void GetInterfaces ()
    {
      var interface1 = _descriptor.Interfaces.First ();
      var interface2 = ReflectionObjectMother.GetSomeDifferentInterfaceType();

      _mutableType.AddInterface (interface2);

      Assert.That (_mutableType.GetInterfaces(), Is.EqualTo (new[] { interface1, interface2 }));
    }

    [Test]
    public void AddField ()
    {
     var newField = _mutableType.AddField (typeof (string), "_newField", FieldAttributes.Private);

      // Correct field info instance
      Assert.That (newField.DeclaringType, Is.SameAs (_mutableType));
      Assert.That (newField.Name, Is.EqualTo ("_newField"));
      Assert.That (newField.FieldType, Is.EqualTo (typeof (string)));
      Assert.That (newField.Attributes, Is.EqualTo (FieldAttributes.Private));
      // Field info is stored
      Assert.That (_mutableType.AddedFields, Is.EqualTo (new[] { newField }));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Field with equal name and signature already exists.\r\nParameter name: name")]
    public void AddField_ThrowsIfAlreadyExist ()
    {
      var field = _descriptor.Fields.First();
      _memberInfoEqualityComparerStub
          .Stub (stub => stub.Equals (Arg<FieldInfo>.Is.Anything, Arg<FieldInfo>.Is.Anything))
          .Return (true);

      _mutableType.AddField (field.FieldType, field.Name, FieldAttributes.Private);
    }

    [Test]
    public void AddField_ReliesOnFieldSignature ()
    {
      var field = _descriptor.Fields.First ();
      _memberInfoEqualityComparerStub
          .Stub (stub => stub.Equals (Arg<FieldInfo>.Is.Anything, Arg<FieldInfo>.Is.Anything))
          .Return (false);

      _mutableType.AddField (field.FieldType, field.Name, FieldAttributes.Private);

      Assert.That (_mutableType.AddedFields, Has.Count.EqualTo (1));
    }

    [Test]
    public void GetFields ()
    {
      var addedField = _mutableType.AddField (ReflectionObjectMother.GetSomeType(), "field2");

      _bindingFlagsEvaluatorMock
        .Stub (stub => stub.HasRightAttributes (Arg<FieldAttributes>.Is.Anything, Arg<BindingFlags>.Is.Anything))
        .Return (true);

      var fields = _mutableType.GetFields (0);

      Assert.That (fields, Is.EquivalentTo (_descriptor.Fields.Concat (addedField)));
    }

    [Test]
    public void GetFields_FilterAddedWithUtility_Added ()
    {
      var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (FieldAttributes.Public, bindingFlags)).Return (false);

      _mutableType.AddField (typeof (int), "_newField", FieldAttributes.Public);
      var fields = _mutableType.GetFields (bindingFlags);

      _bindingFlagsEvaluatorMock.VerifyAllExpectations();
      Assert.That (fields, Is.Empty);
    }

    [Test]
    public void GetFields_FilterAddedWithUtility_Existing ()
    {
      var fieldInfo = _descriptor.Fields.First ();
      var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (fieldInfo.Attributes, bindingFlags)).Return (false);

      var fields = _mutableType.GetFields (bindingFlags);

      _bindingFlagsEvaluatorMock.VerifyAllExpectations ();
      Assert.That (fields, Is.Empty);
    }

    [Test]
    public void GetField ()
    {
      Assert.That (_descriptor.Fields, Has.Count.GreaterThan (1));
      _bindingFlagsEvaluatorMock
          .Stub (stub => stub.HasRightAttributes (Arg<FieldAttributes>.Is.Anything, Arg<BindingFlags>.Is.Anything))
          .Return (true);
      var field = _descriptor.Fields.Last();

      var resultField = _mutableType.GetField (field.Name, BindingFlags.NonPublic | BindingFlags.Instance);

      Assert.That (resultField, Is.SameAs (field));
    }

    [Test]
    public void GetField_NoMatch ()
    {
      Assert.That (_mutableType.GetField ("field"), Is.Null);
    }

    [Test]
    [ExpectedException (typeof (AmbiguousMatchException), ExpectedMessage = "Ambiguous field name 'Field1'.")]
    public void GetField_Ambigious ()
    {
      var fieldName = "Field1";
      _mutableType.AddField (typeof (string), fieldName, 0);
      _bindingFlagsEvaluatorMock
          .Stub (stub => stub.HasRightAttributes (Arg<FieldAttributes>.Is.Anything, Arg<BindingFlags>.Is.Anything))
          .Return (true);
      Assert.That (_mutableType.GetFields().Where (f => f.Name == fieldName).ToArray(), Has.Length.GreaterThan (1));

      _mutableType.GetField (fieldName, 0);
    }

    [Test]
    public void AddConstructor ()
    {
      var attributes = MethodAttributes.Public;
      var parameterDeclarations = ParameterDeclarationObjectMother.CreateMultiple (2);
      var fakeBody = Expression.Empty();
      Func<ConstructorBodyCreationContext, Expression> bodyGenerator = context =>
      {
        Assert.That (context.Parameters, Is.EqualTo (parameterDeclarations.Select (pd => pd.Expression)));
        Assert.That (context.This.Type, Is.SameAs (_mutableType));

        return fakeBody;
      };

      var ctorInfo = _mutableType.AddConstructor (attributes, parameterDeclarations, bodyGenerator);

      // Correct constructor info instance
      Assert.That (ctorInfo.DeclaringType, Is.SameAs (_mutableType));
      Assert.That (ctorInfo.Attributes, Is.EqualTo (attributes));
      var expectedParameterInfos =
          new[]
          {
              new { ParameterType = parameterDeclarations[0].Type },
              new { ParameterType = parameterDeclarations[1].Type }
          };
      var actualParameterInfos = ctorInfo.GetParameters().Select (pi => new { pi.ParameterType });
      Assert.That (actualParameterInfos, Is.EqualTo (expectedParameterInfos));
      Assert.That (ctorInfo.Body, Is.SameAs (fakeBody));

      // Constructor info is stored
      Assert.That (_mutableType.AddedConstructors, Is.EqualTo (new[] { ctorInfo }));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Adding static constructors is not (yet) supported.\r\nParameter name: attributes")]
    public void AddConstructor_ThrowsForStatic ()
    {
      _mutableType.AddConstructor (MethodAttributes.Static, s_emptyParamDecls, context => { throw new NotImplementedException(); });
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Constructor with equal signature already exists.\r\nParameter name: parameterDeclarations")]
    public void AddConstructor_ThrowsIfAlreadyExists ()
    {
      Assert.That (_descriptor.Constructors, Has.Count.EqualTo (1));
      var ctorParameterTypes = _descriptor.Constructors.Single().GetParameters().Select(pi => pi.ParameterType);
      Assert.That (ctorParameterTypes, Is.Empty);
      _bindingFlagsEvaluatorMock
          .Stub (stub => stub.HasRightAttributes (Arg<MethodAttributes>.Is.Anything, Arg<BindingFlags>.Is.Anything))
          .Return (true);
      _memberInfoEqualityComparerStub.Stub (stub => stub.Equals (Arg<MemberInfo>.Is.Anything, Arg<MemberInfo>.Is.Anything)).Return (true);

      _mutableType.AddConstructor (0, s_emptyParamDecls, context => Expression.Empty());
    }

    [Test]
    public void GetConstructors ()
    {
      Assert.That (_descriptor.Constructors, Has.Count.EqualTo (1));
      var existingConstructor = _descriptor.Constructors.Single ();
      var attributes = MethodAttributes.Public;
      var parameterDeclarations = new ArgumentTestHelper (7).ParameterDeclarations; // Need different signature
      var addedConstructor = AddConstructor (_mutableType, attributes, parameterDeclarations);

      _bindingFlagsEvaluatorMock
          .Stub (mock => mock.HasRightAttributes (Arg<MethodAttributes>.Is.Anything, Arg<BindingFlags>.Is.Anything))
          .Return (true);

      var constructors = _mutableType.GetConstructors (0);

      Assert.That (constructors, Has.Length.EqualTo (2));
      Assert.That (constructors[0], Is.TypeOf<MutableConstructorInfo> ());
      var mutatedConstructorInfo = (MutableConstructorInfo) constructors[0];
      Assert.That (mutatedConstructorInfo.UnderlyingSystemConstructorInfo, Is.EqualTo (existingConstructor));

      Assert.That (constructors[1], Is.SameAs (addedConstructor));
    }

    [Test]
    public void GetConstructors_FilterWithUtility_ExistingConstructor ()
    {
      var existingCtorInfo = _descriptor.Constructors.Single();
      var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (existingCtorInfo.Attributes, bindingFlags)).Return (false);

      var constructors = _mutableType.GetConstructors (bindingFlags);

      _bindingFlagsEvaluatorMock.VerifyAllExpectations ();
      Assert.That (constructors, Is.Empty);
    }

    [Test]
    public void GetConstructors_FilterWithUtility_AddedConstructor ()
    {
      var addedCtorInfo = AddConstructor (_mutableType, MethodAttributes.Public);

      var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (addedCtorInfo.Attributes, bindingFlags)).Return (false);
      
      var constructors = _mutableType.GetConstructors (bindingFlags);

      _bindingFlagsEvaluatorMock.VerifyAllExpectations ();
      Assert.That (constructors, Is.Empty);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
      "Constructor is declared by a different type: 'System.String'.\r\nParameter name: constructor")]
    public void GetMutableConstructor_NotEquivalentDeclaringType ()
    {
      var ctorStub = MockRepository.GenerateStub<ConstructorInfo>();
      ctorStub.Stub (stub => stub.DeclaringType).Return (typeof(string));

      _mutableType.GetMutableConstructor (ctorStub);
    }

    [Test]
    public void GetMutableConstructor_MutableConstructorInfo ()
    {
      var ctor = AddConstructor (_mutableType, 0);

      var result = _mutableType.GetMutableConstructor (ctor);

      Assert.That (result, Is.SameAs (ctor));
    }

    [Test]
    public void GetMutableConstructor_StandardConstructorInfo ()
    {
      var standardCtor = _descriptor.Constructors.Single ();
      Assert.That (standardCtor, Is.Not.AssignableTo<MutableConstructorInfo>());

      var result = _mutableType.GetMutableConstructor (standardCtor);

      Assert.That (result.DeclaringType, Is.SameAs (_mutableType));
      Assert.That (result.UnderlyingSystemConstructorInfo, Is.SameAs (standardCtor));
      Assert.That (_mutableType.ExistingConstructors, Has.Member (result));
    }

    [Test]
    public void GetMutableConstructor_StandardConstructorInfo_Twice ()
    {
      var standardCtor = _descriptor.Constructors.Single ();
      Assert.That (standardCtor, Is.Not.AssignableTo<MutableConstructorInfo> ());

      var result1 = _mutableType.GetMutableConstructor (standardCtor);
      var result2 = _mutableType.GetMutableConstructor (standardCtor);

      Assert.That (result1, Is.SameAs (result2));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "The given constructor cannot be mutated.")]
    public void GetMutableConstructor_StandardConstructorInfo_Unknown ()
    {
      var ctorStub = MockRepository.GenerateStub<ConstructorInfo>();
      ctorStub.Stub (stub => stub.DeclaringType).Return (_mutableType.UnderlyingSystemType);

      _mutableType.GetMutableConstructor (ctorStub);
    }

    [Test]
    public void Accept ()
    {
      Assert.That (_mutableType.ExistingInterfaces, Is.Not.Empty);
      var addedInterface = ReflectionObjectMother.GetSomeDifferentInterfaceType ();
      _mutableType.AddInterface (addedInterface);

      Assert.That (_mutableType.ExistingFields, Is.Not.Empty);
      var addedFieldInfo = _mutableType.AddField (ReflectionObjectMother.GetSomeType (), "name", FieldAttributes.Private);

      Assert.That (_mutableType.ExistingConstructors, Is.Not.Empty);
      var addedConstructorInfo = AddConstructor (_mutableType, 0);

      var handlerMock = MockRepository.GenerateMock<ITypeModificationHandler>();
      
      _mutableType.Accept (handlerMock);

      handlerMock.AssertWasCalled (mock => mock.HandleAddedInterface (addedInterface));
      handlerMock.AssertWasCalled (mock => mock.HandleAddedField (addedFieldInfo));
      handlerMock.AssertWasCalled (mock => mock.HandleAddedConstructor (addedConstructorInfo));
    }

    [Test]
    public void HasElementTypeImpl ()
    {
      Assert.That (_mutableType.HasElementType, Is.False);
    }

    [Test]
    public void IsByRefImpl ()
    {
      Assert.That (_mutableType.IsByRef, Is.False);
    }

    [Test]
    public void GetAttributeFlagsImpl ()
    {
      Assert.That (_mutableType.Attributes, Is.EqualTo (_descriptor.Attributes));
    }

    [Test]
    public void GetConstructorImpl ()
    {
      var arguments = new ArgumentTestHelper (typeof (int));
      var addedConstructor = _mutableType.AddConstructor(0, arguments.ParameterDeclarations, ctx => Expression.Empty());
      
      _bindingFlagsEvaluatorMock
          .Stub (stub => stub.HasRightAttributes (Arg<MethodAttributes>.Is.Anything, Arg<BindingFlags>.Is.Anything))
          .Return (true);
      Assert.That (_mutableType.GetConstructors (), Has.Length.GreaterThan (1));

      var resultCtor = _mutableType.GetConstructor (arguments.Types);
      Assert.That (resultCtor, Is.SameAs (addedConstructor));
    }

    [Test]
    public void GetConstructorImpl_NoMatch ()
    {
      Assert.That (_mutableType.GetConstructor (Type.EmptyTypes), Is.Null);
    }

    private MutableConstructorInfo AddConstructor (MutableType mutableType, MethodAttributes attributes, params ParameterDeclaration[] parameterDeclarations)
    {
      return mutableType.AddConstructor (attributes, parameterDeclarations, context => Expression.Empty ());
    }

    public class DomainClass : IDomainInterface
    {
      protected int Field1 = 1;
      protected int Field2 = 2;

      public DomainClass ()
      {
        Dev.Null = Field1;
        Dev.Null = Field2;
      }
    }

    public interface IDomainInterface
    {
    }
  }
}