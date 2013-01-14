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
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Descriptors
{
  /// <summary>
  /// Defines the characteristics of a method.
  /// </summary>
  /// <remarks>
  /// This is used by <see cref="MutableMethodInfo"/> to represent a method, before any mutations.
  /// </remarks>
  public class MethodDescriptor : MethodBaseDescriptor<MethodInfo>
  {
    public static MethodDescriptor Create (
        string name,
        MethodAttributes attributes,
        ParameterDescriptor returnParameter,
        IEnumerable<ParameterDescriptor> parameters,
        MethodInfo baseMethod,
        bool isGenericMethod,
        bool isGenericMethodDefinition,
        bool containsGenericParameters,
        Expression body)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      // returnParameter may be null
      ArgumentUtility.CheckNotNull ("parameters", parameters);
      // baseMethod may be null
      // body may be null

      if (body == null && !attributes.IsSet (MethodAttributes.Abstract))
        throw new ArgumentException ("Non-abstract method must have a body.", "body");

      if (body != null && !returnParameter.Type.IsAssignableFrom (body.Type))
        throw new ArgumentException ("The body's return type must be assignable to the method return type.", "body");

      var readOnlyParameters = parameters.ToList().AsReadOnly();
      return new MethodDescriptor (
          name,
          attributes,
          returnParameter,
          readOnlyParameters,
          baseMethod,
          isGenericMethod,
          isGenericMethodDefinition,
          containsGenericParameters,
          EmptyCustomAttributeDataProvider,
          body);
    }

    public static MethodDescriptor Create (MethodInfo underlyingMethod, IRelatedMethodFinder relatedMethodFinder)
    {
      ArgumentUtility.CheckNotNull ("underlyingMethod", underlyingMethod);
      ArgumentUtility.CheckNotNull ("relatedMethodFinder", relatedMethodFinder);

      // TODO 4695
      // If method visibility is FamilyOrAssembly, change it to Family because the mutated type will be put into a different assembly.
      var attributes = underlyingMethod.Attributes.AdjustVisibilityForAssemblyBoundaries();
      var returnParameter = ParameterDescriptor.Create (underlyingMethod.ReturnParameter);
      var parameters = ParameterDescriptor.CreateFromMethodBase (underlyingMethod);
      var baseMethod = relatedMethodFinder.GetBaseMethod (underlyingMethod);
      var customAttributeDataProvider = GetCustomAttributeProvider (underlyingMethod);
      var body = underlyingMethod.IsAbstract ? null : CreateOriginalBodyExpression (underlyingMethod, underlyingMethod.ReturnType, parameters);

      return new MethodDescriptor (
          underlyingMethod.Name,
          attributes,
          returnParameter,
          parameters,
          baseMethod,
          underlyingMethod.IsGenericMethod,
          underlyingMethod.IsGenericMethodDefinition,
          underlyingMethod.ContainsGenericParameters,
          customAttributeDataProvider,
          body);
    }

    private readonly ParameterDescriptor _returnParameter;
    private readonly MethodInfo _baseMethod;
    private readonly bool _isGenericMethod;
    private readonly bool _isGenericMethodDefinition;
    private readonly bool _containsGenericParameters;

    private MethodDescriptor (
        string name,
        MethodAttributes attributes,
        ParameterDescriptor returnParameter,
        ReadOnlyCollection<ParameterDescriptor> parameters,
        MethodInfo baseMethod,
        bool isGenericMethod,
        bool isGenericMethodDefinition,
        bool containsGenericParameters,
        Func<ReadOnlyCollection<ICustomAttributeData>> customAttributeDataProvider,
        Expression body)
        : base (name, attributes, parameters, customAttributeDataProvider, body)
    {
      Assertion.IsNotNull (returnParameter);
      Assertion.IsNotNull (customAttributeDataProvider);
      Assertion.IsTrue (body == null || returnParameter.Type.IsAssignableFrom (body.Type));

      _returnParameter = returnParameter;
      _baseMethod = baseMethod;
      _isGenericMethod = isGenericMethod;
      _isGenericMethodDefinition = isGenericMethodDefinition;
      _containsGenericParameters = containsGenericParameters;
    }

    public ParameterDescriptor ReturnParameter
    {
      get { return _returnParameter; }
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