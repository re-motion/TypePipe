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
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Rhino.Mocks;
using Remotion.Development.UnitTesting.Enumerables;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class MemberSelectorTest
  {
    private IBindingFlagsEvaluator _bindingFlagsEvaluatorMock;
    
    private MemberSelector _selector;

    private Type _someDeclaringType;

    [SetUp]
    public void SetUp ()
    {
      _bindingFlagsEvaluatorMock = MockRepository.GenerateStrictMock<IBindingFlagsEvaluator>();

      _selector = new MemberSelector (_bindingFlagsEvaluatorMock);

      _someDeclaringType = ReflectionObjectMother.GetSomeType();
    }

    [Test]
    public void SelectTypes ()
    {
      var candidates = new[]
                       {
                           CreateTypeStub (attributes: TypeAttributes.NestedPublic),
                           CreateTypeStub (attributes: TypeAttributes.NestedFamily),
                           CreateTypeStub (attributes: TypeAttributes.Abstract)
                       };
      var bindingFlags = (BindingFlags) 1;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[0].Attributes, bindingFlags)).Return (true);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[1].Attributes, bindingFlags)).Return (false);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[2].Attributes, bindingFlags)).Return (true);

      var result = _selector.SelectTypes (candidates, bindingFlags).ForceEnumeration();

      _bindingFlagsEvaluatorMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (new[] { candidates[0], candidates[2] }));
    }

    [Test]
    public void SelectFields ()
    {
      var candidates = new[]
                       {
                           CreateFieldStub (attributes: FieldAttributes.Assembly),
                           CreateFieldStub (attributes: FieldAttributes.Family),
                           CreateFieldStub (attributes: FieldAttributes.FamORAssem)
                       };
      var bindingFlags = (BindingFlags) 1;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[0].Attributes, bindingFlags)).Return (true);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[1].Attributes, bindingFlags)).Return (false);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[2].Attributes, bindingFlags)).Return (true);

      var result = _selector.SelectFields (candidates, bindingFlags, _someDeclaringType).ForceEnumeration();

      _bindingFlagsEvaluatorMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (new[] { candidates[0], candidates[2] }));
    }

    [Test]
    public void SelectFields_DeclaredOnly ()
    {
      var declaringType1 = typeof (string);
      var declaringType2 = typeof (int);
      var candidates = new[] { CreateFieldStub (declaringType: declaringType1), CreateFieldStub (declaringType: declaringType2) };
      var bindingFlags = (BindingFlags) 1 | BindingFlags.DeclaredOnly;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[1].Attributes, bindingFlags)).Return (true);

      var result = _selector.SelectFields (candidates, bindingFlags, declaringType2).ForceEnumeration();

      _bindingFlagsEvaluatorMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (new[] { candidates[1] }));
    }

    [Test]
    public void SelectMethods ()
    {
      var candidates = new[] { CreateMethodStub(), CreateMethodStub(), CreateMethodStub() };
      var bindingFlags = (BindingFlags) 1;
      var declaringType = ReflectionObjectMother.GetSomeType();

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[0].Attributes, bindingFlags)).Return (true);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[1].Attributes, bindingFlags)).Return (false);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[2].Attributes, bindingFlags)).Return (true);

      var result = _selector.SelectMethods (candidates, bindingFlags, declaringType).ForceEnumeration();

      _bindingFlagsEvaluatorMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (new[] { candidates[0], candidates[2] }));
    }

    [Test]
    public void SelectMethods_DeclaredOnly ()
    {
      var declaringType1 = typeof (string);
      var declaringType2 = typeof (int);
      var candidates = new[] { CreateMethodStub (declaringType: declaringType1), CreateMethodStub (declaringType: declaringType2) };
      var bindingFlags = (BindingFlags) 1 | BindingFlags.DeclaredOnly;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (candidates[1].Attributes, bindingFlags)).Return (true);

      var result = _selector.SelectMethods (candidates, bindingFlags, declaringType2).ForceEnumeration();

      _bindingFlagsEvaluatorMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (new[] { candidates[1] }));
    }

    [Test]
    public void SelectProperties ()
    {
      var candidates =
          new[]
          {
              CreatePropertyStub (accessors: new[] { CreateMethodStub (attributes: MethodAttributes.Final) }),
              // Visbility is encoded in the lower 3 bits.
              CreatePropertyStub (
                  accessors: new[] { CreateMethodStub (attributes: (MethodAttributes) 1), CreateMethodStub (attributes: (MethodAttributes) 2) }),
              // The 4-th bit (value 8) does not contribute to the visibility and should be masked out.
              CreatePropertyStub (
                  accessors: new[] { CreateMethodStub (attributes: (MethodAttributes) 3), CreateMethodStub (attributes: (MethodAttributes) 4) })
          };
      var bindingFlags = (BindingFlags) 1;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (MethodAttributes.Final, bindingFlags)).Return (false);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (((MethodAttributes) 1), bindingFlags)).Return (false);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (((MethodAttributes) 2), bindingFlags)).Return (true);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes ((MethodAttributes) 3, bindingFlags)).Return (true);

      var result = _selector.SelectProperties (candidates, bindingFlags, _someDeclaringType).ForceEnumeration();

      _bindingFlagsEvaluatorMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (new[] { candidates[1], candidates[2] }));
    }

    [Test]
    public void SelectProperties_DeclaredOnly ()
    {
      var declaringType1 = typeof (string);
      var declaringType2 = typeof (int);
      var consideredAccessor = CreateMethodStub();
      var candidates =
          new[]
          {
              CreatePropertyStub (declaringType: declaringType1),
              CreatePropertyStub (declaringType: declaringType2, accessors: new[] { consideredAccessor })
          };
      var bindingFlags = (BindingFlags) 1 | BindingFlags.DeclaredOnly;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (consideredAccessor.Attributes, bindingFlags)).Return (true);

      var result = _selector.SelectProperties (candidates, bindingFlags, declaringType2).ForceEnumeration ();

      _bindingFlagsEvaluatorMock.VerifyAllExpectations ();
      Assert.That (result, Is.EqualTo (new[] { candidates[1] }));
    }

    [Test]
    public void SelectEvents ()
    {
      var candidates = new[]
                       {
                           CreateEventStub (adder: CreateMethodStub (attributes: MethodAttributes.Assembly)),
                           CreateEventStub (adder: CreateMethodStub (attributes: MethodAttributes.Family)),
                           CreateEventStub (adder: CreateMethodStub (attributes: MethodAttributes.FamORAssem))
                       };
      var bindingFlags = (BindingFlags) 1;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (MethodAttributes.Assembly, bindingFlags)).Return (true);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (MethodAttributes.Family, bindingFlags)).Return (false);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (MethodAttributes.FamORAssem, bindingFlags)).Return (true);

      var result = _selector.SelectEvents (candidates, bindingFlags, _someDeclaringType).ForceEnumeration();

      _bindingFlagsEvaluatorMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (new[] { candidates[0], candidates[2] }));
    }

    [Test]
    public void SelectEvents_DeclaredOnly ()
    {
      var declaringType1 = typeof (string);
      var declaringType2 = typeof (int);
      var consideredAdder = CreateMethodStub();
      var candidates = new[]
                       { CreateEventStub (declaringType: declaringType1), CreateEventStub (declaringType: declaringType2, adder: consideredAdder) };
      var bindingFlags = (BindingFlags) 1 | BindingFlags.DeclaredOnly;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (consideredAdder.Attributes, bindingFlags)).Return (true);

      var result = _selector.SelectEvents (candidates, bindingFlags, declaringType2).ForceEnumeration();

      _bindingFlagsEvaluatorMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (new[] { candidates[1] }));
    }

    [Test]
    public void SelectSingleType ()
    {
      var types = new[]
                  {
                      CreateTypeStub ("type1", TypeAttributes.NestedAssembly),
                      CreateTypeStub ("this type is removed because of its name", TypeAttributes.NestedFamily),
                      CreateTypeStub ("type1", TypeAttributes.NestedPublic)
                  };
      var bindingFlags = (BindingFlags)1;

      _bindingFlagsEvaluatorMock.Expect(mock => mock.HasRightAttributes(types[0].Attributes, bindingFlags)).Return(false);
      _bindingFlagsEvaluatorMock.Expect(mock => mock.HasRightAttributes(types[2].Attributes, bindingFlags)).Return(true);

      var result = _selector.SelectSingleType (types, bindingFlags, "type1");

      _bindingFlagsEvaluatorMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (types[2]));
    }

    [Test]
    public void SelectSingleField ()
    {
      var fields = new[]
                   {
                       CreateFieldStub ("field1", FieldAttributes.Assembly),
                       CreateFieldStub ("this field is removed because of its name", FieldAttributes.Family),
                       CreateFieldStub ("field1", FieldAttributes.Public)
                   };
      var bindingFlags = (BindingFlags) 1;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (fields[0].Attributes, bindingFlags)).Return (false);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (fields[2].Attributes, bindingFlags)).Return (true);

      var result = _selector.SelectSingleField (fields, bindingFlags, "field1", _someDeclaringType);

      _bindingFlagsEvaluatorMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (fields[2]));
    }

    [Test]
    public void SelectSingleField_NoMatching ()
    {
      var fields = new[] { CreateFieldStub ("field1"), CreateFieldStub ("wrong name") };
      var bindingFlags = (BindingFlags) 1;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (fields[0].Attributes, bindingFlags)).Return (false);

      var result = _selector.SelectSingleField (fields, bindingFlags, "field1", _someDeclaringType);

      Assert.That (result, Is.Null);
    }

    [Test]
    [ExpectedException (typeof (AmbiguousMatchException), ExpectedMessage = "Ambiguous field name 'field1'.")]
    public void SelectSingleField_Ambiguous ()
    {
      var fields = new[] { CreateFieldStub ("field1", FieldAttributes.Assembly), CreateFieldStub ("field1", FieldAttributes.Public) };
      var bindingFlags = (BindingFlags) 1;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (fields[0].Attributes, bindingFlags)).Return (true);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (fields[1].Attributes, bindingFlags)).Return (true);

      _selector.SelectSingleField (fields, bindingFlags, "field1", _someDeclaringType);
    }

    [Test]
    public void SelectSingleMethod ()
    {
      var methods =
          new[]
          {
              CreateMethodStub ("Method1", MethodAttributes.Assembly),
              CreateMethodStub ("This method is filtered because of its name", MethodAttributes.Family),
              CreateMethodStub ("Method1", MethodAttributes.Public)
          };
      var bindingFlags = (BindingFlags) 1;
      var typesOrNull = new[] { typeof (int), typeof (string) };
      var modifiersOrNull = new[] { new ParameterModifier (2) };

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (methods[0].Attributes, bindingFlags)).Return (false);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (methods[2].Attributes, bindingFlags)).Return (true);

      var binderMock = MockRepository.GenerateStrictMock<Binder>();
      var fakeResult = ReflectionObjectMother.GetSomeMethod();
      binderMock
          .Expect (mock => mock.SelectMethod (bindingFlags, new MethodBase[] { methods[2] }, typesOrNull, modifiersOrNull))
          .Return (fakeResult);

      var result = _selector.SelectSingleMethod (methods, binderMock, bindingFlags, "Method1", _someDeclaringType, typesOrNull, modifiersOrNull);

      _bindingFlagsEvaluatorMock.VerifyAllExpectations();
      binderMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void SelectSingleMethod_ForConstructors_NameIsNotConsidered ()
    {
      var constructors = new[] { CreateConstructorStub (MethodAttributes.Assembly), CreateConstructorStub (MethodAttributes.Family) };
      var bindingFlags = (BindingFlags) 1;
      var typesOrNull = new[] { typeof (int), typeof (string) };
      var modifiersOrNull = new[] { new ParameterModifier (2) };

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (constructors[0].Attributes, bindingFlags)).Return (false);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (constructors[1].Attributes, bindingFlags)).Return (true);

      var binderMock = MockRepository.GenerateStrictMock<Binder>();
      var fakeResult = ReflectionObjectMother.GetSomeConstructor();
      binderMock
          .Expect (mock => mock.SelectMethod (bindingFlags, new MethodBase[] { constructors[1] }, typesOrNull, modifiersOrNull))
          .Return (fakeResult);

      var result = _selector.SelectSingleMethod (constructors, binderMock, bindingFlags, null, _someDeclaringType, typesOrNull, modifiersOrNull);

      _bindingFlagsEvaluatorMock.VerifyAllExpectations ();
      binderMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void SelectSingleMethod_NoMatching ()
    {
      var methods = new[] { CreateMethodStub ("Method1"), CreateMethodStub ("wrong name") };
      var binderStub = MockRepository.GenerateStub<Binder> ();
      var bindingFlags = (BindingFlags) 1;
      var typesOrNull = new[] { typeof (int), typeof (string) };
      var modifiersOrNull = new[] { new ParameterModifier (2) };

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (methods[0].Attributes, bindingFlags)).Return (false);

      var result = _selector.SelectSingleMethod (methods, binderStub, bindingFlags, "Method1", _someDeclaringType, typesOrNull, modifiersOrNull);

      _bindingFlagsEvaluatorMock.VerifyAllExpectations();
      Assert.That (result, Is.Null);
    }

    [Test]
    [ExpectedException (typeof (AmbiguousMatchException), ExpectedMessage = "Ambiguous method name 'Method1'.")]
    public void SelectSingleMethod_TypesNull_Ambiguous ()
    {
      var methods = new[] { CreateMethodStub ("Method1"), CreateMethodStub ("Method1") };
      var bindingFlags = (BindingFlags) 1;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (methods[0].Attributes, bindingFlags)).Return (true);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (methods[1].Attributes, bindingFlags)).Return (true);

      var binderStub = MockRepository.GenerateStub<Binder>();
      _selector.SelectSingleMethod (methods, binderStub, bindingFlags, "Method1", _someDeclaringType, parameterTypesOrNull: null, modifiersOrNull: null);
    }

    [Test]
    public void SelectSingleMethod_TypesNull_SingleCandidate ()
    {
      var methods = new[] { CreateMethodStub ("Method1") };
      var bindingFlags = (BindingFlags) 1;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (methods[0].Attributes, bindingFlags)).Return (true);

      var binderMock = MockRepository.GenerateStrictMock<Binder>();
      var result = _selector.SelectSingleMethod (methods, binderMock, bindingFlags, "Method1", _someDeclaringType, parameterTypesOrNull: null, modifiersOrNull: null);

      binderMock.AssertWasNotCalled (
          mock => mock.SelectMethod (
              Arg<BindingFlags>.Is.Anything, Arg<MethodBase[]>.Is.Anything, Arg<Type[]>.Is.Anything, Arg<ParameterModifier[]>.Is.Anything));
      Assert.That (result, Is.SameAs (methods[0]));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Modifiers must not be specified if parameter types are null.\r\nParameter name: modifiers")]
    public void SelectSingleMethod_TypesNull_ModifiersNotNull ()
    {
      var methods = new[] { CreateMethodStub() };
      var bindingFlags = (BindingFlags) 1;
      var modifiersOrNull = new[] { new ParameterModifier (2) };

      var binderStub = MockRepository.GenerateStub<Binder>();
      _selector.SelectSingleMethod (methods, binderStub, bindingFlags, "Whatever", _someDeclaringType, null, modifiersOrNull);
    }

    [Test]
    public void SelectSingleProperty ()
    {
      var properties =
          new[]
          {
              CreatePropertyStub ("Property1", accessors: new[] { CreateMethodStub() }),
              CreatePropertyStub ("Property2", accessors: new[] { CreateMethodStub (attributes: MethodAttributes.Assembly) }),
              CreatePropertyStub ("Property2", accessors: new[] { CreateMethodStub (attributes: MethodAttributes.Public) })
          };
      var bindingFlags = (BindingFlags) 1;
      var propertyType = ReflectionObjectMother.GetSomeType();
      var indexerTypes = new[] { ReflectionObjectMother.GetSomeOtherType() };
      var modifiers = new[] { new ParameterModifier (2) };

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (MethodAttributes.Assembly, bindingFlags)).Return (true);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (MethodAttributes.Public, bindingFlags)).Return (true);

      var binderMock = MockRepository.GenerateStrictMock<Binder>();
      var fakeResult = ReflectionObjectMother.GetSomeProperty();
      binderMock.Expect (mock => mock.SelectProperty (bindingFlags, new[] { properties[1], properties[2] }, propertyType, indexerTypes, modifiers))
                .Return (fakeResult);

      var result = _selector.SelectSingleProperty (
          properties, binderMock, bindingFlags, "Property2", _someDeclaringType, propertyType, indexerTypes, modifiers);

      _bindingFlagsEvaluatorMock.VerifyAllExpectations();
      binderMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void SelectSingleProperty_NoMatching ()
    {
      var properties = new[] { CreatePropertyStub ("Property2"), CreatePropertyStub ("Property1") };
      var propertyType = ReflectionObjectMother.GetSomeType();
      var indexerTypes = new[] { typeof (int), typeof (string) };
      var modifiers = new[] { new ParameterModifier (2) };
      var bindingFlags = (BindingFlags) 1;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (Arg<MethodAttributes>.Is.Anything, Arg.Is (bindingFlags))).Return (false);

      var binderMock = MockRepository.GenerateStrictMock<Binder>();
      var result = _selector.SelectSingleProperty (
          properties, binderMock, bindingFlags, "Property1", _someDeclaringType, propertyType, indexerTypes, modifiers);

      Assert.That (result, Is.Null);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Modifiers must not be specified if parameter types are null.\r\nParameter name: modifiers")]
    public void SelectSingleProperty_TypesNull_ModifiersNotNull ()
    {
      var properties = new[] { CreatePropertyStub() };
      var bindingFlags = (BindingFlags) 1;
      var propertyType = ReflectionObjectMother.GetSomeType();
      var modifiersOrNull = new[] { new ParameterModifier (2) };

      var binderStub = MockRepository.GenerateStub<Binder> ();
      _selector.SelectSingleProperty (properties, binderStub, bindingFlags, "Whatever", _someDeclaringType, propertyType, null, modifiersOrNull);
    }

    [Test]
    public void SelectSingleEvent ()
    {
      var events = new[]
                   {
                       CreateEventStub ("Event", adder: CreateMethodStub (attributes: MethodAttributes.Assembly)),
                       CreateEventStub ("this event is removed because of its name"),
                       CreateEventStub ("Event", adder: CreateMethodStub (attributes: MethodAttributes.Public))
                   };
      var bindingFlags = (BindingFlags) 1;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (MethodAttributes.Assembly, bindingFlags)).Return (false);
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (MethodAttributes.Public, bindingFlags)).Return (true);

      var result = _selector.SelectSingleEvent (events, bindingFlags, "Event", _someDeclaringType);

      _bindingFlagsEvaluatorMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (events[2]));
    }

    [Test]
    public void SelectSingleEvent_NoMatching ()
    {
      var events = new[] { CreateEventStub ("Event"), CreateEventStub ("wrong name") };
      var bindingFlags = (BindingFlags) 1;

      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (events[0].GetAddMethod (true).Attributes, bindingFlags)).Return (false);

      var result = _selector.SelectSingleEvent (events, bindingFlags, "Event", _someDeclaringType);

      Assert.That (result, Is.Null);
    }

    [Test]
    [ExpectedException (typeof (AmbiguousMatchException), ExpectedMessage = "Ambiguous event name 'Event'.")]
    public void SelectSingleEvent_Ambiguous ()
    {
      var events = new[] { CreateEventStub ("Event"), CreateEventStub ("Event") };
      var bindingFlags = (BindingFlags) 1;

      _bindingFlagsEvaluatorMock
          .Expect (mock => mock.HasRightAttributes (Arg<MethodAttributes>.Is.Anything, Arg.Is(bindingFlags)))
          .Return (true)
          .Repeat.Twice();

      _selector.SelectSingleEvent (events, bindingFlags, "Event", _someDeclaringType);
    }

    private Type CreateTypeStub (string name = "Unspecified", TypeAttributes attributes = TypeAttributes.Public)
    {
      var typeStub = MockRepository.GenerateStub<Type>();
      typeStub.Stub (stub => stub.Name).Return (name);
      typeStub.Stub (stub => stub.Attributes).Return (attributes);

      return typeStub;
    }

    private FieldInfo CreateFieldStub (
        string name = "Unspecified", FieldAttributes attributes = FieldAttributes.PrivateScope, Type declaringType = null)
    {
      var fieldStub = MockRepository.GenerateStub<FieldInfo>();
      fieldStub.Stub (stub => stub.Name).Return (name);
      fieldStub.Stub (stub => stub.Attributes).Return (attributes);
      fieldStub.Stub (stub => stub.DeclaringType).Return (declaringType);

      return fieldStub;
    }

    private ConstructorInfo CreateConstructorStub (MethodAttributes attributes = MethodAttributes.PrivateScope)
    {
      var constructorStub = MockRepository.GenerateStub<ConstructorInfo>();
      constructorStub.Stub (stub => stub.Name).Repeat.Never();
      constructorStub.Stub (stub => stub.Attributes).Return (attributes);
      return constructorStub;
    }

    private MethodInfo CreateMethodStub (
        string name = "Unspecified", MethodAttributes attributes = MethodAttributes.PrivateScope, Type declaringType = null)
    {
      var methodStub = MockRepository.GenerateStub<MethodInfo>();
      methodStub.Stub (stub => stub.Name).Return (name);
      methodStub.Stub (stub => stub.Attributes).Return (attributes);
      methodStub.Stub (stub => stub.DeclaringType).Return (declaringType);
      return methodStub;
    }

    private PropertyInfo CreatePropertyStub (string name = "Unspecified", Type declaringType = null, MethodInfo[] accessors = null)
    {
      var propertyStub = MockRepository.GenerateStub<PropertyInfo>();
      propertyStub.Stub (stub => stub.Name).Return (name);
      propertyStub.Stub (stub => stub.DeclaringType).Return (declaringType);
      propertyStub.Stub (stub => stub.GetAccessors (true)).Return (accessors ?? new[] { CreateMethodStub() });

      return propertyStub;
    }

    private EventInfo CreateEventStub (string name = "Unspecified", Type declaringType = null, MethodInfo adder = null)
    {
      var eventStub = MockRepository.GenerateStub<EventInfo>();
      eventStub.Stub (stub => stub.Name).Return (name);
      eventStub.Stub (stub => stub.DeclaringType).Return (declaringType);
      eventStub.Stub (stub => stub.GetAddMethod (true)).Return (adder ?? CreateMethodStub());

      return eventStub;
    }
  }
}