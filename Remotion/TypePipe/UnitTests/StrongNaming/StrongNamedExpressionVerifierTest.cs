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
using Remotion.TypePipe.StrongNaming;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.StrongNaming
{
  [TestFixture]
  public class StrongNamedExpressionVerifierTest
  {
    [Test]
    public void Visit_Expression ()
    {
      var type = ReflectionObjectMother.GetSomeType();

      Check (Expression.Default (type), true, type);
      Check (Expression.Default (type), false, type);
    }

    [Test]
    public void Visit_Member ()
    {
      Check (Expression.Field (null, NormalizingMemberInfoFromExpressionUtility.GetField (() => DomainType.Field)), true, typeof (DomainType));
      Check (Expression.Field (null, NormalizingMemberInfoFromExpressionUtility.GetField (() => DomainType.Field)), false, typeof (DomainType));
    }

    [Test]
    public void Visit_Call ()
    {
      Check (Expression.Call (null, NormalizingMemberInfoFromExpressionUtility.GetMethod (() => DomainType.Method())), true, typeof (DomainType));
      Check (Expression.Call (null, NormalizingMemberInfoFromExpressionUtility.GetMethod (() => DomainType.Method())), false, typeof (DomainType));
    }

    [Test]
    public void Visit_CatchBlock ()
    {
      Check (Expression.TryCatch (Expression.Empty(), Expression.Catch (typeof (Exception), Expression.Empty())), true, typeof (Exception));
      Check (Expression.TryCatch (Expression.Empty(), Expression.Catch (typeof (Exception), Expression.Empty())), false, typeof (Exception));
    }

    private void Check (Expression expression, bool strongNamed, Type type)
    {
      var strongTypeVerifier = MockRepository.GenerateStrictMock<IStrongNameTypeVerifier> ();
      var visitorPartialMock = MockRepository.GeneratePartialMock<StrongNameExpressionVerifier> (strongTypeVerifier);

      strongTypeVerifier.Expect (mock => mock.IsStrongNamed (type))
          .Return (strongNamed)
          .WhenCalled (
              mi =>
              {
                if (!strongNamed && !(expression is TryExpression))
                  visitorPartialMock.Expect (mock => mock.Visit (Arg<Expression>.Is.Anything)).Repeat.Never();
              });
      strongTypeVerifier.Stub (stub => stub.IsStrongNamed (Arg<Type>.Is.Anything)).Return (true);

      var result = visitorPartialMock.IsStrongNamed (expression);

      strongTypeVerifier.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (strongNamed));
    }

    public class DomainType
    {
      public static int Field;

      public static void Method () {}
    }
  }
}