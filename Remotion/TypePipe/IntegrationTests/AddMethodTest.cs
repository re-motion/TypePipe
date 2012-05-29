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

namespace TypePipe.IntegrationTests
{
  [TestFixture]
  public class AddMethodTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void StaticMethodWithOutParameter ()
    {
      var name = "PublicStaticMethodWithOutParameter";
      var type = AssembleType<DomainType> (
          mutableType => mutableType.AddMethod (
              name,
              MethodAttributes.Public | MethodAttributes.Static,
              typeof (void),
              new[] { new ParameterDeclaration (typeof (int).MakeByRefType(), "parameterName", ParameterAttributes.Out) },
              ctx => Expression.Assign (ctx.Parameters[0], Expression.Constant (7))));

      var addedMethod = type.GetMethod (name);

      Assert.That (addedMethod.Name, Is.EqualTo (name));
      Assert.That (addedMethod.Attributes, Is.EqualTo (MethodAttributes.Public | MethodAttributes.Static));
      Assert.That (addedMethod.ReturnType, Is.SameAs (typeof (void)));

      var singleParameter = addedMethod.GetParameters().Single();
      Assert.That (singleParameter.ParameterType, Is.SameAs (typeof (int).MakeByRefType()));
      Assert.That (singleParameter.Name, Is.EqualTo("parameterName"));
      Assert.That (singleParameter.Attributes, Is.EqualTo(ParameterAttributes.Out));

      var arguments = new object[1];
      addedMethod.Invoke (null, arguments);

      Assert.That (arguments[0], Is.EqualTo (7));
    }

    [Test]
    public void StaticMethodCannotUseThisExpression ()
    {
      var type = AssembleType<DomainType> (
          mutableType => mutableType.AddMethod (
              "StaticMethod",
              MethodAttributes.Public | MethodAttributes.Static,
              typeof (void),
              ParameterDeclaration.EmptyParameters,
              ctx =>
              {
                Assert.That (() => ctx.This, Throws.InvalidOperationException.With.Message.EqualTo ("Static methods cannot use 'This'."));
                return Expression.Empty();
              }));

      var addedMethod = type.GetMethod ("StaticMethod");

      Assert.That (() => addedMethod.Invoke (null, null), Throws.Nothing);
    }

    [Test]
    public void InstanceMethodWithInParameter ()
    {
      var name = "InstanceMethod";
      var type = AssembleType<DomainType> (
          mutableType => mutableType.AddMethod (
              name,
              MethodAttributes.Public,
              typeof (void),
              new[] { new ParameterDeclaration (typeof (string), "parameterName") },
              // TODO 4744: Use Expression.Property (ctx.This, "SettableProperty")
              ctx => Expression.Assign (Expression.Property (ctx.This, typeof (DomainType).GetProperty ("SettableProperty")), ctx.Parameters[0])));

      var addedMethod = type.GetMethod (name);

      Assert.That (addedMethod.Name, Is.EqualTo (name));
      Assert.That (addedMethod.Attributes, Is.EqualTo (MethodAttributes.Public));
      Assert.That (addedMethod.ReturnType, Is.SameAs (typeof (void)));

      var singleParameter = addedMethod.GetParameters ().Single ();
      Assert.That (singleParameter.ParameterType, Is.SameAs (typeof (string)));
      Assert.That (singleParameter.Name, Is.EqualTo ("parameterName"));
      Assert.That (singleParameter.Attributes, Is.EqualTo (ParameterAttributes.In));

      var instance = (DomainType) Activator.CreateInstance (type);
      var arguments = new object[] { "test string" };
      addedMethod.Invoke (instance, arguments);

      Assert.That (instance.SettableProperty, Is.EqualTo ("test string"));
    }

    [Test]
    public void MethodsWithReturnValue ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            mutableType.AddMethod (
                "MethodWithExactResultType",
                MethodAttributes.Public | MethodAttributes.Static,
                typeof (string),
                ParameterDeclaration.EmptyParameters,
                ctx => Expression.Constant ("return value"));

            mutableType.AddMethod (
                "MethodWithBoxingConvertibleResultType",
                MethodAttributes.Public | MethodAttributes.Static,
                typeof (object),
                ParameterDeclaration.EmptyParameters,
                ctx => Expression.Constant (7));

