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
using System.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration
{
  /// <summary>
  /// Provides extensions methods for <see cref="ITypeAssemblyContext"/> instances.
  /// </summary>
  public static class TypeAssemblyContextExtensions
  {
    public static MutableType CreateClass (this ITypeAssemblyContext context, string name, string @namespace, Type baseType)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      // Name space may be null.
      ArgumentUtility.CheckNotNull ("baseType", baseType);

      var attributes = TypeAttributes.Public | TypeAttributes.Class;
      return context.CreateType (name, @namespace, attributes, baseType);
    }

    public static MutableType CreateInterface (this ITypeAssemblyContext context, string name, string @namespace)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      // Name space may be null.

      var attributes = TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract;
      return context.CreateType (name, @namespace, attributes, baseType: null);
    }
  }
}