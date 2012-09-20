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

namespace Remotion.TypePipe.MutableReflection
{
  public static class TypePipeCustomAttributeData
  {
    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (MemberInfo member)
    {
      ArgumentUtility.CheckNotNull ("member", member);

      return GetCustomAttributes (CustomAttributeData.GetCustomAttributes, member);
    }

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (ParameterInfo parameter)
    {
      ArgumentUtility.CheckNotNull ("parameter", parameter);

      return GetCustomAttributes (CustomAttributeData.GetCustomAttributes, parameter);
    }

    private static IEnumerable<ICustomAttributeData> GetCustomAttributes<T> (Func<T, IEnumerable<CustomAttributeData>> customAttributeUtility, T info)
    {
      var typePipeCustomAttributeProvider = info as ITypePipeCustomAttributeProvider;
      if (typePipeCustomAttributeProvider != null)
        return typePipeCustomAttributeProvider.GetCustomAttributeData ();
      else
        return customAttributeUtility(info).Select (a => new CustomAttributeDataAdapter (a)).Cast<ICustomAttributeData> ();
    }
  }
}