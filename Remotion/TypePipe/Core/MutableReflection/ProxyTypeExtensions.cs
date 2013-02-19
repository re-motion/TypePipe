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
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// This class contains useful convenience APIs for <see cref="ProxyType"/>.
  /// </summary>
  public static class ProxyTypeExtensions
  {
    public static MutableMethodInfo AddAbstractMethod (
        this ProxyType proxyType,
        string name,
        MethodAttributes attributes = MethodAttributes.Public,
        Type returnType = null,
        IEnumerable<ParameterDeclaration> parameters = null)
    {
      ArgumentUtility.CheckNotNull ("proxyType", proxyType);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      // Return type may be null.
      // Parameters may be null.

      var abstractAttributes = attributes.Set (MethodAttributes.Abstract | MethodAttributes.Virtual);
      return proxyType.AddMethod (name, abstractAttributes, returnType, parameters, bodyProvider: null);
    }
  }
}