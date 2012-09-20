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
  /// This is used by <see cref="MutableParameterInfo"/> to represent the original parameter, before any mutations.
  /// </remarks>
  public class UnderlyingParameterInfoDescriptor : UnderlyingInfoDescriptorBase<ParameterInfo>
  {
    public static UnderlyingParameterInfoDescriptor Create (ParameterDeclaration parameterDeclaration)
    {
      ArgumentUtility.CheckNotNull ("parameterDeclaration", parameterDeclaration);

      return new UnderlyingParameterInfoDescriptor (
          null,
          parameterDeclaration.Type,
          parameterDeclaration.Name,
          parameterDeclaration.Attributes,
          () => new ICustomAttributeData[0].ToList().AsReadOnly());
    }

    public static UnderlyingParameterInfoDescriptor Create (ParameterInfo originalParameter)
    {
      ArgumentUtility.CheckNotNull ("originalParameter", originalParameter);

      var customAttributeDataProvider = GetCustomAttributeProvider (originalParameter);

      return new UnderlyingParameterInfoDescriptor (
          originalParameter, originalParameter.ParameterType, originalParameter.Name, originalParameter.Attributes, customAttributeDataProvider);
    }

    public static IEnumerable<UnderlyingParameterInfoDescriptor> CreateFromDeclarations (IEnumerable<ParameterDeclaration> parameterDeclarations)
    {
      ArgumentUtility.CheckNotNull ("parameterDeclarations", parameterDeclarations);

      return parameterDeclarations.Select (Create);
    }

    public static IEnumerable<UnderlyingParameterInfoDescriptor> CreateFromMethodBase (MethodBase methodBase)
    {
      ArgumentUtility.CheckNotNull ("methodBase", methodBase);

      return methodBase.GetParameters ().Select (Create);
    }

    private readonly Type _type;
    private readonly ParameterAttributes _attributes;
    private readonly ParameterExpression _expression;

    private UnderlyingParameterInfoDescriptor (
        ParameterInfo underlyingSystemMember,
        Type type,
        string name,
        ParameterAttributes attributes,
        Func<ReadOnlyCollection<ICustomAttributeData>> customAttributeDataProvider)
        : base (underlyingSystemMember, name, customAttributeDataProvider)
    {
      Assertion.IsNotNull (type, "type");
      Assertion.IsNotNull (name, "name");

      _type = type;
      _attributes = attributes;
      _expression = Microsoft.Scripting.Ast.Expression.Parameter (type, name);
    }

    public Type Type
    {
      get { return _type; }
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