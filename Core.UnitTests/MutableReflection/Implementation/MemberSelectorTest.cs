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
using System.Reflection.Emit;
using System.Threading;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Moq;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class MemberSelectorTest
  {
    private Mock<IBindingFlagsEvaluator> _bindingFlagsEvaluatorMock;
    
    private MemberSelector _selector;

    private Type _someDeclaringType;

    [SetUp]
    public void SetUp ()
    {
      _bindingFlagsEvaluatorMock = new Mock<IBindingFlagsEvaluator> (MockBehavior.Strict);

      _selector = new MemberSelector (_bindingFlagsEvaluatorMock.Object);

      _someDeclaringType = ReflectionObjectMother.GetSomeType();
    }

    [Test]
    public void SelectTypes ()
    {
      var candidates = new[]
                       {
                           CreateTypeStub (attributes: TypeAttributes.Public),
                           CreateTypeStub (attributes: TypeAttributes.NotPublic),
                           CreateTypeStub (attributes: TypeAttributes.Sealed)
                       };
      var bindingFlags = (BindingFlags) 1;

      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (candidates[0].Attributes, bindingFlags)).Returns (true).Verifiable();
      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (candidates[1].Attributes, bindingFlags)).Returns (false).Verifiable();
      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (candidates[2].Attributes, bindingFlags)).Returns (true).Verifiable();

      var result = _selector.SelectTypes (candidates, bindingFlags).ForceEnumeration();

      _bindingFlagsEvaluatorMock.Verify();
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

      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (candidates[0].Attributes, bindingFlags)).Returns (true).Verifiable();
      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (candidates[1].Attributes, bindingFlags)).Returns (false).Verifiable();
      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (candidates[2].Attributes, bindingFlags)).Returns (true).Verifiable();

      var result = _selector.SelectFields (candidates, bindingFlags, _someDeclaringType).ForceEnumeration();

      _bindingFlagsEvaluatorMock.Verify();
      Assert.That (result, Is.EqualTo (new[] { candidates[0], candidates[2] }));
    }

    [Test]
    public void SelectFields_DeclaredOnly ()
    {
      var declaringType1 = typeof (string);
      var declaringType2 = typeof (int);
      var candidates = new[] { CreateFieldStub (declaringType: declaringType1), CreateFieldStub (declaringType: declaringType2) };
      var bindingFlags = (BindingFlags) 1 | BindingFlags.DeclaredOnly;

      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (candidates[1].Attributes, bindingFlags)).Returns (true).Verifiable();

      var result = _selector.SelectFields (candidates, bindingFlags, declaringType2).ForceEnumeration();

      _bindingFlagsEvaluatorMock.Verify();
      Assert.That (result, Is.EqualTo (new[] { candidates[1] }));
    }

    [Test]
    public void SelectMethods ()
    {
      const MethodAttributes wantedAttribute = (MethodAttributes) 1;
      const MethodAttributes unwantedAttribute = (MethodAttributes) 2;
      var candidates = new[]
                       {
                           CreateMethodStub (attributes: wantedAttribute),
                           CreateMethodStub (attributes: unwantedAttribute),
                           CreateMethodStub (attributes: wantedAttribute)
                       };
      var bindingFlags = (BindingFlags) 1;
      var declaringType = ReflectionObjectMother.GetSomeType();

      var sequence = new MockSequence();
      _bindingFlagsEvaluatorMock.InSequence (sequence).Setup (mock => mock.HasRightAttributes (wantedAttribute, bindingFlags)).Returns (true);
      _bindingFlagsEvaluatorMock.InSequence (sequence).Setup (mock => mock.HasRightAttributes (unwantedAttribute, bindingFlags)).Returns (false);
      _bindingFlagsEvaluatorMock.InSequence (sequence).Setup (mock => mock.HasRightAttributes (wantedAttribute, bindingFlags)).Returns (true);

      var result = _selector.SelectMethods (candidates, bindingFlags, declaringType).ForceEnumeration();

      _bindingFlagsEvaluatorMock.Verify();
      Assert.That (result, Is.EqualTo (new[] { candidates[0], candidates[2] }));
    }

    [Test]
    public void SelectMethods_DeclaredOnly ()
    {
      var declaringType1 = typeof (string);
      var declaringType2 = typeof (int);
      var candidates = new[] { CreateMethodStub (declaringType: declaringType1), CreateMethodStub (declaringType: declaringType2) };
      var bindingFlags = (BindingFlags) 1 | BindingFlags.DeclaredOnly;

      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (candidates[1].Attributes, bindingFlags)).Returns (true).Verifiable();

      var result = _selector.SelectMethods (candidates, bindingFlags, declaringType2).ForceEnumeration();

      _bindingFlagsEvaluatorMock.Verify();
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

      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (MethodAttributes.Final, bindingFlags)).Returns (false).Verifiable();
      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (((MethodAttributes) 1), bindingFlags)).Returns (false).Verifiable();
      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (((MethodAttributes) 2), bindingFlags)).Returns (true).Verifiable();
      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes ((MethodAttributes) 3, bindingFlags)).Returns (true).Verifiable();

      var result = _selector.SelectProperties (candidates, bindingFlags, _someDeclaringType).ForceEnumeration();

      _bindingFlagsEvaluatorMock.Verify();
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

      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (consideredAccessor.Attributes, bindingFlags)).Returns (true).Verifiable();

      var result = _selector.SelectProperties (candidates, bindingFlags, declaringType2).ForceEnumeration ();

      _bindingFlagsEvaluatorMock.Verify();
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

      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (MethodAttributes.Assembly, bindingFlags)).Returns (true).Verifiable();
      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (MethodAttributes.Family, bindingFlags)).Returns (false).Verifiable();
      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (MethodAttributes.FamORAssem, bindingFlags)).Returns (true).Verifiable();

      var result = _selector.SelectEvents (candidates, bindingFlags, _someDeclaringType).ForceEnumeration();

      _bindingFlagsEvaluatorMock.Verify();
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

      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (consideredAdder.Attributes, bindingFlags)).Returns (true).Verifiable();

      var result = _selector.SelectEvents (candidates, bindingFlags, declaringType2).ForceEnumeration();

      _bindingFlagsEvaluatorMock.Verify();
      Assert.That (result, Is.EqualTo (new[] { candidates[1] }));
    }

    [Test]
    public void SelectSingleType ()
    {
      var types = new[]
                  {
                      CreateTypeStub ("type1", TypeAttributes.Class),
                      CreateTypeStub ("this type is removed because of its name", TypeAttributes.NotPublic),
                      CreateTypeStub ("type1", TypeAttributes.Sealed)
                  };
      var bindingFlags = (BindingFlags)1;

      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes(types[0].Attributes, bindingFlags)).Returns(false).Verifiable();
      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes(types[2].Attributes, bindingFlags)).Returns(true).Verifiable();

      var result = _selector.SelectSingleType (types, bindingFlags, "type1");

      _bindingFlagsEvaluatorMock.Verify();
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

      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (fields[0].Attributes, bindingFlags)).Returns (false).Verifiable();
      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (fields[2].Attributes, bindingFlags)).Returns (true).Verifiable();

      var result = _selector.SelectSingleField (fields, bindingFlags, "field1", _someDeclaringType);

      _bindingFlagsEvaluatorMock.Verify();
      Assert.That (result, Is.SameAs (fields[2]));
    }

    [Test]
    public void SelectSingleField_NoMatching ()
    {
      var fields = new[] { CreateFieldStub ("field1"), CreateFieldStub ("wrong name") };
      var bindingFlags = (BindingFlags) 1;

      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (fields[0].Attributes, bindingFlags)).Returns (false).Verifiable();

      var result = _selector.SelectSingleField (fields, bindingFlags, "field1", _someDeclaringType);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void SelectSingleField_Ambiguous ()
    {
      var fields = new[] { CreateFieldStub ("field1", FieldAttributes.Assembly), CreateFieldStub ("field1", FieldAttributes.Public) };
      var bindingFlags = (BindingFlags) 1;

      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (fields[0].Attributes, bindingFlags)).Returns (true).Verifiable();
      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (fields[1].Attributes, bindingFlags)).Returns (true).Verifiable();
      Assert.That (
          () => _selector.SelectSingleField (fields, bindingFlags, "field1", _someDeclaringType),
          Throws.InstanceOf<AmbiguousMatchException>()
              .With.Message.EqualTo ("Ambiguous field name 'field1'."));
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

      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (methods[0].Attributes, bindingFlags)).Returns (false).Verifiable();
      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (methods[2].Attributes, bindingFlags)).Returns (true).Verifiable();

      var binderMock = new Mock<Binder> (MockBehavior.Strict);
      var fakeResult = ReflectionObjectMother.GetSomeMethod();
      binderMock
          .Setup (mock => mock.SelectMethod (bindingFlags, It.Is<MethodBase[]> (param => param.SequenceEqual (new MethodBase[] { methods[2] })), typesOrNull, modifiersOrNull))
          .Returns (fakeResult)
          .Verifiable();

      var result = _selector.SelectSingleMethod (methods, binderMock.Object, bindingFlags, "Method1", _someDeclaringType, typesOrNull, modifiersOrNull);

      _bindingFlagsEvaluatorMock.Verify();
      binderMock.Verify();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void SelectSingleMethod_ForConstructors_NameIsNotConsidered ()
    {
      var constructors = new[] { CreateConstructorStub (MethodAttributes.Assembly), CreateConstructorStub (MethodAttributes.Family) };
      var bindingFlags = (BindingFlags) 1;
      var typesOrNull = new[] { typeof (int), typeof (string) };
      var modifiersOrNull = new[] { new ParameterModifier (2) };

      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (constructors[0].Attributes, bindingFlags)).Returns (false).Verifiable();
      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (constructors[1].Attributes, bindingFlags)).Returns (true).Verifiable();

      var binderMock = new Mock<Binder> (MockBehavior.Strict);
      var fakeResult = ReflectionObjectMother.GetSomeConstructor();
      binderMock
          .Setup (mock => mock.SelectMethod (bindingFlags, It.Is<MethodBase[]> (param => param.SequenceEqual (new MethodBase[] { constructors[1] })), typesOrNull, modifiersOrNull))
          .Returns (fakeResult)
          .Verifiable();

      var result = _selector.SelectSingleMethod (constructors, binderMock.Object, bindingFlags, null, _someDeclaringType, typesOrNull, modifiersOrNull);

      _bindingFlagsEvaluatorMock.Verify();
      binderMock.Verify();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void SelectSingleMethod_NoMatching ()
    {
      var methods = new[] { CreateMethodStub ("Method1"), CreateMethodStub ("wrong name") };
      var binderStub = new Mock<Binder>();
      var bindingFlags = (BindingFlags) 1;
      var typesOrNull = new[] { typeof (int), typeof (string) };
      var modifiersOrNull = new[] { new ParameterModifier (2) };

      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (methods[0].Attributes, bindingFlags)).Returns (false).Verifiable();

      var result = _selector.SelectSingleMethod (methods, binderStub.Object, bindingFlags, "Method1", _someDeclaringType, typesOrNull, modifiersOrNull);

      _bindingFlagsEvaluatorMock.Verify();
      Assert.That (result, Is.Null);
    }

    [Test]
    public void SelectSingleMethod_TypesNull_Ambiguous ()
    {
      var methods = new[] { CreateMethodStub ("Method1"), CreateMethodStub ("Method1") };
      var bindingFlags = (BindingFlags) 1;

      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (methods[0].Attributes, bindingFlags)).Returns (true).Verifiable();
      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (methods[1].Attributes, bindingFlags)).Returns (true).Verifiable();

      var binderStub = new Mock<Binder>();
      Assert.That (
          () => _selector.SelectSingleMethod (methods, binderStub.Object, bindingFlags, "Method1", _someDeclaringType, parameterTypesOrNull: null, modifiersOrNull: null),
          Throws.InstanceOf<AmbiguousMatchException>()
              .With.Message.EqualTo ("Ambiguous method name 'Method1'."));
    }

    [Test]
    public void SelectSingleMethod_TypesNull_SingleCandidate ()
    {
      var methods = new[] { CreateMethodStub ("Method1") };
      var bindingFlags = (BindingFlags) 1;

      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (methods[0].Attributes, bindingFlags)).Returns (true).Verifiable();

      var binderMock = new Mock<Binder> (MockBehavior.Strict);
      var result = _selector.SelectSingleMethod (methods, binderMock.Object, bindingFlags, "Method1", _someDeclaringType, parameterTypesOrNull: null, modifiersOrNull: null);

      binderMock.Verify (
          mock => mock.SelectMethod (
              It.IsAny<BindingFlags>(),
              It.IsAny<MethodBase[]>(),
              It.IsAny<Type[]>(),
              It.IsAny<ParameterModifier[]>()),
          Times.Never());
      Assert.That (result, Is.SameAs (methods[0]));
    }

    [Test]
    public void SelectSingleMethod_TypesNull_ModifiersNotNull ()
    {
      var methods = new[] { CreateMethodStub() };
      var bindingFlags = (BindingFlags) 1;
      var modifiersOrNull = new[] { new ParameterModifier (2) };

      var binderStub = new Mock<Binder>();
      Assert.That (
          () => _selector.SelectSingleMethod (methods, binderStub.Object, bindingFlags, "Whatever", _someDeclaringType, null, modifiersOrNull),
          Throws.ArgumentException
              .With.Message.EqualTo ("Modifiers must not be specified if parameter types are null.\r\nParameter name: modifiers"));
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

      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (MethodAttributes.Assembly, bindingFlags)).Returns (true).Verifiable();
      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (MethodAttributes.Public, bindingFlags)).Returns (true).Verifiable();

      var binderMock = new Mock<Binder> (MockBehavior.Strict);
      var fakeResult = ReflectionObjectMother.GetSomeProperty();
      binderMock
          .Setup (
              mock => mock.SelectProperty (
                  bindingFlags,
                  It.Is<PropertyInfo[]> (param => param.SequenceEqual (new[] { properties[1], properties[2] })),
                  propertyType,
                  indexerTypes,
                  modifiers))
          .Returns (fakeResult)
          .Verifiable();

      var result = _selector.SelectSingleProperty (
          properties, binderMock.Object, bindingFlags, "Property2", _someDeclaringType, propertyType, indexerTypes, modifiers);

      _bindingFlagsEvaluatorMock.Verify();
      binderMock.Verify();
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

      _bindingFlagsEvaluatorMock
          .Setup (mock => mock.HasRightAttributes (It.IsAny<MethodAttributes>(), bindingFlags))
          .Returns (false)
          .Verifiable();

      var binderMock = new Mock<Binder> (MockBehavior.Strict);
      var result = _selector.SelectSingleProperty (
          properties, binderMock.Object, bindingFlags, "Property1", _someDeclaringType, propertyType, indexerTypes, modifiers);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void SelectSingleProperty_TypesNull_ModifiersNotNull ()
    {
      var properties = new[] { CreatePropertyStub() };
      var bindingFlags = (BindingFlags) 1;
      var propertyType = ReflectionObjectMother.GetSomeType();
      var modifiersOrNull = new[] { new ParameterModifier (2) };

      var binderStub = new Mock<Binder>();
      Assert.That (
          () => _selector.SelectSingleProperty (properties, binderStub.Object, bindingFlags, "Whatever", _someDeclaringType, propertyType, null, modifiersOrNull),
          Throws.ArgumentException
              .With.Message.EqualTo ("Modifiers must not be specified if parameter types are null.\r\nParameter name: modifiers"));
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

      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (MethodAttributes.Assembly, bindingFlags)).Returns (false).Verifiable();
      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (MethodAttributes.Public, bindingFlags)).Returns (true).Verifiable();

      var result = _selector.SelectSingleEvent (events, bindingFlags, "Event", _someDeclaringType);

      _bindingFlagsEvaluatorMock.Verify();
      Assert.That (result, Is.SameAs (events[2]));
    }

    [Test]
    public void SelectSingleEvent_NoMatching ()
    {
      var events = new[] { CreateEventStub ("Event"), CreateEventStub ("wrong name") };
      var bindingFlags = (BindingFlags) 1;

      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (events[0].GetAddMethod (true).Attributes, bindingFlags)).Returns (false).Verifiable();

      var result = _selector.SelectSingleEvent (events, bindingFlags, "Event", _someDeclaringType);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void SelectSingleEvent_Ambiguous ()
    {
      var events = new[] { CreateEventStub ("Event"), CreateEventStub ("Event") };
      var bindingFlags = (BindingFlags) 1;

      _bindingFlagsEvaluatorMock.Setup (mock => mock.HasRightAttributes (It.IsAny<MethodAttributes>(), bindingFlags)).Returns (true).Verifiable();

      Assert.That (
          () => _selector.SelectSingleEvent (events, bindingFlags, "Event", _someDeclaringType),
          Throws.InstanceOf<AmbiguousMatchException>()
              .With.Message.EqualTo ("Ambiguous event name 'Event'."));
      _bindingFlagsEvaluatorMock.Verify (mock => mock.HasRightAttributes (It.IsAny<MethodAttributes>(), bindingFlags), Times.Exactly (2));
    }

    private Type CreateTypeStub (string name = null, TypeAttributes attributes = TypeAttributes.Public)
    {
      name = name ?? Guid.NewGuid().ToString();
      var assemblyName = new AssemblyName { Name = name };
      var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly (assemblyName, AssemblyBuilderAccess.Run);
      var moduleBuilder = assemblyBuilder.DefineDynamicModule (assemblyBuilder.GetName().Name);
      var typeBuilder = moduleBuilder.DefineType (name, attributes);
      return typeBuilder.CreateType();
    }

    private FieldInfo CreateFieldStub (
        string name = "Unspecified", FieldAttributes attributes = FieldAttributes.PrivateScope, Type declaringType = null)
    {
      var fieldStub = new Mock<FieldInfo>();
      fieldStub.SetupGet (stub => stub.Name).Returns (name);
      fieldStub.SetupGet (stub => stub.Attributes).Returns (attributes);
      fieldStub.SetupGet (stub => stub.DeclaringType).Returns (declaringType);

      return fieldStub.Object;
    }

    private ConstructorInfo CreateConstructorStub (MethodAttributes attributes = MethodAttributes.PrivateScope)
    {
      var constructorStub = new Mock<ConstructorInfo>();

      // Necessary because of Moq bug https://github.com/moq/moq4/issues/802
      constructorStub.CallBase = true;

      constructorStub.SetupGet (stub => stub.Name).Throws (new AssertionException ("Do not access the Name property!"));
      constructorStub.SetupGet (stub => stub.Attributes).Returns (attributes);
      return constructorStub.Object;
    }

    private MethodInfo CreateMethodStub (
        string name = "Unspecified", MethodAttributes attributes = MethodAttributes.PrivateScope, Type declaringType = null)
    {
      var methodStub = new Mock<MethodInfo>();

      // Necessary because of Moq bug https://github.com/moq/moq4/issues/802
      methodStub.CallBase = true;

      methodStub.SetupGet (stub => stub.Name).Returns (name);
      methodStub.SetupGet (stub => stub.Attributes).Returns (attributes);
      methodStub.SetupGet (stub => stub.DeclaringType).Returns (declaringType);
      return methodStub.Object;
    }

    private PropertyInfo CreatePropertyStub (string name = "Unspecified", Type declaringType = null, MethodInfo[] accessors = null)
    {
      var propertyStub = new Mock<PropertyInfo>();

      // Necessary because of Moq bug https://github.com/moq/moq4/issues/802
      propertyStub.CallBase = true;

      propertyStub.SetupGet (stub => stub.Name).Returns (name);
      propertyStub.SetupGet (stub => stub.DeclaringType).Returns (declaringType);
      propertyStub.Setup (stub => stub.GetAccessors (true)).Returns (accessors ?? new[] { CreateMethodStub() });

      return propertyStub.Object;
    }

    private EventInfo CreateEventStub (string name = "Unspecified", Type declaringType = null, MethodInfo adder = null)
    {
      var eventStub = new Mock<EventInfo>();
      eventStub.SetupGet (stub => stub.Name).Returns (name);
      eventStub.SetupGet (stub => stub.DeclaringType).Returns (declaringType);
      eventStub.Setup (stub => stub.GetAddMethod (true)).Returns (adder ?? CreateMethodStub());

      return eventStub.Object;
    }
  }
}