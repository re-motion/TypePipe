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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions
{
  /// <summary>
  /// Adapts <see cref="FieldBuilder"/> with the <see cref="IFieldBuilder"/> interface.
  /// </summary>
  public class FieldBuilderAdapter : IFieldBuilder
  {
    private readonly FieldBuilder _fieldBuilder;

    public FieldBuilderAdapter (FieldBuilder fieldBuilder)
    {
      ArgumentUtility.CheckNotNull ("fieldBuilder", fieldBuilder);
      _fieldBuilder = fieldBuilder;
    }

    public FieldBuilder FieldBuilder
    {
      get { return _fieldBuilder; }
    }

    public void SetCustomAttribute (CustomAttributeBuilder customBuilder)
    {
      ArgumentUtility.CheckNotNull ("customBuilder", customBuilder);
      _fieldBuilder.SetCustomAttribute (customBuilder);
    }

    [CLSCompliant (false)]
    public IEmittableOperand GetEmittableOperand ()
    {
      return new EmittableField (_fieldBuilder);
    }
  }
}