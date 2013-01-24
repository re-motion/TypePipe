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
using System.Diagnostics;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;
using System.Dynamic.Utils;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// <see cref="Type"/> extension methods that are useful when working in the <see cref="Remotion.TypePipe.MutableReflection"/> domain.
  /// </summary>
  public static class TypeExtensions
  {
    public static bool IsRuntimeType (this Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);

      // ReSharper disable PossibleMistakenCallToGetType.2
      return type.GetType().FullName == "System.RuntimeType";
      // ReSharper restore PossibleMistakenCallToGetType.2
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
      ArgumentUtility.CheckNotNull ("fromType", fromType);

      // CustomTypes are only assignable from themselves.
      if (toType is CustomType)
        return toType == fromType;

      // 1) The base type of the CustomType may be assignable to the other type.
      // 2) The implemented interfaces of the CustomType may be assignable to the other interface.
      if (fromType is CustomType)
        return toType.IsAssignableFrom (fromType.BaseType) || fromType.GetInterfaces().Any (toType.IsAssignableFrom);

      return toType.IsAssignableFrom (fromType);
    }
  }
}