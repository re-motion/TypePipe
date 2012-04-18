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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.MutableReflection;

namespace TypePipe.IntegrationTests
{
  [TestFixture]
  public class ModifyMethodBodyTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void ExistingPublicVirtualMethod_PreviousBodyWithModifiedArguments ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var mutableMethod = mutableType.GetMutableMethod (typeof (DomainType).GetMethod ("PublicVirtualMethod"));
            mutableMethod.SetBody (ctx => ctx.GetPreviousBody (Expression.Multiply (Expression.Constant (2), ctx.Parameters[0])));
          });

      var instance = (DomainType) Activator.CreateInstance (type);
      var result = instance.PublicVirtualMethod (7);

      Assert.That (result, Is.EqualTo ("14"));
    }

    [Test]
    public void MethodWithOutAndRefParameters ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var mutableMethod = mutableType.GetMutableMethod (typeof (DomainType).GetMethod ("MethodWithOutAndRefParameters"));
            mutableMethod.SetBody (
                ctx =>
                {
                  var tempLocal = Expression.Variable (typeof (int), "temp");
                  return Expression.Block (
                      new[] { tempLocal },
                      Expression.Assign (tempLocal, Expression.Multiply (ctx.Parameters[0], Expression.Constant (3))),
                      ctx.GetPreviousBody (tempLocal, ctx.Parameters[1]),
                      Expression.Assign (ctx.Parameters[1], ExpressionHelper.StringConcat (ctx.Parameters[1], Expression.Constant (" test"))),
                      Expression.Assign (ctx.Parameters[0], tempLocal));
                });
          });

      var instance = (DomainType) Activator.CreateInstance (type);
      string s;
      int i = 2;
      instance.MethodWithOutAndRefParameters (ref i, out s);

      Assert.That (i, Is.EqualTo (7));
      Assert.That (s, Is.EqualTo ("hello test"));
    }

    [Test]
    public void ExistingProtectedVirtualMethod_PreviousBody ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var method = typeof (DomainType).GetMethod ("ProtectedVirtualMethod", BindingFlags.NonPublic | BindingFlags.Instance);
            var mutableMethod = mutableType.GetMutableMethod (method);
            mutableMethod.SetBody (
                ctx => ExpressionHelper.StringConcat (Expression.Constant ("hello "), Expression.Call (ctx.GetPreviousBody(), "ToString", null)));
          });

      var instance = (DomainType) Activator.CreateInstance (type);
      var result = PrivateInvoke.InvokeNonPublicMethod (instance, "ProtectedVirtualMethod", 7.1);

      Assert.That (result, Is.EqualTo ("hello 7.1"));
    }

    [Test]
    public void AddedMethodBody_PreviousBody ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          mutableType.AddMethod (
              "AddedMethod", MethodAttributes.Public, typeof (int), ParameterDeclaration.EmptyParameters, ctx => Expression.Constant (7)),
          mutableType =>
          {
            var mutableMethod = mutableType.AddedMethods.Single ();
            Assert.That (mutableMethod.IsVirtual, Is.False);
            mutableMethod.SetBody (ctx => Expression.Add (ctx.GetPreviousBody (), Expression.Constant (1)));
          });

      var method = type.GetMethod ("AddedMethod");
      var instance = (DomainType) Activator.CreateInstance (type);
      var result = method.Invoke (instance, null);

      Assert.That (result, Is.EqualTo (8));
    }

    [Test]
    public void CallsToOriginalMethod_InvokeNewBody ()
    {
      var type = AssembleType<DomainType> (
        mutableType =>
        {
          var mutableMethod = mutableType.GetMutableMethod (typeof (DomainType).GetMethod ("PublicVirtualMethod"));
          mutableMethod.SetBody (ctx => ctx.GetPreviousBody (Expression.Multiply (Expression.Constant (2), ctx.Parameters[0])));
        });

      var instance = (DomainType) Activator.CreateInstance (type);
      var result = instance.CallsOriginalMethod (7);

      Assert.That (result, Is.EqualTo ("-14"));
    }

    [Test]
    public void ModifyingNonVirtualAndStaticMethods_Throws ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var nonVirtualMethod = mutableType.GetMutableMethod (typeof (DomainType).GetMethod ("PublicMethod"));
            Assert.That (
                () => nonVirtualMethod.SetBody (ctx => Expression.Constant (7)),
                Throws.TypeOf<NotSupportedException>().With.Message.EqualTo (
                    "The body of the existing non-virtual method 'PublicMethod' cannot be replaced."));

            var staticMethod = mutableType.GetMutableMethod (typeof (DomainType).GetMethod ("PublicStaticMethod"));
            Assert.That (
                () => staticMethod.SetBody (
                    ctx =>
                    {
                      Assert.That (ctx.IsStatic, Is.True);
                      return Expression.Constant (8);
                    }),
                Throws.TypeOf<NotSupportedException>().With.Message.EqualTo (
                    "The body of the existing non-virtual method 'PublicStaticMethod' cannot be replaced."));
          });

      var instance = (DomainType) Activator.CreateInstance (type);
      Assert.That (instance.PublicMethod(), Is.EqualTo (12));

      var method = type.GetMethod ("PublicStaticMethod", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
      Assert.That (method.Invoke (null, null), Is.EqualTo (13));
    }

    [Test]
    public void ModifyingNonVirtualAddedMethod ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var nonVirtualAttributes = MethodAttributes.Public;
            mutableType.AddMethod ("Method", nonVirtualAttributes, typeof (int), ParameterDeclaration.EmptyParameters, ctx => Expression.Constant (7));
          },
          mutableType =>
          {
            var addedMethod = mutableType.AddedMethods.Single();
            addedMethod.SetBody (ctx => Expression.Constant (8));
          });

      var instance = (DomainType) Activator.CreateInstance (type);
      var method = type.GetMethod ("Method");
      Assert.That (method.Invoke (instance, null), Is.EqualTo (8));
    }

    [Test]
    public void ChainPreviousBodyInvocations ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var mutableMethod = mutableType.GetMutableMethod (typeof (DomainType).GetMethod ("PublicVirtualMethod"));
            mutableMethod.SetBody (ctx => ctx.GetPreviousBody (Expression.Multiply (Expression.Constant (2), ctx.Parameters[0])));
          },
          mutableType =>
          {
            var mutableMethod = mutableType.GetMutableMethod (typeof (DomainType).GetMethod ("PublicVirtualMethod"));
            mutableMethod.SetBody (ctx => ctx.GetPreviousBody (Expression.Add (Expression.Constant (2), ctx.Parameters[0])));
          });

      var instance = (DomainType) Activator.CreateInstance (type);
      var result = instance.PublicVirtualMethod (3); // (3 + 2) * 2 != (2 * 2) + 3

      Assert.That (result, Is.EqualTo ("10"));
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

            var modifiedMethod = mutableType.GetMutableMethod (originalMethod);
            modifiedMethod.SetBody (ctx => ExpressionHelper.StringConcat(Expression.Constant ("hello "), Expression.Call (ctx.GetPreviousBody (), "ToString", null)));
          });

      var method = type.GetMethod ("AddedMethod");
      var instance = (DomainType) Activator.CreateInstance (type);
      var result = method.Invoke (instance, null);

      Assert.That (result, Is.EqualTo ("hello 7"));
    }

    [Test]
    [Ignore ("TODO 4789")]
    public void ExistingMethodWithGenericParameters ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var mutableMethod = mutableType.ExistingMethods.Single (m => m.Name == "GenericMethod");
            Assert.That (mutableMethod.IsGenericMethod, Is.True);
            Assert.That (mutableMethod.IsGenericMethodDefinition, Is.True);
            var genericParameters = mutableMethod.GetGenericArguments();
            var genericParameterNames = genericParameters.Select (t => t.Name);
            Assert.That (genericParameterNames, Is.EqualTo (new[] { "TKey", "TValue" }));

            mutableMethod.SetBody (
                ctx =>
                {
                  Assert.That (ctx.Parameters[0].Type, Is.SameAs (typeof (IDictionary<,>).MakeGenericType (genericParameters)));
                  var containsKeyMethod = ctx.Parameters[0].Type.GetMethod ("ContainsKey");
                  return Expression.IfThenElse (
                      Expression.Call (ctx.Parameters[0], containsKeyMethod, ctx.Parameters[1]),
                      ctx.GetPreviousBody(),
                      Expression.Default (ctx.Parameters[1].Type));
                });
          });

      var method = type.GetMethod ("GenericMethod").MakeGenericMethod (typeof (int), typeof (string));
      var instance = (DomainType) Activator.CreateInstance (type);

      var dict = new Dictionary<int, string> { { 7, "seven" } };
      var result1 = method.Invoke (instance, new object[] { dict, 7 });
      var result2 = method.Invoke (instance, new object[] { dict, 8 });

      Assert.That (result1, Is.EqualTo ("seven"));
      Assert.That (result2, Is.EqualTo (null));
    }

    public class DomainType
    {
      public virtual string PublicVirtualMethod(int i)
      {
        return i.ToString();
      }

      protected virtual string ProtectedVirtualMethod (double d)
      {
        return d.ToString (CultureInfo.InvariantCulture);
      }

      public string CallsOriginalMethod (int i)
      {
        return PublicVirtualMethod (-i);
      }

      public int PublicMethod () { return 12; }
      public static int PublicStaticMethod () { return 13; }

      public virtual void MethodWithOutAndRefParameters (ref int i, out string s)
      {
        i++;
        s = "hello";
      }

      public virtual TValue GenericMethod<TKey, TValue> (IDictionary<TKey, TValue> dict, TKey key)
          where TKey : IComparable<TKey>
          where TValue : class
      {
        return dict[key];
      }
    }
  }
}