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

namespace Remotion.TypePipe.Expressions
{
  /// <summary>
  /// Identifies a primitive custom expressions, i.e., an <see cref="Expression"/> that cannot be reduced to a standard expression and must be
  /// handled during code generation.
  /// Implementations of this interface must return <see cref="PrimitiveTypePipeExpressionBase.TypePipeExpressionType"/> from 
  /// <see cref="Expression.NodeType"/>.
  /// Expressions of this type can be handled using an <see cref="IPrimitiveTypePipeExpressionVisitor"/>.
  /// </summary>
  /// <seealso cref="PrimitiveTypePipeExpressionBase"/>
  /// <seealso cref="PrimitiveTypePipeExpressionVisitorBase"/>
  public interface IPrimitiveTypePipeExpression
  {
    Expression Accept (IPrimitiveTypePipeExpressionVisitor visitor);
  }
}