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
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.Implementation;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// Holds instance initialization code.
  /// </summary>
  public class InstanceInitialization
  {
    private readonly ParameterExpression _semantics = Expression.Parameter (typeof (InitializationSemantics), "initializationSemantics");
    private readonly List<Expression> _initailizations = new List<Expression>();

    /// <summary>
    /// Represents a parameter of type <see cref="InitializationSemantics"/> which can be used to determine the
    /// initialization context in which the code is executed.
    /// </summary>
    public ParameterExpression Semantics
    {
      get { return _semantics; }
    }

    /// <summary>
    /// A list of expressions that represent the initialization code.
    /// The code may use <see cref="Semantics"/> to determine in which initialization context it is executed.
    /// </summary>
    public List<Expression> Expressions
    {
      get { return _initailizations; }
    }
  }
}