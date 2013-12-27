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
  /// Represents a <see cref="ConstructorInfo"/> on a descendent of a <see cref="CustomType"/>.
  /// This class can be configured with constant values.
  /// </summary>
  public class ConstructorOnCustomType : CustomConstructorInfo
  {
    private readonly IReadOnlyCollection<ParameterOnCustomMember> _parameters;

    public ConstructorOnCustomType (CustomType declaringType, MethodAttributes attributes, IEnumerable<ParameterDeclaration> parameters)
        : base (declaringType, attributes)
    {
      ArgumentUtility.CheckNotNull ("parameters", parameters);

      _parameters = parameters.Select ((p, i) => new ParameterOnCustomMember (this, i, p.Name, p.Type, p.Attributes)).ToList().AsReadOnly();
    }

    public override IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      return Enumerable.Empty<ICustomAttributeData>();
    }

    public override ParameterInfo[] GetParameters ()
    {
      return _parameters.Cast<ParameterInfo>().ToArray();
    }
  }
}