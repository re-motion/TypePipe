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
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public class ArgumentTestHelper
  {
    private readonly object[] _values;
    private readonly Type[] _types;
    private readonly ParameterDeclaration[] _parameterDeclarations;
    private readonly ConstantExpression[] _constantExpressions;

    public ArgumentTestHelper (params object[] values)
    {
      ArgumentUtility.CheckNotNull ("values", values);

      _values = values;
      _types = values.Select (value => value.GetType()).ToArray();
      _parameterDeclarations = _types.Select ((t, i) => new ParameterDeclaration (t, "p" + i)).ToArray ();
      _constantExpressions = values.Select (Expression.Constant).ToArray();
    }

    public object[] Values
    {
      get { return _values; }
    }

    public Type[] Types
    {
      get { return _types; }
    }

    public ParameterDeclaration[] ParameterDeclarations
    {
      get { return _parameterDeclarations; }
    }

    public ConstantExpression[] ConstantExpressions
    {
      get { return _constantExpressions; }
    }

    public Expression[] Expressions
    {
      get { return _constantExpressions.Cast<Expression>().ToArray(); }
    }
  }
}