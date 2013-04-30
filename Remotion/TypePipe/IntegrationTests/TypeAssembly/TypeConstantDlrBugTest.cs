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
  public class TypeConstantDlrBugTest : TypeAssemblerIntegrationTestBase
  {
    // https://connect.microsoft.com/VisualStudio/feedback/details/785822/lambdaexpression-compiletomethod-generates-code-that-throws-an-typeaccessexception-for-certain-expressions

    [Test]
    public void TypeConstant ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType o) => o.Method());

      var type = AssembleType<DomainType> (p => p.GetOrAddOverride (method).SetBody (ctx => Expression.Constant (typeof (int))));

      var instance = (DomainType) Activator.CreateInstance (type);
      Assert.That (instance.Method(), Is.SameAs (typeof (int)));
    }

    public class DomainType
    {
      public virtual Type Method () { return null; }
    }
  }
}