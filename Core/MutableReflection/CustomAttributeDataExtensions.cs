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
  /// Extension methods for the <see cref="ICustomAttributeData"/> interface.
  /// </summary>
  public static class CustomAttributeDataExtensions
  {
    public static object CreateInstance (this ICustomAttributeData customAttributeData)
    {
      ArgumentUtility.CheckNotNull ("customAttributeData", customAttributeData);

      var instance = customAttributeData.Constructor.Invoke (customAttributeData.ConstructorArguments.ToArray());
      foreach (var namedArgument in customAttributeData.NamedArguments)
      {
        if (namedArgument.MemberInfo is FieldInfo)
          ((FieldInfo) namedArgument.MemberInfo).SetValue (instance, namedArgument.Value);
        else if (namedArgument.MemberInfo is PropertyInfo)
          ((PropertyInfo) namedArgument.MemberInfo).SetValue (instance, namedArgument.Value, new object[0]);
        else
          throw new InvalidOperationException ("customAttributeNamedArgument.MemberInfo can only be FieldInfo or PropertyInfo.");
      }

      return instance;
    }
  }
}