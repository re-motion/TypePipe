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
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class GenerationCompletedEventTest : TypeAssemblerIntegrationTestBase
  {
    [Ignore ("TODO 5482")]
    [Test]
    public void GetGeneratedMember_Types ()
    {
      Type generatedProxyType = null;

      var type = AssembleType<DomainType> (
          typeContext =>
          {
            var additionalType = typeContext.CreateType ("MyType", "MyNamespace", TypeAttributes.Public, typeof (object));

            typeContext.GenerationCompleted += ctx =>
            {
              generatedProxyType = (Type) ctx.GetGeneratedMember (typeContext.ProxyType);

              var generatedAdditionalType = (Type) ctx.GetGeneratedMember (additionalType);
              Assert.That (generatedAdditionalType.Name, Is.EqualTo ("MyType"));
              Assert.That (generatedAdditionalType.Namespace, Is.EqualTo ("MyNamespace"));
              Assert.That (generatedAdditionalType.Attributes, Is.EqualTo (TypeAttributes.Public));
              Assert.That (generatedAdditionalType.BaseType, Is.SameAs (typeof (object)));
            };
          });

      Assert.That (type, Is.SameAs (generatedProxyType));
    }

    [Ignore ("TODO 5482")]
    [Test]
    public void GetGeneratedMember_ClassMembers ()
    {
      MethodInfo method = null;
      FieldInfo field = null;
      PropertyInfo property = null;
      EventInfo event_ = null;

      var type = AssembleType<DomainType> (
          typeContext =>
          {
            var proxyType = typeContext.ProxyType;

            var addedField = proxyType.AddField ("MyField", FieldAttributes.Public, typeof (int));
            var addedMethod = proxyType.AddMethod (
                "MyMethod", MethodAttributes.Public, typeof (void), ParameterDeclaration.None, ctx => Expression.Empty());
            var addedProperty = proxyType.AddProperty (
                "MyProperty", typeof (string), ParameterDeclaration.None, MethodAttributes.Public, null, ctx => Expression.Empty());
            var addedEvent = proxyType.AddEvent (
                "MyEvent", typeof (Action), MethodAttributes.Public, ctx => Expression.Empty(), ctx => Expression.Empty());

            typeContext.GenerationCompleted +=
                ctx =>
                {
                  method = (MethodInfo) ctx.GetGeneratedMember (addedMethod);
                  field = (FieldInfo) ctx.GetGeneratedMember (addedField);
                  event_ = (EventInfo) ctx.GetGeneratedMember (addedEvent);
                  property = (PropertyInfo) ctx.GetGeneratedMember (addedProperty);
                };
          });

      Assert.That (field, Is.SameAs (type.GetFields().Single()));
      Assert.That (field.Name, Is.EqualTo ("MyField"));
      Assert.That (field.FieldType, Is.SameAs (typeof (int)));

      Assert.That (method, Is.SameAs (type.GetMethods().Single (m => m.Name == "MyMethod")));
      Assert.That (method.ReturnType, Is.SameAs (typeof (void)));
      Assert.That (method.Attributes, Is.EqualTo (MethodAttributes.Public));
      Assert.That (method.GetParameters(), Is.Empty);

      Assert.That (property, Is.SameAs (type.GetProperties().Single()));
      Assert.That (property.Name, Is.EqualTo ("MyProperty"));
      Assert.That (property.PropertyType, Is.SameAs (typeof (string)));
      Assert.That (property.GetIndexParameters(), Is.Empty);

      Assert.That (event_, Is.SameAs (type.GetEvents().Single()));
      Assert.That (event_.Name, Is.EqualTo ("MyEvent"));
      Assert.That (event_.EventHandlerType, Is.SameAs (typeof (Action)));
    }

    public class DomainType {}
  }
}