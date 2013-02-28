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
using System.Linq;
using System.Reflection;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Contains extensions methods on <see cref="Type"/> that are useful when working with
  /// <see cref="Remotion.TypePipe.MutableReflection"/> objects.
  /// </summary>
  public static class TypeExtensions
  {
    /// <summary>
    /// Determines whether the current <see cref="Type"/> instance itself is of type <see cref="System.RuntimeType"/>, i.e., the type is a standard
    /// reflection type.
    /// </summary>
    /// <param name="type">The type instance.</param>
    /// <returns><c>true</c> if the given type is an instance of <see cref="System.RuntimeType"/>; otherwise, <c>false</c>.</returns>
    public static bool IsRuntimeType (this Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);

      // ReSharper disable PossibleMistakenCallToGetType.2
      return type.GetType().FullName == "System.RuntimeType";
      // ReSharper restore PossibleMistakenCallToGetType.2
    }

    /// <summary>
    /// Determines whether a given type is serializable.
    /// Use this as an replacement for <see cref="Type.IsSerializable"/>.
    /// </summary>
    /// <returns><c>true</c> if the <see cref="Type"/> is serializable; otherwise, <c>false</c>.</returns>
    public static bool IsSerializableFast (this Type type)
    {
      var customType = type as CustomType;
      if (customType != null)
        return customType.Attributes.IsSet (TypeAttributes.Serializable);

      return type.IsSerializable;
    }

    /// <summary>
    /// Determines whether an instance of the current <see cref="Type"/> can be assigned from an instance of the specified type.
    /// Use this as an replacement for <see cref="Type.IsAssignableFrom"/>.
    /// </summary>
    /// <param name="toType">The current type, i.e., the left-hand side of the assignment.</param>
    /// <param name="fromType">The other type, i.e., the right-hand side of the assignment.</param>
    /// <returns><c>true</c> if this type is "assignable from" the specified type; <c>false</c> otherwise.</returns>
    public static bool IsAssignableFromFast (this Type toType, Type fromType)
    {
      ArgumentUtility.CheckNotNull ("toType", toType);
      // fromType may be null.

      if (fromType == null)
        return false;

      // Necessary for CustomTypes (reference equality only); an optimization for RuntimeTypes.
      if (ReferenceEquals (toType, fromType))
        return true;

      // 1) The base type of the CustomType may be assignable to the type.
      // 2) An interface of the CustomType may be assignable to the other interface.
      // 3) The CustomType satisfies all of the generic constraints of the type parameter (already sufficiently expressed with 1) and 2)).
      if (toType is CustomType || fromType is CustomType)
        return toType.IsAssignableFromFast (fromType.BaseType) || fromType.GetInterfaces().Any (toType.IsAssignableFromFast);

      return toType.IsAssignableFrom (fromType);
    }

    /// <summary>
    /// This method is an replacement for <see cref="Type.GetTypeCode"/> which internally accesses the <see cref="Type.UnderlyingSystemType"/> property.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The appropriate type code.</returns>
    public static TypeCode GetTypeCodeFast (this Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);

      return type is CustomType ? TypeCode.Object : Type.GetTypeCode (type);
    }

    /// <summary>
    /// Substitutes the type parameters of the generic type definition and returns a <see cref="Type"/> object representing the resulting
    /// constructed type. Use this as a replacement for <see cref="Type.MakeGenericType"/>.
    /// </summary>
    /// <param name="genericTypeDefinition">The generic type definition.</param>
    /// <param name="typeArguments">The type arguments.</param>
    /// <returns>The constructed type.</returns>
    public static Type MakeTypePipeGenericType (this Type genericTypeDefinition, params Type[] typeArguments)
    {
      ArgumentUtility.CheckNotNull ("typeArguments", typeArguments);
      ArgumentUtility.CheckNotNullOrItemsNull ("typeArguments", typeArguments);

      if (!genericTypeDefinition.IsGenericTypeDefinition)
      {
        var message = string.Format (
            "'{0}' is not a generic type definition. {1} may only be called on a type for which Type.IsGenericTypeDefinition is true.",
            genericTypeDefinition.Name,
            MethodInfo.GetCurrentMethod().Name);
        throw new InvalidOperationException (message);
      }

      var typeParameters = genericTypeDefinition.GetGenericArguments();
      GenericArgumentUtility.ValidateGenericArguments (typeParameters, typeArguments, genericTypeDefinition.Name);

      var instantiationInfo = new TypeInstantiationInfo (genericTypeDefinition, typeArguments);
      var instantiations = new Dictionary<TypeInstantiationInfo, TypeInstantiation>();

      return instantiationInfo.Instantiate (instantiations);
    }
  }
}