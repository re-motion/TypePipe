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
using System.Linq;
using System.Reflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Represents a <see cref="PropertyInfo"/> that can be modified.
  /// </summary>
  public class MutablePropertyInfo : CustomPropertyInfo, IMutableMember
  {
    private readonly CustomAttributeContainer _customAttributeContainer = new CustomAttributeContainer();

    private readonly ReadOnlyCollection<PropertyParameterInfoWrapper> _indexParameters;

    public MutablePropertyInfo (
        ProxyType declaringType, string name, PropertyAttributes attributes, MutableMethodInfo getMethod, MutableMethodInfo setMethod)
        : base (declaringType, name, attributes, getMethod, setMethod)
    {
      IEnumerable<ParameterInfo> indexParameters;
      if (getMethod != null)
        indexParameters = getMethod.GetParameters();
      else
      {
        var setMethodParameters = setMethod.GetParameters();
        indexParameters = setMethodParameters.Take (setMethodParameters.Length - 1);
      }

      _indexParameters = indexParameters.Select (p => new PropertyParameterInfoWrapper (this, p)).ToList().AsReadOnly();
    }

    public ProxyType MutableDeclaringType
    {
      get { return (ProxyType) DeclaringType; }
    }

    public MutableMethodInfo MutableGetMethod
    {
      get { return (MutableMethodInfo) GetGetMethod (true); }
    }

    public MutableMethodInfo MutableSetMethod
    {
      get { return (MutableMethodInfo) GetSetMethod (true); }
    }

    public ReadOnlyCollection<CustomAttributeDeclaration> AddedCustomAttributes
    {
      get { return _customAttributeContainer.AddedCustomAttributes; }
    }

    public void AddCustomAttribute (CustomAttributeDeclaration customAttribute)
    {
      ArgumentUtility.CheckNotNull ("customAttribute", customAttribute);

      _customAttributeContainer.AddCustomAttribute (customAttribute);
    }

    public override IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      return _customAttributeContainer.AddedCustomAttributes.Cast<ICustomAttributeData> ();
    }

    public override ParameterInfo[] GetIndexParameters ()
    {
      return _indexParameters.Cast<ParameterInfo>().ToArray();
    }
  }
}