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
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.MutableReflection;

namespace TypePipe.IntegrationTests
{
  [TestFixture]
  [Ignore ("TODO 4785")]
  public class ModifyMethodBodyTest : TypeAssemblerIntegrationTestBase
  {
    private static readonly MethodInfo s_stringConcatMethod = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });

    [Test]
    public void ExistingPublicVirtualMethodUsePreviousBodyWithModifiedArguments ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            //var mutableMethod = mutableType.GetMutableMethod (typeof (DomainType).GetMethod ("PublicVirtualMethod"));
            //mutableMethod.SetBody (ctx => ctx.GetPreviousBody (Expression.Multiply (Expression.Constant (2), ctx.Parameter[0])));
          });

      var instance = (DomainType) Activator.CreateInstance (type);
      var result = instance.PublicVirtualMethod (7);

      Assert.That (result, Is.EqualTo ("14"));
    }

    [Test]
    public void ExistingProtectedVirtualMethodUsePreviousBody ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            //var method = typeof (DomainType).GetMethod ("ProtectedVirtualMethod", BindingFlags.NonPublic | BindingFlags.Instance);
            //var mutableMethod = mutableType.GetMutableMethod (method);
            //mutableMethod.SetBody (
            //ctx => Expression.Add (
            //    Expression.Constant ("hello "),
            //    Expression.Call (ctx.GetPreviousBody(), "ToString"),
            //    s_stringConcatMethod);
          });

      var instance = (DomainType) Activator.CreateInstance (type);
      var result = PrivateInvoke.InvokeNonPublicMethod (instance, "ProtectedVirtualMethod", 7.1);

      Assert.That (result, Is.EqualTo ("hello 7.1"));
    }

    [Test]
    public void CallsToOriginalMethodInvokeNewBody ()
    {
      var type = AssembleType<DomainType> (
        mutableType =>
        {
          //var mutableMethod = mutableType.GetMutableMethod (typeof (DomainType).GetMethod ("PublicVirtualMethod"));
          //mutableMethod.SetBody (ctx => ctx.GetPreviousBody (Expression.Multiply (Expression.Constant (2), ctx.Parameter[0])));
        });

      var instance = (DomainType) Activator.CreateInstance (type);
      var result = instance.CallsOriginalMethod (7);

      Assert.That (result, Is.EqualTo ("-14"));
    }

    [Test]
    public void ModifyingNonVirtualAndStaticMethodsThrows ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            //var nonVirtualMethod = mutableType.GetMutableMethod (typeof (DomainType).GetMethod ("PublicMethod"));
            //Assert.That (
            //    () => nonVirtualMethod.SetBody (ctx => Expression.Constant (7)),
            //    Throws.InvalidOperationException.With.Message.EqualTo (
            //        "Non-virtual methods cannot be replaced with ReflectionEmit code generation strategy."));

            //var staticMethod = mutableType.GetMutableMethod (typeof (DomainType).GetMethod ("PublicStaticMethod"));
            //Assert.That (
            //    () => staticMethod.SetBody (
            //        ctx =>
            //        {
            //          Assert.That (ctx.IsStatic, Is.True);
            //          return Expression.Constant (8);
            //        },
            //        Throws.InvalidOperationException.With.Message.EqualTo (
            //            "Static methods cannot be replaced with ReflectionEmit code generation strategy.")));
          });

      var instance = (DomainType) Activator.CreateInstance (type);
      Assert.That (instance.PublicMethod (), Is.EqualTo (12));

      var method = type.GetMethod ("PublicStaticMethod");
      Assert.That (method.Invoke(null, null), Is.EqualTo (13));
    }

    [Test]
    public void ChainPreviousBodyInvocations ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            //var mutableMethod = mutableType.GetMutableMethod (typeof (DomainType).GetMethod ("PublicVirtualMethod"));
            //mutableMethod.SetBody (ctx => ctx.GetPreviousBody (Expression.Multiply (Expression.Constant (2), ctx.Parameter[0])));
          },
          mutableType =>
          {
            //var mutableMethod = mutableType.GetMutableMethod (typeof (DomainType).GetMethod ("PublicVirtualMethod"));
            //mutableMethod.SetBody (ctx => ctx.GetPreviousBody (Expression.Add (Expression.Constant (2), ctx.Parameter[0])));
          });

      var instance = (DomainType) Activator.CreateInstance (type);
      var result = instance.PublicVirtualMethod (3); // (3 + 2) * 2 != (2 * 2) + 3

      Assert.That (result, Is.EqualTo ("10"));
    }

    [Test]
    public void AddedMethodBodyUsePreviousBody ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          mutableType.AddMethod (
              "AddedMethod", MethodAttributes.Public, typeof (int), ParameterDeclaration.EmptyParameters, ctx => Expression.Constant (7)),
          mutableType =>
          {
            var mutableMethod = mutableType.AddedMethods.Single();
            Assert.That (mutableMethod.IsVirtual, Is.False);
            //mutableMethod.SetBody (ctx => Expression.Add (ctx.GetPreviousBody(), Expression.Constant (1)));
          });

      var method = type.GetMethod ("AddedMethod");
      var instance = (DomainType) Activator.CreateInstance (type);
      var result = method.Invoke (instance, null);

      Assert.That (result, Is.EqualTo (8));
    }

    [Test]
    public void AddedMethodCallsModfiedMethod ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var originalMethod = typeof (DomainType).GetMethod ("PublicVirtualMethod");
            mutableType.AddMethod (
                "AddedMethod",
                MethodAttributes.Public,
                typeof (string),
                ParameterDeclaration.EmptyParameters,
                ctx => Expression.Call (ctx.This, originalMethod, Expression.Constant(7)));

            //var modifiedMethod = mutableType.GetMutableMethod (originalMethod);
            //modifiedMethod.SetBody (
            //    ctx => Expression.Add (
            //        Expression.Constant ("hello "),
            //        Expression.Call (ctx.GetPreviousBody(), "ToString"),
            //        s_stringConcatMethod));
          });

      var method = type.GetMethod ("AddedMethod");
      var instance = (DomainType) Activator.CreateInstance (type);
      var result = method.Invoke (instance, null);

      Assert.That (result, Is.EqualTo ("hello 7"));
    }

    public class DomainType
    {
      public virtual string PublicVirtualMethod(int i)
      {
        return i.ToString();
      }

      protected virtual string ProtectedVirtualMethod (double d)
      {
        return d.ToString();
      }

      public string CallsOriginalMethod (int i)
      {
        return PublicVirtualMethod (-i);
      }

      public int PublicMethod () { return 12; }
      public static int PublicStaticMethod () { return 13; }

    }
  }
}