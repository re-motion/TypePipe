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

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// A utility class that is used to implement the <see cref="ITypePipeCustomAttributeProvider"/> interface for mutable reflection objects.
  /// </summary>
  public static class TypePipeCustomAttributeImplementationUtility
  {
    public static object[] GetCustomAttributes (MemberInfo member, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("member", member);

      return GetCustomAttributes (member, typeof (object), inherit);
    }

    public static object[] GetCustomAttributes (MemberInfo member, Type attributeType, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("member", member);
      ArgumentUtility.CheckNotNull ("attributeType", attributeType);

      return TypePipeCustomAttributeData.GetCustomAttributes (member, inherit)
          .Where (a => attributeType.IsAssignableFrom (a.Type))
          .Select (a => a.CreateInstance())
          .ToArray();
    }

    public static bool IsDefined (MemberInfo member, Type attributeType, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("member", member);
      ArgumentUtility.CheckNotNull ("attributeType", attributeType);

      return TypePipeCustomAttributeData.GetCustomAttributes (member, inherit)
          .Any (a => attributeType.IsAssignableFrom (a.Type));
    }
  }
}