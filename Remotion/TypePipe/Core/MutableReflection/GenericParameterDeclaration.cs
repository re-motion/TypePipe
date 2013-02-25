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
using Remotion.TypePipe.MutableReflection.SignatureBuilding;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Holds all values required to declare a generic parameter.
  /// </summary>
  /// <remarks>
  /// This is used by <see cref="ProxyType.AddGenericMethod"/> when declaring generic methods.
  /// </remarks>
  public class GenericParameterDeclaration
  {
    private readonly string _name;
    private readonly GenericParameterAttributes _attributes;
    private readonly Func<GenericParametersContext, Type> _baseConstraintProvider;
    private readonly Func<GenericParametersContext, IEnumerable<Type>> _interfaceConstraintsProvider;

    public GenericParameterDeclaration (
        string name,
        GenericParameterAttributes attributes = GenericParameterAttributes.None,
        Func<GenericParametersContext, Type> baseConstraintProvider = null,
        Func<GenericParametersContext, IEnumerable<Type>> interfaceConstraintsProvider = null)
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

    public Func<GenericParametersContext, Type> BaseConstraintProvider
    {
      get { return _baseConstraintProvider; }
    }

    public Func<GenericParametersContext, IEnumerable<Type>> InterfaceConstraintsProvider
    {
      get { return _interfaceConstraintsProvider; }
    }
  }
}