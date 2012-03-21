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
using Remotion.Utilities;

namespace Remotion.TypePipe.Expressions
{
  /// <summary>
  /// Base class for custom expressions that also implement the <see cref="ITypePipeExpression"/> interface.
  /// </summary>
  public abstract class TypePipeExpressionBase : Expression, ITypePipeExpression
  {
    private readonly Type _type;

    protected TypePipeExpressionBase (Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);
      _type = type;
    }

    public override Type Type
    {
      get { return _type; }
    }

    public override ExpressionType NodeType
    {
      get { return ExpressionType.Extension; }
    }

    public abstract Expression Accept (ITypePipeExpressionVisitor visitor);
  }
}