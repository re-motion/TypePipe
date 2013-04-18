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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Remotion.TypePipe.Expressions;
using Remotion.Utilities;

// This is on purpose.
// ReSharper disable CheckNamespace
namespace Microsoft.Scripting.Ast
// ReSharper restore CheckNamespace
{
  // Do not add docs to this class.
  // Provides factory methods for creating custom expressions.
  public partial class Expression
  {
    /// <summary>
    /// Creates a new <see cref="NewDelegateExpression"/> that represents the instantiation of a delegate.
    /// </summary>
    /// <param name="delegateType">The type of the delegate.</param>
    /// <param name="target">The target instance or <see langword="null"/> for static methods.</param>
    /// <param name="method">The method.</param>
    /// <returns></returns>
    public static NewDelegateExpression NewDelegate (Type delegateType, Expression target, MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("delegateType", delegateType);
      // target can be null for static methods
      ArgumentUtility.CheckNotNull ("method", method);

      return new NewDelegateExpression (delegateType, target, method);
    }

    // TODO 5370
    public static NewArrayExpression ArrayConstant<T> (IEnumerable<T> elements)
    {
      ArgumentUtility.CheckNotNull ("elements", elements);

      return Expression.NewArrayInit (typeof (T), elements.Select (e => Expression.Constant (e)).Cast<Expression>());
    }
  }
}