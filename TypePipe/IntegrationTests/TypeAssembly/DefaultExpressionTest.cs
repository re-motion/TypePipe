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
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Dlr.Ast;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class DefaultExpressionTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void GenericParameters ()
    {
      var method1 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType o) => o.ReferenceType());
      var method2 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType o) => o.ValueType());
      var method3 = NormalizingMemberInfoFromExpressionUtility.GetGenericMethodDefinition ((DomainType o) => o.Unconstrained<Dev.T>());
      var method4 = NormalizingMemberInfoFromExpressionUtility.GetGenericMethodDefinition ((DomainType o) => o.ReferenceTypeConstraint<Dev.T>());
      var method5 = NormalizingMemberInfoFromExpressionUtility.GetGenericMethodDefinition ((DomainType o) => o.NotNuallableValueTypeConstraint<int>());
      var method6 = NormalizingMemberInfoFromExpressionUtility.GetGenericMethodDefinition ((DomainType o) => o.ClassBaseTypeConstraint<DomainType>());

      var type = AssembleType<DomainType> (
          proxyType =>
          {
            proxyType.GetOrAddOverride (method1).SetBody (ctx => Expression.Default (typeof (string)));
            proxyType.GetOrAddOverride (method2).SetBody (ctx => Expression.Default (typeof (int)));
            proxyType.GetOrAddOverride (method3).SetBody (ctx => Expression.Default (ctx.GenericParameters[0]));
            proxyType.GetOrAddOverride (method4).SetBody (ctx => Expression.Default (ctx.GenericParameters[0]));
            proxyType.GetOrAddOverride (method5).SetBody (ctx => Expression.Default (ctx.GenericParameters[0]));
            proxyType.GetOrAddOverride (method6).SetBody (ctx => Expression.Default (ctx.GenericParameters[0]));
          });

      var instance = (DomainType) Activator.CreateInstance (type);

      Assert.That (instance.ReferenceType(), Is.Null);
      Assert.That (instance.Unconstrained<string>(), Is.Null);
      Assert.That (instance.ReferenceTypeConstraint<string>(), Is.Null);
      Assert.That (instance.NotNuallableValueTypeConstraint<int>(), Is.EqualTo (0));
      Assert.That (instance.ClassBaseTypeConstraint<DomainType>(), Is.Null);
    }

    public class DomainType
    {
      public virtual object ReferenceType () { throw new NotImplementedException (); }
      public virtual ValueType ValueType () { throw new NotImplementedException (); }
      public virtual T Unconstrained<T> () { throw new NotImplementedException (); }
      public virtual T ReferenceTypeConstraint<T> () where T : class { throw new NotImplementedException (); }
      public virtual T NotNuallableValueTypeConstraint<T> () where T : struct { throw new NotImplementedException (); }
      public virtual T ClassBaseTypeConstraint<T> () where T : DomainType { throw new NotImplementedException (); }
    }
  }
}