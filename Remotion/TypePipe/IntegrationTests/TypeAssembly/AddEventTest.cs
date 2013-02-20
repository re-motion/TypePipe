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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.Development.UnitTesting;
using Remotion.Utilities;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class AddEventTest : TypeAssemblerIntegrationTestBase
  {
    public override void TearDown ()
    {
      base.TearDown ();

      DomainType.StaticDelegateField = null;
    }

    [Test]
    public void AddRemove ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var addField = NormalizingMemberInfoFromExpressionUtility.GetField ((DomainType obj) => obj.AddCalled);
            var removeField = NormalizingMemberInfoFromExpressionUtility.GetField ((DomainType obj) => obj.RemoveCalled);
            proxyType.AddEvent (
                "Event",
                typeof (Func<int, string, long>),
                addBodyProvider: ctx =>
                {
                  Assert.That (ctx.Parameters.Single().Type, Is.SameAs (typeof (Func<int, string, long>)));
                  return Expression.Assign (Expression.Field (ctx.This, addField), Expression.Constant (true));
                },
                removeBodyProvider: ctx =>
                {
                  Assert.That (ctx.Parameters.Single().Type, Is.SameAs (typeof (Func<int, string, long>)));
                  return Expression.Assign (Expression.Field (ctx.This, removeField), Expression.Constant (true));
                },
                raiseBodyProvider: ctx =>
                {
                  Assert.That (ctx.Parameters.Select (p => p.Type), Is.EqualTo (new[] { typeof (int), typeof (string) }));
                  Assert.That (ctx.ReturnType, Is.SameAs (typeof (long)));
                  return Expression.Constant (7L);
                });
          });

      var event_ = type.GetEvent ("Event");
      var instance = (DomainType) Activator.CreateInstance (type);

      event_.AddEventHandler (instance, null);
      Assert.That (instance.AddCalled, Is.True);
      event_.RemoveEventHandler (instance, null);
      Assert.That (instance.RemoveCalled, Is.True);
      var result = instance.InvokeNonPublicMethod<long> (event_.GetRaiseMethod(), 0, "");
      Assert.That (result, Is.EqualTo (7L));
    }

    [Test]
    public void AccessorAttributes ()
    {
      AssembleType<DomainType> (
          proxyType =>
          {
            var accessorAttributes = MethodAttributes.Public | MethodAttributes.Virtual;
            var event_ = proxyType.AddEvent (
                "Event",
                typeof (Action),
                accessorAttributes,
                addBodyProvider: ctx => Expression.Empty(),
                removeBodyProvider: ctx => Expression.Empty());

            Assert.That (
                event_.GetAddMethod().Attributes, Is.EqualTo (accessorAttributes | MethodAttributes.SpecialName | MethodAttributes.HideBySig));
            Assert.That (
                event_.GetRemoveMethod().Attributes, Is.EqualTo (accessorAttributes | MethodAttributes.SpecialName | MethodAttributes.HideBySig));
          });
    }

    [Test]
    public void Redeclare_UsingAccesssors ()
    {
      var dummyRaiseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType o) => o.DummyRaiseMethod (7));
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var existingEvent = proxyType.GetEvent ("ExistingEvent");
            var addMethod = proxyType.GetOrAddOverride (existingEvent.GetAddMethod());
            var removeMethod = proxyType.GetOrAddOverride (existingEvent.GetRemoveMethod());
            Assert.That (existingEvent.GetRaiseMethod (true), Is.Null);
            var raiseMethod = proxyType.GetOrAddOverride (dummyRaiseMethod);

            proxyType.AddEvent (existingEvent.Name, addMethod: addMethod, removeMethod: removeMethod, raiseMethod: raiseMethod);
          });

      var newEvent = type.GetEvent ("ExistingEvent", BindingFlags.Public | BindingFlags.Instance);
      Assertion.IsNotNull (newEvent);
      var instance = (DomainType) Activator.CreateInstance (type);

      Assert.That (instance.AddCalled, Is.False);
      Assert.That (instance.RemoveCalled, Is.False);
      newEvent.AddEventHandler (instance, null);
      Assert.That (instance.AddCalled, Is.True);
      newEvent.RemoveEventHandler (instance, null);
      Assert.That (instance.RemoveCalled, Is.True);

      var newRaiseMethod = type.GetMethod ("DummyRaiseMethod");
      Assert.That (newEvent.GetRaiseMethod(), Is.SameAs (newRaiseMethod));
    }

    [Test]
    public void Redeclare_AddCustomAttribute ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var existingEvent = proxyType.GetEvent ("ExistingEvent");
            var addMethod = proxyType.GetOrAddOverride (existingEvent.GetAddMethod (true));
            var removeMethod = proxyType.GetOrAddOverride (existingEvent.GetRemoveMethod (true));
            var event_ = proxyType.AddEvent (existingEvent.Name, addMethod: addMethod, removeMethod: removeMethod);

            var attributeCtor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new AbcAttribute (""));
            var customAttributes = new CustomAttributeDeclaration (attributeCtor, new object[] { "derived" });
            event_.AddCustomAttribute (customAttributes);
          });

      var newEvent = type.GetEvent ("ExistingEvent", BindingFlags.Public | BindingFlags.Instance);
      Assertion.IsNotNull (newEvent);

      var attributesArgs = Attribute.GetCustomAttributes (newEvent, inherit: true).Cast<AbcAttribute>().Select (a => a.Arg);
      Assert.That (attributesArgs, Is.EquivalentTo (new[] { "base", "derived" }));
    }

    [Test]
    public void Static ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var staticField = NormalizingMemberInfoFromExpressionUtility.GetField (() => DomainType.StaticDelegateField);
            var accessorAttributes = MethodAttributes.Private | MethodAttributes.Static;
            var method = proxyType.AddMethod (
                "StaticMethod",
                accessorAttributes,
                typeof (void),
                new[] { new ParameterDeclaration (typeof (Action), "handler") },
                ctx => Expression.Assign (Expression.Field (null, staticField), ctx.Parameters.Single()));
            proxyType.AddEvent ("StaticEvent", EventAttributes.SpecialName, addMethod: method, removeMethod: method);
          });

      var event_ = type.GetEvent ("StaticEvent", BindingFlags.NonPublic | BindingFlags.Static);
      Assertion.IsNotNull (event_);
      Assert.That (event_.Attributes, Is.EqualTo (EventAttributes.SpecialName));

      Assert.That (DomainType.StaticDelegateField, Is.Null);
      var handler = new Action (() => { });
      // event_.AddEventHandler(null, handler) // Does not work because add method is private.
      event_.GetAddMethod (true).Invoke (null, new object[] { handler });
      Assert.That (DomainType.StaticDelegateField, Is.SameAs (handler));
    }

    public class DomainType
    {
      public static Delegate StaticDelegateField;

      public bool AddCalled;
      public bool RemoveCalled;

      [Abc ("base")]
      public virtual event Func<int, string> ExistingEvent
      {
        add { Dev.Null = value; AddCalled = true; }
        remove { Dev.Null = value; RemoveCalled = true; }
      }
      public virtual string DummyRaiseMethod (int arg) { Dev.Null = arg; return ""; }
    }

    [AttributeUsage (AttributeTargets.Event, Inherited = true, AllowMultiple = true)]
    public class AbcAttribute : Attribute
    {
      public AbcAttribute (string arg) { Arg = arg; }
      public string Arg { get; set; }
    }
  }
}