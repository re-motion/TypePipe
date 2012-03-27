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
using Microsoft.Scripting.Ast;
using Remotion.TypePipe.Expressions;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Defines the characteristics of a constructor.
  /// </summary>
  /// <remarks>
  /// This is used by <see cref="MutableConstructorInfo"/> to represent the original constructor, before any mutations.
  /// </remarks>
  public class UnderlyingConstructorInfoDescriptor
  {
    public static UnderlyingConstructorInfoDescriptor Create (
        MethodAttributes attributes,
        IEnumerable<ParameterDeclaration> parameterDeclarations,
        IEnumerable<Expression> arguments)
    {
      ArgumentUtility.CheckNotNull ("parameterDeclarations", parameterDeclarations);
      ArgumentUtility.CheckNotNull ("arguments", arguments);

      return CreateInternal(null, attributes, parameterDeclarations, arguments);
    }

    public static UnderlyingConstructorInfoDescriptor Create (ConstructorInfo originalConstructorInfo)
    {
      ArgumentUtility.CheckNotNull ("originalConstructorInfo", originalConstructorInfo);

      var parameterInfos = originalConstructorInfo.GetParameters();
      var parameterDeclarations = parameterInfos.Select (pi => new ParameterDeclaration (pi.ParameterType, pi.Name, pi.Attributes));
      var arguments = parameterInfos.Select (pi => Expression.Parameter (pi.ParameterType, pi.Name)).ToArray();

      return CreateInternal (originalConstructorInfo, originalConstructorInfo.Attributes, parameterDeclarations, arguments);
    }

    private static UnderlyingConstructorInfoDescriptor CreateInternal (
        ConstructorInfo originalConstructorInfo,
        MethodAttributes attributes,
        IEnumerable<ParameterDeclaration> parameterDeclarations,
        IEnumerable<Expression> arguments)
    {
      var body = new OriginalBodyExpression (typeof (void), arguments);
      return new UnderlyingConstructorInfoDescriptor (originalConstructorInfo, attributes, parameterDeclarations, arguments, body);
    }

    private readonly ConstructorInfo _underlyingSystemConstructorInfo;
    private readonly MethodAttributes _attributes;
    private readonly IEnumerable<ParameterDeclaration> _parameterDeclarations;
    private readonly IEnumerable<Expression> _arguments;
    private readonly Expression _body;

    private UnderlyingConstructorInfoDescriptor (
        ConstructorInfo underlyingSystemConstructorInfo,
        MethodAttributes attributes,
        IEnumerable<ParameterDeclaration> parameterDeclarations,
        IEnumerable<Expression> arguments,
        Expression body)
    {
      _underlyingSystemConstructorInfo = underlyingSystemConstructorInfo;
      _attributes = attributes;
      _parameterDeclarations = parameterDeclarations;
      _arguments = arguments;
      _body = body;
    }

    public ConstructorInfo UnderlyingSystemConstructorInfo
    {
      get { return _underlyingSystemConstructorInfo; }
    }

    public MethodAttributes Attributes
    {
      get { return _attributes; }
    }

    public IEnumerable<ParameterDeclaration> ParameterDeclarations
    {
      get { return _parameterDeclarations; }
    }

    public IEnumerable<Expression> Arguments
    {
      get { return _arguments; }
    }

    public Expression Body
    {
      get { return _body; }
    }
  }
}