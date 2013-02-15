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
using System.Reflection.Emit;
using Remotion.Collections;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions
{
  /// <summary>
  /// Adapts <see cref="PropertyBuilder"/> with the <see cref="IPropertyBuilder"/> interface.
  /// </summary>
  public class PropertyBuilderAdapter : BuilderAdapterBase, IPropertyBuilder
  {
    private readonly PropertyBuilder _propertyBuilder;
    private readonly ReadOnlyDictionary<IMethodBuilder, MethodBuilder> _methodMapping;

    [CLSCompliant (false)]
    public PropertyBuilderAdapter (PropertyBuilder propertyBuilder, ReadOnlyDictionary<IMethodBuilder, MethodBuilder> methodMapping)
        : base (ArgumentUtility.CheckNotNull ("propertyBuilder", propertyBuilder).SetCustomAttribute)
    {
      ArgumentUtility.CheckNotNull ("methodMapping", methodMapping);

      _propertyBuilder = propertyBuilder;
      _methodMapping = methodMapping;
    }

    [CLSCompliant (false)]
    public void SetGetMethod (IMethodBuilder getMethodBuilder)
    {
      ArgumentUtility.CheckNotNull ("getMethodBuilder", getMethodBuilder);

      _propertyBuilder.SetGetMethod (_methodMapping[getMethodBuilder]);
    }

    [CLSCompliant (false)]
    public void SetSetMethod (IMethodBuilder setMethodBuilder)
    {
      ArgumentUtility.CheckNotNull ("setMethodBuilder", setMethodBuilder);

      _propertyBuilder.SetSetMethod (_methodMapping[setMethodBuilder]);
    }
  }
}