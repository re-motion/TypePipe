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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Represents a <see cref="PropertyInfo"/> that can be modified.
  /// </summary>
  public class MutablePropertyInfo : CustomPropertyInfo, IMutableInfo
  {
    public MutablePropertyInfo (ProxyType declaringType, string name, MutableMethodInfo getMethod, MutableMethodInfo setMethod)
        : base (declaringType, name, PropertyAttributes.None, getMethod, setMethod)
    {
      // TODO: test initialization
    }

    // tODO test
    public MutableMethodInfo MutableGetMethod
    {
      get { return (MutableMethodInfo) GetGetMethod (true); }
    }

    // tODO test
    public MutableMethodInfo MutableSetMethod
    {
      get { return (MutableMethodInfo) GetSetMethod (true); }
    }

    public ReadOnlyCollection<CustomAttributeDeclaration> AddedCustomAttributes
    {
      get { throw new System.NotImplementedException(); }
    }

    public void AddCustomAttribute (CustomAttributeDeclaration customAttributeDeclaration)
    {
      throw new System.NotImplementedException();
    }

    public override IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      throw new System.NotImplementedException();
    }

    public override ParameterInfo[] GetIndexParameters ()
    {
      throw new System.NotImplementedException();
    }
  }
}