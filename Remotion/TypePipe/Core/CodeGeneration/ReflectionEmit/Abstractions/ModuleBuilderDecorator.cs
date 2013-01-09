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
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions
{
  /// <summary>
  /// Decorates an instance of <see cref="IModuleBuilder"/> to allow <see cref="MutableType"/>s to be used in signatures and 
  /// for checking strong-name compatibility.
  /// </summary>
  public class ModuleBuilderDecorator : IModuleBuilder
  {
    private readonly IModuleBuilder _moduleBuilder;
    private readonly IEmittableOperandProvider _emittableOperandProvider;

    [CLSCompliant (false)]
    public ModuleBuilderDecorator (IModuleBuilder moduleBuilder, IEmittableOperandProvider emittableOperandProvider)
    {
      ArgumentUtility.CheckNotNull ("moduleBuilder", moduleBuilder);
      ArgumentUtility.CheckNotNull ("emittableOperandProvider", emittableOperandProvider);

      _moduleBuilder = moduleBuilder;
      _emittableOperandProvider = emittableOperandProvider;
    }

    [CLSCompliant (false)]
    public IModuleBuilder InnerModuleBuilder
    {
      get { return _moduleBuilder; }
    }

    [CLSCompliant (false)]
    public ITypeBuilder DefineType (string name, TypeAttributes attr, Type parent)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("parent", parent);

      var emittableParent = _emittableOperandProvider.GetEmittableType (parent);
      var typeBuilder = _moduleBuilder.DefineType (name, attr, emittableParent);

      return new TypeBuilderDecorator (typeBuilder, _emittableOperandProvider);
    }

    public string SaveToDisk ()
    {
      return _moduleBuilder.SaveToDisk();
    }
  }
}