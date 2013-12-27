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
using System.Reflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using System.Linq;

namespace Remotion.TypePipe.MutableReflection.Generics
{
  /// <summary>
  /// Represents a property on a constructed type.
  /// </summary>
  public class PropertyOnTypeInstantiation : CustomPropertyInfo
  {
    private readonly IReadOnlyCollection<ParameterInfo> _indexParameters;
    private readonly PropertyInfo _property;

    public PropertyOnTypeInstantiation (
        TypeInstantiation declaringType,
        PropertyInfo property,
        MethodOnTypeInstantiation getMethod,
        MethodOnTypeInstantiation setMethod)
        : base (declaringType, property.Name, property.Attributes, getMethod, setMethod)
    {
      _property = property;
      _indexParameters = property.GetIndexParameters().Select (p => new MemberParameterOnInstantiation (this, p))
          .Cast<ParameterInfo>().ToList().AsReadOnly();
    }

    public PropertyInfo PropertyOnGenericType
    {
      get { return _property; }
    }

    public override IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      return TypePipeCustomAttributeData.GetCustomAttributes (_property);
    }

    public override ParameterInfo[] GetIndexParameters ()
    {
      return _indexParameters.ToArray();
    }
  }
}