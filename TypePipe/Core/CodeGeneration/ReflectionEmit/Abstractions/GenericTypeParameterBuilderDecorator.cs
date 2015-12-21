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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions
{
  /// <summary>
  /// Decorates an instance of <see cref="GenericTypeParameterBuilder"/> to allow <see cref="CustomType"/>s to be used in signatures and 
  /// for checking strong-name compatibility.
  /// </summary>
  public class GenericTypeParameterBuilderDecorator : BuilderDecoratorBase, IGenericTypeParameterBuilder
  {
    private readonly IGenericTypeParameterBuilder _genericTypeParameterBuilder;

    public GenericTypeParameterBuilderDecorator (
        IGenericTypeParameterBuilder genericTypeParameterBuilder, IEmittableOperandProvider emittableOperandProvider)
        : base (genericTypeParameterBuilder, emittableOperandProvider)
    {
      _genericTypeParameterBuilder = genericTypeParameterBuilder;
    }

    public void SetGenericParameterAttributes (GenericParameterAttributes genericParameterAttributes)
    {
      _genericTypeParameterBuilder.SetGenericParameterAttributes (genericParameterAttributes);
    }

    public void RegisterWith (IEmittableOperandProvider emittableOperandProvider, MutableGenericParameter genericParameter)
    {
      ArgumentUtility.CheckNotNull ("emittableOperandProvider", emittableOperandProvider);
      ArgumentUtility.CheckNotNull ("genericParameter", genericParameter);

      _genericTypeParameterBuilder.RegisterWith (emittableOperandProvider, genericParameter);
    }

    public void SetBaseTypeConstraint (Type baseTypeConstraint)
    {
      ArgumentUtility.CheckNotNull ("baseTypeConstraint", baseTypeConstraint);

      var emittableBaseTypeConstraint = EmittableOperandProvider.GetEmittableType (baseTypeConstraint);
      _genericTypeParameterBuilder.SetBaseTypeConstraint (emittableBaseTypeConstraint);
    }

    public void SetInterfaceConstraints (Type[] interfaceConstraints)
    {
      ArgumentUtility.CheckNotNull ("interfaceConstraints", interfaceConstraints);

      var emittableInterfaceConstraints = interfaceConstraints.Select (EmittableOperandProvider.GetEmittableType).ToArray();
      _genericTypeParameterBuilder.SetInterfaceConstraints (emittableInterfaceConstraints);
    }
  }
}