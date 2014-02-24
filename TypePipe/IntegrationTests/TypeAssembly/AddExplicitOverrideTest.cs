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
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Dlr.Ast;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class AddExplicitOverrideTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void BaseMethod ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.BaseMethod());
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var explicitOverride = proxyType.AddExplicitOverride (
                method, ctx => ExpressionHelper.StringConcat (ctx.CallBase (method), Expression.Constant (" explicitly overridden")));
            Assert.That (explicitOverride.BaseMethod, Is.Null);
          });

      var instance = (DomainType) Activator.CreateInstance (type);
      Assert.That (instance.BaseMethod(), Is.EqualTo ("DomainTypeBase.BaseMethod explicitly overridden"));
    }

    [Test]
    public void InterfaceMethod ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IDomainInterface obj) => obj.Method());

      var type = AssembleType<DomainType> (p => p.AddExplicitOverride (method, ctx => Expression.Constant ("explicitly implemented")));

      var instance = Activator.CreateInstance (type);
      Assert.That (((DomainType) instance).Method(), Is.EqualTo ("DomainType.Method"));
      Assert.That (((IDomainInterface) instance).Method(), Is.EqualTo ("explicitly implemented"));
    }

    [Test]
    public void InterfaceMethod_AlreadyExplicitlyImplemented ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IDomainInterface obj) => obj.ExplicitlyImplemented());

      var type = AssembleType<DomainType> (p => p.AddExplicitOverride (method, ctx => Expression.Constant ("new explicit impl")));

      var instance = Activator.CreateInstance (type);
      Assert.That (((IDomainInterface) instance).ExplicitlyImplemented(), Is.EqualTo ("new explicit impl"));
    }

    public class DomainTypeBase
    {
      public virtual string BaseMethod ()
      {
        return "DomainTypeBase.BaseMethod";
      }
    }

    public class DomainType : DomainTypeBase, IDomainInterface
    {
      public string Method ()
      {
        return "DomainType.Method";
      }
      string IDomainInterface.ExplicitlyImplemented () { return "DomainType.ExplicitlyImplemented"; }
    }

    public interface IDomainInterface
    {
      string Method ();
      string ExplicitlyImplemented ();
    }
  }
}