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
using JetBrains.Annotations;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class AddMethodTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void StaticMethodWithOutParameter ()
    {
      var name = "PublicStaticMethodWithOutParameter";
      var type = AssembleType<DomainType> (
          proxyType => proxyType.AddMethod (
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
    public void StaticMethodCannotUseThis ()
    {
      var type = AssembleType<DomainType> (
          proxyType => proxyType.AddMethod (
              "StaticMethod",
              MethodAttributes.Public | MethodAttributes.Static,
              typeof (void),
              ParameterDeclaration.None,
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
          proxyType => proxyType.AddMethod (
              name,
              MethodAttributes.Public,
              typeof (void),
              new[] { new ParameterDeclaration (typeof (string), "parameterName") },
              ctx => Expression.Assign (Expression.Property (ctx.This, "SettableProperty"), ctx.Parameters[0])));

      var addedMethod = type.GetMethod (name);

      Assert.That (addedMethod.Name, Is.EqualTo (name));
      Assert.That (addedMethod.Attributes, Is.EqualTo (MethodAttributes.Public));
      Assert.That (addedMethod.ReturnType, Is.SameAs (typeof (void)));

      var singleParameter = addedMethod.GetParameters().Single();
      Assert.That (singleParameter.ParameterType, Is.SameAs (typeof (string)));
      Assert.That (singleParameter.Name, Is.EqualTo ("parameterName"));
      Assert.That (singleParameter.Attributes, Is.EqualTo (ParameterAttributes.None));

      var instance = (DomainType) Activator.CreateInstance (type);
      var arguments = new object[] { "test string" };
      addedMethod.Invoke (instance, arguments);

      Assert.That (instance.SettableProperty, Is.EqualTo ("test string"));
    }

    [Test]
    public void MethodWithUnnamedParameters ()
    {
      var type = AssembleType<DomainType> (
          p =>
          {
            var method = p.AddMethod ("Method", parameters: new[] { new ParameterDeclaration (typeof (int)) }, bodyProvider: ctx => Expression.Empty());
            Assert.That (method.MutableParameters.Single().Name, Is.Null);
          });

      var parameter = type.GetMethod ("Method").GetParameters().Single();
      Assert.That (parameter.Name, Is.Empty);
    }

    [Test]
    public void MethodsWithReturnValue ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            proxyType.AddMethod (
                "MethodWithExactResultType",
                MethodAttributes.Public | MethodAttributes.Static,
                typeof (string),
                ParameterDeclaration.None,
                ctx => Expression.Constant ("return value"));

            proxyType.AddMethod (
                "MethodWithBoxingConvertibleResultType",
                MethodAttributes.Public | MethodAttributes.Static,
                typeof (object),
                ParameterDeclaration.None,
                ctx => Expression.Constant (7));

            proxyType.AddMethod (
                "MethodWithReferenceConvertibleResultType",
                MethodAttributes.Public | MethodAttributes.Static,
                typeof (object),
                ParameterDeclaration.None,
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
          proxyType =>
          {
            var exceptionMessagePart = "Use Expression.Convert or Expression.ConvertChecked to make the conversion explicit.";
            CheckAddMethodThrows (
                proxyType,
                "MethodWithPotentiallyDangerousValueConversion",
                typeof (int),
                Expression.Constant (7L),
                "Type 'System.Int64' cannot be implicitly converted to type 'System.Int32'. " + exceptionMessagePart);

            CheckAddMethodThrows (
                proxyType,
                "MethodWithPotentiallyDangerousReferenceConversion",
                typeof (string),
                Expression.Constant (null, typeof (object)),
                "Type 'System.Object' cannot be implicitly converted to type 'System.String'. " + exceptionMessagePart);

            CheckAddMethodThrows (
                proxyType,
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
    public void MethodUsingAddedMethodInBody ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var method1 = proxyType.AddMethod (
                "Method1",
                MethodAttributes.Private | MethodAttributes.Static,
                typeof (int),
                ParameterDeclaration.None,
                ctx => Expression.Constant (7));

            proxyType.AddMethod (
                "Method2",
                MethodAttributes.Public | MethodAttributes.Static,
                typeof (int),
                ParameterDeclaration.None,
                ctx => Expression.Increment (Expression.Call (method1)));
          });

      var addedMethod = type.GetMethod ("Method2");

      Assert.That (addedMethod.Invoke (null, null), Is.EqualTo (8));
    }

    [Test]
    public void MethodUsingBaseMembers ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var baseField = NormalizingMemberInfoFromExpressionUtility.GetField ((DomainType obj) => obj.ExistingField);
            var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.ExistingMethod());
            proxyType.AddMethod (
                "AddedMethod",
                MethodAttributes.Public,
                typeof (string),
                ParameterDeclaration.None,
                ctx => Expression.Block (
                    Expression.Assign (Expression.Field (ctx.This, baseField), Expression.Constant ("blah")),
                    Expression.Call (ctx.This, baseMethod)));
          });

      var addedMethod = type.GetMethod ("AddedMethod");
      var instance = Activator.CreateInstance (type);
      var result = addedMethod.Invoke (instance, null);

      Assert.That (result, Is.EqualTo ("blah"));
    }

    [Test]
    public void MethodUsingBaseMethodWithOutAndRefParameters ()
    {
      var type = AssembleType<DomainType> (
          p => p.AddMethod (
              "AddedMethod",
              MethodAttributes.Public,
              typeof (void),
              new[]
              { new ParameterDeclaration (typeof (int).MakeByRefType(), "i"), new ParameterDeclaration (typeof (string).MakeByRefType(), "s") },
              ctx => Expression.Call (ctx.This, "MethodWithOutAndRefParameters", Type.EmptyTypes, ctx.Parameters.Cast<Expression>().ToArray())));

      var addedMethod = type.GetMethod ("AddedMethod");
      var instance = Activator.CreateInstance (type);
      var arguments = new object[] { 0, "hello" };

      addedMethod.Invoke (instance, arguments);

      Assert.That (arguments, Is.EqualTo (new object[] { 7, "hello abc" }));
    }

    [Test]
    public void MethodsRequiringForwardDeclarations ()
    {
      // public static int Method1 (int i) {
      //   if (i <= 0)
      //     return i;
      //   else
      //     return Method2 (i);
      // }
      // private static int Method2 (int i) {
      //   return Method1 (i - 1);
      // }
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var method1 = proxyType.AddMethod (
                "Method1",
                MethodAttributes.Public | MethodAttributes.Static,
                typeof (int),
                new[] { new ParameterDeclaration (typeof (int), "i") },
                ctx => Expression.Throw (Expression.Constant (new NotImplementedException()), typeof (int)));

            var method2 = proxyType.AddMethod (
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
          () => mutableType.AddMethod (name, MethodAttributes.Public, returnType, ParameterDeclaration.None, ctx => body),
          Throws.InvalidOperationException.With.Message.EqualTo (exceptionMessage));
    }

    public class DomainType
    {
      [UsedImplicitly] public string ExistingField;

      public string SettableProperty { get; set; }

      public string ExistingMethod ()
      {
        return ExistingField;
      }

      public void MethodWithOutAndRefParameters (out int i, ref string s)
      {
        i = 7;
        s += " abc";
      }
    }
  }
}