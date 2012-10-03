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
using Remotion.Development.UnitTesting;
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

      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DerivedClass obj) => obj.Method (""));
      var mutableMethod = mutableType.GetOrAddMutableMethod (method);
      CheckAttributeInheritance (mutableMethod, method);

      var parameter = method.GetParameters().Single();
      var mutableParameter = (MutableParameterInfo) mutableMethod.GetParameters().Single();
      CheckAttributeInheritance (mutableParameter, parameter);

      // TODO 4793
      //var returnParameter = method.ReturnParameter;
      //var mutableReturnParameter = mutableMethod.ReturnParameter;
      //CheckAttributes (mutableReturnParameter, returnParameter);

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

      // TODO 4791: propert getter, property setter

      // TODO 4791
      //var @event = type.GetEvents().Single ...
      //var mutableEvent = mutableType.AllMutableEvents().Single();
      //CheckAttributes (mutableEvent, @event);

      // TODO 4791: event adder, event remover
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
      var actualNonInheritAttributes = typePipeAttributeProvider.GetCustomAttributes (false);
      var actualInheritAttributes = typePipeAttributeProvider.GetCustomAttributes (true);
      var expectedNonInheritAttributes = attributeProvider.GetCustomAttributes (false);
      var expectedInheritAttributes = attributeProvider.GetCustomAttributes (true);

      Comparison<object> typeComparer = (a, b) => a.GetType() == b.GetType() ? 0 : -1;
      Assert.That (actualNonInheritAttributes, Is.EqualTo (expectedNonInheritAttributes).Using (typeComparer));
      Assert.That (actualInheritAttributes, Is.EqualTo (expectedInheritAttributes).Using (typeComparer));
    }

    private void CheckAttributeInheritanceAllowMultiple (ITypePipeCustomAttributeProvider typePipeAttributeProvider, ICustomAttributeProvider attributeProvider)
    {
      var filterType = typeof (AllowMultipleBaseAttribute);
      var actualNonInheritAttributes = (AllowMultipleBaseAttribute[]) typePipeAttributeProvider.GetCustomAttributes (filterType, false);
      var actualInheritAttributes = (AllowMultipleBaseAttribute[]) typePipeAttributeProvider.GetCustomAttributes (filterType, true);
      var expectedNonInheritAttributes = (AllowMultipleBaseAttribute[]) attributeProvider.GetCustomAttributes (filterType, false);
      var expectedInheritAttributes = (AllowMultipleBaseAttribute[]) attributeProvider.GetCustomAttributes (filterType, true);

      Comparison<AllowMultipleBaseAttribute> multipleAttributeComparer = (a, b) => a.CtorArg == b.CtorArg ? 0 : -1;
      Assert.That (actualNonInheritAttributes, Is.EqualTo (expectedNonInheritAttributes).Using (multipleAttributeComparer));
      Assert.That (actualInheritAttributes, Is.EqualTo (expectedInheritAttributes).Using (multipleAttributeComparer));
    }

    [Inheritable, NonInheritable]
    [InheritableAllowMultiple ("base"), InheritableNonMultiple ("base")]
    class BaseClass
    {
      [Inheritable, NonInheritable]
      [return: Inheritable, NonInheritable]
      public virtual int Method (string arg) { Dev.Null = arg; return 0; }

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
      public override int Method (string arg) { return 0; }
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

    enum MyEnum { A, B, C }
  }
}