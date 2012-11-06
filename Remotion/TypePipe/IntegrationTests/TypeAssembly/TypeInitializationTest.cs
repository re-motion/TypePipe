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
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class TypeInitializationTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void Standard ()
    {
      var field = NormalizingMemberInfoFromExpressionUtility.GetField (() => DomainType.StaticField);

      var type = AssembleType<DomainType> (
          mutableType =>
          {
            Assert.That (mutableType.TypeInitializations, Is.Empty);

            var initializationExpression = Expression.Assign (Expression.Field (null, field), Expression.Constant ("abc"));
            mutableType.AddTypeInitialization (initializationExpression);

            Assert.That (mutableType.TypeInitializations, Is.EqualTo (new[] { initializationExpression }));
          });

      Assert.That (DomainType.StaticField, Is.Null);
      RuntimeHelpers.RunClassConstructor (type.TypeHandle);
      Assert.That (DomainType.StaticField, Is.EqualTo ("abc"));
    }

    [Test]
    public void TypeInitializer ()
    {
      AssembleType<DomainType> (
          mutableType =>
          {
            var message = "Type initializers (static constructors) cannot be modified via this API, use MutableType.AddTypeInitialization instead.";
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

            Assert.That (() => mutableType.TypeInitializer, Throws.TypeOf<NotSupportedException>().With.Message.EqualTo (message));
            Assert.That (() => mutableType.GetConstructors (bindingFlags), Throws.TypeOf<NotSupportedException>().With.Message.EqualTo (message));
            Assert.That (
                () => mutableType.GetConstructor (bindingFlags, null, Type.EmptyTypes, null),
                Throws.TypeOf<NotSupportedException>().With.Message.EqualTo (message));
            Assert.That (
                () => mutableType.AddConstructor (MethodAttributes.Static, ParameterDeclaration.EmptyParameters, ctx => null),
                Throws.TypeOf<NotSupportedException>().With.Message.EqualTo (
                    "Type initializers (static constructors) cannot be added via this API, use MutableType.AddTypeInitialization instead."));
          });
    }

    public class DomainType
    {
      public static string StaticField;
    }

    public class TypeWithInitializer
    {
      static TypeWithInitializer () { }
    }
  }
}