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
  /// Implements <see cref="ICustomAttributeDataRetriever"/> for standard .NET reflection objects and mutable reflection objects
  /// using <see cref="CustomAttributeData"/> and <see cref="ITypePipeCustomAttributeProvider"/> respectivly.
  /// </summary>
  public class CustomAttributeDataRetriever : ICustomAttributeDataRetriever
  {
    public IEnumerable<ICustomAttributeData> GetCustomAttributeData (MemberInfo member)
    {
      ArgumentUtility.CheckNotNull ("member", member);

      return RetrieveCustomAttributes (CustomAttributeData.GetCustomAttributes, member);
    }

    public IEnumerable<ICustomAttributeData> GetCustomAttributeData (ParameterInfo parameter)
    {
      ArgumentUtility.CheckNotNull ("parameter", parameter);

      return RetrieveCustomAttributes (CustomAttributeData.GetCustomAttributes, parameter);
    }

    public IEnumerable<ICustomAttributeData> GetCustomAttributeData (Assembly assembly)
    {
      ArgumentUtility.CheckNotNull ("assembly", assembly);

      return RetrieveCustomAttributes (CustomAttributeData.GetCustomAttributes, assembly);
    }

    public IEnumerable<ICustomAttributeData> GetCustomAttributeData (Module module)
    {
      ArgumentUtility.CheckNotNull ("module", module);

      return RetrieveCustomAttributes (CustomAttributeData.GetCustomAttributes, module);
    }

    private IEnumerable<ICustomAttributeData> RetrieveCustomAttributes<T> (Func<T, IEnumerable<CustomAttributeData>> customAttributeProvider, T info)
    {
      var typePipeCustomAttributeProvider = info as ITypePipeCustomAttributeProvider;
      if (typePipeCustomAttributeProvider != null)
        return typePipeCustomAttributeProvider.GetCustomAttributeData();
      else
        return customAttributeProvider (info).Select (a => new CustomAttributeDataAdapter (a)).Cast<ICustomAttributeData>();
    }
  }
}