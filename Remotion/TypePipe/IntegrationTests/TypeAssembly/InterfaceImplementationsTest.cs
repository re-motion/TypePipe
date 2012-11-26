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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;

namespace TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class InterfaceImplementationsTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void Implement ()
    {
      var interfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IOtherInterface obj) => obj.OtherMethod());
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            mutableType.AddInterface (typeof (IOtherInterface));
            var method = mutableType.GetOrAddMutableMethod (interfaceMethod);
            method.SetBody (
                ctx =>
                {
                  Assert.That (ctx.HasBaseMethod, Is.False);
                  Assert.That (ctx.HasPreviousBody, Is.False);

                  return Expression.Constant ("implemented");
                });
          });

      var instance = (IOtherInterface) Activator.CreateInstance (type);

      Assert.That (instance.OtherMethod(), Is.EqualTo ("implemented"));
    }

    [Test]
    public void Modify ()
    {
      var interfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IDomainInterface obj) => obj.Method());
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var method = mutableType.GetOrAddMutableMethod (interfaceMethod);
            method.SetBody (
                ctx =>
                {
                  Assert.That (ctx.HasBaseMethod, Is.False);
                  Assert.That (ctx.HasPreviousBody, Is.True);

                  return ExpressionHelper.StringConcat (ctx.PreviousBody, Expression.Constant (" modified"));
                });
          });

      var instance = (IOtherInterface) Activator.CreateInstance (type);

      Assert.That (instance.OtherMethod(), Is.EqualTo ("DomainType.Method modified"));
    }

    // TODO

    public abstract class BaseType : IBaseInterface
    {
      public string BaseMethod ()
      {
        return "DomainTypeBase.BaseMethod";
      }
    }

    public class DomainType : IDomainInterface
    {
      public string Method ()
      {
        return "DomainType.Method";
      }
    }

    public interface IBaseInterface
    {
      string BaseMethod ();
    }

    public interface IDomainInterface
    {
      string Method ();
    }

    public interface IOtherInterface
    {
      string OtherMethod ();
    }
  }
}