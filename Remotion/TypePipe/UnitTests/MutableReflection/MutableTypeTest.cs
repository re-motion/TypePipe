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
using Remotion.TypePipe.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableTypeTest
  {
    private static readonly IEnumerable<ParameterDeclaration> s_emptyParamDecls = Enumerable.Empty<ParameterDeclaration>();

    private IUnderlyingTypeStrategy _typeStrategyStub;
    private IEqualityComparer<MemberInfo> _memberInfoEqualityComparerStub;
    private IBindingFlagsEvaluator _bindingFlagsEvaluatorMock;
    private MutableType _mutableType;

    [SetUp]
    public void SetUp ()
    {
      _typeStrategyStub = MockRepository.GenerateStub<IUnderlyingTypeStrategy>();
      _memberInfoEqualityComparerStub = MockRepository.GenerateStub<IEqualityComparer<MemberInfo>>();
      _bindingFlagsEvaluatorMock = MockRepository.GenerateMock<IBindingFlagsEvaluator>();
      
      _mutableType = CreateMutableType();
    }

    [Test]
    public void Initialization_WithoutConstructors ()
    {
      Assert.That (_mutableType.AddedInterfaces, Is.Empty);
      Assert.That (_mutableType.AddedFields, Is.Empty);
      Assert.That (_mutableType.AddedConstructors, Is.Empty);
      Assert.That (_mutableType.ExistingConstructors, Is.Empty);
    }

    [Test]
    public void Initialization_WithConstructors ()
    {
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance; // Don't return static constructors by default
      var ctorInfo = ReflectionObjectMother.GetSomeDefaultConstructor();
      _typeStrategyStub.Stub (stub => stub.GetConstructors (bindingFlags)).Return (new[] { ctorInfo });

      var mutableType = CreateMutableType();

      Assert.That (mutableType.ExistingConstructors, Has.Count.EqualTo (1));
      var existingCtor = mutableType.ExistingConstructors.Single();

      Assert.That (existingCtor.UnderlyingSystemConstructorInfo, Is.EqualTo (ctorInfo));
      Assert.That (existingCtor.DeclaringType, Is.SameAs (mutableType));
    }

    [Test]
    public void UnderlyingSystemType ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      _typeStrategyStub.Stub (stub => stub.GetUnderlyingSystemType()).Return (type);

      Assert.That (_mutableType.UnderlyingSystemType, Is.SameAs (type));
    }

    [Test]
    public void UnderlyingSystemType_ForNull ()
    {
      _typeStrategyStub.Stub (stub => stub.GetUnderlyingSystemType()).Return (null);

      Assert.That (_mutableType.UnderlyingSystemType, Is.SameAs (_mutableType));
    }

    [Test]
    public void Assembly ()
    {
      Assert.That (_mutableType.Assembly, Is.Null);
    }

    [Test]
    public void BaseType ()
    {
      var baseType = ReflectionObjectMother.GetSomeType();
      _typeStrategyStub.Stub (stub => stub.GetBaseType()).Return (baseType);

      Assert.That (_mutableType.BaseType, Is.SameAs (baseType));
    }

    [Test]
    public void Name ()
    {
      _typeStrategyStub.Stub (stub => stub.GetName()).Return ("bar");

      Assert.That (_mutableType.Name, Is.EqualTo ("bar"));
    }

    [Test]
    public void Namespace ()
    {
      _typeStrategyStub.Stub (stub => stub.GetNamespace ()).Return ("foo");

      Assert.That (_mutableType.Namespace, Is.EqualTo ("foo"));
    }

    [Test]
    public void FullName ()
    {
      _typeStrategyStub.Stub (stub => stub.GetFullName ()).Return ("foo.bar");

      Assert.That (_mutableType.FullName, Is.EqualTo ("foo.bar"));
    }

    [Test]
    public new void ToString ()
    {
      _typeStrategyStub.Stub (stub => stub.GetToStringRepresentation ()).Return ("foo");

      Assert.That (_mutableType.ToString(), Is.EqualTo ("foo"));
    }

    [Test]
    public void IsEquivalentTo_Type_False ()
    {
      var underlyingType = ReflectionObjectMother.GetSomeType();
      var type = ReflectionObjectMother.GetSomeDifferentType();
      _typeStrategyStub.Stub (stub => stub.GetUnderlyingSystemType()).Return (underlyingType);

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
      _typeStrategyStub.Stub (stub => stub.GetInterfaces()).Return (Type.EmptyTypes);

      _mutableType.AddInterface (typeof (IDisposable));
      _mutableType.AddInterface (typeof (IComparable));

      Assert.That (_mutableType.AddedInterfaces, Is.EqualTo (new[] { typeof (IDisposable), typeof (IComparable) }));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Type must be an interface.\r\nParameter name: interfaceType")]
    public void AddInterface_ThrowsIfNotAnInterface ()
    {
      _mutableType.AddInterface (typeof (string));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Interface 'System.IDisposable' is already implemented.\r\nParameter name: interfaceType")]
    public void AddInterface_ThrowsIfAlreadyImplemented ()
    {
      _typeStrategyStub.Stub (stub => stub.GetInterfaces()).Return (new[] { typeof (IDisposable) });

      _mutableType.AddInterface (typeof (IDisposable));
    }

    [Test]
    public void GetInterfaces ()
    {
      _typeStrategyStub.Stub (stub => stub.GetInterfaces()).Return (new[] { typeof (IDisposable) });
      _mutableType.AddInterface (typeof (IComparable));

      Assert.That (_mutableType.GetInterfaces(), Is.EqualTo (new[] { typeof (IDisposable), typeof (IComparable) }));
    }

    [Test]
    public void AddField ()
    {
      _typeStrategyStub.Stub (stub => stub.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
          .Return (new FieldInfo[0]);

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
      var field = MutableFieldInfoObjectMother.Create (name: "_bla", fieldType: typeof (string));
      _typeStrategyStub.Stub (stub => stub.GetFields (Arg<BindingFlags>.Is.Anything)).Return (new[] { field });
      _memberInfoEqualityComparerStub
          .Stub (stub => stub.Equals (Arg<FieldInfo>.Is.Anything, Arg<FieldInfo>.Is.Anything))
          .Return (true);

      _mutableType.AddField (typeof (string), "_bla", FieldAttributes.Private);
    }

    [Test]
    public void AddField_ReliesOnFieldSignature ()
    {
      var field = MutableFieldInfoObjectMother.Create (name: "_foo", fieldType: typeof (object));
      _typeStrategyStub.Stub (stub => stub.GetFields (Arg<BindingFlags>.Is.Anything)).Return (new[] { field });
      var attributes = FieldAttributes.Private;
      var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
      _memberInfoEqualityComparerStub.Stub (stub => stub.Equals (Arg<FieldInfo>.Is.Anything, Arg<FieldInfo>.Is.Anything))
          .Return (false);
      _bindingFlagsEvaluatorMock.Stub (stub => stub.HasRightAttributes (attributes, bindingFlags)).Return (true);

      _mutableType.AddField (typeof (string), "_foo", attributes);
      var fields = _mutableType.GetFields (bindingFlags);

      Assert.That (fields, Has.Length.EqualTo (2));
    }

    [Test]
    public void GetFields ()
    {
      var field1 = MutableFieldInfoObjectMother.Create();
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
      _typeStrategyStub.Stub (stub => stub.GetFields (bindingFlags)).Return (new[] { field1 });
      var attributes = FieldAttributes.Private;
      _bindingFlagsEvaluatorMock.Stub (stub => stub.HasRightAttributes (attributes, bindingFlags)).Return (true);

      var field2 = _mutableType.AddField (ReflectionObjectMother.GetSomeType(), "field2", attributes);
      var fields = _mutableType.GetFields (bindingFlags);

      Assert.That (fields, Is.EqualTo (new[] { field1, field2 }));
    }

    [Test]
    public void GetFields_FilterAddedWithUtility ()
    {
      _typeStrategyStub.Stub (stub => stub.GetFields (Arg<BindingFlags>.Is.Anything)).Return (new FieldInfo[0]);
      var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (FieldAttributes.Public, bindingFlags)).Return (false);

      _mutableType.AddField (typeof (int), "_newField", FieldAttributes.Public);
      var fields = _mutableType.GetFields (bindingFlags);

      _bindingFlagsEvaluatorMock.VerifyAllExpectations();
      Assert.That (fields, Is.Empty);
    }

    [Test]
    public void GetField ()
    {
      var field1 = MutableFieldInfoObjectMother.Create (name: "field1", fieldType: typeof (string));
      var field2 = MutableFieldInfoObjectMother.Create (name: "field2", fieldType: typeof (int));
      _typeStrategyStub.Stub (stub => stub.GetFields (Arg<BindingFlags>.Is.Anything)).Return (new[] { field1, field2 });
      _bindingFlagsEvaluatorMock
          .Stub (stub => stub.HasRightAttributes (Arg<FieldAttributes>.Is.Anything, Arg<BindingFlags>.Is.Anything))
          .Return (true);

      var resultField = _mutableType.GetField ("field2", BindingFlags.NonPublic | BindingFlags.Instance);

      Assert.That (resultField, Is.SameAs (field2));
    }

    [Test]
    public void GetField_NoMatch ()
    {
      _typeStrategyStub.Stub (stub => stub.GetFields (Arg<BindingFlags>.Is.Anything)).Return (new FieldInfo[0]);

      Assert.That (_mutableType.GetField ("field"), Is.Null);
    }

    [Test]
    [ExpectedException (typeof (AmbiguousMatchException), ExpectedMessage = "Ambiguous field name 'foo'.")]
    public void GetField_Ambigious ()
    {
      var field1 = MutableFieldInfoObjectMother.Create (name: "foo", fieldType: typeof (string));
      var field2 = MutableFieldInfoObjectMother.Create (name: "foo", fieldType: typeof (int));
      _typeStrategyStub.Stub (stub => stub.GetFields (Arg<BindingFlags>.Is.Anything)).Return (new[] { field1, field2 });
      _bindingFlagsEvaluatorMock
          .Stub (stub => stub.HasRightAttributes (Arg<FieldAttributes>.Is.Anything, Arg<BindingFlags>.Is.Anything))
          .Return (true);

      _mutableType.GetField ("foo", 0);
    }

    [Test]
    public void AddConstructor ()
    {
      var attributes = MethodAttributes.Public;
      var parameterDeclarations = ParameterDeclarationObjectMother.CreateMultiple (2);
      var fakeBody = Expression.Empty();
      Func<ConstructorAdditionContext, Expression> bodyGenerator = context =>
      {
        Assert.That (context.ParameterExpressions, Is.EqualTo (parameterDeclarations.Select (pd => pd.Expression)));
        Assert.That (context.ThisExpression.Type, Is.SameAs (_mutableType));

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
      var existingCtor = ReflectionObjectMother.GetSomeDefaultConstructor();
      var mutableType = CreateMutableType (existingCtor);

      _bindingFlagsEvaluatorMock
          .Stub (stub => stub.HasRightAttributes (Arg<MethodAttributes>.Is.Anything, Arg<BindingFlags>.Is.Anything))
          .Return (true);
      _memberInfoEqualityComparerStub.Stub (stub => stub.Equals (Arg<MemberInfo>.Is.Anything, Arg<MemberInfo>.Is.Anything)).Return (true);

      mutableType.AddConstructor (0, s_emptyParamDecls, context => Expression.Empty());
    }

    [Test]
    public void GetConstructors ()
    {
      var existingConstructor = ReflectionObjectMother.GetSomeDefaultConstructor();
      var mutableType = CreateMutableType (existingConstructor);

      var attributes = MethodAttributes.Public;
      var parameterDeclarations = new ArgumentTestHelper (7).ParameterDeclarations; // Need different signature
      var addedConstructor = AddConstructor (mutableType, attributes, parameterDeclarations);

      _bindingFlagsEvaluatorMock
          .Stub (mock => mock.HasRightAttributes (Arg<MethodAttributes>.Is.Anything, Arg<BindingFlags>.Is.Anything))
          .Return (true);

      var constructors = mutableType.GetConstructors (0);

      Assert.That (constructors, Has.Length.EqualTo (2));
      Assert.That (constructors[0], Is.TypeOf<MutableConstructorInfo> ());
      var mutatedConstructorInfo = (MutableConstructorInfo) constructors[0];
      Assert.That (mutatedConstructorInfo.UnderlyingSystemConstructorInfo, Is.EqualTo (existingConstructor));

      Assert.That (constructors[1], Is.SameAs (addedConstructor));
    }

    [Test]
    public void GetConstructors_FilterWithUtility_ExistingConstructor ()
    {
      var existingCtorInfo = ReflectionObjectMother.GetSomeDefaultConstructor();
      var mutableType = CreateMutableType (existingCtorInfo);

      var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (existingCtorInfo.Attributes, bindingFlags)).Return (false);

      var constructors = mutableType.GetConstructors (bindingFlags);

      _bindingFlagsEvaluatorMock.VerifyAllExpectations ();
      Assert.That (constructors, Is.Empty);
    }

    [Test]
    public void GetConstructors_FilterWithUtility_AddedConstructor ()
    {
      var addedCtorInfo = AddConstructor (_mutableType, MethodAttributes.Public);

      var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
      _bindingFlagsEvaluatorMock.Stub (stub => stub.HasRightAttributes (addedCtorInfo.Attributes, bindingFlags)).Return (false);
      
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
      var someCtor = ReflectionObjectMother.GetSomeDefaultConstructor ();
      var mutableType = CreateMutableType (someCtor);

      // Stub underlying type so that declaring type check in GetMutableConstructor succeeds
      _typeStrategyStub.Stub (stub => stub.GetUnderlyingSystemType ()).Return (someCtor.DeclaringType);

      var result = mutableType.GetMutableConstructor (someCtor);

      Assert.That (result.DeclaringType, Is.SameAs (mutableType));
      Assert.That (result.UnderlyingSystemConstructorInfo, Is.SameAs (someCtor));
    }

    [Test]
    public void GetMutableConstructor_StandardConstructorInfo_Twice ()
    {
      var someCtor = ReflectionObjectMother.GetSomeDefaultConstructor ();
      var mutableType = CreateMutableType (someCtor);
     
      // Stub underlying type so that declaring type check in GetMutableConstructor succeeds
      _typeStrategyStub.Stub (stub => stub.GetUnderlyingSystemType ()).Return (someCtor.DeclaringType);

      var result1 = mutableType.GetMutableConstructor (someCtor);
      var result2 = mutableType.GetMutableConstructor (someCtor);

      Assert.That (result1, Is.SameAs (result2));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "The given constructor cannot be mutated.")]
    public void GetMutableConstructor_StandardConstructorInfo_Unknown ()
    {
      var someCtor = ReflectionObjectMother.GetSomeDefaultConstructor ();
      var mutableType = CreateMutableType();

      // Stub underlying type so that declaring type check in GetMutableConstructor succeeds
      _typeStrategyStub.Stub (stub => stub.GetUnderlyingSystemType ()).Return (someCtor.DeclaringType);

      mutableType.GetMutableConstructor (someCtor);
    }

    [Test]
    public void Accept ()
    {
      _typeStrategyStub
          .Stub (stub => stub.GetInterfaces())
          .Return (new[] { ReflectionObjectMother.GetSomeInterfaceType() });
      var addedInterface = ReflectionObjectMother.GetSomeDifferentInterfaceType ();
      _mutableType.AddInterface (addedInterface);

      _typeStrategyStub
          .Stub (stub => stub.GetFields (Arg<BindingFlags>.Is.Anything))
          .Return (new[] { ReflectionObjectMother.GetSomeField() });
      var addedFieldInfo = _mutableType.AddField (ReflectionObjectMother.GetSomeType (), "name", FieldAttributes.Private);

      _typeStrategyStub
          .Stub (stub => stub.GetConstructors (Arg<BindingFlags>.Is.Anything))
          .Return (new[] { ReflectionObjectMother.GetSomeDefaultConstructor () });
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
      _typeStrategyStub.Stub (stub => stub.GetAttributeFlags()).Return (TypeAttributes.Sealed);

      Assert.That (_mutableType.Attributes, Is.EqualTo (TypeAttributes.Sealed));
    }

    [Test]
    public void GetConstructorImpl ()
    {
      var constructor1 = MutableConstructorInfoObjectMother.Create();
      var arguments = new ArgumentTestHelper (typeof (int));
      var constructor2 = MutableConstructorInfoObjectMother.CreateForNewWithParameters (parameterDeclarations: arguments.ParameterDeclarations);
      var mutableType = CreateMutableType (constructor1, constructor2);
      var mutableConstructor2 = mutableType.ExistingConstructors.Single (c => c.UnderlyingSystemConstructorInfo == constructor2);
      
      _bindingFlagsEvaluatorMock
          .Stub (stub => stub.HasRightAttributes (Arg<MethodAttributes>.Is.Anything, Arg<BindingFlags>.Is.Anything))
          .Return (true);

      var resultCtor = mutableType.GetConstructor (arguments.Types);
      Assert.That (resultCtor, Is.SameAs (mutableConstructor2));
    }

    [Test]
    public void GetConstructorImpl_NoMatch ()
    {
      Assert.That (_mutableType.GetConstructor (Type.EmptyTypes), Is.Null);
    }

    private MutableType CreateMutableType (params ConstructorInfo[] existingConstructors)
    {
      _typeStrategyStub.Stub (stub => stub.GetConstructors (Arg<BindingFlags>.Is.Anything)).Return (existingConstructors).Repeat.Once();
      
      return new MutableType (_typeStrategyStub, _memberInfoEqualityComparerStub, _bindingFlagsEvaluatorMock);
    }

    private MutableConstructorInfo AddConstructor (MutableType mutableType, MethodAttributes attributes, params ParameterDeclaration[] parameterDeclarations)
    {
      return mutableType.AddConstructor (attributes, parameterDeclarations, context => Expression.Empty ());
    }

  }
}