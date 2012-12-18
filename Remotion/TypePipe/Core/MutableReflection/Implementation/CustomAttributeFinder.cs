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
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// A utility class that is used to implement the <see cref="CustomAttributeFinder"/> interface for mutable reflection objects.
  /// </summary>
  public static class CustomAttributeFinder
  {
    public static object[] GetCustomAttributes (ICustomAttributeDataProvider customAttributeDataProvider, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("customAttributeDataProvider", customAttributeDataProvider);

      return GetCustomAttributes (customAttributeDataProvider, typeof (object), inherit);
    }

    public static object[] GetCustomAttributes (ICustomAttributeDataProvider customAttributeDataProvider, Type attributeType, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("customAttributeDataProvider", customAttributeDataProvider);
      ArgumentUtility.CheckNotNull ("attributeType", attributeType);

      return GetCustomAttributes (customAttributeDataProvider.GetCustomAttributeData (inherit), attributeType);
    }

    public static bool IsDefined (ICustomAttributeDataProvider customAttributeDataProvider, Type attributeType, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("customAttributeDataProvider", customAttributeDataProvider);
      ArgumentUtility.CheckNotNull ("attributeType", attributeType);

      return IsDefined (customAttributeDataProvider.GetCustomAttributeData (inherit), attributeType);
    }

    private static object[] GetCustomAttributes (IEnumerable<ICustomAttributeData> customAttributeDatas, Type attributeType)
    {
      var attributes = customAttributeDatas
          .Where (a => attributeType.IsAssignableFrom (a.Type))
          .Select (a => a.CreateInstance())
          .ToList();

      var attributeArray = (object[]) Array.CreateInstance (attributeType, attributes.Count);
      attributes.CopyTo (attributeArray);

      return attributeArray;
    }

    private static bool IsDefined (IEnumerable<ICustomAttributeData> customAttributeDatas, Type attributeType)
    {
      return customAttributeDatas.Any (a => attributeType.IsAssignableFrom (a.Type));
    }
  }
}