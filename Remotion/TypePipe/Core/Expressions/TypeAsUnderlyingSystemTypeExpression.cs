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
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.Expressions
{
  /// <summary>
  /// Represents an <see cref="Expression"/> of a <see cref="MutableType"/> as its <see cref="MutableType.UnderlyingSystemType"/>.
  /// </summary>
  public class TypeAsUnderlyingSystemTypeExpression : TypePipeExpressionBase
  {
    private readonly Expression _innerExpression;

    public TypeAsUnderlyingSystemTypeExpression (Expression innerExpression)
      : base (ArgumentUtility.CheckNotNull ("innerExpression", innerExpression).Type.UnderlyingSystemType)
    {
      _innerExpression = innerExpression;
    }

    public Expression InnerExpression
    {
      get { return _innerExpression; }
    }

    public override Expression Accept (ITypePipeExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      return visitor.VisitTypeAsUnderlyingSystemType (this);
    }
    
    protected internal override Expression VisitChildren (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      
      var newInnerExpression = visitor.Visit (_innerExpression);
      if (newInnerExpression != _innerExpression)
        return new TypeAsUnderlyingSystemTypeExpression (newInnerExpression);
      else
        return this;
    }
  }
}