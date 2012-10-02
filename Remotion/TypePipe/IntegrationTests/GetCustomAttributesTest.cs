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
  [TestFixture]
  public class GetCustomAttributesTest
  {
    // TODO 5062: Tests:
    // AttributeInstantiation (with complex arguments, named fields, named properties on just one exemplary member)
    // Inheritance (AttributeUsage vs. bool parameter on all member kinds and parameters) => rename GetCustomAttributes test below
    // Inheritance_WithAllowMultipleTrue/False (if AllowMultiple is False, recursion must stop when first match is found) 

    [Test]
    [Ignore("TODO 5062")]
    public void GetCustomAttributes ()
    {
      var type = typeof (DerivedClass);
      var mutableType = CreateMutableType(type);
      CheckAttributes (mutableType, type);

      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DerivedClass obj) => obj.OverriddenMethod (""));
      var mutableMethod = mutableType.AllMutableMethods.Single (m => m.Name == "OverriddenMethod");
      CheckAttributes (mutableMethod, method);

      var parameter = method.GetParameters().Single();
      var mutableParameter = (MutableParameterInfo) mutableMethod.GetParameters().Single();
      CheckAttributes (mutableParameter, parameter);

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
      //var @event = type.GetEvents().Single();
      //var mutableEvent = mutableType.AllMutableEvents().Single();
      //CheckAttributes (mutableEvent, @event);

      var implClass = typeof (ClassImplementingInterface);
      var mutableImplClass = CreateMutableType (implClass);
      CheckAttributes (mutableImplClass, implClass);

      var interfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((ClassImplementingInterface obj) => obj.InterfaceMethod ());
      var mutableInterfaceMethod = mutableImplClass.AllMutableMethods.Single();
      CheckAttributes (mutableInterfaceMethod, interfaceMethod);
    }

    private void CheckAttributes (ITypePipeCustomAttributeProvider typePipeAttributeProvider, ICustomAttributeProvider attributeProvider)
    {
      var actualNonInheritAttributes = typePipeAttributeProvider.GetCustomAttributes (false);
      var actualInheritAttributes = typePipeAttributeProvider.GetCustomAttributes (true);
      var expectedNonInheritAttributes = attributeProvider.GetCustomAttributes (false);
      var expectedInheritAttributes = attributeProvider.GetCustomAttributes (true);

      Comparison<object> typeComparer = (a, b) => a.GetType() == b.GetType() ? 0 : -1;

      Assert.That (actualNonInheritAttributes, Is.EqualTo (expectedNonInheritAttributes).Using (typeComparer));
      Assert.That (actualInheritAttributes, Is.EqualTo (expectedInheritAttributes).Using (typeComparer));
    }

    private static MutableType CreateMutableType (Type underlyingType)
    {
      return new MutableType (UnderlyingTypeDescriptor.Create (underlyingType), new MemberSelector (new BindingFlagsEvaluator ()), new RelatedMethodFinder ());
    }

    [Inheritable, NonInheritable]
    class BaseClass
    {
      [Inheritable, NonInheritable]
      [return: Inheritable, NonInheritable]
      public virtual int OverriddenMethod ([Inheritable, NonInheritable] string arg)
      {
        Dev.Null = arg;
        return 0; 
      }

      [Inheritable, NonInheritable]
      public virtual string OverriddenProperty { [Inheritable, NonInheritable] get; set; }

      // ReSharper disable EventNeverInvoked.Global
      [Inheritable, NonInheritable]
      public virtual event EventHandler OverridenEvent;
      // ReSharper restore EventNeverInvoked.Global
    }

    class DerivedClass : BaseClass {

      public override int OverriddenMethod (string arg) { return -1; }
      public override string OverriddenProperty { get; set; }
      public override event EventHandler OverridenEvent;
    }

    [Inheritable, NonInheritable]
    interface IInterface
    {
      [Inheritable, NonInheritable]
      void InterfaceMethod ();
    }

    class ClassImplementingInterface : IInterface
    {
      public void InterfaceMethod () { }
    }

    [AttributeUsage (AttributeTargets.All, Inherited = true)]
    public sealed class InheritableAttribute : Attribute { }

    [AttributeUsage (AttributeTargets.All, Inherited = false)]
    public sealed class NonInheritableAttribute : Attribute { }
  }
}