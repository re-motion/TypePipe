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
using System.Linq;
using System.Reflection.Emit;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.Expressions;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.Expressions;
using Moq;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.LambdaCompilation
{
  [TestFixture]
  public class CodeGenerationExpressionEmitterTest
  {
    public interface IChildExpressionEmitter
    {
      void EmitChildExpression (Expression childExpression);
    }

    private Mock<IILGenerator> _ilGeneratorMock;
    private CodeGenerationExpressionEmitter _emitter;
    private Mock<IChildExpressionEmitter> _childExpressionEmitterMock;

    [SetUp]
    public void SetUp ()
    {
      _ilGeneratorMock = new Mock<IILGenerator> (MockBehavior.Strict);
      _childExpressionEmitterMock = new Mock<IChildExpressionEmitter> (MockBehavior.Strict);
      _emitter = new CodeGenerationExpressionEmitter (_ilGeneratorMock.Object, _childExpressionEmitterMock.Object.EmitChildExpression);
    }

    [Test]
    public void VisitExtension_Throws ()
    {
      Assert.That (
          () => _emitter.VisitExtension (null),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo (
                  "All non-primitive TypePipe expressions must be reduced before code generation."));
    }

    [Test]
    public void VisitThis ()
    {
      var expression = ExpressionTreeObjectMother.GetSomeThisExpression();
      _ilGeneratorMock.Setup (mock => mock.Emit (OpCodes.Ldarg_0)).Verifiable();

      var result = _emitter.VisitThis (expression);

      _ilGeneratorMock.Verify();
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitNewDelegate ()
    {
      var delegateType = typeof (Action);
      var delegateCtor = delegateType.GetConstructors().Single();
      var targetExpression = ExpressionTreeObjectMother.GetSomeExpression (typeof (DomainType));
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method());
      var expression = new NewDelegateExpression (delegateType, targetExpression, method);

      _childExpressionEmitterMock.Setup (mock => mock.EmitChildExpression (expression.Target)).Verifiable();
      _ilGeneratorMock.Setup (mock => mock.Emit (OpCodes.Ldftn, expression.Method)).Verifiable();
      _ilGeneratorMock.Setup (mock => mock.Emit (OpCodes.Newobj, delegateCtor)).Verifiable();

      var result = _emitter.VisitNewDelegate (expression);

      _childExpressionEmitterMock.Verify();
      _ilGeneratorMock.Verify();
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitNewDelegate_Static ()
    {
      var delegateType = typeof (Action);
      var delegateCtor = delegateType.GetConstructors ().Single ();
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => DomainType.StaticMethod());
      var expression = new NewDelegateExpression (delegateType, null, method);

      _ilGeneratorMock.Setup (mock => mock.Emit (OpCodes.Ldnull)).Verifiable();
      _ilGeneratorMock.Setup (mock => mock.Emit (OpCodes.Ldftn, expression.Method)).Verifiable();
      _ilGeneratorMock.Setup (mock => mock.Emit (OpCodes.Newobj, delegateCtor)).Verifiable();

      _emitter.VisitNewDelegate (expression);

      _ilGeneratorMock.Verify();
    }

    [Test]
    public void VisitNewDelegate_Virtual ()
    {
      var delegateType = typeof (Action);
      var delegateCtor = delegateType.GetConstructors ().Single ();
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((BaseType obj) => obj.VirtualMethod());
      var targetExpression = ExpressionTreeObjectMother.GetSomeExpression (typeof (DomainType));
      var expression = new NewDelegateExpression (delegateType, targetExpression, method);

      _childExpressionEmitterMock.Setup (mock => mock.EmitChildExpression (expression.Target)).Verifiable();
      _ilGeneratorMock.Setup (mock => mock.Emit (OpCodes.Dup)).Verifiable();
      _ilGeneratorMock.Setup (mock => mock.Emit (OpCodes.Ldvirtftn, expression.Method)).Verifiable();
      _ilGeneratorMock.Setup (mock => mock.Emit (OpCodes.Newobj, delegateCtor)).Verifiable();

      _emitter.VisitNewDelegate (expression);

      _childExpressionEmitterMock.Verify();
      _ilGeneratorMock.Verify();
    }

    [Test]
    public void VisitBox ()
    {
      var expression = ExpressionTreeObjectMother.GetSomeBoxAndCastExpression();
      Assert.That (expression.Type, Is.Not.SameAs (expression.Operand.Type));
      _childExpressionEmitterMock.Setup (mock => mock.EmitChildExpression (expression.Operand)).Verifiable();
      _ilGeneratorMock.Setup (mock => mock.Emit (OpCodes.Box, expression.Operand.Type)).Verifiable();
      _ilGeneratorMock.Setup (mock => mock.Emit (OpCodes.Castclass, expression.Type)).Verifiable();

      var result = _emitter.VisitBox (expression);

      _childExpressionEmitterMock.Verify();
      _ilGeneratorMock.Verify();
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitUnbox ()
    {
      var expression = ExpressionTreeObjectMother.GetSomeUnboxExpression();
      _childExpressionEmitterMock.Setup (mock => mock.EmitChildExpression (expression.Operand)).Verifiable();
      _ilGeneratorMock.Setup (mock => mock.Emit (OpCodes.Unbox_Any, expression.Type)).Verifiable();

      var result = _emitter.VisitUnbox (expression);

      _childExpressionEmitterMock.Verify();
      _ilGeneratorMock.Verify();
      Assert.That (result, Is.SameAs (expression));
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