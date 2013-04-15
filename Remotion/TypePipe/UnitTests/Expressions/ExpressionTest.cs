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
using Remotion.Development.TypePipe.UnitTesting.Expressions;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.Expressions;
using Remotion.Development.UnitTesting.Reflection;

namespace Remotion.TypePipe.UnitTests.Expressions
{
  [TestFixture]
  public class ExpressionTest
  {
    [Test]
    public void NewDelegate ()
    {
      var delegateType = typeof (Action);
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => Method());
      var target = ExpressionTreeObjectMother.GetSomeExpression (method.DeclaringType);

      var result = Expression.NewDelegate (delegateType, target, method);

      Assert.That (result.Type, Is.SameAs (delegateType));
      Assert.That (result.Target, Is.SameAs (target));
      Assert.That (result.Method, Is.SameAs (method));
    }

    [Test]
    public void NewDelegate_StaticMethod ()
    {
      var delegateType = typeof (Action);
      var staticMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => StaticMethod());

      var result = Expression.NewDelegate (delegateType, null, staticMethod);

      Assert.That (result.Target, Is.Null);
    }

    void Method () { }
    static void StaticMethod () { }
  }
}