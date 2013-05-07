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
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [Ignore ("TODO 5551")]
  [TestFixture]
  public class ReImplementInterfaceMethodTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void Virtual_Overrides ()
    {
      var interfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IMyInterface o) => o.Method1());
      var type = AssembleType<DomainType> (
          p => p.GetOrAddOverrideOrReImplement (interfaceMethod)
                .SetBody (
                    ctx =>
                    {
                      Assert.That (ctx.BaseMethod, Is.EqualTo (interfaceMethod));
                      Assert.That (ctx.DeclaringType.AddedInterfaces, Is.Empty);

                      return ExpressionHelper.StringConcat (ctx.PreviousBody, Expression.Constant ("override"));
                    }));

      var method = GetDeclaredMethod (type, "Method1");
      Assert.That (method.Attributes.IsSet (MethodAttributes.ReuseSlot), Is.True);
      var instance = (DomainType) Activator.CreateInstance (type);

      var result1 = instance.Method1();
      var result2 = instance.As<IMyInterface>().Method1();

      Assert.That (result1, Is.EqualTo ("1 override"));
      Assert.That (result2, Is.EqualTo ("1 override"));
    }

    [Test]
    public void NonVirtual_ReImplements_AndCallsBase ()
    {
      var interfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IMyInterface o) => o.Method2 ());
      var type = AssembleType<DomainType> (
          p => p.GetOrAddOverrideOrReImplement (interfaceMethod)
                .SetBody (
                    ctx =>
                    {
                      Assert.That (ctx.BaseMethod, Is.EqualTo (interfaceMethod));
                      Assert.That (ctx.DeclaringType.AddedInterfaces, Is.EqualTo (new[] { typeof (IMyInterface) }));

                      return ExpressionHelper.StringConcat (ctx.PreviousBody, Expression.Constant ("re-implementation"));
                    }));

      var method = GetDeclaredMethod (type, "Method2");
      Assert.That (method.Attributes.IsSet (MethodAttributes.NewSlot), Is.True);
      var instance = (DomainType) Activator.CreateInstance (type);

      var result1 = instance.Method2();
      var result2 = instance.As<IMyInterface>().Method2();

      Assert.That (result1, Is.EqualTo ("2"));
      Assert.That (result2, Is.EqualTo ("2 re-implementation"));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "Cannot re-implement interface method 'Method3' because its base implementation is not accessible.")]
    public void NonVirtual_NonAccessible ()
    {
      var interfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IMyInterface o) => o.Method3());
      AssembleType<DomainType> (p => p.GetOrAddOverrideOrReImplement (interfaceMethod).SetBody (ctx => null));
    }

    private class DomainType : IMyInterface
    {
      public virtual string Method1 () { return "1 "; }
      public string Method2 () { return "2 "; }
      string IMyInterface.Method3 () { return "3 "; }
    }

    interface IMyInterface
    {
      string Method1 ();
      string Method2 ();
      string Method3 ();
    }
  }
}