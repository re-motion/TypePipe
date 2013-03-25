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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// Represents a <see cref="MethodInfo"/> on a descendent of a <see cref="CustomType"/>.
  /// This class can be configured with constant values.
  /// </summary>
  public class MethodOnCustomType : CustomMethodInfo
  {
    private readonly ParameterInfo _returnParameter;
    private readonly ReadOnlyCollection<ParameterOnCustomMember> _parameters;

    public MethodOnCustomType (
        Type declaringType,
        string name,
        MethodAttributes attributes,
        IEnumerable<Type> typeArguments,
        Type returnType,
        IEnumerable<ParameterDeclaration> parameters)
        : base (declaringType, name, attributes, null, typeArguments)
    {
      ArgumentUtility.CheckNotNull ("parameters", parameters);
      ArgumentUtility.CheckNotNull ("returnType", returnType);

      _returnParameter = new ParameterOnCustomMember (this, -1, null, returnType, ParameterAttributes.None);
      _parameters = parameters.Select ((p, i) => new ParameterOnCustomMember (this, i, p.Name, p.Type, p.Attributes)).ToList().AsReadOnly();
    }

    public override ParameterInfo ReturnParameter
    {
      get { return _returnParameter; }
    }

    public override IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      return Enumerable.Empty<ICustomAttributeData>();
    }

    public override ParameterInfo[] GetParameters ()
    {
      return _parameters.Cast<ParameterInfo>().ToArray();
    }

    public override MethodInfo GetBaseDefinition ()
    {
      throw new NotImplementedException();
    }
  }
}