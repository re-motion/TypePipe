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
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class CustomEventInfoTest
  {
    private MethodInfo _publicAddMethod;
    private MethodInfo _publicRemoveMethod;
    private MethodInfo _publicRaiseMethod;
    private MethodInfo _nonPublicAddMethod;
    private MethodInfo _nonPublicRemoveMethod;
    private MethodInfo _nonPublicRaiseMethod;

    [SetUp]
    public void SetUp ()
    {
      var methods = typeof (DomainType).GetMethods (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
      _publicAddMethod = methods.Single (m => m.Name == "PublicAddMethod");
      _publicRemoveMethod = methods.Single (m => m.Name == "PublicRemoveMethod");
      _publicRaiseMethod = methods.Single (m => m.Name == "PublicRaiseMethod");
      _nonPublicAddMethod = methods.Single (m => m.Name == "NonPublicAddMethod");
      _nonPublicRemoveMethod = methods.Single (m => m.Name == "NonPublicRemoveMethod");
      _nonPublicRaiseMethod = methods.Single (m => m.Name == "NonPublicRaiseMethod");
    }

    [Test]
    public void Initialization ()
    {
      var declaringType = CustomTypeObjectMother.Create();
      var name = "Event";
      var attributes = (EventAttributes) 7;

      var result = CreateCustomEventInfo (declaringType, name, attributes, _publicAddMethod, _publicRemoveMethod, _publicRaiseMethod);

      Assert.That (result.Attributes, Is.EqualTo (attributes));
      Assert.That (result.DeclaringType, Is.EqualTo (declaringType));
      Assert.That (result.Name, Is.EqualTo (name));
      Assert.That (result.GetAddMethod(), Is.SameAs (_publicAddMethod));
      Assert.That (result.GetRemoveMethod(), Is.SameAs (_publicRemoveMethod));
      Assert.That (result.GetRaiseMethod(), Is.SameAs (_publicRaiseMethod));
    }

    [Test]
    public void GetAddMethod ()
    {
      var event1 = CreateCustomEventInfo (addMethod: _nonPublicAddMethod);
      var event2 = CreateCustomEventInfo (addMethod: _publicAddMethod);

      Assert.That (event1.GetAddMethod (true), Is.SameAs (_nonPublicAddMethod));
      Assert.That (event1.GetAddMethod (false), Is.Null);
      Assert.That (event2.GetAddMethod (true), Is.SameAs (_publicAddMethod));
      Assert.That (event2.GetAddMethod (false), Is.SameAs (_publicAddMethod));
    }

    [Test]
    public void GetRemoveMethod ()
    {
      var event1 = CreateCustomEventInfo (removeMethod: _nonPublicRemoveMethod);
      var event2 = CreateCustomEventInfo (removeMethod: _publicRemoveMethod);

      Assert.That (event1.GetRemoveMethod (true), Is.SameAs (_nonPublicRemoveMethod));
      Assert.That (event1.GetRemoveMethod (false), Is.Null);
      Assert.That (event2.GetRemoveMethod (true), Is.SameAs (_publicRemoveMethod));
      Assert.That (event2.GetRemoveMethod (false), Is.SameAs (_publicRemoveMethod));
    }

    [Test]
    public void CustomAttributeMethods ()
    {
      var event_ = CreateCustomEventInfo();
      event_.CustomAttributeDatas = new[] { CustomAttributeDeclarationObjectMother.Create (typeof (ObsoleteAttribute)) };

      Assert.That (event_.GetCustomAttributes (false).Select (a => a.GetType()), Is.EqualTo (new[] { typeof (ObsoleteAttribute) }));
      Assert.That (event_.GetCustomAttributes (typeof (NonSerializedAttribute), false), Is.Empty);

      Assert.That (event_.IsDefined (typeof (ObsoleteAttribute), false), Is.True);
      Assert.That (event_.IsDefined (typeof (NonSerializedAttribute), false), Is.False);
    }

    [Test]
    public new void ToString ()
    {
      var name = "MyEvent";
      var eventTypeName = _publicAddMethod.GetParameters().Single().ParameterType.Name;
      var event_ = CreateCustomEventInfo (name: name, addMethod: _publicAddMethod);

      Assert.That (event_.ToString (), Is.EqualTo (eventTypeName + " MyEvent"));
    }

    [Test]
    public void ToDebugString ()
    {
      var declaringType = CustomTypeObjectMother.Create ();
      var name = "MyEvent";
      var event_ = CreateCustomEventInfo (declaringType, name, addMethod: _publicAddMethod);

      // Note: ToDebugString is defined in CustomFieldInfo base class.
      Assertion.IsNotNull (event_.DeclaringType);
      var declaringTypeName = event_.DeclaringType.Name;
      var eventTypeName = event_.EventHandlerType.Name;
      var eventName = event_.Name;
      var expected = "TestableCustomEvent = \"" + eventTypeName + " " + eventName + "\", DeclaringType = \"" + declaringTypeName + "\"";

      Assert.That (event_.ToDebugString (), Is.EqualTo (expected));
    }

    [Test]
    public void UnsupportedMembers ()
    {
      var event_ = CreateCustomEventInfo();

      UnsupportedMemberTestHelper.CheckProperty (() => event_.ReflectedType, "ReflectedType");
      UnsupportedMemberTestHelper.CheckMethod (() => event_.GetOtherMethods (true), "GetOtherMethods");
    }

    private TestableCustomEventInfo CreateCustomEventInfo (
        CustomType declaringType = null,
        string name = "Event",
        EventAttributes attributes = (EventAttributes) 7,
        MethodInfo addMethod = null,
        MethodInfo removeMethod = null,
        MethodInfo raiseMethod = null)
    {
      declaringType = declaringType ?? CustomTypeObjectMother.Create();
      addMethod = addMethod ?? _publicAddMethod;
      removeMethod = removeMethod ?? _publicRemoveMethod;
      raiseMethod = raiseMethod ?? _publicRaiseMethod;

      return new TestableCustomEventInfo (declaringType, name, attributes, addMethod, removeMethod, raiseMethod);
    }

    class TestableCustomEventInfo : CustomEventInfo
    {
      public TestableCustomEventInfo (
          CustomType declaringType,
          string name,
          EventAttributes attributes,
          MethodInfo addMethod,
          MethodInfo removeMethod,
          MethodInfo raiseMethod)
          : base (declaringType, name, attributes, addMethod, removeMethod, raiseMethod) {}

      public IEnumerable<ICustomAttributeData> CustomAttributeDatas;

      public override IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
      {
        return CustomAttributeDatas;
      }
    }

    delegate void MyEventDelegate (string arg1, int arg2);

    class DomainType
    {
      public void PublicAddMethod (MyEventDelegate delegate_) { }
      public void PublicRemoveMethod (MyEventDelegate delegate_) { }
      public void PublicRaiseMethod (string arg1, int arg2) { }

      private void NonPublicAddMethod (MyEventDelegate delegate_) { }
      private void NonPublicRemoveMethod (MyEventDelegate delegate_) { }
      private void NonPublicRaiseMethod (string arg1, int arg2) { }
    }
  }
}