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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Expressions;

namespace Remotion.TypePipe.Expressions
{
  /// <summary>
  /// Defines an interface for classes handling <see cref="IPrimitiveTypePipeExpression"/> instances.
  /// </summary>
  public interface IPrimitiveTypePipeExpressionVisitor
  {
    Expression VisitThis (ThisExpression node);
    Expression VisitNewDelegate (NewDelegateExpression node);

    // TODO review: Easy way to remove these methods from the code-generation-independent interface?
    Expression VisitBox (BoxExpression node);
    Expression VisitUnbox (UnboxExpression node);
  }
}