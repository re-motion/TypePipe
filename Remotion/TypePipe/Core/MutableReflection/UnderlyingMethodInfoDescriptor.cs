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
        IEnumerable<UnderlyingParameterInfoDescriptor> parameterDescriptors,
        MethodInfo baseMethod,
        bool isGenericMethod,
        bool isGenericMethodDefinition,
        bool containsGenericParameters,
        Expression body)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("returnType", returnType);
      ArgumentUtility.CheckNotNull ("parameterDescriptors", parameterDescriptors);
      // Base method may be null
      ArgumentUtility.CheckNotNull ("body", body);

      if (!returnType.IsAssignableFrom (body.Type))
        throw new ArgumentException ("The body's return type must be assignable to the method return type.", "body");

      var readonlyParameterDescriptors = parameterDescriptors.ToList().AsReadOnly();
      return new UnderlyingMethodInfoDescriptor (
          null,
          name,
          attributes,
          returnType,
          readonlyParameterDescriptors,
          baseMethod,
          isGenericMethod,
          isGenericMethodDefinition,
          containsGenericParameters,
          () => new ReadOnlyCollection<ICustomAttributeData> (new ICustomAttributeData[0]),
          body);
    }

    public static UnderlyingMethodInfoDescriptor Create (MethodInfo originalMethod, IRelatedMethodFinder relatedMethodFinder)
    {
      ArgumentUtility.CheckNotNull ("originalMethod", originalMethod);
      ArgumentUtility.CheckNotNull ("relatedMethodFinder", relatedMethodFinder);

      // TODO 4695
      // If method visibility is FamilyOrAssembly, change it to Family because the mutated type will be put into a different assembly.
      var attributes = originalMethod.Attributes.AdjustVisibilityForAssemblyBoundaries();
      var parameterDeclarations = UnderlyingParameterInfoDescriptor.CreateFromMethodBase (originalMethod).ToList().AsReadOnly();
      var baseMethod = relatedMethodFinder.GetBaseMethod (originalMethod);
      var customAttributeDataProvider = GetCustomAttributeProvider (originalMethod);
      var body = CreateOriginalBodyExpression (originalMethod, originalMethod.ReturnType, parameterDeclarations);

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
          customAttributeDataProvider,
          body);
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
        ReadOnlyCollection<UnderlyingParameterInfoDescriptor> parameterDescriptors,
        MethodInfo baseMethod,
        bool isGenericMethod,
        bool isGenericMethodDefinition,
        bool containsGenericParameters,
        Func<ReadOnlyCollection<ICustomAttributeData>> customAttributeDataProvider,
        Expression body)
        : base (underlyingSystemMethodInfo, name, attributes, parameterDescriptors, customAttributeDataProvider, body)
    {
      Assertion.IsNotNull (returnType);
      Assertion.IsNotNull (customAttributeDataProvider);
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