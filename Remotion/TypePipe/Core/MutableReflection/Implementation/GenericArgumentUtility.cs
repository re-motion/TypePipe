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
using System.Linq;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// Provides functionality to validate if the generic arguments are appropriate for the specified generic parameters.
  /// </summary>
  public static class GenericArgumentUtility
  {
    public static void ValidateGenericArguments (Type[] typeParameters, Type[] typeArguments, string typeOrMethodName)
    {
      ArgumentUtility.CheckNotNull ("typeParameters", typeParameters);
      ArgumentUtility.CheckNotNull ("typeArguments", typeArguments);
      ArgumentUtility.CheckNotNullOrEmpty ("typeOrMethodName", typeOrMethodName);

      if (typeParameters.Length != typeArguments.Length)
      {
        var message = string.Format (
            "The generic definition '{0}' has {1} generic parameter(s), but {2} generic argument(s) were provided. "
            + "A generic argument must be provided for each generic parameter.",
            typeOrMethodName,
            typeParameters.Length,
            typeArguments.Length);
        throw new ArgumentException (message, "typeArguments");
      }

      for (int i = 0; i < typeParameters.Length; i++)
      {
        var parameter = typeParameters[i];
        var argument = typeArguments[i];

        if (!IsValidGenericArgument (parameter, argument))
        {
          var message = string.Format (
              "Generic argument '{0}' at position {1} on '{2}' violates a constraint of type parameter '{3}'.",
              argument.Name,
              i,
              typeOrMethodName,
              parameter.Name);
          throw new ArgumentException (message, "typeArguments");
        }
      }
    }

    private static bool IsValidGenericArgument (Type parameter, Type argument)
    {
      var attr = parameter.GenericParameterAttributes;
      return
          (!attr.IsSet (GenericParameterAttributes.DefaultConstructorConstraint) || HasPublicDefaultCtor (argument) || argument.IsValueType)
          && (!attr.IsSet (GenericParameterAttributes.ReferenceTypeConstraint) || argument.IsClass)
          && (!attr.IsSet (GenericParameterAttributes.NotNullableValueTypeConstraint) || IsNotNullableValueType (argument))
          && parameter.GetGenericParameterConstraints().All (constraint => SkipValidation (constraint) || constraint.IsAssignableFromFast (argument));
    }

    private static bool HasPublicDefaultCtor (Type argument)
    {
      return argument.GetConstructor (Type.EmptyTypes) != null;
    }

    private static bool IsNotNullableValueType (Type argument)
    {
      return argument.IsValueType && Nullable.GetUnderlyingType (argument) == null;
    }

    private static bool SkipValidation (Type constraint)
    {
      // Skip validaiton for constraints that are of generic nature themselves (which would be very complex). 
      // Users will get a TypeLoadException at code generation time violating such a constraint.
      return constraint.ContainsGenericParameters;
    }
  }
}