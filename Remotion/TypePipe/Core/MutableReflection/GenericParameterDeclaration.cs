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
using System.Reflection;
using Remotion.FunctionalProgramming;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Holds all information required to declare a generic parameter on a type or method.
  /// </summary>
  /// <remarks>
  /// This is used by <see cref="ProxyType.AddGenericMethod"/> when declaring generic methods.
  /// </remarks>
  public class GenericParameterDeclaration
  {
    public static readonly GenericParameterDeclaration[] None = new GenericParameterDeclaration[0];

    public static GenericParameterDeclaration CreateEquivalent (Type genericParameter)
    {
      ArgumentUtility.CheckNotNull ("genericParameter", genericParameter);

      if (!genericParameter.IsGenericParameter)
        throw new ArgumentException ("The specified type must be a generic parameter (IsGenericParameter must be true).", "genericParameter");

      var oldGenericParameters = genericParameter.DeclaringMethod.GetGenericArguments();
      var instantiations = new Dictionary<TypeInstantiationInfo, TypeInstantiation>();

      Func<GenericParameterContext, Type> baseConstraintProvider =
          ctx => Substitute (oldGenericParameters, ctx, instantiations, genericParameter.BaseType);
      Func<GenericParameterContext, IEnumerable<Type>> interfaceConstraintsProvider =
          ctx => genericParameter
                     .GetGenericParameterConstraints()
                     .Where (g => g.IsInterface)
                     .Select (g => Substitute (oldGenericParameters, ctx, instantiations, g));

      return new GenericParameterDeclaration (
          genericParameter.Name, genericParameter.GenericParameterAttributes, baseConstraintProvider, interfaceConstraintsProvider);
    }

    private static Type Substitute (
        Type[] oldGenericParameters,
        GenericParameterContext genericParameterContext,
        Dictionary<TypeInstantiationInfo, TypeInstantiation> instantiations,
        Type type)
    {
      var parametersToArguments = oldGenericParameters.Zip (genericParameterContext.GenericParameters).ToDictionary (t => t.Item1, t => t.Item2);
      return TypeSubstitutionUtility.SubstituteGenericParameters (parametersToArguments, instantiations, type);
    }

    private readonly string _name;
    private readonly GenericParameterAttributes _attributes;
    private readonly Func<GenericParameterContext, Type> _baseConstraintProvider;
    private readonly Func<GenericParameterContext, IEnumerable<Type>> _interfaceConstraintsProvider;

    public GenericParameterDeclaration (
        string name,
        GenericParameterAttributes attributes = GenericParameterAttributes.None,
        Func<GenericParameterContext, Type> baseConstraintProvider = null,
        Func<GenericParameterContext, IEnumerable<Type>> interfaceConstraintsProvider = null)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      // Base constraint provider may be null.
      // Interface constraint provider may be null.

      _name = name;
      _attributes = attributes;
      _baseConstraintProvider = baseConstraintProvider ?? (ctx => typeof (object));
      _interfaceConstraintsProvider = interfaceConstraintsProvider ?? (ctx => Type.EmptyTypes);
    }

    public GenericParameterAttributes Attributes
    {
      get { return _attributes; }
    }

    public string Name
    {
      get { return _name; }
    }

    public Func<GenericParameterContext, Type> BaseConstraintProvider
    {
      get { return _baseConstraintProvider; }
    }

    public Func<GenericParameterContext, IEnumerable<Type>> InterfaceConstraintsProvider
    {
      get { return _interfaceConstraintsProvider; }
    }
  }
}