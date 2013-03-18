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
  public class TestableCustomMethodInfo : CustomMethodInfo
  {
    public TestableCustomMethodInfo (
        CustomType declaringType,
        string name,
        MethodAttributes attributes,
        MethodInfo genericMethodDefinition,
        IEnumerable<Type> typeArguments)
        : base (declaringType, name, attributes, genericMethodDefinition, typeArguments)
    {
    }

    public ParameterInfo ReturnParameter_;
    public ParameterInfo[] Parameters;
    public MethodInfo BaseDefinition;
    public IEnumerable<ICustomAttributeData> CustomAttributeDatas;

    public override ParameterInfo ReturnParameter
    {
      get { return ReturnParameter_; }
    }

    public override IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      return CustomAttributeDatas;
    }

    public override ParameterInfo[] GetParameters ()
    {
      return Parameters;
    }

    public override MethodInfo GetBaseDefinition ()
    {
      return BaseDefinition;
    }
  }
}