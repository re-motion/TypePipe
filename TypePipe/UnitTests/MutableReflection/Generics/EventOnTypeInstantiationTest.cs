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
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection.Generics;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection.Generics;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  [TestFixture]
  public class EventOnTypeInstantiationTest
  {
    private TypeInstantiation _declaringType;
    private EventInfo _originalEvent;
    private MethodOnTypeInstantiation _addMethod;
    private MethodOnTypeInstantiation _removeMethod;
    private MethodOnTypeInstantiation _raiseMethod;

    private EventOnTypeInstantiation _event;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = TypeInstantiationObjectMother.Create();
      _originalEvent = GetType().GetEvent ("Event");
      _addMethod = MethodOnTypeInstantiationObjectMother.Create (_declaringType, GetType().GetMethod ("add_Event"));
      _removeMethod = MethodOnTypeInstantiationObjectMother.Create (_declaringType, GetType().GetMethod ("remove_Event"));
      _raiseMethod = MethodOnTypeInstantiationObjectMother.Create (_declaringType, GetType().GetMethod ("RaiseMethod"));

      _event = new EventOnTypeInstantiation (_declaringType, _originalEvent, _addMethod, _removeMethod, _raiseMethod);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_event.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (_event.EventOnGenericType, Is.SameAs (_originalEvent));
      Assert.That (_event.EventHandlerType, Is.SameAs (_originalEvent.EventHandlerType));
      Assert.That (_event.Name, Is.EqualTo (_originalEvent.Name));
      Assert.That (_event.Attributes, Is.EqualTo (_originalEvent.Attributes));
      Assert.That (_event.GetAddMethod(), Is.SameAs (_addMethod));
      Assert.That (_event.GetRemoveMethod(), Is.SameAs (_removeMethod));
      Assert.That (_event.GetRaiseMethod(), Is.SameAs (_raiseMethod));
    }

    [Test]
    public void GetCustomAttributeData ()
    {
      var customAttributes = new[] { CustomAttributeDeclarationObjectMother.Create() };
      var evt = CustomEventInfoObjectMother.Create (customAttributes: customAttributes);
      var evtInstantiation = new EventOnTypeInstantiation (_declaringType, evt, _addMethod, _removeMethod, _raiseMethod);

      Assert.That (evtInstantiation.GetCustomAttributeData (), Is.EqualTo (customAttributes));
    }

    public event EventHandler Event;

    public void RaiseMethod (object sender, EventArgs args) { }
  }
}