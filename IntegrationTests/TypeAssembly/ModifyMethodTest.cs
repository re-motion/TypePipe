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
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class ModifyMethodTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void ExistingPublicVirtualMethod_PreviousBodyWithModifiedArguments ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var mutableMethod = proxyType.GetOrAddOverride (typeof (DomainType).GetMethod ("PublicVirtualMethod"));
            mutableMethod.SetBody (ctx => ctx.InvokePreviousBodyWithArguments (Expression.Multiply (Expression.Constant (2), ctx.Parameters[0])));
          });

      var instance = (DomainType) Activator.CreateInstance (type);
      var result = instance.PublicVirtualMethod (7);

      Assert.That (result, Is.EqualTo ("14"));
    }

    [Test]
    public void MethodWithOutAndRefParameters ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var mutableMethod = proxyType.GetOrAddOverride (typeof (DomainType).GetMethod ("MethodWithOutAndRefParameters"));
            mutableMethod.SetBody (
                ctx =>
                {
                  var tempLocal = Expression.Variable (typeof (int), "temp");
                  return Expression.Block (
                      new[] { tempLocal },
                      Expression.Assign (tempLocal, Expression.Multiply (ctx.Parameters[0], Expression.Constant (3))),
                      ctx.InvokePreviousBodyWithArguments (tempLocal, ctx.Parameters[1]),
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
          proxyType =>
          {
            var method = typeof (DomainType).GetMethod ("ProtectedVirtualMethod", BindingFlags.NonPublic | BindingFlags.Instance);
            var mutableMethod = proxyType.GetOrAddOverride (method);
            mutableMethod.SetBody (
                ctx => ExpressionHelper.StringConcat (Expression.Constant ("hello "), Expression.Call (ctx.PreviousBody, "ToString", null)));
          });

      var instance = (DomainType) Activator.CreateInstance (type);
      var result = PrivateInvoke.InvokeNonPublicMethod (instance, "ProtectedVirtualMethod", 7);

      Assert.That (result, Is.EqualTo ("hello 7"));
    }

    [Test]
    public void SetBodyOfAddedMethod_Virtual ()
    {
      var type = AssembleType<DomainType> (
          proxyType => proxyType.AddMethod (
              "AddedMethod",
              MethodAttributes.Public | MethodAttributes.Virtual,
              typeof (int),
              ParameterDeclaration.None,
              ctx => Expression.Constant (7)),
          proxyType =>
          {
            var addedMethod = proxyType.AddedMethods.Single();
            Assert.That (addedMethod.IsVirtual, Is.True);
            addedMethod.SetBody (ctx => Expression.Add (ctx.PreviousBody, Expression.Constant (1)));
          });

      var method = type.GetMethod ("AddedMethod");
      var instance = (DomainType) Activator.CreateInstance (type);
      var result = method.Invoke (instance, null);

      Assert.That (result, Is.EqualTo (8));
    }

    [Test]
    public void SetBodyOfAddedMethod_NonVirtual ()
    {
      var type = AssembleType<DomainType> (
          proxyType => proxyType.AddMethod (
              "AddedMethod",
              MethodAttributes.Public,
              typeof (int),
              ParameterDeclaration.None,
              ctx => Expression.Constant (7)),
          proxyType =>
          {
            var addedMethod = proxyType.AddedMethods.Single();
            Assert.That (addedMethod.IsVirtual, Is.False);
            addedMethod.SetBody (ctx => Expression.Add (ctx.PreviousBody, Expression.Constant (1)));
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
        proxyType =>
        {
          var mutableMethod = proxyType.GetOrAddOverride (typeof (DomainType).GetMethod ("PublicVirtualMethod"));
          mutableMethod.SetBody (ctx => ctx.InvokePreviousBodyWithArguments (Expression.Multiply (Expression.Constant (2), ctx.Parameters[0])));
        });

      var instance = (DomainType) Activator.CreateInstance (type);
      var result = instance.CallsOriginalMethod (7);

      Assert.That (result, Is.EqualTo ("-14"));
    }

    [Test]
    public void ModifyingNonVirtualAndStaticAndFinalMethods_Throws ()
    {
      var nonVirtualMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.PublicMethod());
      var staticMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => DomainType.PublicStaticMethod());
      var finalMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.FinalMethod());

      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var message = "Only virtual methods can be overridden.";
            Assert.That (
                () => proxyType.GetOrAddOverride (nonVirtualMethod),
                Throws.TypeOf<ArgumentException>().With.Message.StartsWith (message));

            Assert.That (
                () => proxyType.GetOrAddOverride (staticMethod),
                Throws.TypeOf<ArgumentException>().With.Message.StartsWith (message));

            Assert.That (
                () => proxyType.GetOrAddOverride (finalMethod),
                Throws.TypeOf<NotSupportedException>().With.Message.EqualTo ("Cannot override final method 'DomainType.FinalMethod'."));
          });

      var instance = (DomainType) Activator.CreateInstance (type);
      Assert.That (instance.PublicMethod(), Is.EqualTo (12));

      var method = type.GetMethod ("PublicStaticMethod", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
      Assert.That (method.Invoke (null, null), Is.EqualTo (13));
    }

    [Test]
    public void ChainPreviousBodyInvocations ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var mutableMethod = proxyType.GetOrAddOverride (typeof (DomainType).GetMethod ("PublicVirtualMethod"));
            mutableMethod.SetBody (ctx => ctx.InvokePreviousBodyWithArguments (Expression.Multiply (Expression.Constant (2), ctx.Parameters[0])));
          },
          proxyType =>
          {
            var mutableMethod = proxyType.GetOrAddOverride (typeof (DomainType).GetMethod ("PublicVirtualMethod"));
            mutableMethod.SetBody (ctx => ctx.InvokePreviousBodyWithArguments (Expression.Add (Expression.Constant (2), ctx.Parameters[0])));
          });

      var instance = (DomainType) Activator.CreateInstance (type);
      var result = instance.PublicVirtualMethod (3); // (3 + 2) * 2 != (2 * 2) + 3

      Assert.That (result, Is.EqualTo ("10"));
    }

    [Test]
    public void AddedMethodCallsModfiedMethod ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var originalMethod = typeof (DomainType).GetMethod ("PublicVirtualMethod");
            proxyType.AddMethod (
                "AddedMethod",
                MethodAttributes.Public,
                typeof (string),
                ParameterDeclaration.None,
                ctx => Expression.Call (ctx.This, originalMethod, Expression.Constant(7)));

            var modifiedMethod = proxyType.GetOrAddOverride (originalMethod);
            modifiedMethod.SetBody (ctx => ExpressionHelper.StringConcat(Expression.Constant ("hello "), Expression.Call (ctx.PreviousBody, "ToString", null)));
          });

      var method = type.GetMethod ("AddedMethod");
      var instance = (DomainType) Activator.CreateInstance (type);
      var result = method.Invoke (instance, null);

      Assert.That (result, Is.EqualTo ("hello 7"));
    }

    public class DomainTypeBase
    {
      public virtual int FinalMethod () { return -14; }
    }

    public class DomainType : DomainTypeBase
    {
      public virtual string PublicVirtualMethod (int i)
      {
        return "" + i;
      }

      protected virtual string ProtectedVirtualMethod (int i)
      {
        return "" + i;
      }

      public string CallsOriginalMethod (int i)
      {
        return PublicVirtualMethod (-i);
      }

      public int PublicMethod () { return 12; }
      public static int PublicStaticMethod () { return 13; }
      public sealed override int FinalMethod () { return 14; }

      public virtual void MethodWithOutAndRefParameters (ref int i, out string s)
      {
        i++;
        s = "hello";
      }
    }
  }
}