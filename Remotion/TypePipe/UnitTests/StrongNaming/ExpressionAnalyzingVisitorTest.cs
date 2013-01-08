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
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
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

      CheckVisitMethod ((v, e) => v.Visit (e), _visitor, expression, _someBool);
    }

    [Test]
    public void Visit_Null ()
    {
      Assert.That (_visitor.Visit (node: null), Is.Null);
    }

    [Test]
    public void VisitBinary ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => BinaryMethod (7, 8));
      var expression = BinaryExpression.MakeBinary (ExpressionType.Add, Expression.Constant (7), Expression.Constant (8), false, method);
      _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (typeof (ExpressionAnalyzingVisitorTest))).Return (_someBool);
      _typeAnalyzerMock.Stub (stub => stub.IsStrongNamed (typeof (int))).Return (true);

      CheckVisitMethod (ExpressionVisitorTestHelper.CallVisitBinary, _visitor, expression, _someBool);
    }

    [Test]
    public void VisitBinary_NullMethod ()
    {
      var expression = BinaryExpression.MakeBinary (ExpressionType.Add, Expression.Constant (7), Expression.Constant (8));
      Assert.That (expression.Method, Is.Null);
      _typeAnalyzerMock.Stub (stub => stub.IsStrongNamed (typeof (int))).Return (true);

      CheckVisitMethod (ExpressionVisitorTestHelper.CallVisitBinary, _visitor, expression, expectedCompatibility: true);
    }

    [Test]
    public void VisitCatchBlock ()
    {
      var expression = Expression.Catch (typeof (NullReferenceException), Expression.Default (typeof (int)));
      _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (typeof (NullReferenceException))).Return (_someBool);
      _typeAnalyzerMock.Stub (stub => stub.IsStrongNamed (typeof (int))).Return (true);

      CheckVisitMethod (ExpressionVisitorTestHelper.CallVisitCatchBlock, _visitor, expression, _someBool);
    }

    [Test]
    public void VisitDynamic ()
    {
      var callSiteBinderStub = MockRepository.GenerateStub<CallSiteBinder>();
      var expression = Expression.Dynamic (callSiteBinderStub, typeof (double), Expression.Constant (7));
      _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (typeof (Func<CallSite, int, double>))).Return (_someBool);
      _typeAnalyzerMock.Stub (stub => stub.IsStrongNamed (typeof (int))).Return (true);

      CheckVisitMethod (ExpressionVisitorTestHelper.CallVisitDynamic, _visitor, expression, _someBool);
    }

    [Test]
    public void VisitElementInit ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => Add (7));
      var expression = Expression.ElementInit (method, Expression.Constant (7));
      _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (typeof (ExpressionAnalyzingVisitorTest))).Return (_someBool);
      _typeAnalyzerMock.Stub (stub => stub.IsStrongNamed (typeof (int))).Return (true);

      CheckVisitMethod (ExpressionVisitorTestHelper.CallVisitElementInit, _visitor, expression, _someBool);
    }

    [Test]
    public void VisitIndex ()
    {
      var instance = Expression.Default (typeof (ExpressionAnalyzingVisitorTest));
      var indexer = typeof (ExpressionAnalyzingVisitorTest).GetProperty ("Item");
      var expression = Expression.MakeIndex (instance, indexer, new[] { Expression.Constant (7) });
      // Implicitly covered through 'node.Object.Type'.
      _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (typeof (ExpressionAnalyzingVisitorTest))).Return (_someBool);
      _typeAnalyzerMock.Stub (stub => stub.IsStrongNamed (typeof (int))).Return (true);

      CheckVisitMethod (ExpressionVisitorTestHelper.CallVisitIndex, _visitor, expression, _someBool);
    }

    [Test]
    public void VisitMember ()
    {
      var member = NormalizingMemberInfoFromExpressionUtility.GetField (() => string.Empty);
      var expression = Expression.Field (null, member);
      _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (typeof (string))).Return (_someBool);

      CheckVisitMethod (ExpressionVisitorTestHelper.CallVisitMember, _visitor, expression, _someBool);
    }

    [Test]
    public void VisitMethodCall_DeclaringType ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => GC.Collect());
      var expression = Expression.Call (method);
      _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (typeof (GC))).Return (_someBool);

      CheckVisitMethod (ExpressionVisitorTestHelper.CallVisitMethodCall, _visitor, expression, _someBool);
    }

    [Test]
    public void VisitMethodCall_GenericArguments ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => GenericMethod<double>());
      var expression = Expression.Call (method);
      _typeAnalyzerMock.Stub (stub => stub.IsStrongNamed (typeof (ExpressionAnalyzingVisitorTest))).Return (true);
      _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (typeof (double))).Return (_someBool);

      CheckVisitMethod (ExpressionVisitorTestHelper.CallVisitMethodCall, _visitor, expression, _someBool);
    }

    [Test]
    public void VisitSwitch ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => Comparison (7, 8));
      var switchValue = Expression.Parameter (typeof (int));
      var defaultBody = Expression.Empty();
      var switchCase = Expression.SwitchCase (Expression.Empty(), Expression.Constant (7));
      var expression = Expression.Switch (switchValue, defaultBody, method, switchCase);

      _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (typeof (ExpressionAnalyzingVisitorTest))).Return (_someBool);
      _typeAnalyzerMock.Stub (stub => stub.IsStrongNamed (typeof (int))).Return (true);
      _typeAnalyzerMock.Stub (stub => stub.IsStrongNamed (typeof (void))).Return (true);

      CheckVisitMethod (ExpressionVisitorTestHelper.CallVisitSwitch, _visitor, expression, _someBool);
    }

    [Test]
    public void VisitSwitch_NullMethod ()
    {
      var switchCase = Expression.SwitchCase (Expression.Empty(), Expression.Constant (7));
      var expression = Expression.Switch (Expression.Parameter (typeof (int)), Expression.Empty(), switchCase);
      Assert.That (expression.Comparison, Is.Null);

      _typeAnalyzerMock.Stub (stub => stub.IsStrongNamed (typeof (int))).Return (true);
      _typeAnalyzerMock.Stub (stub => stub.IsStrongNamed (typeof (void))).Return (true);

      CheckVisitMethod (ExpressionVisitorTestHelper.CallVisitSwitch, _visitor, expression, expectedCompatibility: true);
    }

    [Test]
    public void VisitUnary ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => UnaryMethod (7));
      var expression = BinaryExpression.MakeUnary (ExpressionType.Negate, Expression.Constant (7), null, method);
      _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (typeof (ExpressionAnalyzingVisitorTest))).Return (_someBool);
      _typeAnalyzerMock.Stub (stub => stub.IsStrongNamed (typeof (int))).Return (true);

      CheckVisitMethod (ExpressionVisitorTestHelper.CallVisitUnary, _visitor, expression, _someBool);
    }

    [Test]
    public void VisitUnary_NullMethod ()
    {
      var expression = BinaryExpression.MakeUnary (ExpressionType.Negate, Expression.Constant (7), null);
      Assert.That (expression.Method, Is.Null);
      _typeAnalyzerMock.Stub (stub => stub.IsStrongNamed (typeof (int))).Return (true);

      CheckVisitMethod (ExpressionVisitorTestHelper.CallVisitUnary, _visitor, expression, expectedCompatibility: true);
    }

    private void CheckVisitMethod<T> (
        Func<ExpressionAnalyzingVisitor, T, object> visitMethodInvoker, ExpressionAnalyzingVisitor visitor, T expression, bool expectedCompatibility)
    {
      var result = visitMethodInvoker (visitor, expression);

      _typeAnalyzerMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (result));
      Assert.That (_visitor.IsStrongNameCompatible, Is.EqualTo (expectedCompatibility));
    }

    static void GenericMethod<[UsedImplicitly] T> () { }
    static int BinaryMethod (int x, int y) { return x + y; }
    static int UnaryMethod (int x) { return -x; }
    void Add (int x) { Dev.Null = x; }
    bool Comparison (int s, int i) { Dev.Null = s; Dev.Null = i; return false; }
    public int this[int i] { get { return i; } }
  }
}