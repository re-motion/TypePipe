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

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// Wraps a <see cref="ParameterInfo"/> of a property's accessor method so that the result represents an index parameter
  /// of the <see cref="PropertyInfo"/> (with the right <see cref="ParameterInfo.Member"/>).
  /// </summary>
  public class PropertyParameterInfoWrapper : CustomParameterInfo
  {
    private readonly ParameterInfo _parameter;

    public PropertyParameterInfoWrapper (MemberInfo member, ParameterInfo parameter)
        : base(member, parameter.Position, parameter.Name, parameter.ParameterType, parameter.Attributes)
    {
      _parameter = parameter;
    }

    public override IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      return TypePipeCustomAttributeData.GetCustomAttributes (_parameter);
    }
  }
}