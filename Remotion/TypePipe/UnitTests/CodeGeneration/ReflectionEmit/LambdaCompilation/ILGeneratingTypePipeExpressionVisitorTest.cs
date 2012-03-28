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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.LambdaCompilation
{
  [TestFixture]
  public class ILGeneratingTypePipeExpressionVisitorTest
  {
    public interface IChildExpressionEmitter
    {
      void EmitChildExpression (Expression childExpression);
    }

    private IILGenerator _ilGeneratorMock;
    private ILGeneratingTypePipeExpressionVisitor _visitor;
    private IChildExpressionEmitter _childExpressionEmitterMock;

    [SetUp]
    public void SetUp ()
    {
      _ilGeneratorMock = MockRepository.GenerateStrictMock<IILGenerator>();
      _childExpressionEmitterMock = MockRepository.GenerateStrictMock<IChildExpressionEmitter>();
      _visitor = new ILGeneratingTypePipeExpressionVisitor (_ilGeneratorMock, _childExpressionEmitterMock.EmitChildExpression);
    }

    [Test]
    public void VisitThisExpression ()
    {
      var thisExpression = new ThisExpression (ReflectionObjectMother.GetSomeType ());
      _ilGeneratorMock.Expect (mock => mock.Emit (OpCodes.Ldarg_0));

      var result = _visitor.VisitThis (thisExpression);

      _ilGeneratorMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (thisExpression));
    }

    [Test]
    public void VisitTypeAsUnderlyingSystemTypeExpression ()
    {
      var typeWithUnderlyingSystemType = MutableTypeObjectMother.CreateForExistingType();
      var innerExpression = Expression.Constant (null, typeWithUnderlyingSystemType);
      var expression = new TypeAsUnderlyingSystemTypeExpression (innerExpression);

      _childExpressionEmitterMock.Expect (mock => mock.EmitChildExpression (innerExpression));

      // No calls to _ilGeneratorMock expected.

      var result = _visitor.VisitTypeAsUnderlyingSystemType (expression);

      _childExpressionEmitterMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException), ExpectedMessage = "OriginalBodyExpression must be replaced before code generation.")]
    public void VisitOriginalBodyExpression ()
    {
      var expression = new OriginalBodyExpression (ReflectionObjectMother.GetSomeType(), Enumerable.Empty<Expression>());

      _visitor.VisitOriginalBody (expression);
    }
  }
}