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
    /// <summary>
    /// Creates an additional <see cref="MutableType"/> representing an public top-level class.
    /// </summary>
    /// <param name="context">A type assembly context.</param>
    /// <param name="name">The class name.</param>
    /// <param name="namespace">The namespace of the class.</param>
    /// <param name="baseType">The base type of the class.</param>
    /// <returns>A new mutable type representing a class.</returns>
    public static MutableType CreateClass (this ITypeAssemblyContext context, string name, string @namespace, Type baseType)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      // Name space may be null.
      ArgumentUtility.CheckNotNull ("baseType", baseType);

      var attributes = TypeAttributes.Public | TypeAttributes.Class;
      return context.CreateType (name, @namespace, attributes, baseType);
    }

    /// <summary>
    /// Creates an additional <see cref="MutableType"/> representing an public interface.
    /// </summary>
    /// <param name="context">A type assembly context.</param>
    /// <param name="name">The interface name.</param>
    /// <param name="namespace">The namespace of the interface.</param>
    /// <returns>A new mutable type representing an interface.</returns>
    public static MutableType CreateInterface (this ITypeAssemblyContext context, string name, string @namespace)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      // Name space may be null.

      var attributes = TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract;
      return context.CreateType (name, @namespace, attributes, baseType: null);
    }
  }
}