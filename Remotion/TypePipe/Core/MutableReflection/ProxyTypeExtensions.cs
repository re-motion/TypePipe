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
using Microsoft.Scripting.Ast;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// This class contains useful convenience APIs for <see cref="ProxyType"/>.
  /// </summary>
  public static class ProxyTypeExtensions
  {
    /// <summary>
    /// Adds the method by delegating to <see cref="ProxyType.AddMethod" /> and providing default values.
    /// The default is to create an public instance method with return type void and no parameters.
    /// </summary>
    /// <param name="proxyType">The proxy type instance to call the <see cref="ProxyType.AddMethod"/> on.</param>
    /// <param name="name">The name of the method.</param>
    /// <param name="bodyProvider">The body provider.</param>
    /// <param name="attributes">Method attributes (default: public).</param>
    /// <param name="returnType">Return type (default: void).</param>
    /// <param name="parameters">Parameter declarations (default: no parameters).</param>
    /// <returns></returns>
    public static MutableMethodInfo AddMethod (
        this ProxyType proxyType,
        string name,
        Func<MethodBodyCreationContext, Expression> bodyProvider,
        MethodAttributes attributes = MethodAttributes.Public,
        Type returnType = null,
        IEnumerable<ParameterDeclaration> parameters = null)
    {
      ArgumentUtility.CheckNotNull ("proxyType", proxyType);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("bodyProvider", bodyProvider);
      // return type may be null.
      // parameters may be null.

      returnType = returnType ?? typeof (void);
      parameters = parameters ?? ParameterDeclaration.EmptyParameters;

      return proxyType.AddMethod (name, attributes, returnType, parameters, bodyProvider);
    }
  }
}