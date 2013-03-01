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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.UnitTests.MutableReflection.Implementation;
using Rhino.Mocks;
using Remotion.Development.UnitTesting;
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
      _genericParameterAttributes = (GenericParameterAttributes) 8;

      _parameter = new MutableGenericParameter (memberSelectorMock, _position, _name, _namespace, _genericParameterAttributes);

      _baseTypeConstraint = typeof (DomainType);
      _interfaceConstraint = ReflectionObjectMother.GetSomeInterfaceType();

      _constrainedParameter = GenericParameterObjectMother.Create (
          baseTypeConstraint: _baseTypeConstraint, interfaceConstraints: new[] { _interfaceConstraint }.AsOneTime());
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_parameter.DeclaringType, Is.Null);
      Assert.That (_parameter.DeclaringMethod, Is.Null);
      Assert.That (_parameter.GenericParameterPosition, Is.EqualTo (_position));
      Assert.That (_parameter.Name, Is.EqualTo (_name));
      Assert.That (_parameter.Namespace, Is.EqualTo (_namespace));
      Assert.That (_parameter.FullName, Is.Null);
      Assert.That (_parameter.Attributes, Is.EqualTo (TypeAttributes.AutoLayout | TypeAttributes.AnsiClass | TypeAttributes.Class | TypeAttributes.Public));
      Assert.That (_parameter.IsGenericType, Is.False);
      Assert.That (_parameter.GenericParameterAttributes, Is.EqualTo (_genericParameterAttributes));
      Assert.That (_parameter.BaseType, Is.SameAs (typeof (object)));
      Assert.That (_parameter.GetInterfaces(), Is.Empty);
    }

    [Test]
    public void IsGenericParameter ()
    {
      Assert.That (_parameter.IsGenericParameter, Is.True);
    }

    [Test]
    public void InitializeDeclaringMember_DeclaringType ()
    {
      var declaringMember = ProxyTypeObjectMother.Create();
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
    public void SetBaseTypeConstraint ()
    {
      Assert.That (_parameter.BaseType, Is.SameAs (typeof(object)));
      _parameter.SetBaseTypeConstraint (_baseTypeConstraint);

      Assert.That (_parameter.BaseType, Is.SameAs (_baseTypeConstraint));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "A base type constraint must be a class.\r\nParameter name: baseTypeConstraint")]
    public void SetBaseTypeConstraint_ThrowsForNonClasses ()
    {
      _parameter.SetBaseTypeConstraint (typeof (IDisposable));
    }

    [Test]
    public void SetInterfaceConstraints ()
    {
      Assert.That (_parameter.GetInterfaces(), Is.Empty);
      _parameter.SetInterfaceConstraints (new[] { _interfaceConstraint }.AsOneTime());

      Assert.That (_parameter.GetInterfaces(), Is.EqualTo (new[] { _interfaceConstraint }));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "All interface constraints must be interfaces.\r\nParameter name: interfaceConstraints")]
    public void SetInterfaceConstraints_ThrowsForNonInterfaces ()
    {
      _parameter.SetInterfaceConstraints (new[] { typeof (string) });
    }

    [Test]
    public void GetGenericParameterConstraints ()
    {
      var result = _constrainedParameter.GetGenericParameterConstraints();

      Assert.That (result, Is.EqualTo (new[] { _baseTypeConstraint, _interfaceConstraint }));
    }

    [Test]
    public void GetGenericParameterConstraints_NoBaseTypeConstraint ()
    {
      _parameter.SetInterfaceConstraints (new[] { _interfaceConstraint });

      var result = _parameter.GetGenericParameterConstraints();

      Assert.That (result, Is.EqualTo (new[] { _interfaceConstraint }));
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
      var result = _constrainedParameter.InvokeNonPublicMethod ("GetAllInterfaces");

      Assert.That (result, Is.EqualTo (new[] { _interfaceConstraint, typeof (IDomainInterface) }));
    }

    [Test]
    public void GetAllInterfaces_Distinct ()
    {
      Assert.That (_baseTypeConstraint.GetInterfaces(), Contains.Item (typeof (IDomainInterface)));
      _parameter.SetBaseTypeConstraint (_baseTypeConstraint);
      _parameter.SetInterfaceConstraints (new[] { typeof (IDomainInterface) });

      var result = _parameter.InvokeNonPublicMethod ("GetAllInterfaces");

      Assert.That (result, Is.EqualTo (new[] { typeof (IDomainInterface) }));
    }

    [Test]
    public void GetAllFields ()
    {
      var result = _constrainedParameter.InvokeNonPublicMethod ("GetAllFields");

      var field = NormalizingMemberInfoFromExpressionUtility.GetField ((DomainType obj) => obj.Field);
      Assert.That (result, Has.Member (field));
      Assert.That (result, Is.EquivalentTo (typeof (DomainType).GetFields (c_allMembers)));
    }

    [Test]
    public void GetAllConstructors ()
    {
      Assert.That (typeof (DomainType).GetConstructors(), Is.Not.Empty);
      var result = _constrainedParameter.InvokeNonPublicMethod ("GetAllConstructors");

      Assert.That (result, Is.Empty);
    }

    [Test]
    public void GetAllConstructors_NewConstraint ()
    {
      var parameter = GenericParameterObjectMother.Create (genericParameterAttributes: GenericParameterAttributes.DefaultConstructorConstraint);

      var result = parameter.GetConstructors (c_allMembers).Single();

      Assert.That (result, Is.TypeOf<GenericParameterDefaultConstructor>());
      Assert.That (result.DeclaringType, Is.SameAs(parameter));
    }

    [Test]
    public void GetAllMethods ()
    {
      var result = _constrainedParameter.InvokeNonPublicMethod ("GetAllMethods");

      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method());
      Assert.That (result, Has.Member (method));
      Assert.That (result, Is.EquivalentTo (typeof (DomainType).GetMethods (c_allMembers)));
    }

    [Test]
    public void GetAllProperties ()
    {
      var result = _constrainedParameter.InvokeNonPublicMethod<IEnumerable<PropertyInfo>> ("GetAllProperties");

      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DomainType obj) => obj.Property);
      Assert.That (result, Is.EqualTo (new[] { property }));
    }

    [Test]
    public void GetAllEvents ()
    {
      var result = _constrainedParameter.InvokeNonPublicMethod<IEnumerable<EventInfo>> ("GetAllEvents");

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
      baseMemberSelectorMock.Expect (mock => mock.SelectMethods (methods, c_allMembers, baseTypeConstraint)).Return (methods);
      baseMemberSelectorMock.Expect (mock => mock.SelectProperties (properties, c_allMembers, baseTypeConstraint)).Return (properties);
      baseMemberSelectorMock.Expect (mock => mock.SelectEvents (events, c_allMembers, baseTypeConstraint)).Return (events);

      var parameter = GenericParameterObjectMother.Create (baseTypeConstraint: baseTypeConstraint, interfaceConstraints: new[] { _interfaceConstraint });

      parameter.InvokeNonPublicMethod ("GetAllFields");
      parameter.InvokeNonPublicMethod ("GetAllConstructors");
      parameter.InvokeNonPublicMethod ("GetAllMethods");
      parameter.InvokeNonPublicMethod ("GetAllProperties");
      parameter.InvokeNonPublicMethod ("GetAllEvents");

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