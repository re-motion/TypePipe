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
  /// Determines if a type can be subclassed and whether its members are visible from a derived type.
  /// </summary>
  public static class SubclassFilterUtility
  {
    public static bool IsSubclassable (Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);

      // TODO 4744: check that baseType.IsVisible

      return !type.IsSealed && !type.IsInterface && HasAccessibleConstructor (type);
    }

    public static bool IsVisibleFromSubclass (FieldInfo fieldInfo)
    {
      ArgumentUtility.CheckNotNull ("fieldInfo", fieldInfo);

      return fieldInfo.IsPublic || fieldInfo.IsFamilyOrAssembly || fieldInfo.IsFamily;
    }

    public static bool IsVisibleFromSubclass (MethodBase methodBase)
    {
      ArgumentUtility.CheckNotNull ("methodBase", methodBase);

      return methodBase.IsPublic || methodBase.IsFamilyOrAssembly || methodBase.IsFamily;
    }

    private static bool HasAccessibleConstructor (Type type)
    {
      const BindingFlags allInstanceMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
      return type.GetConstructors (allInstanceMembers).Where (IsVisibleFromSubclass).Any();
    }
  }
}