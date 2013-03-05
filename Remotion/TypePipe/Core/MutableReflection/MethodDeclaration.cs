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
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Holds all values required to declare a method.
  /// </summary>
  /// <remarks>This can be used to create methods that have an equal or similiar signature to an existing method.</remarks>
  public class MethodDeclaration
  {
    public static MethodDeclaration CreateForEquivalentSignature (MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);

      return null;
    }

    private readonly ReadOnlyCollection<GenericParameterDeclaration> _genericParameters;
    private readonly Func<GenericParameterContext, Type> _returnTypeProvider;
    private readonly Func<GenericParameterContext, IEnumerable<ParameterDeclaration>> _parameterProvider;

    public MethodDeclaration (
        IEnumerable<GenericParameterDeclaration> genericParameters,
        Func<GenericParameterContext, Type> returnTypeProvider,
        Func<GenericParameterContext, IEnumerable<ParameterDeclaration>> parameterProvider)
    {
      ArgumentUtility.CheckNotNull ("genericParameters", genericParameters);
      ArgumentUtility.CheckNotNull ("returnTypeProvider", returnTypeProvider);
      ArgumentUtility.CheckNotNull ("parameterProvider", parameterProvider);

      _genericParameters = genericParameters.ToList().AsReadOnly();
      _returnTypeProvider = returnTypeProvider;
      _parameterProvider = parameterProvider;
    }

    public ReadOnlyCollection<GenericParameterDeclaration> GenericParameters
    {
      get { return _genericParameters; }
    }

    public Func<GenericParameterContext, Type> ReturnTypeProvider
    {
      get { return _returnTypeProvider; }
    }

    public Func<GenericParameterContext, IEnumerable<ParameterDeclaration>> ParameterProvider
    {
      get { return _parameterProvider; }
    }
  }
}