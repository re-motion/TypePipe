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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Dlr.Ast;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class ConstantRuntimeInfosDlrBugTest : TypeAssemblerIntegrationTestBase
  {
    // RM-5560
    // https://connect.microsoft.com/VisualStudio/feedback/details/785822/lambdaexpression-compiletomethod-generates-code-that-throws-an-typeaccessexception-for-certain-expressions

    [Test]
    public void TypeConstant ()
    {
      var template = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType o) => o.TypeConstant());
      var type = typeof (int);

      var instance = ReturnMemberInfoViaConstantExpression (template, type);

      Assert.That (instance.TypeConstant(), Is.SameAs (type));
    }

    [Explicit ("FieldInfo inside Expression.Constant is not supported by the DLR.")]
    [Test]
    public void FieldConstant ()
    {
      var template = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType o) => o.FieldConstant());
      var field = NormalizingMemberInfoFromExpressionUtility.GetField (() => Type.EmptyTypes);

      var instance = ReturnMemberInfoViaConstantExpression (template, field);

      Assert.That (instance.FieldConstant(), Is.EqualTo (field));
    }

    [Test]
    public void ConstructorConstant ()
    {
      var template = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType o) => o.ConstructorConstant());
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new object());

      var instance = ReturnMemberInfoViaConstantExpression (template, constructor);

      Assert.That (instance.ConstructorConstant(), Is.EqualTo (constructor));
    }

    [Test]
    public void MethodConstant ()
    {
      var template = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType o) => o.MethodConstant());
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((object o) => o.ToString());

      var instance = ReturnMemberInfoViaConstantExpression (template, method);

      Assert.That (instance.MethodConstant(), Is.EqualTo (method));
    }

    // PropertyInfo and EventInfo cannot be loaded via the 'ldtoken' opcode, so we don't need to test them.

    private DomainType ReturnMemberInfoViaConstantExpression (MethodInfo template, MemberInfo member)
    {
      var type = AssembleType<DomainType> (p => p.GetOrAddOverride (template).SetBody (ctx => Expression.Constant (member)));
      return (DomainType) Activator.CreateInstance (type);
    }

    public class DomainType
    {
      public virtual Type TypeConstant () { return null; }
      public virtual FieldInfo FieldConstant () { return null; }
      public virtual ConstructorInfo ConstructorConstant () { return null; }
      public virtual MethodInfo MethodConstant () { return null; }
    }
  }
}