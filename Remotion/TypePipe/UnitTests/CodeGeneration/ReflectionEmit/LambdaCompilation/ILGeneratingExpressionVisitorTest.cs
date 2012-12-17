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
using System.Reflection.Emit;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.UnitTests.Expressions;
using Rhino.Mocks;
using Is = NUnit.Framework.Is;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.LambdaCompilation
{
  [TestFixture]
  public class ILGeneratingExpressionVisitorTest
  {
    public interface IChildExpressionEmitter
    {
      void EmitChildExpression (Expression childExpression);
    }

    private IILGenerator _ilGeneratorMock;
    private ILGeneratingExpressionVisitor _visitor;
    private IChildExpressionEmitter _childExpressionEmitterMock;

    [SetUp]
    public void SetUp ()
    {
      _ilGeneratorMock = MockRepository.GenerateStrictMock<IILGenerator>();
      _childExpressionEmitterMock = MockRepository.GenerateStrictMock<IChildExpressionEmitter>();
      _visitor = new ILGeneratingExpressionVisitor (_ilGeneratorMock, _childExpressionEmitterMock.EmitChildExpression);
    }

    [Test]
    public void VisitThis ()
    {
      var expression = ExpressionTreeObjectMother.GetSomeThisExpression();
      _ilGeneratorMock.Expect (mock => mock.Emit (OpCodes.Ldarg_0));

      var result = _visitor.VisitThis (expression);

      _ilGeneratorMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "OriginalBodyExpression must be replaced before code generation.")]
    public void VisitOriginalBody ()
    {
      _visitor.VisitOriginalBody (ExpressionTreeObjectMother.GetSomeOriginalBodyExpression());
    }

    [Test]
    public void VisitNewDelegate ()
    {
      var delegateType = typeof (Action);
      var delegateCtor = delegateType.GetConstructors().Single();
      var targetExpression = ExpressionTreeObjectMother.GetSomeExpression (typeof (DomainType));
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method());
      var expression = new NewDelegateExpression (delegateType, targetExpression, method);

      _childExpressionEmitterMock.Expect (mock => mock.EmitChildExpression (expression.Target));
      _ilGeneratorMock.Expect (mock => mock.Emit (OpCodes.Ldftn, expression.Method));
      _ilGeneratorMock.Expect (mock => mock.Emit (OpCodes.Newobj, delegateCtor));

      var result = _visitor.VisitNewDelegate (expression);

      _childExpressionEmitterMock.VerifyAllExpectations();
      _ilGeneratorMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitNewDelegate_Static ()
    {
      var delegateType = typeof (Action);
      var delegateCtor = delegateType.GetConstructors ().Single ();
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => DomainType.StaticMethod());
      var expression = new NewDelegateExpression (delegateType, null, method);

      _ilGeneratorMock.Expect (mock => mock.Emit (OpCodes.Ldnull));
      _ilGeneratorMock.Expect (mock => mock.Emit (OpCodes.Ldftn, expression.Method));
      _ilGeneratorMock.Expect (mock => mock.Emit (OpCodes.Newobj, delegateCtor));

      _visitor.VisitNewDelegate (expression);

      _ilGeneratorMock.VerifyAllExpectations ();
    }

    [Test]
    public void VisitNewDelegate_Virtual ()
    {
      var delegateType = typeof (Action);
      var delegateCtor = delegateType.GetConstructors ().Single ();
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((BaseType obj) => obj.VirtualMethod());
      var targetExpression = ExpressionTreeObjectMother.GetSomeExpression (typeof (DomainType));
      var expression = new NewDelegateExpression (delegateType, targetExpression, method);

      _childExpressionEmitterMock.Expect (mock => mock.EmitChildExpression (expression.Target));
      _ilGeneratorMock.Expect (mock => mock.Emit (OpCodes.Dup));
      _ilGeneratorMock.Expect (mock => mock.Emit (OpCodes.Ldvirtftn, expression.Method));
      _ilGeneratorMock.Expect (mock => mock.Emit (OpCodes.Newobj, delegateCtor));

      _visitor.VisitNewDelegate (expression);

      _ilGeneratorMock.VerifyAllExpectations ();
    }

    class BaseType
    {
      public virtual void VirtualMethod () { }
    }

    class DomainType : BaseType
    {
      public void Method () { }
      public static void StaticMethod () { }
      public override void VirtualMethod () { }
    }
  }
}