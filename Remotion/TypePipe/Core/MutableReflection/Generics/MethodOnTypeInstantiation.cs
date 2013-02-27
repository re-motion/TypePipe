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
  /// Represents a method on a constructed type.
  /// </summary>
  public class MethodOnTypeInstantiation : CustomMethodInfo
  {
    private readonly MethodInfo _method;
    private readonly ParameterInfo _returnParameter;
    private readonly ReadOnlyCollection<ParameterInfo> _parameters;

    public MethodOnTypeInstantiation (TypeInstantiation declaringType, MethodInfo method)
        : base (
            declaringType,
            ArgumentUtility.CheckNotNull ("method", method).Name,
            method.Attributes,
            method.IsGenericMethod,
            method.IsGenericMethod ? method.GetGenericMethodDefinition() : null,
            method.GetGenericArguments())
    {
      _method = method;
      _returnParameter = new MemberParameterOnInstantiation (this, method.ReturnParameter);
      _parameters = method.GetParameters().Select (p => new MemberParameterOnInstantiation (this, p)).Cast<ParameterInfo>().ToList().AsReadOnly();
    }

    public MethodInfo MethodOnGenericType
    {
      get { return _method; }
    }

    public override ParameterInfo ReturnParameter
    {
      get { return _returnParameter; }
    }

    public override IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      return TypePipeCustomAttributeData.GetCustomAttributes (_method);
    }
    
    public override ParameterInfo[] GetParameters ()
    {
      return _parameters.ToArray();
    }

    public override MethodInfo GetBaseDefinition ()
    {
      throw new NotImplementedException();
    }
  }
}