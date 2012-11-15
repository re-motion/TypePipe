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
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Defines the characteristics of a parameter.
  /// </summary>
  /// <remarks>
  /// This is used by <see cref="MutableParameterInfo"/> to represent a parameter, before any mutations.
  /// </remarks>
  public class UnderlyingParameterInfoDescriptor : UnderlyingInfoDescriptorBase<ParameterInfo>
  {
    public static readonly UnderlyingParameterInfoDescriptor[] EmptyParameters = new UnderlyingParameterInfoDescriptor[0];

    public static ReadOnlyCollection<UnderlyingParameterInfoDescriptor> CreateFromDeclarations (IEnumerable<ParameterDeclaration> parameterDeclarations)
    {
      ArgumentUtility.CheckNotNull ("parameterDeclarations", parameterDeclarations);

      return parameterDeclarations.Select (Create).ToList().AsReadOnly();
    }

    public static ReadOnlyCollection<UnderlyingParameterInfoDescriptor> CreateFromMethodBase (MethodBase methodBase)
    {
      ArgumentUtility.CheckNotNull ("methodBase", methodBase);

      return methodBase.GetParameters().Select (Create).ToList().AsReadOnly();
    }

    private static UnderlyingParameterInfoDescriptor Create (ParameterDeclaration parameterDeclaration, int position)
    {
      return new UnderlyingParameterInfoDescriptor (
          null,
          parameterDeclaration.Type,
          position,
          parameterDeclaration.Name,
          parameterDeclaration.Attributes,
          EmptyCustomAttributeDataProvider);
    }

    private static UnderlyingParameterInfoDescriptor Create (ParameterInfo originalParameter)
    {
      return new UnderlyingParameterInfoDescriptor (
          originalParameter,
          originalParameter.ParameterType,
          originalParameter.Position,
          originalParameter.Name,
          originalParameter.Attributes,
          GetCustomAttributeProvider (originalParameter));
    }

    private readonly Type _type;
    private readonly int _position;
    private readonly ParameterAttributes _attributes;
    private readonly ParameterExpression _expression;

    private UnderlyingParameterInfoDescriptor (
        ParameterInfo underlyingSystemMember,
        Type type,
        int position,
        string name,
        ParameterAttributes attributes,
        Func<ReadOnlyCollection<ICustomAttributeData>> customAttributeDataProvider)
        : base (underlyingSystemMember, name, customAttributeDataProvider)
    {
      Assertion.IsNotNull (type, "type");
      Assertion.IsTrue (position >= 0);
      Assertion.IsNotNull (name, "name");

      _type = type;
      _position = position;
      _attributes = attributes;
      _expression = Microsoft.Scripting.Ast.Expression.Parameter (type, name);
    }

    public Type Type
    {
      get { return _type; }
    }

    public int Position
    {
      get { return _position; }
    }

    public ParameterAttributes Attributes
    {
      get { return _attributes; }
    }

    public ParameterExpression Expression
    {
      get { return _expression; }
    }
  }
}