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
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.Implementation.ReflectionEmit.Abstractions
{
  /// <summary>
  /// Adapts <see cref="PropertyBuilder"/> with the <see cref="IPropertyBuilder"/> interface.
  /// </summary>
  public class PropertyBuilderAdapter : BuilderAdapterBase, IPropertyBuilder
  {
    private readonly PropertyBuilder _propertyBuilder;

    [CLSCompliant (false)]
    public PropertyBuilderAdapter (PropertyBuilder propertyBuilder)
        : base (ArgumentUtility.CheckNotNull ("propertyBuilder", propertyBuilder).SetCustomAttribute)
    {
      _propertyBuilder = propertyBuilder;
    }

    [CLSCompliant (false)]
    public void SetGetMethod (IMethodBuilder getMethodBuilder)
    {
      var adapter = ArgumentUtility.CheckNotNullAndType<MethodBuilderAdapter> ("getMethodBuilder", getMethodBuilder);

      _propertyBuilder.SetGetMethod (adapter.AdaptedMethodBuilder);
    }

    [CLSCompliant (false)]
    public void SetSetMethod (IMethodBuilder setMethodBuilder)
    {
      var adapter = ArgumentUtility.CheckNotNullAndType<MethodBuilderAdapter> ("setMethodBuilder", setMethodBuilder);

      _propertyBuilder.SetSetMethod (adapter.AdaptedMethodBuilder);
    }
  }
}