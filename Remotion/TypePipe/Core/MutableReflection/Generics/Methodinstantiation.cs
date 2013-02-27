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
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Generics
{
  /// <summary>
  /// Represents a constructed generic <see cref="MethodInfo"/>, i.e., a generic method definition that was instantiated with type arguments.
  /// This class is needed because the the original reflection classes do not work in combination with <see cref="CustomType"/> instances.
  /// </summary>
  // TODO <remarks>Instances of this class are returned by <see cref="MutableMethodInfo.MakeGenericMethod"/>.</remarks>
  public class MethodInstantiation : CustomMethodInfo
  {
    private readonly MethodInfo _genericMethodDefinition;
    private readonly ReadOnlyCollection<Type> _typeArguments;

    public MethodInstantiation (MethodInfo genericMethodDefinition, IEnumerable<Type> typeArguments)
        : base (
            ArgumentUtility.CheckNotNull ("genericMethodDefinition", genericMethodDefinition).DeclaringType,
            genericMethodDefinition.Name,
            genericMethodDefinition.Attributes)
    {
      ArgumentUtility.CheckNotNull ("typeArguments", typeArguments);

      _genericMethodDefinition = genericMethodDefinition;
      _typeArguments = typeArguments.ToList().AsReadOnly();
    }

    public override ParameterInfo ReturnParameter
    {
      get { throw new NotImplementedException(); }
    }

    public override IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      return TypePipeCustomAttributeData.GetCustomAttributes (_genericMethodDefinition);
    }

    public override MethodInfo GetBaseDefinition ()
    {
      throw new NotImplementedException();
    }

    public override Type[] GetGenericArguments ()
    {
      return _typeArguments.ToArray();
    }

    public override ParameterInfo[] GetParameters ()
    {
      throw new NotImplementedException();
    }
  }
}