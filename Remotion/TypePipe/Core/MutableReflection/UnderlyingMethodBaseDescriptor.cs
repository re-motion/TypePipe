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
  public abstract class UnderlyingMethodBaseDescriptor<TMethodBase> : UnderlyingInfoDescriptorBase<TMethodBase>
      where TMethodBase : MethodBase
  {
    protected static Expression CreateOriginalBodyExpression (
        MethodBase methodBase, Type returnType, IEnumerable<UnderlyingParameterInfoDescriptor> parameterDescriptors)
    {
      ArgumentUtility.CheckNotNull ("methodBase", methodBase);
      ArgumentUtility.CheckNotNull ("returnType", returnType);
      ArgumentUtility.CheckNotNull ("parameterDescriptors", parameterDescriptors);

      var parameterExpressions = parameterDescriptors.Select (pd => pd.Expression);
      return new OriginalBodyExpression (methodBase, returnType, parameterExpressions.Cast<Expression>());
    }

    private readonly MethodAttributes _attributes;
    private readonly ReadOnlyCollection<UnderlyingParameterInfoDescriptor> _parameterDescriptors;
    private readonly Expression _body;

    protected UnderlyingMethodBaseDescriptor (
        TMethodBase underlyingSystemMethodMember,
        string name,
        MethodAttributes attributes,
        ReadOnlyCollection<UnderlyingParameterInfoDescriptor> parameterDescriptors,
        Func<ReadOnlyCollection<ICustomAttributeData>> customAttributeDataProvider,
        Expression body)
        : base (underlyingSystemMethodMember, name, customAttributeDataProvider)
    {
      Assertion.IsFalse (string.IsNullOrEmpty (name));
      Assertion.IsNotNull (parameterDescriptors);
      Assertion.IsNotNull (body);

      _attributes = attributes;
      _parameterDescriptors = parameterDescriptors;
      _body = body;
    }

    public MethodAttributes Attributes
    {
      get { return _attributes; }
    }

    public ReadOnlyCollection<UnderlyingParameterInfoDescriptor> ParameterDescriptors
    {
      get { return _parameterDescriptors; }
    }

    public Expression Body
    {
      get { return _body; }
    }
  }
}