            mutableType.AddMethod (
                "MethodWithReferenceConvertibleResultType",
                MethodAttributes.Public | MethodAttributes.Static,
                typeof (object),
                ParameterDeclaration.EmptyParameters,
                ctx => Expression.Constant ("string"));
          });

      var result1 = type.GetMethod ("MethodWithExactResultType").Invoke (null, null);
      var result2 = type.GetMethod ("MethodWithBoxingConvertibleResultType").Invoke (null, null);
      var result3 = type.GetMethod ("MethodWithReferenceConvertibleResultType").Invoke (null, null);

      Assert.That (result1, Is.EqualTo ("return value"));
      Assert.That (result2, Is.EqualTo (7));
      Assert.That (result3, Is.EqualTo ("string"));
    }

    [Test]
    public void MethodsWithInvalidReturnValue ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var exceptionMessagePart = "Use Expression.Convert or Expression.ConvertChecked to make the conversion explicit.";
            CheckAddMethodThrows (
                mutableType,
                "MethodWithPotentiallyDangerousValueConversion",
                typeof (int),
                Expression.Constant (7L),
                "Type 'System.Int64' cannot be implicitly converted to type 'System.Int32'. " + exceptionMessagePart);

            CheckAddMethodThrows (
                mutableType,
                "MethodWithPotentiallyDangerousReferenceConversion",
                typeof (string),
                Expression.Constant (null, typeof (object)),
                "Type 'System.Object' cannot be implicitly converted to type 'System.String'. " + exceptionMessagePart);

            CheckAddMethodThrows (
                mutableType,
                "MethodWithInvalidResultType",
                typeof (int),
                Expression.Constant ("string"),
                "Type 'System.String' cannot be implicitly converted to type 'System.Int32'. " + exceptionMessagePart);
          });

      Assert.That (type.GetMethod ("MethodWithPotentiallyDangerousValueConversion"), Is.Null);
      Assert.That (type.GetMethod ("MethodWithPotentiallyDangerousReferenceConversion"), Is.Null);
      Assert.That (type.GetMethod ("MethodWithInvalidResultType"), Is.Null);
    }

    [Test]
    public void MethodUsingMutableMethodInBody ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var method1 = mutableType.AddMethod (
                "Method1",
                MethodAttributes.Private | MethodAttributes.Static,
                typeof (int),
                ParameterDeclaration.EmptyParameters,
                ctx => Expression.Constant (7));

            mutableType.AddMethod (
                "Method2",
                MethodAttributes.Public | MethodAttributes.Static,
                typeof (int),
                ParameterDeclaration.EmptyParameters,
                ctx => Expression.Increment (Expression.Call (method1)));
          });

      var addedMethod = type.GetMethod ("Method2");

      Assert.That (addedMethod.Invoke (null, null), Is.EqualTo (8));
    }

    [Test]
    public void MethodUsingExistingMembers ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var existingField = mutableType.ExistingMutableFields.Single (f => f.Name == "ExistingField");
            var existingMethod = mutableType.ExistingMutableMethods.Single (m => m.Name == "ExistingMethod");
            mutableType.AddMethod (
                "AddedMethod",
                MethodAttributes.Public,
                typeof (string),
                ParameterDeclaration.EmptyParameters,
                ctx => Expression.Block (
                    Expression.Assign (Expression.Field (ctx.This, existingField), Expression.Constant ("blah")),
                    Expression.Call (ctx.This, existingMethod)));
          });

      var addedMethod = type.GetMethod ("AddedMethod");
      var instance = Activator.CreateInstance (type);
      var result = addedMethod.Invoke (instance, null);

      Assert.That (result, Is.EqualTo ("blah"));
    }

    [Test]
    public void MethodsRequiringForwardDeclarations ()
    {
      // public static int Method1 (int i)
      // {
      //   if (i <= 0)
      //     return i;
      //   else
      //     return Method2 (i);
      // }
      // public static int Method2 (int i)
      // {
      //   return Method1 (i - 1);
      // }
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var method1 = mutableType.AddMethod (
                "Method1",
                MethodAttributes.Public | MethodAttributes.Static,
                typeof (int),
                new[] { new ParameterDeclaration (typeof (int), "i") },
                ctx => Expression.Throw (Expression.Constant (new NotImplementedException()), typeof (int)));

            var method2 = mutableType.AddMethod (
                "Method2",
                MethodAttributes.Private | MethodAttributes.Static,
                typeof (int),
                new[] { new ParameterDeclaration (typeof (int), "i") },
                ctx => Expression.Call (method1, Expression.Decrement (ctx.Parameters[0])));

            method1.SetBody (
                ctx => Expression.Condition (
                    Expression.LessThanOrEqual (ctx.Parameters[0], Expression.Constant (0)),
                    ctx.Parameters[0],
                    Expression.Call (method2, ctx.Parameters[0])));
          });

      var addedMethod = type.GetMethod ("Method1");

      Assert.That (addedMethod.Invoke (null, new object[] { 7 }), Is.EqualTo (0));
      Assert.That (addedMethod.Invoke (null, new object[] { -8 }), Is.EqualTo (-8));
    }

    private void CheckAddMethodThrows (MutableType mutableType, string name, Type returnType, Expression body, string exceptionMessage)
    {
      Assert.That (
          () => mutableType.AddMethod (name, MethodAttributes.Public, returnType, ParameterDeclaration.EmptyParameters, ctx => body),
          Throws.InvalidOperationException.With.Message.EqualTo (exceptionMessage));
    }

    public class DomainType
    {
      public string ExistingField;

      public string SettableProperty { get; set; }

      public string ExistingMethod ()
      {
        return ExistingField;
      }
    }
  }
}