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
using Remotion.Text;
using System.Linq;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [Ignore ("TODO 5229")]
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

    [Test]
    public void Override_Implicit ()
    {
      var interfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IBaseInterface obj) => obj.BaseMethod());
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var method = mutableType.GetOrAddMutableMethod (interfaceMethod);
            method.SetBody (
                ctx =>
                {
                  Assert.That (ctx.HasBaseMethod, Is.True);
                  Assert.That (ctx.HasPreviousBody, Is.True);

                  return ExpressionHelper.StringConcat (ctx.PreviousBody, Expression.Constant (" implicitly overridden"));
                });
          });

      var instance = (IOtherInterface) Activator.CreateInstance (type);

      Assert.That (instance.OtherMethod(), Is.EqualTo ("DomainTypeBase.BaseMethod implicitly overridden"));
    }

    [Test]
    public void Override_Explicit_ShadowedBase ()
    {
      var interfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IBaseInterface obj) => obj.ShadowedBaseMethod());
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var method = mutableType.GetOrAddMutableMethod (interfaceMethod);
            method.SetBody (
                ctx =>
                {
                  Assert.That (ctx.HasBaseMethod, Is.True);
                  Assert.That (ctx.HasPreviousBody, Is.True);

                  return ExpressionHelper.StringConcat (ctx.PreviousBody, Expression.Constant (" explicitly overridden"));
                });
          });

      var instance = (IOtherInterface) Activator.CreateInstance (type);

      Assert.That (instance.OtherMethod(), Is.EqualTo ("DomainTypeBase.BaseMethod explicitly overridden"));
    }

    public abstract class DomainTypeBase : IBaseInterface
    {
      public virtual string BaseMethod ()
      {
        return "DomainTypeBase.BaseMethod";
      }

      public virtual string ShadowedBaseMethod ()
      {
        return "DomainTypeBase.ShadowedBaseMethod";
      }
    }

    [Test]
    public void Spike ()
    {
      var interfaceMapping = typeof (DomainType).GetInterfaceMap (typeof (IBaseInterface));
      Console.WriteLine (SeparatedStringBuilder.Build (", ", interfaceMapping.InterfaceMethods.Select ((mi, i) => new { InterfaceMethod = mi, ImplementingType = interfaceMapping .TargetMethods[i].DeclaringType })));
      var interfaceMapping2 = typeof (DomainType2).GetInterfaceMap (typeof (IBaseInterface));
      Console.WriteLine (SeparatedStringBuilder.Build (", ", interfaceMapping2.InterfaceMethods.Select ((mi, i) => new { InterfaceMethod = mi, ImplementingType = interfaceMapping2.TargetMethods[i].DeclaringType })));
    }

    public class DomainType : DomainTypeBase, IDomainInterface
    {
      public virtual string Method ()
      {
        return "DomainType.Method";
      }

      public new string ShadowedBaseMethod ()
      {
        return "DomainType.ShadowedBaseMethod";
      }
    }

    public class DomainType2 : DomainTypeBase, IDomainInterface, IBaseInterface
    {
      public virtual string Method ()
      {
        return "DomainType.Method";
      }

      public new string ShadowedBaseMethod ()
      {
        return "DomainType.ShadowedBaseMethod";
      }
    }

    public interface IBaseInterface
    {
      string BaseMethod ();
      string ShadowedBaseMethod ();
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