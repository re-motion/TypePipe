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
using JetBrains.Annotations;
using NUnit.Framework;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.IntegrationTests.MutableReflection
{
  [TestFixture]
  public class GetCustomAttributesTest
  {
    [Test]
    public void GetCustomAttributes_AttributeInstantiation_NoFilter ()
    {
      var attr1 = CreateAttribute<TestAttribute> (new object[0]);
      var attr2 = CreateAttribute<DomainAttribute> (new object[] { "" });
      var mutableInfo = CreateMutableInfo (attr1, attr2);

      var attributes = mutableInfo.GetCustomAttributes (false);

      var attributeTypes = attributes.Select (a => a.GetType());
      Assert.That (attributeTypes, Is.EquivalentTo (new[] { typeof (TestAttribute), typeof (DomainAttribute) }));
    }

    [Test]
    public virtual void GetCustomAttributes_AttributeInstantiation ()
    {
      var field = NormalizingMemberInfoFromExpressionUtility.GetField ((DomainAttribute obj) => obj.Field);
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DomainAttribute obj) => obj.Property);
      var attr = CreateAttribute<DomainAttribute> (
          new object[] { new object[] { "ctorArg", 7, null, typeof (double), MyEnum.B, new[] { 1, 2 } } },
          new NamedArgumentDeclaration (field, "named arg"),
          new NamedArgumentDeclaration (property, new object[] { "named arg", 8, typeof (int), MyEnum.C, new[] { 3, 4 } }));
      var mutableInfo = CreateMutableInfo (attr);

      var attributes = mutableInfo.GetCustomAttributes (typeof (DomainAttribute), false);

      var attribute = (DomainAttribute) attributes.Single();
      Assert.That (attribute.CtorArg, Is.EqualTo (new object[] { "ctorArg", 7, null, typeof (double), MyEnum.B, new[] { 1, 2 } }));
      Assert.That (attribute.Field, Is.EqualTo ("named arg"));
      Assert.That (attribute.Property, Is.EqualTo (new object[] { "named arg", 8, typeof (int), MyEnum.C, new[] { 3, 4 } }));
    }

    [Test]
    public void GetCustomAttributes_AttributeInstantiation_AllowMultiple ()
    {
      var attr1 = CreateAttribute<InheritableAllowMultipleAttribute> (new object[] { "1" });
      var attr2 = CreateAttribute<InheritableAllowMultipleAttribute> (new object[] { "2" });
      var mutableInfo = CreateMutableInfo (attr2, attr1); // mix order.

      var attributes = mutableInfo.GetCustomAttributes (typeof (InheritableAllowMultipleAttribute), false);

      var attributeCtorArgs = attributes.Cast<InheritableAllowMultipleAttribute>().Select (a => a.CtorArg);
      Assert.That (attributeCtorArgs, Is.EquivalentTo (new[] { "1", "2" }));
    }

    [Test]
    public void GetCustomAttributes_Inheritance_BehavesLikeReflection ()
    {
      var type = typeof (DomainType);
      var proxyType = MutableTypeObjectMother.Create (type);
      CheckAttributeInheritance (proxyType, type);

      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method());
      var mutableMethod = proxyType.GetOrAddOverride (method);
      CheckAttributeInheritance (mutableMethod, method);

      // TODO 4791
      //var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DerivedClass obj) => obj.OverriddenProperty);
      //var mutableProperty = MutableType.AllMutableProperties.Single();
      //CheckAttributes (mutableProperty, property);

      // TODO 4791
      //var getter = property.GetGetMethod();
      //var mutableGetter = mutableProperty.GetGetMethod();
      //CheckAttributes (mutableGetter, getter);

      // TODO 4791
      //var setter = property.GetSetMethod();
      //var mutableSetter = mutableProperty.GetSetMethod();
      //CheckAttributes (mutableSetter, setter);

      // TODO 4791
      //var @event = type.GetEvents().Single();
      //var mutableEvent = MutableType.AllMutableEvents().Single();
      //CheckAttributes (mutableEvent, @event);

      // TODO 4791
      //var eventAdder = @event.GetAddMethod();
      //var mutableEventAdder = ...
      //CheckAttributes (mutableEventAdder, @eventAdder);

      // TODO 4791
      //var eventsetter = @event.GetAddMethod();
      //var mutableEventsetter = ...
      //CheckAttributes (mutableEventsetter, @eventsetter);
    }

    [Test]
    public void GetCustomAttributes_Inheritance_AllowMultiple_BehavesLikeReflection ()
    {
      var type = typeof (DomainType);
      var proxyType = MutableTypeObjectMother.Create (type);
      CheckAttributeInheritanceAllowMultiple (proxyType);

      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.AllowMultipleMethod());
      var mutableMethod = proxyType.GetOrAddOverride (method);
      CheckAttributeInheritanceAllowMultiple (mutableMethod);

      // TODO 4791
      //var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DerivedClass obj) => obj.AllowMultipleProperty);
      //var mutableProperty = MutableType.GetMutableProperty(property);
      //CheckAttributes (mutableEvent, @event);

      // TODO 4791
      //var @event = type.GetEvents().Single ...
      //var mutableEvent = MutableType.AllMutableEvents().Single();
      //CheckAttributes (mutableEvent, @event);
    }

    [Test]
    public void GetCustomAttributes_ArrayType_BehavesLikeReflection ()
    {
      var member = MethodBase.GetCurrentMethod();
      var mutableInfo = CreateMutableInfo();
      var inherit = BooleanObjectMother.GetRandomBoolean();

      var expectedType1 = member.GetCustomAttributes (inherit).GetType();
      Assert.That (mutableInfo.GetCustomAttributes (inherit), Is.TypeOf (expectedType1));

      var expectedType2 = member.GetCustomAttributes (typeof (BaseAttribute), inherit).GetType();
      Assert.That (mutableInfo.GetCustomAttributes (typeof (BaseAttribute), inherit), Is.TypeOf (expectedType2));
    }

    [Test]
    public void IsDefined ()
    {
      var member = NormalizingMemberInfoFromExpressionUtility.GetMember (() => IsDefinedMemberWithDerivedAttribute());

      Assert.That (member.IsDefined (typeof (UnrelatedAttribute), false), Is.False);
      Assert.That (member.IsDefined (typeof (DerivedAttribute), false), Is.True);
      Assert.That (member.IsDefined (typeof (BaseAttribute), false), Is.True);
      Assert.That (member.IsDefined (typeof (IDerivedAttributeInterface), false), Is.True);
    }

    [Test]
    public void IsDefined_Inheritance_BehavesLikeReflection ()
    {
      var type = typeof (DomainType);
      var proxyType = MutableTypeObjectMother.Create (type);
      CheckIsDefinedInheritance (proxyType);

      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method());
      var mutableMethod = proxyType.GetOrAddOverride (method);
      CheckIsDefinedInheritance (mutableMethod);

      // TODO Review: Uncomment
      // TODO 4791
      //var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DerivedClass obj) => obj.Property);
      //var mutableProperty = MutableType.GetMutableProperty(property);
      //CheckIsDefinedInheritance (mutableEvent, @event);

      // TODO 4791
      //var @event = type.GetEvents().Single ...
      //var mutableEvent = MutableType.AllMutableEvents().Single();
      //CheckIsDefinedInheritance (mutableEvent, @event);
    }

    private CustomAttributeDeclaration CreateAttribute<T> (object[] ctorArgs, params NamedArgumentDeclaration[] namedArguments)
        where T : Attribute
    {
      var ctor = typeof (T).GetConstructors().Single();
      return new CustomAttributeDeclaration (ctor, ctorArgs, namedArguments);
    }

    private IMutableMember CreateMutableInfo (params CustomAttributeDeclaration[] customAttributes)
    {
      var member = new MutableFieldInfo (MutableTypeObjectMother.Create (GetType()), "member", typeof (int), FieldAttributes.Private);
      foreach (var customAttriubte in customAttributes)
        member.AddCustomAttribute (customAttriubte);

      return member;
    }

    private void CheckAttributeInheritance (IOwnCustomAttributeDataProvider ownAttributeDataProvider, ICustomAttributeProvider attributeProvider)
    {
      var actualNonInheritableAttributes = ownAttributeDataProvider.GetCustomAttributes (false);
      var actualInheritableAttributes = ownAttributeDataProvider.GetCustomAttributes (true);
      var expectedNonInheritableAttributes = attributeProvider.GetCustomAttributes (false);
      var expectedInheritableAttributes = attributeProvider.GetCustomAttributes (true);

      Comparison<object> typeComparer = (a, b) => a.GetType() == b.GetType() ? 0 : -1;
      Assert.That (actualNonInheritableAttributes, Is.EqualTo (expectedNonInheritableAttributes).Using (typeComparer));
      Assert.That (actualInheritableAttributes, Is.EqualTo (expectedInheritableAttributes).Using (typeComparer));
    }

    private void CheckAttributeInheritanceAllowMultiple (IMutableMember mutableInfo)
    {
      mutableInfo.AddCustomAttribute (CreateAttribute<InheritableAllowMultipleAttribute> (new object[] { "derived1" }));
      mutableInfo.AddCustomAttribute (CreateAttribute<NonInheritableAllowMultipleAttribute> (new object[] { "derived2" }));

      var filterType = typeof (AllowMultipleBaseAttribute);
      var actualAttributes = (AllowMultipleBaseAttribute[]) mutableInfo.GetCustomAttributes (filterType, true);

      Assert.That (actualAttributes.Select (a => a.CtorArg), Is.EquivalentTo (new[] { "derived1", "derived2", "base1" }));
    }

    private void CheckIsDefinedInheritance (IOwnCustomAttributeDataProvider ownAttributeDataProvider)
    {
      Assert.That (ownAttributeDataProvider.IsDefined (typeof (InheritableAttribute), true), Is.True);
      Assert.That (ownAttributeDataProvider.IsDefined (typeof (NonInheritableAttribute), true), Is.False);
    }

    [Inheritable, NonInheritable]
    [InheritableAllowMultiple ("base1"), NonInheritableAllowMultiple ("base2")]
    public class DomainType
    {
      [Inheritable, NonInheritable]
      public virtual void Method () { }

      [Inheritable, NonInheritable]
      public virtual string Property { [Inheritable, NonInheritable] get; [Inheritable, NonInheritable] set; }

      // ReSharper disable ValueParameterNotUsed
      [Inheritable, NonInheritable]
      public virtual event EventHandler Event { [Inheritable, NonInheritable] add { } [Inheritable, NonInheritable] remove { } }
      // ReSharper restore ValueParameterNotUsed

      [InheritableAllowMultiple ("base1"), NonInheritableAllowMultiple ("base2")]
      public virtual void AllowMultipleMethod () { }

      [InheritableAllowMultiple ("base1"), NonInheritableAllowMultiple ("base2")]
      public virtual string AllowMultipleProperty
      {
        [InheritableAllowMultiple ("base1"), NonInheritableAllowMultiple ("base2")] get;
        [InheritableAllowMultiple ("base1"), NonInheritableAllowMultiple ("base2")] set;
      }

      // ReSharper disable ValueParameterNotUsed
      [InheritableAllowMultiple ("base1"), NonInheritableAllowMultiple ("base2")]
      public virtual event EventHandler AllowMultipleEvent
      {
        [InheritableAllowMultiple ("base1"), NonInheritableAllowMultiple ("base2")] add { }
        [InheritableAllowMultiple ("base1"), NonInheritableAllowMultiple ("base2")] remove { }
      }
      // ReSharper restore ValueParameterNotUsed
    }

    public class DomainAttribute : Attribute
    {
      [UsedImplicitly] public string Field;

      public DomainAttribute (object ctorArg)
      {
        CtorArg = ctorArg;
      }

      public object CtorArg { get; private set; }
      public object Property { get; set; }
    }

    public enum MyEnum { A, B, C }

    [Derived]
    void IsDefinedMemberWithDerivedAttribute () { }

    public class BaseAttribute : Attribute { }
    public class DerivedAttribute : BaseAttribute, IDerivedAttributeInterface { }
    interface IDerivedAttributeInterface { }
    public class UnrelatedAttribute : Attribute { }

    [AttributeUsage (AttributeTargets.All, Inherited = true)]
    public  class InheritableAttribute : Attribute { }

    [AttributeUsage (AttributeTargets.All, Inherited = false)]
    public class NonInheritableAttribute : Attribute { }

    [AttributeUsage (AttributeTargets.All, AllowMultiple = true)]
    public abstract class AllowMultipleBaseAttribute : Attribute
    {
      protected AllowMultipleBaseAttribute (string arg) { CtorArg = arg; }

      public string CtorArg { get; private set; }
    }

    [AttributeUsage (AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    public class InheritableAllowMultipleAttribute : AllowMultipleBaseAttribute
    {
      public InheritableAllowMultipleAttribute (string arg) : base(arg) { }
    }

    [AttributeUsage (AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class NonInheritableAllowMultipleAttribute : AllowMultipleBaseAttribute
    {
      public NonInheritableAllowMultipleAttribute (string arg) : base(arg) { }
    }
  }
}