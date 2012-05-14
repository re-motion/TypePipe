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
  /// Defines the characteristics of a constructor.
  /// </summary>
  /// <remarks>
  /// This is used by <see cref="MutableConstructorInfo"/> to represent the original constructor, before any mutations.
  /// </remarks>
  public class UnderlyingConstructorInfoDescriptor  : UnderlyingMethodBaseDescriptor<ConstructorInfo>
  {
    public static UnderlyingConstructorInfoDescriptor Create (
        MethodAttributes attributes, IEnumerable<ParameterDeclaration> parameterDeclarations, Expression body)
    {
      ArgumentUtility.CheckNotNull ("parameterDeclarations", parameterDeclarations);
      ArgumentUtility.CheckNotNull ("body", body);

      if (body.Type != typeof(void))
        throw new ArgumentException ("Constructor bodies must have void return type.", "body");

      var parameterDeclarationReadOnlyCollection = parameterDeclarations.ToList ().AsReadOnly ();
      return new UnderlyingConstructorInfoDescriptor (null, attributes, parameterDeclarationReadOnlyCollection, body);
    }

    public static UnderlyingConstructorInfoDescriptor Create (ConstructorInfo originalConstructor)
    {
      ArgumentUtility.CheckNotNull ("originalConstructor", originalConstructor);

      // TODO 4695
      // If ctor visibility is FamilyOrAssembly, change it to Family because the mutated type will be put into a different assembly.
      var attributes = originalConstructor.Attributes.AdjustVisibilityForAssemblyBoundaries();
      var parameterDeclarations = ParameterDeclaration.CreateForEquivalentSignature(originalConstructor).ToList().AsReadOnly();
      var body = CreateOriginalBodyExpression (typeof (void), parameterDeclarations);

      return new UnderlyingConstructorInfoDescriptor (originalConstructor, attributes, parameterDeclarations, body);
    }

    private UnderlyingConstructorInfoDescriptor (
        ConstructorInfo underlyingSystemMethodBase,
        MethodAttributes attributes,
        ReadOnlyCollection<ParameterDeclaration> parameterDeclarations, 
        Expression body)
      : base (underlyingSystemMethodBase, ".ctor", attributes, parameterDeclarations, body)
    {
      Assertion.IsTrue (body.Type == typeof (void));
    }
  }
}