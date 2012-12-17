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
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// A utility class that is used to implement the <see cref="ITypePipeCustomAttributeProvider"/> interface for mutable reflection objects.
  /// </summary>
  public static class TypePipeCustomAttributeImplementationUtility
  {
    public static object[] GetCustomAttributes (ICustomAttributeProvider customAttributeProvider, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("customAttributeProvider", customAttributeProvider);

      return GetCustomAttributes (customAttributeProvider, typeof (object), inherit);
    }

    public static object[] GetCustomAttributes (ICustomAttributeProvider customAttributeProvider, Type attributeType, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("customAttributeProvider", customAttributeProvider);
      ArgumentUtility.CheckNotNull ("attributeType", attributeType);

      return GetCustomAttributes (GetCustomAttributeDatas (customAttributeProvider, inherit), attributeType);
    }

    public static bool IsDefined (ICustomAttributeProvider customAttributeProvider, Type attributeType, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("customAttributeProvider", customAttributeProvider);
      ArgumentUtility.CheckNotNull ("attributeType", attributeType);

      return IsDefined (GetCustomAttributeDatas (customAttributeProvider, inherit), attributeType);
    }

    private static IEnumerable<ICustomAttributeData> GetCustomAttributeDatas (ICustomAttributeProvider customAttributeProvider, bool inherit)
    {
      Assertion.IsTrue (customAttributeProvider is MemberInfo || customAttributeProvider is ParameterInfo);

      return customAttributeProvider is MemberInfo
                 ? TypePipeCustomAttributeData.GetCustomAttributes ((MemberInfo) customAttributeProvider, inherit)
                 : TypePipeCustomAttributeData.GetCustomAttributes ((ParameterInfo) customAttributeProvider);
    }

    private static object[] GetCustomAttributes (IEnumerable<ICustomAttributeData> customAttributeDatas, Type attributeType)
    {
      var attributeArray = customAttributeDatas
          .Where (a => attributeType.IsAssignableFrom (a.Type))
          .Select (a => a.CreateInstance())
          .ToArray();

      if (attributeArray.GetType().GetElementType() != attributeType)
      {
        var typedAttributeArray = Array.CreateInstance (attributeType, attributeArray.Length);
        Array.Copy (attributeArray, typedAttributeArray, attributeArray.Length);
        attributeArray = (object[]) typedAttributeArray;
      }

      return attributeArray;
    }

    private static bool IsDefined (IEnumerable<ICustomAttributeData> customAttributeDatas, Type attributeType)
    {
      return customAttributeDatas.Any (a => attributeType.IsAssignableFrom (a.Type));
    }
  }
}