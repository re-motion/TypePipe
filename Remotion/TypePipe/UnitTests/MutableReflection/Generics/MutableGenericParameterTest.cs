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
using JetBrains.Annotations;
using NUnit.Framework;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection.Generics;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection.Implementation;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.MutableReflection.Implementation;
using Rhino.Mocks;
using Remotion.Development.UnitTesting.Enumerables;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  [TestFixture]
  public class MutableGenericParameterTest
  {
    private const BindingFlags c_allMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    private int _position;
    private string _name;
    private string _namespace;
    private GenericParameterAttributes _genericParameterAttributes;
    private Type _baseTypeConstraint;
    private Type _interfaceConstraint;

    private MutableGenericParameter _parameter;
    private MutableGenericParameter _constrainedParameter;

    [SetUp]
    public void SetUp ()
    {
      var memberSelectorMock = MockRepository.GenerateStrictMock<IMemberSelector>();
      _position = 7;
      _name = "_parameter";
      _namespace = "namespace";
      _genericParameterAttributes = (GenericParameterAttributes) 7;

      _parameter = new MutableGenericParameter (memberSelectorMock, _position, _name, _namespace, _genericParameterAttributes);

      _baseTypeConstraint = typeof (DomainType);
      _interfaceConstraint = ReflectionObjectMother.GetSomeInterfaceType();

      _constrainedParameter = MutableGenericParameterObjectMother.Create (constraints: new[] { _baseTypeConstraint, _interfaceConstraint }.AsOneTime());
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_parameter.DeclaringType, Is.Null);
      Assert.That (_parameter.DeclaringMethod, Is.Null);
      Assert.That (_parameter.GenericParameterPosition, Is.EqualTo (_position));
      Assert.That (_parameter.Name, Is.EqualTo (_name));
      Assert.That (_parameter.Namespace, Is.EqualTo (_namespace));
      Assert.That (_parameter.Attributes, Is.EqualTo (TypeAttributes.AutoLayout | TypeAttributes.AnsiClass | TypeAttributes.Class | TypeAttributes.Public));
      Assert.That (_parameter.IsGenericType, Is.False);
      Assert.That (_parameter.GenericParameterAttributes, Is.EqualTo (_genericParameterAttributes));
      Assert.That (_parameter.BaseType, Is.SameAs (typeof (object)));
      Assert.That (_parameter.GetInterfaces(), Is.Empty);
    }

    [Test]
    public void Initialization_ValueType ()
    {
      var parameter = MutableGenericParameterObjectMother.Create (genericParameterAttributes: GenericParameterAttributes.NotNullableValueTypeConstraint);
      Assert.That (parameter.BaseType, Is.SameAs (typeof (ValueType)));
    }

    [Test]
    public void FullName ()
    {
      Assert.That (_parameter.FullName, Is.Null);
    }

    [Test]
    public void IsGenericParameter ()
    {
      Assert.That (_parameter.IsGenericParameter, Is.True);
    }

    [Test]
    public void InitializeDeclaringMember_DeclaringType ()
    {
      var declaringMember = MutableTypeObjectMother.Create();
      _parameter.InitializeDeclaringMember (declaringMember);

      Assert.That (_parameter.DeclaringType, Is.SameAs (declaringMember));
      Assert.That (_parameter.DeclaringMethod, Is.Null);
    }

    [Test]
    public void InitializeDeclaringMember_MethodBase ()
    {
      var declaringMember = MutableMethodInfoObjectMother.Create();
      _parameter.InitializeDeclaringMember (declaringMember);

      Assert.That (_parameter.DeclaringType, Is.SameAs (declaringMember.DeclaringType));
      Assert.That (_parameter.DeclaringMethod, Is.SameAs (declaringMember));
    }

    [Test]
    public void InitializeDeclaringMember_ThrowsIfAlreadyInitialized ()
    {
      var declaringMember = MutableMethodInfoObjectMother.Create();
      _parameter.InitializeDeclaringMember (declaringMember);

      Assert.That (
          () => _parameter.InitializeDeclaringMember (declaringMember),
          Throws.InvalidOperationException.With.Message.EqualTo ("InitializeDeclaringMember must be called exactly once."));
    }

    [Test]
    public void SetGenericParameterConstraints_BaseAndInterfaceConstraints ()
    {
      Assert.That (_parameter.BaseType, Is.SameAs (typeof (object)));
      Assert.That (_parameter.GetInterfaces(), Is.Empty);
      Assert.That (_parameter.GetGenericParameterConstraints(), Is.Empty);

      _parameter.SetGenericParameterConstraints (new[] { _baseTypeConstraint, _interfaceConstraint }.AsOneTime());

      Assert.That (_parameter.BaseType, Is.SameAs (_baseTypeConstraint));
      Assert.That (_parameter.GetInterfaces(), Is.EqualTo (new[] { _interfaceConstraint, typeof (IDomainInterface) }));
      Assert.That (_parameter.GetGenericParameterConstraints(), Is.EqualTo (new[] { _baseTypeConstraint, _interfaceConstraint }));
    }

    [Test]
    public void SetGenericParameterConstraints_NoBaseConstraint ()
    {
      _parameter.SetGenericParameterConstraints (Type.EmptyTypes);

      Assert.That (_parameter.BaseType, Is.SameAs (typeof (object)));
    }

    [Test]
    public void SetGenericParameterConstraints_GenericParameter ()
    {
      var genericParameter = ReflectionObjectMother.GetSomeGenericParameter();
      Assert.That (genericParameter.IsClass, Is.True);

      _parameter.SetGenericParameterConstraints (new[] { genericParameter });

      Assert.That (_parameter.BaseType, Is.SameAs (typeof (object)));
    }

    [Test]
    public void SetGenericParameterConstraints_ValueTypeBaseConstraint ()
    {
      var message = "A generic parameter cannot be constrained by a value type.\r\nParameter name: constraints";
      Assert.That (
          () => _parameter.SetGenericParameterConstraints (new[] { ReflectionObjectMother.GetSomeValueType() }),
          Throws.ArgumentException.With.Message.EqualTo (message));
      Assert.That (
          () => _parameter.SetGenericParameterConstraints (new[] { typeof (ValueType) }),
          Throws.ArgumentException.With.Message.EqualTo (message));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "A generic parameter cannot have a base constraint if the NotNullableValueTypeConstraint flag is set.\r\nParameter name: constraints")]
    public void SetGenericParameterConstraints_BaseConstraintConflicts_WithNotNullableValueTypeConstraint ()
    {
      var parameter = MutableGenericParameterObjectMother.Create (genericParameterAttributes: GenericParameterAttributes.NotNullableValueTypeConstraint);
      parameter.SetGenericParameterConstraints (new[] { ReflectionObjectMother.GetSomeSubclassableType() });
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "A generic parameter cannot have multiple base constraints.\r\nParameter name: constraints")]
    public void SetGenericParameterConstraints_MoreThanOneBaseConstraint ()
    {
      var baseConstraint1 = ReflectionObjectMother.GetSomeSubclassableType();
      var baseConstraint2 = ReflectionObjectMother.GetSomeSubclassableType();

      _parameter.SetGenericParameterConstraints (new[] { baseConstraint1, baseConstraint2 });
    }

    [Test]
    public void CustomAttributeMethods ()
    {
      var declaration = CustomAttributeDeclarationObjectMother.Create (typeof (ObsoleteAttribute));
      _parameter.AddCustomAttribute (declaration);

      Assert.That (_parameter.AddedCustomAttributes, Is.EqualTo (new[] { declaration }));
      Assert.That (_parameter.GetCustomAttributeData().Select (a => a.Type), Is.EquivalentTo (new[] { typeof (ObsoleteAttribute) }));
    }

    [Test]
    public void GetAllInterfaces ()
    {
      Assert.That (_constrainedParameter.GetGenericParameterConstraints(), Contains.Item (_baseTypeConstraint));

      Assert.That (_constrainedParameter.GetAllInterfaces(), Is.EqualTo (new[] { _interfaceConstraint, typeof (IDomainInterface) }));
    }

    [Test]
    public void GetAllInterfaces_Distinct ()
    {
      Assert.That (_baseTypeConstraint.GetInterfaces(), Contains.Item (typeof (IDomainInterface)));
      _parameter.SetGenericParameterConstraints (new[] { _baseTypeConstraint, typeof (IDomainInterface) });

      Assert.That (_parameter.GetAllInterfaces(), Is.EqualTo (new[] { typeof (IDomainInterface) }));
    }

    [Test]
    public void GetAllFields ()
    {
      var result = _constrainedParameter.GetAllFields();

      var field = NormalizingMemberInfoFromExpressionUtility.GetField ((DomainType obj) => obj.Field);
      Assert.That (result, Has.Member (field));
      Assert.That (result, Is.EquivalentTo (typeof (DomainType).GetFields (c_allMembers)));
    }

    [Test]
    public void GetAllConstructors ()
    {
      Assert.That (typeof (DomainType).GetConstructors(), Is.Not.Empty);

      Assert.That (_constrainedParameter.GetAllConstructors(), Is.Empty);
    }

    [Test]
    public void GetAllConstructors_NewConstraint ()
    {
      var parameter = MutableGenericParameterObjectMother.Create (genericParameterAttributes: GenericParameterAttributes.DefaultConstructorConstraint);

      var result = parameter.GetConstructors (c_allMembers).Single();

      Assert.That (result, Is.TypeOf<GenericParameterDefaultConstructor>());
      Assert.That (result.DeclaringType, Is.SameAs(parameter));
    }

    [Test]
    public void GetAllMethods ()
    {
      var result = _constrainedParameter.GetAllMethods();

      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method());
      Assert.That (result, Has.Member (method));
      Assert.That (result, Is.EquivalentTo (typeof (DomainType).GetMethods (c_allMembers)));
    }

    [Test]
    public void GetAllProperties ()
    {
      var result = _constrainedParameter.GetAllProperties();

      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DomainType obj) => obj.Property);
      Assert.That (result, Is.EqualTo (new[] { property }));
    }

    [Test]
    public void GetAllEvents ()
    {
      var result = _constrainedParameter.GetAllEvents();

      var event_ = typeof (DomainType).GetEvent ("Event");
      Assert.That (result, Is.EqualTo (new[] { event_ }));
    }

    [Test]
    public void GetAllXXX_UsesAllBindingFlagsToRetrieveMembers ()
    {
      var fields = _baseTypeConstraint.GetFields (c_allMembers);
      var methods = _baseTypeConstraint.GetMethods (c_allMembers);
      var properties = _baseTypeConstraint.GetProperties (c_allMembers);
      var events = _baseTypeConstraint.GetEvents (c_allMembers);

      var baseMemberSelectorMock = MockRepository.GenerateStrictMock<IMemberSelector>();
      var baseTypeConstraint = CustomTypeObjectMother.Create (
          baseMemberSelectorMock, fields: fields, methods: methods, properties: properties, events: events);

      baseMemberSelectorMock.Expect (mock => mock.SelectFields (fields, c_allMembers, baseTypeConstraint)).Return (fields);
      // Note: GetMethods is optimized for retrieving all the methods; so there is no memberSelectorMock call.
      baseMemberSelectorMock.Expect (mock => mock.SelectProperties (properties, c_allMembers, baseTypeConstraint)).Return (properties);
      baseMemberSelectorMock.Expect (mock => mock.SelectEvents (events, c_allMembers, baseTypeConstraint)).Return (events);

      var parameter = MutableGenericParameterObjectMother.Create (constraints: new[] { baseTypeConstraint, _interfaceConstraint });

      Assert.That (parameter.GetAllFields(), Is.EqualTo (fields));
      Assert.That (parameter.GetAllMethods(), Is.EqualTo (methods));
      Assert.That (parameter.GetAllProperties(), Is.EqualTo (properties));
      Assert.That (parameter.GetAllEvents(), Is.EqualTo (events));

      baseMemberSelectorMock.AssertWasNotCalled (
          mock => mock.SelectMethods (Arg<IEnumerable<ConstructorInfo>>.Is.Anything, Arg<BindingFlags>.Is.Anything, Arg<Type>.Is.Anything));
      baseMemberSelectorMock.VerifyAllExpectations();
    }

    class DomainType : IDomainInterface
    {
      public int Field;
      public void Method () { }
      [UsedImplicitly] public int Property { get; set; }
      [UsedImplicitly] public event EventHandler Event;
    }
    interface IDomainInterface { }
  }
}