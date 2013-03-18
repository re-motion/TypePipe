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
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Contains extensions methods on <see cref="MethodInfo"/> that are useful when working with
  /// <see cref="Remotion.TypePipe.MutableReflection"/> objects.
  /// </summary>
  public static class MethodInfoExtensions
  {
    /// <summary>
    /// Determines whether the current <see cref="MethodInfo"/> instance itself is of type <see cref="System.Reflection.RuntimeMethodInfo"/>, i.e.,
    /// the method is a standard reflection <see cref="MethodInfo"/>.
    /// </summary>
    /// <param name="method">The method instance.</param>
    /// <returns><c>true</c> if the given method is an instance of <see cref="System.Reflection.RuntimeMethodInfo"/>; otherwise, <c>false</c>.</returns>
    public static bool IsRuntimeMethodInfo (this MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);

      return method.GetType().FullName == "System.Reflection.RuntimeMethodInfo";
    }

    /// <summary>
    /// Determines whether the current <see cref="MethodInfo"/> is a generic method instantiation, that means,
    /// <see cref="MethodInfo.IsGenericMethod"/> is <c>true</c> and <see cref="MethodInfo.IsGenericMethodDefinition"/> is <c>false</c>.
    /// </summary>
    /// <param name="method">The method.</param>
    /// <returns><c>true</c> if the method is a generic method instantiation; otherwise, <c>false</c>.</returns>
    public static bool IsGenericMethodInstantiation (this MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);

      return method.IsGenericMethod && !method.IsGenericMethodDefinition;
    }

    /// <summary>
    /// Substitutes the type parameters of the generic type definition and returns a <see cref="MethodInfo"/> object representing the resulting
    /// constructed method. Use this as a replacement for <see cref="MethodInfo.MakeGenericMethod"/>.
    /// </summary>
    /// <param name="genericMethodDefinition">The generic method definition.</param>
    /// <param name="typeArguments">The type arguments.</param>
    /// <returns>The generic method instantiation.</returns>
    public static MethodInfo MakeTypePipeGenericMethod (this MethodInfo genericMethodDefinition, params Type[] typeArguments)
    {
      ArgumentUtility.CheckNotNull ("genericMethodDefinition", genericMethodDefinition);
      ArgumentUtility.CheckNotNullOrItemsNull ("typeArguments", typeArguments);

      if (!genericMethodDefinition.IsGenericMethodDefinition)
      {
        var message = string.Format (
            "'{0}' is not a generic method definition. MakeTypePipeGenericMethod may only be called on a method for which "
            + "MethodInfo.IsGenericMethodDefinition is true.",
            genericMethodDefinition.Name);
        throw new InvalidOperationException (message);
      }

      var typeParameters = genericMethodDefinition.GetGenericArguments();
      GenericArgumentUtility.ValidateGenericArguments (typeParameters, typeArguments, genericMethodDefinition.Name);

      var instantiationInfo = new MethodInstantiationInfo (genericMethodDefinition, typeArguments);
      return instantiationInfo.Instantiate();
    }
  }
}