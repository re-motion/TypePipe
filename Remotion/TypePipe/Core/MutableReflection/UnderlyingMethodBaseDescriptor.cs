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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Microsoft.Scripting.Ast;
using Remotion.TypePipe.Expressions;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Defines the characteristics of a <see cref="MethodBase"/>, i.e., a method or constructor.
  /// </summary>
  /// <typeparam name="TMethodBase">The concrete type of method (<see cref="MethodInfo"/> or <see cref="ConstructorInfo"/>).</typeparam>
  /// <remarks>
  /// This is used as a base class for <see cref="UnderlyingMethodInfoDescriptor"/> and <see cref="UnderlyingConstructorInfoDescriptor"/>.
  /// </remarks>
  public abstract class UnderlyingMethodBaseDescriptor<TMethodBase> 
      where TMethodBase : MethodBase
  {
    protected static Expression CreateOriginalBodyExpression (Type returnType, IEnumerable<ParameterDeclaration> parameterDeclarations)
    {
      ArgumentUtility.CheckNotNull ("returnType", returnType);
      ArgumentUtility.CheckNotNull ("parameterDeclarations", parameterDeclarations);

      var parameterExpressions = parameterDeclarations.Select (pd => pd.Expression);
      return new OriginalBodyExpression (returnType, parameterExpressions.Cast<Expression> ());
    }

    protected static MethodAttributes GetMethodAttributesWithAdjustedVisibiity (TMethodBase originalMethodBase)
    {
      ArgumentUtility.CheckNotNull ("originalMethodBase", originalMethodBase);

      return originalMethodBase.IsFamilyOrAssembly
                 ? MethodAttributeUtility.ChangeVisibility (originalMethodBase.Attributes, MethodAttributes.Family)
                 : originalMethodBase.Attributes;
    }

    private readonly TMethodBase _underlyingSystemMethodBase;
    private readonly string _name;
    private readonly MethodAttributes _attributes;
    private readonly ReadOnlyCollection<ParameterDeclaration> _parameterDeclarations;
    private readonly Expression _body;

    protected UnderlyingMethodBaseDescriptor (
        TMethodBase underlyingSystemMethodBase,
        string name,
        MethodAttributes attributes,
        ReadOnlyCollection<ParameterDeclaration> parameterDeclarations,
        Expression body)
    {
      Assertion.IsFalse (string.IsNullOrEmpty (name));
      Assertion.IsNotNull (parameterDeclarations);
      Assertion.IsNotNull (body);

      _underlyingSystemMethodBase = underlyingSystemMethodBase;
      _name = name;
      _attributes = attributes;
      _parameterDeclarations = parameterDeclarations;
      _body = body;
    }

    public TMethodBase UnderlyingSystemMethodBase
    {
      get { return _underlyingSystemMethodBase; }
    }

    public string Name
    {
      get { return _name; }
    }

    public MethodAttributes Attributes
    {
      get { return _attributes; }
    }

    public ReadOnlyCollection<ParameterDeclaration> ParameterDeclarations
    {
      get { return _parameterDeclarations; }
    }

    public Expression Body
    {
      get { return _body; }
    }
  }
}