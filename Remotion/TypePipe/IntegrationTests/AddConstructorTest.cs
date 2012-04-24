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
using System.Linq;

namespace TypePipe.IntegrationTests
{
  [TestFixture]
  public class AddConstructorTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void Standard ()
    {
      var type = AssembleType<DomainType> (
          mutableType => mutableType.AddConstructor (
              MethodAttributes.Public, 
              new[] { new ParameterDeclaration (typeof (int), "i") }, 
              context => Expression.Block (
                  context.GetConstructorCall (Expression.Call (context.Parameters[0], "ToString", Type.EmptyTypes)),
                  Expression.Assign (Expression.Field (context.This, "_addedConstructorInitializedValue"), Expression.Constant ("hello")))));

      var addedCtor = type.GetConstructor (new[] { typeof (int) });
      Assert.That (addedCtor, Is.Not.Null);
      Assert.That (
          addedCtor.Attributes,
          Is.EqualTo (MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName));

      var arguments = new object[] { 7 };
      var instance = (DomainType) addedCtor.Invoke (arguments);

      Assert.That (instance.ConstructorInitializedValue, Is.EqualTo ("7"));
      Assert.That (instance.AddedConstructorInitializedValue, Is.EqualTo ("hello"));
    }

    [Test]
    public void ParameterMetadataAndOutParameter ()
    {
      var type = AssembleType<DomainType> (
          mutableType => mutableType.AddConstructor (
              MethodAttributes.Public,
              new[] { new ParameterDeclaration (typeof (int), "i"), new ParameterDeclaration (typeof (string).MakeByRefType (), "s", ParameterAttributes.Out) },
              context =>
              {
                var toStringResultLocal = Expression.Variable (typeof (string), "toStringResult");
                return Expression.Block (
                    new[] { toStringResultLocal },
                    Expression.Assign (toStringResultLocal, Expression.Call (context.Parameters[0], "ToString", Type.EmptyTypes)),
                    context.GetConstructorCall (toStringResultLocal),
                    Expression.Assign (context.Parameters[1], toStringResultLocal));
              }));

      var addedCtor = type.GetConstructor (new[] { typeof (int), typeof (string).MakeByRefType () });
      Assert.That (addedCtor, Is.Not.Null);

      var actualParameterData = addedCtor.GetParameters ().Select (pi => new { pi.Name, pi.ParameterType, pi.Attributes });
      var expectedParameterData =
          new[]
          {
              new { Name = "i", ParameterType = typeof (int), Attributes = ParameterAttributes.In },
              new { Name = "s", ParameterType = typeof (string).MakeByRefType(), Attributes = ParameterAttributes.Out }
          };
      Assert.That (actualParameterData, Is.EqualTo (expectedParameterData));

      var arguments = new object[] { 7, null };
      addedCtor.Invoke (arguments);

      Assert.That (arguments[1], Is.EqualTo ("7"));
    }

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