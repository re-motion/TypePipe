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
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions
{
  /// <summary>
  /// Decorates an instance of <see cref="IPropertyBuilder"/> to allow <see cref="CustomType"/>s to be used in signatures and 
  /// for checking strong-name compatibility.
  /// </summary>
  public class PropertyBuilderDecorator : BuilderDecoratorBase, IPropertyBuilder
  {
    private readonly IPropertyBuilder _propertyBuilder;

    [CLSCompliant (false)]
    public PropertyBuilderDecorator (IPropertyBuilder propertyBuilder, IEmittableOperandProvider emittableOperandProvider)
        : base (propertyBuilder, emittableOperandProvider)
    {
      _propertyBuilder = propertyBuilder;
    }

    [CLSCompliant (false)]
    public IPropertyBuilder DecoratedPropertyBuilder
    {
      get { return _propertyBuilder; }
    }

    [CLSCompliant (false)]
    public void SetGetMethod (IMethodBuilder getMethodBuilder)
    {
      var decorator = ArgumentUtility.CheckNotNullAndType<MethodBuilderDecorator> ("getMethodBuilder", getMethodBuilder);

      _propertyBuilder.SetGetMethod (decorator.DecoratedMethodBuilder);
    }

    [CLSCompliant (false)]
    public void SetSetMethod (IMethodBuilder setMethodBuilder)
    {
      var decorator = ArgumentUtility.CheckNotNullAndType<MethodBuilderDecorator> ("setMethodBuilder", setMethodBuilder);

      _propertyBuilder.SetSetMethod (decorator.DecoratedMethodBuilder);
    }
  }
}