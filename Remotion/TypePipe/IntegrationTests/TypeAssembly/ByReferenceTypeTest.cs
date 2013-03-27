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

    [Ignore ("TODO 5424")]
    [Test]
    public void GenericParameterByRefType ()
    {
      var mutatingMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IMyInterface o) => o.MutatingMethod());
      var method = typeof (DomainType).GetMethod ("GenericMethod");

      var type = AssembleType<DomainType> (
          p => p.GetOrAddOverride (method).SetBody (
              ctx => Expression.Block (
                  Expression.Call (Expression.Convert (ctx.Parameters[0], typeof (IMyInterface)), mutatingMethod),
                  Expression.Call (Expression.Convert (ctx.Parameters[1], typeof (IMyInterface)), mutatingMethod),
                  Expression.Assign (ctx.Parameters[2], Expression.Default (ctx.GenericParameters[1])))));

      var instance = (DomainType) Activator.CreateInstance (type);

      var byValue = new MyStruct();
      var byRef = new MyStruct();
      // ReSharper disable RedundantAssignment
      var obj = new object();
      // ReSharper restore RedundantAssignment
      instance.GenericMethod (byValue, ref byRef, out obj);

      Assert.That (byValue.Field, Is.EqualTo (0));
      Assert.That (byRef.Field, Is.EqualTo (1));
      Assert.That (obj, Is.Null);
    }

    public class DomainType
    {
      public virtual void GenericMethod<TRef, TOut> (TRef arg0, ref TRef arg1, out TOut arg2)
          where TRef : IMyInterface
      {
        throw new NotImplementedException();
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