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
  /// Defines the characteristics of a constructor.
  /// </summary>
  /// <remarks>
  /// This is used by <see cref="MutableConstructorInfo"/> to represent the original constructor, before any mutations.
  /// </remarks>
  public class UnderlyingConstructorInfoDescriptor
  {
    public static UnderlyingConstructorInfoDescriptor Create (
        MethodAttributes attributes, IEnumerable<ParameterDeclaration> parameterDeclarations, Expression body)
    {
      ArgumentUtility.CheckNotNull ("parameterDeclarations", parameterDeclarations);
      ArgumentUtility.CheckNotNull ("body", body);

      // TODO 4743: Check body type: must be void.
      var parameterDeclarationReadOnlyCollection = parameterDeclarations.ToList ().AsReadOnly ();
      return new UnderlyingConstructorInfoDescriptor (null, attributes, parameterDeclarationReadOnlyCollection, body);
    }

    public static UnderlyingConstructorInfoDescriptor Create (ConstructorInfo originalConstructorInfo)
    {
      ArgumentUtility.CheckNotNull ("originalConstructorInfo", originalConstructorInfo);

      // TODO 4695
      // If ctor visibility is FamilyOrAssembly, change it to Family because the mutated type will be put into a different assembly.
      var attributes = originalConstructorInfo.IsFamilyOrAssembly
                           ? ChangeVisibility (originalConstructorInfo.Attributes, MethodAttributes.Family)
                           : originalConstructorInfo.Attributes;
      var parameterDeclarations = 
          originalConstructorInfo.GetParameters()
              .Select (pi => new ParameterDeclaration (pi.ParameterType, pi.Name, pi.Attributes))
              .ToList()
              .AsReadOnly();
      var parameterExpressions = parameterDeclarations.Select (pd => pd.Expression);
      var body = new OriginalBodyExpression (typeof (void), parameterExpressions.Cast<Expression>());
      
      return new UnderlyingConstructorInfoDescriptor (originalConstructorInfo, attributes, parameterDeclarations, body);
    }

    private static MethodAttributes ChangeVisibility (MethodAttributes originalAttributes, MethodAttributes newVisibility)
    {
      return (originalAttributes & ~MethodAttributes.MemberAccessMask) | newVisibility;
    }

    private readonly ConstructorInfo _underlyingSystemConstructorInfo;
    private readonly MethodAttributes _attributes;
    private readonly ReadOnlyCollection<ParameterDeclaration> _parameterDeclarations;
    private readonly Expression _body;

    private UnderlyingConstructorInfoDescriptor (
        ConstructorInfo underlyingSystemConstructorInfo,
        MethodAttributes attributes,
        ReadOnlyCollection<ParameterDeclaration> parameterDeclarations, 
        Expression body)
    {
      // TODO 4743: Convert to assertions.
      ArgumentUtility.CheckNotNull ("parameterDeclarations", parameterDeclarations);
      ArgumentUtility.CheckNotNull ("body", body);

      // TODO 4743: Assert body type is always void.

      _underlyingSystemConstructorInfo = underlyingSystemConstructorInfo;
      _attributes = attributes;
      _parameterDeclarations = parameterDeclarations;
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