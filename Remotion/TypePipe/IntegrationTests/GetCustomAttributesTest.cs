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
using NUnit.Framework;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace TypePipe.IntegrationTests
{
  [Ignore ("TODO 5062")]
  [TestFixture]
  public class GetCustomAttributesTest
  {
    [Test]
    [Domain("")]
    public void GetCustomAttributes_AttributeInstantiation_NoFilter ()
    {
      var mutableMember = CreateMutableMember (MethodBase.GetCurrentMethod ());

      var attributes = mutableMember.GetCustomAttributes (false);

      var attributeTypes = attributes.Select (a => a.GetType());
      Assert.That (attributeTypes, Is.EquivalentTo (new[] { typeof (TestAttribute), typeof (DomainAttribute) }));
    }

    [Test]
    [Domain (new object[] { "ctorArg", 7, null, typeof (double), MyEnum.B, new[] { 1, 2 } },
        Field = "named arg",
        Property = new object[] { "named arg", 8, typeof (int), MyEnum.C, new[] { 3, 4 } })]
    public void GetCustomAttributes_AttributeInstantiation ()
    {
      var mutableMember = CreateMutableMember (MethodBase.GetCurrentMethod());

      var attributes = mutableMember.GetCustomAttributes (typeof (DomainAttribute), false);

      var attribute = (DomainAttribute) attributes.Single();
      Assert.That (attribute.CtorArg, Is.EqualTo (new object[] { "ctorArg", 7, null, typeof (double), MyEnum.B, new[] { 1, 2 } }));
      Assert.That (attribute.Field, Is.EqualTo ("named arg"));
      Assert.That (attribute.Property, Is.EqualTo (new object[] { "named arg", 8, typeof (int), MyEnum.C, new[] { 3, 4 } }));
    }

    [Test]
    [InheritableAllowMultipleAttribute ("2"), InheritableAllowMultipleAttribute ("1")]
    public void GetCustomAttributes_AttributeInstantiation_AllowMultiple ()
    {
      var mutableMember = CreateMutableMember (MethodBase.GetCurrentMethod ());

      var attributes = mutableMember.GetCustomAttributes (typeof (InheritableAllowMultipleAttribute), false);

      var attributeCtorArgs = attributes.Cast<InheritableAllowMultipleAttribute>().Select (a => a.CtorArg);
      Assert.That (attributeCtorArgs, Is.EquivalentTo (new[] { "1", "2" }));
    }

    [Test]
    public void GetCustomAttributes_Inheritance_BehavesLikeReflection ()
    {
      var type = typeof (DerivedClass);
      var mutableType = CreateMutableType (type);
      CheckAttributeInheritance (mutableType, type);

      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DerivedClass obj) => obj.Method ());
      var mutableMethod = mutableType.GetOrAddMutableMethod (method);
      CheckAttributeInheritance (mutableMethod, method);

      // TODO 4791
      //var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DerivedClass obj) => obj.OverriddenProperty);
      //var mutableProperty = mutableType.AllMutableProperties.Single();
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
      //var mutableEvent = mutableType.AllMutableEvents().Single();
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
      var type = typeof (DerivedClass);
      var mutableType = CreateMutableType (type);
      CheckAttributeInheritanceAllowMultiple (mutableType, type);

      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DerivedClass obj) => obj.AllowMultipleMethod());
      var mutableMethod = mutableType.GetOrAddMutableMethod (method);
      CheckAttributeInheritanceAllowMultiple (mutableMethod, method);

      // TODO 4791
      //var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DerivedClass obj) => obj.AllowMultipleProperty);
      //var mutableProperty = mutableType.GetMutableProperty(property);
      //CheckAttributes (mutableEvent, @event);

      // TODO 4791
      //var @event = type.GetEvents().Single ...
      //var mutableEvent = mutableType.AllMutableEvents().Single();
      //CheckAttributes (mutableEvent, @event);
    }

    [Test]
    public void GetCustomAttributes_ArrayType_BehavesLikeReflection ()
    {
      var member = MethodBase.GetCurrentMethod();
      var mutableMember = CreateMutableMember (member);
      var inherit = BooleanObjectMother.GetRandomBoolean();

      Assert.That (mutableMember.GetCustomAttributes (inherit), Is.TypeOf (member.GetCustomAttributes (inherit).GetType()));
      Assert.That (mutableMember.GetCustomAttributes (typeof (BaseAttribute), inherit), Is.TypeOf (member.GetCustomAttributes (inherit).GetType()));
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
      var type = typeof (DerivedClass);
      var mutableType = CreateMutableType (type);
      CheckIsDefinedInheritance (mutableType, type);

      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DerivedClass obj) => obj.AllowMultipleMethod ());
      var mutableMethod = mutableType.GetOrAddMutableMethod (method);
      CheckIsDefinedInheritance (mutableMethod, method);

      // TODO 4791
      //var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DerivedClass obj) => obj.AllowMultipleProperty);
      //var mutableProperty = mutableType.GetMutableProperty(property);
      //CheckIsDefinedInheritance (mutableEvent, @event);

      // TODO 4791
      //var @event = type.GetEvents().Single ...
      //var mutableEvent = mutableType.AllMutableEvents().Single();
      //CheckIsDefinedInheritance (mutableEvent, @event);
    }

    private IMutableMember CreateMutableMember (MethodBase underlyingMethod)
    {
      var mutableType = CreateMutableType (typeof (GetCustomAttributesTest));
      return mutableType.GetOrAddMutableMethod ((MethodInfo) underlyingMethod);
    }

    private MutableType CreateMutableType (Type underlyingType)
    {
      return new MutableType (UnderlyingTypeDescriptor.Create (underlyingType), new MemberSelector (new BindingFlagsEvaluator ()), new RelatedMethodFinder ());
    }

    private void CheckAttributeInheritance (ITypePipeCustomAttributeProvider typePipeAttributeProvider, ICustomAttributeProvider attributeProvider)
    {
      var actualNonInheritableAttributes = typePipeAttributeProvider.GetCustomAttributes (false);
      var actualInheritableAttributes = typePipeAttributeProvider.GetCustomAttributes (true);
      var expectedNonInheritableAttributes = attributeProvider.GetCustomAttributes (false);
      var expectedInheritableAttributes = attributeProvider.GetCustomAttributes (true);

      Comparison<object> typeComparer = (a, b) => a.GetType() == b.GetType() ? 0 : -1;
      Assert.That (actualNonInheritableAttributes, Is.EqualTo (expectedNonInheritableAttributes).Using (typeComparer));
      Assert.That (actualInheritableAttributes, Is.EqualTo (expectedInheritableAttributes).Using (typeComparer));
    }

    private void CheckAttributeInheritanceAllowMultiple (ITypePipeCustomAttributeProvider typePipeAttributeProvider, ICustomAttributeProvider attributeProvider)
    {
      var filterType = typeof (AllowMultipleBaseAttribute);
      var actualNonInheritableAttributes = (AllowMultipleBaseAttribute[]) typePipeAttributeProvider.GetCustomAttributes (filterType, false);
      var actualInheritableAttributes = (AllowMultipleBaseAttribute[]) typePipeAttributeProvider.GetCustomAttributes (filterType, true);
      var expectedNonInheritableAttributes = (AllowMultipleBaseAttribute[]) attributeProvider.GetCustomAttributes (filterType, false);
      var expectedInheritableAttributes = (AllowMultipleBaseAttribute[]) attributeProvider.GetCustomAttributes (filterType, true);

      Comparison<AllowMultipleBaseAttribute> multipleAttributeComparer = (a, b) => a.CtorArg == b.CtorArg ? 0 : -1;
      Assert.That (actualNonInheritableAttributes, Is.EqualTo (expectedNonInheritableAttributes).Using (multipleAttributeComparer));
      Assert.That (actualInheritableAttributes, Is.EqualTo (expectedInheritableAttributes).Using (multipleAttributeComparer));
    }

    private void CheckIsDefinedInheritance (ITypePipeCustomAttributeProvider typePipeAttributeProvider, ICustomAttributeProvider attributeProvider)
    {
      Assert.That (
          typePipeAttributeProvider.IsDefined (typeof (InheritableAttribute), true),
          Is.EqualTo (attributeProvider.IsDefined (typeof (InheritableAttribute), true)));
      Assert.That (
          typePipeAttributeProvider.IsDefined (typeof (NonInheritableAttribute), true),
          Is.EqualTo (attributeProvider.IsDefined (typeof (NonInheritableAttribute), true)));
    }

    [Inheritable, NonInheritable]
    [InheritableAllowMultiple ("base"), InheritableNonMultiple ("base")]
    class BaseClass
    {
      [Inheritable, NonInheritable]
      public virtual void Method () { }

      [Inheritable, NonInheritable]
      public virtual string Property { [Inheritable, NonInheritable] get; [Inheritable, NonInheritable] set; }

      // ReSharper disable ValueParameterNotUsed
      [Inheritable, NonInheritable]
      public virtual event EventHandler Event { [Inheritable, NonInheritable] add { } [Inheritable, NonInheritable] remove { } }
      // ReSharper restore ValueParameterNotUsed

      [InheritableAllowMultiple ("base"), InheritableNonMultiple ("base")]
      public virtual void AllowMultipleMethod () { }

      [InheritableAllowMultiple ("base"), InheritableNonMultiple ("base")]
      public virtual string AllowMultipleProperty
      {
        [InheritableAllowMultiple ("base"), InheritableNonMultiple ("base")] get;
        [InheritableAllowMultiple ("base"), InheritableNonMultiple ("base")] set;
      }

      // ReSharper disable ValueParameterNotUsed
      [InheritableAllowMultiple ("base"), InheritableNonMultiple ("base")]
      public virtual event EventHandler AllowMultipleEvent
      {
        [InheritableAllowMultiple ("base"), InheritableNonMultiple ("base")] add { }
        [InheritableAllowMultiple ("base"), InheritableNonMultiple ("base")] remove { }
      }
      // ReSharper restore ValueParameterNotUsed
    }

    [InheritableAllowMultiple ("derived"), InheritableNonMultiple ("derived")]
    class DerivedClass : BaseClass
    {
      public override void Method () { }
      public override string Property { get; set; }
      public override event EventHandler Event;

      [InheritableAllowMultiple ("derived"), InheritableNonMultiple ("derived")]
      public override void AllowMultipleMethod () { }

      [InheritableAllowMultiple ("derived"), InheritableNonMultiple ("derived")]
      public override string AllowMultipleProperty
      {
        [InheritableAllowMultiple ("derived"), InheritableNonMultiple ("derived")] get;
        [InheritableAllowMultiple ("derived"), InheritableNonMultiple ("derived")] set;
      }

      [InheritableAllowMultiple ("derived"), InheritableNonMultiple ("derived")]
      public override event EventHandler AllowMultipleEvent
      {
        [InheritableAllowMultiple ("derived"), InheritableNonMultiple ("derived")] add { }
        [InheritableAllowMultiple ("derived"), InheritableNonMultiple ("derived")] remove { }
      }
    }

    class DomainAttribute : Attribute
    {
      public string Field;

      public DomainAttribute (object ctorArg)
      {
        CtorArg = ctorArg;
      }

      public object CtorArg { get; private set; }
      public object Property { get; set; }
    }

    enum MyEnum { A, B, C }

    [DerivedAttribute]
    void IsDefinedMemberWithDerivedAttribute () { }

    class BaseAttribute : Attribute { }
    class DerivedAttribute : BaseAttribute, IDerivedAttributeInterface { }
    interface IDerivedAttributeInterface { }
    class UnrelatedAttribute : Attribute { }

    [AttributeUsage (AttributeTargets.All, Inherited = true)]
    class InheritableAttribute : Attribute { }

    [AttributeUsage (AttributeTargets.All, Inherited = false)]
    class NonInheritableAttribute : Attribute { }

    [AttributeUsage (AttributeTargets.All, AllowMultiple = true)]
    abstract class AllowMultipleBaseAttribute : Attribute
    {
      public AllowMultipleBaseAttribute (string arg) { CtorArg = arg; }

      public string CtorArg { get; private set; }
    }

    [AttributeUsage (AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    class InheritableAllowMultipleAttribute : AllowMultipleBaseAttribute
    {
      public InheritableAllowMultipleAttribute (string arg) : base(arg) { }
    }

    [AttributeUsage (AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    class InheritableNonMultipleAttribute : AllowMultipleBaseAttribute
    {
      public InheritableNonMultipleAttribute (string arg) : base(arg) { }
    }
  }
}