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
    public GenericParameterDeclaration (
        string name,
        GenericParameterAttributes attributes = GenericParameterAttributes.None,
        Func<GenericParametersContext, Type> baseConstraintProvider = null,
        Func<GenericParametersContext, IEnumerable<Type>> interfaceConstraintsProvider = null)
    {
    }

    //public GenericParameterDeclaration (
    //    string name,
    //    GenericParameterAttributes attributes = GenericParameterAttributes.None,
    //    Type baseConstraint = null,
    //    IEnumerable<Type> interfaceConstraints = null)
    //    : this (name, attributes, ctx => baseConstraint, ctx => interfaceConstraints)
    //{
    //}
  }
}