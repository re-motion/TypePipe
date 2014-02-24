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

namespace Remotion.TypePipe.MutableReflection.Generics
{
  /// <summary>
  /// Represents a default constructor on a generic parameter that is constrained 
  /// with <see cref="GenericParameterAttributes.DefaultConstructorConstraint"/>.
  /// </summary>
  /// <remarks>This class is an implementation detail of <see cref="MutableGenericParameter"/>.</remarks>
  public class GenericParameterDefaultConstructor : CustomConstructorInfo
  {
    private const MethodAttributes c_attributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;

    public GenericParameterDefaultConstructor (MutableGenericParameter declaringType)
        : base (declaringType, c_attributes)
    {
    }

    public override IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      return new ICustomAttributeData[0];
    }

    public override ParameterInfo[] GetParameters ()
    {
      return new ParameterInfo[0];
    }
  }
}