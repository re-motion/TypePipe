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
using System.Reflection;
using System.Reflection.Emit;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions
{
  /// <summary>
  /// Adapts <see cref="GenericTypeParameterBuilder"/> with the <see cref="IGenericTypeParameterBuilder"/> interface.
  /// </summary>
  public class GenericTypeParameterBuilderAdapter : BuilderAdapterBase, IGenericTypeParameterBuilder
  {
    private readonly GenericTypeParameterBuilder _genericTypeParameterBuilder;

    public GenericTypeParameterBuilderAdapter (GenericTypeParameterBuilder genericTypeParameterBuilder)
        : base(ArgumentUtility.CheckNotNull ("genericTypeParameterBuilder", genericTypeParameterBuilder).SetCustomAttribute)
    {
      _genericTypeParameterBuilder = genericTypeParameterBuilder;
    }

    public void RegisterWith (IEmittableOperandProvider emittableOperandProvider, MutableGenericParameter genericParameter)
    {
      ArgumentUtility.CheckNotNull ("emittableOperandProvider", emittableOperandProvider);
      ArgumentUtility.CheckNotNull ("genericParameter", genericParameter);

      emittableOperandProvider.AddMapping (genericParameter, _genericTypeParameterBuilder);
    }

    public void SetGenericParameterAttributes (GenericParameterAttributes genericParameterAttributes)
    {
      _genericTypeParameterBuilder.SetGenericParameterAttributes (genericParameterAttributes);
    }

    public void SetBaseTypeConstraint (Type baseTypeConstraint)
    {
      ArgumentUtility.CheckNotNull ("baseTypeConstraint", baseTypeConstraint);

      _genericTypeParameterBuilder.SetBaseTypeConstraint (baseTypeConstraint);
    }

    public void SetInterfaceConstraints (params Type[] interfaceConstraints)
    {
      ArgumentUtility.CheckNotNull ("interfaceConstraints", interfaceConstraints);

      _genericTypeParameterBuilder.SetInterfaceConstraints (interfaceConstraints);
    }
  }
}