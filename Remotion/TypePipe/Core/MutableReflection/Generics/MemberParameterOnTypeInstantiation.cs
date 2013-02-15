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
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Generics
{
  /// <summary>
  /// Represents a parameter used in a member on a constructed type.
  /// </summary>
  public class MemberParameterOnTypeInstantiation : CustomParameterInfo
  {
    private static Type Substitute (MemberInfo declaringMember, Type parameterType)
    {
      var declaringType = declaringMember.DeclaringType as TypeInstantiation;
      if (declaringType == null)
      {
        var message = string.Format (
            "{0} can only created with members of {1}.", typeof (MemberParameterOnTypeInstantiation).Name, typeof (TypeInstantiation).Name);
        throw new ArgumentException (message, "declaringMember");
      }

      return declaringType.SubstituteGenericParameters (parameterType);
    }

    private readonly ParameterInfo _parameter;

    public MemberParameterOnTypeInstantiation (MemberInfo declaringMember, ParameterInfo parameter)
        : base (
            declaringMember,
            ArgumentUtility.CheckNotNull ("parameter", parameter).Position,
            parameter.Name,
            Substitute (ArgumentUtility.CheckNotNull ("declaringMember", declaringMember), parameter.ParameterType),
            parameter.Attributes)
    {
      _parameter = parameter;
    }

    public ParameterInfo MemberParameterOnGenericType
    {
      get { return _parameter; }
    }

    public override IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      return TypePipeCustomAttributeData.GetCustomAttributes (_parameter);
    }
  }
}