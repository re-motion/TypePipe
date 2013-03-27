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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class ByReferenceTypeTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void MutableByRefType ()
    {
      var type = AssembleType<DomainType> (
          typeContext =>
          {
            var byRefType = typeContext.ProxyType.MakeByRefType();
            typeContext.ProxyType.AddMethod (
                "Method",
                MethodAttributes.Public | MethodAttributes.Static,
                typeof (void),
                new[] { new ParameterDeclaration (byRefType) },
                ctx => Expression.Assign (ctx.Parameters[0], Expression.New (typeContext.ProxyType)));
          });

      var method = type.GetMethod ("Method");
      var arguments = new object[] { null };
      method.Invoke (null, arguments);

      Assert.That (arguments[0], Is.Not.Null.And.TypeOf (type));
    }

    [Test]
    public void GenericParameterByRefType ()
    {
      var method = typeof (DomainType).GetMethod ("GenericMethod");
      var field = NormalizingMemberInfoFromExpressionUtility.GetField ((DomainType o) => o.WasCalled);

      var type = AssembleType<DomainType> (
          p => p.GetOrAddOverride (method).SetBody (
              ctx => Expression.Block (ctx.PreviousBody, Expression.Assign (Expression.Field (ctx.This, field), Expression.Constant (true)))));

      var instance1 = (DomainType) Activator.CreateInstance (type);
      var instance2 = (DomainType) Activator.CreateInstance (type);

      // ReSharper disable RedundantAssignment
      var byValue1 = new MyStruct();
      var byRef1 = new MyStruct();
      var outValue1 = 7;
      IMyInterface byValue2 = new MyStruct();
      IMyInterface byRef2 = new MyStruct();
      var outValue2 = new object();
      // ReSharper restore RedundantAssignment

      instance1.GenericMethod (byValue1, ref byRef1, out outValue1);
      instance2.GenericMethod (byValue2, ref byRef2, out outValue2);

      Assert.That (instance1.WasCalled, Is.True);
      Assert.That (byValue1.Field, Is.EqualTo (0));
      Assert.That (byRef1.Field, Is.EqualTo (1));
      Assert.That (outValue1, Is.EqualTo (0));

      Assert.That (instance2.WasCalled, Is.True);
      Assert.That (((MyStruct) byValue2).Field, Is.EqualTo (1));
      Assert.That (((MyStruct) byRef2).Field, Is.EqualTo (1));
      Assert.That (outValue2, Is.Null);
    }

    [Ignore ("TODO 5497")]
    [Test]
    public void GenericParameterByRefType_NoBoxing ()
    {
      var mutatingMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IMyInterface o) => o.MutatingMethod());
      var type = AssembleType<DomainType> (
          p => p.AddMethod (
              "Method",
              MethodAttributes.Public,
              new[] { new GenericParameterDeclaration ("T", constraintProvider: ctx => new[] { typeof (IMyInterface) }) },
              ctx => typeof (void),
              ctx => new[] { new ParameterDeclaration (ctx.GenericParameters[0].MakeByRefType()) },
              ctx => Expression.Call (ctx.Parameters[0], mutatingMethod)));

      var method = type.GetMethod ("Method").MakeGenericMethod (typeof (MyStruct));
      var instance = Activator.CreateInstance (type);

      var byRef = new MyStruct();
      method.Invoke (instance, new object[] { byRef });

      Assert.That (byRef.Field, Is.EqualTo (1));
    }

    public class DomainType
    {
      [UsedImplicitly] public bool WasCalled;

      public virtual void GenericMethod<TRef, TOut> (TRef arg1, ref TRef arg2, out TOut arg3)
          where TRef : IMyInterface
      {
        arg1.MutatingMethod();
        arg2.MutatingMethod();
        arg3 = default (TOut);
      }
    }

    public interface IMyInterface
    {
      void MutatingMethod ();
    }
    public struct MyStruct : IMyInterface
    {
      public int Field;
      public void MutatingMethod () { Field++; }
    }
  }
}