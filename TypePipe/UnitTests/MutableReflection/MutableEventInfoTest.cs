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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableEventInfoTest
  {
    private MutableType _declaringType;
    private string _name;
    private EventAttributes _attributes;
    private Type _argumentType;
    private Type _returnType;
    private Type _handlerType;
    private MutableMethodInfo _addMethod;
    private MutableMethodInfo _removeMethod;
    private MutableMethodInfo _raiseMethod;

    private MutableEventInfo _event;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = MutableTypeObjectMother.Create();
      _name = "Event";
      _attributes = (EventAttributes) 7;
      _argumentType = ReflectionObjectMother.GetSomeType();
      _returnType = ReflectionObjectMother.GetSomeOtherType();
      _handlerType = typeof (Func<,>).MakeGenericType (_argumentType, _returnType);
      _addMethod = MutableMethodInfoObjectMother.Create (
          attributes: MethodAttributes.Public,
          returnType: typeof (void),
          parameters: new[] { new ParameterDeclaration (_handlerType, "handler") });
      _removeMethod = MutableMethodInfoObjectMother.Create (
          attributes: MethodAttributes.Public,
          returnType: typeof (void),
          parameters: new[] { new ParameterDeclaration (_handlerType, "handler") });
      _raiseMethod = MutableMethodInfoObjectMother.Create (
          attributes: MethodAttributes.Public,
          returnType: _returnType,
          parameters: new[] { new ParameterDeclaration (_argumentType, "") });

      _event = new MutableEventInfo (_declaringType, _name, _attributes, _addMethod, _removeMethod, _raiseMethod);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_event.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (_event.MutableDeclaringType, Is.SameAs (_declaringType));
      Assert.That (_event.Name, Is.EqualTo (_name));
      Assert.That (_event.Attributes, Is.EqualTo (_attributes));
      Assert.That (_event.EventHandlerType, Is.SameAs (_handlerType));
      Assert.That (_event.MutableAddMethod, Is.SameAs (_addMethod));
      Assert.That (_event.MutableRemoveMethod, Is.SameAs (_removeMethod));
      Assert.That (_event.MutableRaiseMethod, Is.SameAs (_raiseMethod));
    }

    [Test]
    public void Initialization_NoRaiseMethod ()
    {
      var event_ = new MutableEventInfo (_declaringType, _name, _attributes, _addMethod, _removeMethod, null);

      Assert.That (event_.MutableRaiseMethod, Is.Null);
    }

    [Test]
    public void Initialization_PrivateAccessors ()
    {
      var addMethod = MutableMethodInfoObjectMother.Create (
          attributes: MethodAttributes.Private,
          returnType: typeof (void),
          parameters: new[] { new ParameterDeclaration (_handlerType, "handler") });
      var removeMethod = MutableMethodInfoObjectMother.Create (
          attributes: MethodAttributes.Private,
          returnType: typeof (void),
          parameters: new[] { new ParameterDeclaration (_handlerType, "handler") });
      var raiseMethod = MutableMethodInfoObjectMother.Create (
          attributes: MethodAttributes.Private,
          returnType: _returnType,
          parameters: new[] { new ParameterDeclaration (_argumentType, "") });

      var event_ = new MutableEventInfo (_declaringType, _name, _attributes, addMethod, removeMethod, raiseMethod);

      Assert.That (event_.MutableAddMethod, Is.SameAs (addMethod));
      Assert.That (event_.MutableRemoveMethod, Is.SameAs (removeMethod));
      Assert.That (event_.MutableRaiseMethod, Is.SameAs (raiseMethod));
    }

    [Test]
    public void CustomAttributeMethods ()
    {
      var declaration = CustomAttributeDeclarationObjectMother.Create (typeof (ObsoleteAttribute));
      _event.AddCustomAttribute (declaration);

      Assert.That (_event.AddedCustomAttributes, Is.EqualTo (new[] { declaration }));
      Assert.That (_event.GetCustomAttributeData().Select (a => a.Type), Is.EquivalentTo (new[] { typeof (ObsoleteAttribute) }));
    }

    [Test]
    public void ToDebugString ()
    {
      // Note: ToDebugString is defined in CustomEventInfo base class.
      Assertion.IsNotNull (_event.DeclaringType);
      var declaringTypeName = _event.DeclaringType.Name;
      var eventHandlerTypeName = _event.EventHandlerType.Name + "[" + _argumentType.Name + "," + _returnType.Name + "]";
      var eventName = _event.Name;
      var expected = "MutableEvent = \"" + eventHandlerTypeName + " " + eventName + "\", DeclaringType = \"" + declaringTypeName + "\"";

      Assert.That (_event.ToDebugString(), Is.EqualTo (expected));
    }
  }
}