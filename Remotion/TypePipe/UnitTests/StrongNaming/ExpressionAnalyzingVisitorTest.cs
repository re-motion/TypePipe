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
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.StrongNaming;
using Remotion.TypePipe.UnitTests.Expressions;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.StrongNaming
{
  [TestFixture]
  public class ExpressionAnalyzingVisitorTest
  {
    private ITypeAnalyzer _typeAnalyzerMock;

    private ExpressionAnalyzingVisitor _visitor;

    private bool _someBool;

    [SetUp]
    public void SetUp ()
    {
      _typeAnalyzerMock = MockRepository.GenerateStrictMock<ITypeAnalyzer>();

      _visitor = new ExpressionAnalyzingVisitor (_typeAnalyzerMock);

      _someBool = BooleanObjectMother.GetRandomBoolean();
    }

    [Test]
    public void Visit ()
    {
      var someType = ReflectionObjectMother.GetSomeType();
      var expression = ExpressionTreeObjectMother.GetSomeExpression (someType);
      _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (someType)).Return (_someBool);

      CheckVisitMethod ((v, e) => v.Visit (e), _visitor, expression);
    }

    [Test]
    public void Visit_Null ()
    {
      Assert.That (_visitor.Visit (node: null), Is.Null);
    }

    [Test]
    public void VisitCatchBlock ()
    {
      var expression = Expression.Catch (typeof (NullReferenceException), Expression.Default (typeof (int)));
      _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (typeof (NullReferenceException))).Return (_someBool);
      _typeAnalyzerMock.Stub (stub => stub.IsStrongNamed (typeof (int))).Return (true);

      CheckVisitMethod (ExpressionVisitorTestHelper.CallVisitCatchBlock, _visitor, expression);
    }

    [Test]
    public void VisitMember ()
    {
      var member = NormalizingMemberInfoFromExpressionUtility.GetField (() => string.Empty);
      Expression expression = Expression.Field (null, member);
      _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (typeof (string))).Return (_someBool);

      CheckVisitMethod (ExpressionVisitorTestHelper.CallVisitMember, _visitor, expression);
    }

    [Test]
    public void VisitMethodCall ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => GC.Collect());
      Expression expression = Expression.Call (method);
      _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (typeof (GC))).Return (_someBool);

      CheckVisitMethod (ExpressionVisitorTestHelper.CallVisitMethodCall, _visitor, expression);
    }

    private void CheckVisitMethod<T> (Func<ExpressionAnalyzingVisitor, T, T> visitMethodInvoker, ExpressionAnalyzingVisitor visitor, T expression)
    {
      var result = visitMethodInvoker (visitor, expression);

      _typeAnalyzerMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (result));
      Assert.That (_visitor.IsStrongNameStrongNameCompatible, Is.EqualTo (_someBool));
    }
  }
}