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
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.BuilderAbstractions
{
  /// <summary>
  /// Adapts <see cref="TypeBuilder"/> with the <see cref="ITypeBuilder"/> interface.
  /// </summary>
  [CLSCompliant (false)]
  public class TypeBuilderAdapter : ITypeBuilder
  {
    private readonly TypeBuilder _typeBuilder;

    public TypeBuilderAdapter (TypeBuilder typeBuilder)
    {
      ArgumentUtility.CheckNotNull ("typeBuilder", typeBuilder);

      _typeBuilder = typeBuilder;
    }

    public void AddInterfaceImplementation (Type interfaceType)
    {
      ArgumentUtility.CheckNotNull ("interfaceType", interfaceType);
      _typeBuilder.AddInterfaceImplementation (interfaceType);
    }

    public IFieldBuilder DefineField (string name, Type type, FieldAttributes attributes)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("type", type);

      return new FieldBuilderAdapter (_typeBuilder.DefineField (name, type, attributes));
    }

    public IConstructorBuilder DefineConstructor (MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes)
    {
      ArgumentUtility.CheckNotNull ("parameterTypes", parameterTypes);

      var constructorBuilder = _typeBuilder.DefineConstructor (attributes, callingConvention, parameterTypes);
      return new ConstructorBuilderAdapter (constructorBuilder);
    }

    public Type CreateType ()
    {
      return _typeBuilder.CreateType();
    }
  }
}