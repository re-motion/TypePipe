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
using System.Reflection;
using Remotion.TypePipe.Expressions;

// This is on purpose.
// ReSharper disable CheckNamespace
namespace Microsoft.Scripting.Ast
// ReSharper restore CheckNamespace
{
  /// <summary>
  /// Provides factory methods for creating <see cref="ITypePipeExpression"/>.
  /// </summary>
  public partial class Expression
  {
    /// <summary>
    /// Creates an expression which represents the instantiation of a delegate type.
    /// </summary>
    /// <param name="delegateType">Type of the delegate.</param>
    /// <param name="target">The target instance, <see langword="null"/> for static methods.</param>
    /// <param name="targetMethod">The target method.</param>
    /// <returns></returns>
    public static NewDelegateExpression NewDelegate (Type delegateType, Expression target, MethodInfo targetMethod)
    {
      return null;
    }
  }
}