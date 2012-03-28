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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.TypePipe.MutableReflection;

namespace TypePipe.IntegrationTests
{
  [TestFixture]
  public class AddConstructorTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    [Ignore ("TODO 4686")]
    public void AddConstructor ()
    {
      var type = AssembleType<DomainType> (
          mutableType => mutableType.AddConstructor (
              MethodAttributes.Public, 
              new[] { new ParameterDeclaration (typeof (int), "i")}, 
              context => Expression.Block (
                 context.GetConstructorCallExpression (Expression.Call (context.ParameterExpressions[0], "ToString", Type.EmptyTypes)),
                 Expression.Assign (Expression.Field (context.ThisExpression, "_addedConstructorInitializedValue"), Expression.Constant ("hello"))
                 )));

      var addedCtor = type.GetConstructor (new[] { typeof (int) });
      Assert.That (addedCtor, Is.Not.Null);
      Assert.That (
          addedCtor.Attributes,
          Is.EqualTo (MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName));

      var instance = (DomainType) addedCtor.Invoke (new object[] { 7 });

      Assert.That (instance.ConstructorInitializedValue, Is.EqualTo ("7"));
      Assert.That (instance.AddedConstructorInitializedValue, Is.EqualTo ("hello"));
    }

    // TODO 4705: Add integration test proving that added ctor calls modified existing ctor on subclass proxy (not base ctor).

    public class DomainType
    {
      private readonly string _constructorInitializedValue;
      protected string _addedConstructorInitializedValue;

      public DomainType (string s)
      {
        _constructorInitializedValue = s;
      }

      public string ConstructorInitializedValue
      {
        get { return _constructorInitializedValue; }
      }

      public string AddedConstructorInitializedValue
      {
        get { return _addedConstructorInitializedValue; }
      }
    }
  }
}