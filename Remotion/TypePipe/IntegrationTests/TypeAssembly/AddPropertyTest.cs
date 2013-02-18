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
using JetBrains.Annotations;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [Ignore ("TODO 5423")]
  [TestFixture]
  public class AddPropertyTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void ReadWrite_WithNewField ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var field = proxyType.AddField ("_field", typeof (string));
            proxyType.AddProperty (
                "Property",
                typeof (string),
                getBodyProvider: ctx => Expression.Field (ctx.This, field),
                setBodyProvider: ctx => Expression.Assign (Expression.Field (ctx.This, field), ctx.Parameters[0]));
          });

      var property = type.GetProperty ("Property");
      var backingField = type.GetField ("_field", BindingFlags.NonPublic | BindingFlags.Instance);
      Assertion.IsNotNull (backingField);
      var instance = Activator.CreateInstance (type);

      Assert.That (backingField.GetValue (instance), Is.Null);
      Assert.That (property.GetValue (instance, null), Is.Null);
      property.SetValue (instance, "Test", null);
      Assert.That (backingField.GetValue (instance), Is.EqualTo ("Test"));
      Assert.That (property.GetValue (instance, null), Is.EqualTo ("Test"));
    }

    [Test]
    public void ReadOnly_WirteOnly ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            proxyType.AddProperty ("ReadOnly", typeof (int), getBodyProvider: ctx => Expression.Default (typeof (int)));
            proxyType.AddProperty ("WriteOnly", typeof (int), setBodyProvider: ctx => Expression.Empty());
          });

      var readOnlyProperty = type.GetProperty ("ReadOnly");
      var writeOnlyProperty = type.GetProperty ("WriteOnly");

      Assert.That (readOnlyProperty.CanWrite, Is.False);
      Assert.That (readOnlyProperty.GetSetMethod (true), Is.Null);
      Assert.That (writeOnlyProperty.CanRead, Is.False);
      Assert.That (writeOnlyProperty.GetGetMethod (true), Is.Null);
    }

    [Test]
    public void IndexParameters ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var indexedSetterField = proxyType.GetField ("IndexedSetterField");
            proxyType.AddProperty (
                "Property",
                typeof (string),
                new[] { new ParameterDeclaration (typeof (string), "index0"), new ParameterDeclaration (typeof (string), "index1") },
                ctx =>
                {
                  Assert.That (ctx.ReturnType, Is.SameAs (typeof (string)));
                  Assert.That (ctx.Parameters.Count, Is.EqualTo (2));
                  return ExpressionHelper.StringConcat (ctx.Parameters[0], ctx.Parameters[1]);
                },
                ctx =>
                {
                  Assert.That (ctx.ReturnType, Is.SameAs (typeof (void)));
                  Assert.That (ctx.Parameters.Count, Is.EqualTo (3));
                  var value = ExpressionHelper.StringConcat (ExpressionHelper.StringConcat (ctx.Parameters[0], ctx.Parameters[1]), ctx.Parameters[2]);
                  return Expression.Assign (Expression.Field (ctx.This, indexedSetterField), value);
                });
          });

      var property = type.GetProperty ("Property");
      var instance = (DomainType) Activator.CreateInstance (type);

      Assert.That (property.GetValue (instance, new object[] { "a ", "b" }), Is.EqualTo ("a b"));
      property.SetValue (instance, "value", new object[] { "a ", "b " });
      Assert.That (instance.IndexedSetterField, Is.EqualTo ("a b value"));
    }

    [Test]
    public void RedeclareExisting ()
    {
      
    }

    public class DomainType
    {
      [UsedImplicitly] public string IndexedSetterField;
    }
  }
}