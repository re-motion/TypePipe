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
using Remotion.Reflection.MemberSignatures;
using Remotion.Utilities;
using Remotion.FunctionalProgramming;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Defines the characteristics of a method.
  /// </summary>
  /// <remarks>
  /// This is used by <see cref="MutableMethodInfo"/> to represent the original method, before any mutations.
  /// </remarks>
  public class UnderlyingMethodInfoDescriptor : UnderlyingMethodBaseDescriptor<MethodInfo>
  {
    public static UnderlyingMethodInfoDescriptor Create (
        string name,
        MethodAttributes attributes,
        Type returnType,
        IEnumerable<ParameterDeclaration> parameterDeclarations,
        MethodInfo baseMethod,
        bool isGenericMethod,
        bool isGenericMethodDefinition,
        bool containsGenericParameters,
        Expression body)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("returnType", returnType);
      ArgumentUtility.CheckNotNull ("parameterDeclarations", parameterDeclarations);
      // Base method may be null
      ArgumentUtility.CheckNotNull ("body", body);

      if (!returnType.IsAssignableFrom (body.Type))
        throw new ArgumentException ("The body's return type must be assignable to the method return type.", "body");

      var parameterDeclarationReadOnlyCollection = parameterDeclarations.ToList().AsReadOnly();
      return new UnderlyingMethodInfoDescriptor (
          null,
          name,
          attributes,
          returnType,
          parameterDeclarationReadOnlyCollection,
          baseMethod,
          isGenericMethod,
          isGenericMethodDefinition,
          containsGenericParameters,
          body);
    }

    public static UnderlyingMethodInfoDescriptor Create (MethodInfo originalMethod, IRelatedMethodFinder relatedMethodFinder)
    {
      ArgumentUtility.CheckNotNull ("originalMethod", originalMethod);
      ArgumentUtility.CheckNotNull ("relatedMethodFinder", relatedMethodFinder);

      // TODO 4695
      // If method visibility is FamilyOrAssembly, change it to Family because the mutated type will be put into a different assembly.
      var attributes = GetMethodAttributesWithAdjustedVisibiity (originalMethod);
      var parameterDeclarations = ParameterDeclaration.CreateForEquivalentSignature (originalMethod).ToList ().AsReadOnly ();
      var baseMethod = GetBaseMethod(originalMethod, relatedMethodFinder);
      var body = CreateOriginalBodyExpression (originalMethod.ReturnType, parameterDeclarations);

      return new UnderlyingMethodInfoDescriptor (
          originalMethod,
          originalMethod.Name,
          attributes,
          originalMethod.ReturnType,
          parameterDeclarations,
          baseMethod,
          originalMethod.IsGenericMethod,
          originalMethod.IsGenericMethodDefinition,
          originalMethod.ContainsGenericParameters,
          body);
    }

    private static MethodInfo GetBaseMethod (MethodInfo method, IRelatedMethodFinder relatedMethodFinder)
    {
      var rootDefinition = method.GetBaseDefinition ();
      if (method.Equals (rootDefinition))
        return null;

      return relatedMethodFinder.GetBaseMethod (method.Name, MethodSignature.Create (method), method.DeclaringType.BaseType);
    }

    private readonly Type _returnType;
    private readonly MethodInfo _baseMethod;
    private readonly bool _isGenericMethod;
    private readonly bool _isGenericMethodDefinition;
    private readonly bool _containsGenericParameters;

    private UnderlyingMethodInfoDescriptor (
        MethodInfo underlyingSystemMethodInfo,
        string name,
        MethodAttributes attributes,
        Type returnType,
        ReadOnlyCollection<ParameterDeclaration> parameterDeclarations,
        MethodInfo baseMethod,
        bool isGenericMethod,
        bool isGenericMethodDefinition,
        bool containsGenericParameters,
        Expression body)
        : base (underlyingSystemMethodInfo, name, attributes, parameterDeclarations, body)
    {
      Assertion.IsNotNull (returnType);
      Assertion.IsTrue (returnType.IsAssignableFrom (body.Type));

      _returnType = returnType;
      _baseMethod = baseMethod;
      _isGenericMethod = isGenericMethod;
      _isGenericMethodDefinition = isGenericMethodDefinition;
      _containsGenericParameters = containsGenericParameters;
    }

    public Type ReturnType
    {
      get { return _returnType; }
    }

    public MethodInfo BaseMethod
    {
      get { return _baseMethod; }
    }

    public bool IsGenericMethod
    {
      get { return _isGenericMethod; }
    }

    public bool IsGenericMethodDefinition
    {
      get { return _isGenericMethodDefinition; }
    }

    public bool ContainsGenericParameters
    {
      get { return _containsGenericParameters; }
    }
  }
}