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
  /// Decorates an instance of <see cref="ITypeBuilder"/> to allow <see cref="MutableType"/>s to be used in signatures and 
  /// for checking strong-name compatibility.
  /// </summary>
  public class TypeBuilderDecorator : ITypeBuilder
  {
    private readonly ITypeBuilder _typeBuilder;
    private readonly IEmittableOperandProvider _emittableOperandProvider;

    [CLSCompliant (false)]
    public TypeBuilderDecorator (ITypeBuilder typeBuilder, IEmittableOperandProvider emittableOperandProvider)
    {
      ArgumentUtility.CheckNotNull ("typeBuilder", typeBuilder);
      ArgumentUtility.CheckNotNull ("emittableOperandProvider", emittableOperandProvider);

      _typeBuilder = typeBuilder;
      _emittableOperandProvider = emittableOperandProvider;
    }

    public void SetCustomAttribute (CustomAttributeDeclaration customAttributeDeclaration)
    {
      throw new NotImplementedException();
    }

    public void RegisterWith (IEmittableOperandProvider emittableOperandProvider, MutableType type)
    {
      throw new NotImplementedException();
    }

    public void AddInterfaceImplementation (Type interfaceType)
    {
      throw new NotImplementedException();
    }

    public IFieldBuilder DefineField (string name, Type type, FieldAttributes attributes)
    {
      throw new NotImplementedException();
    }

    [CLSCompliant (false)]    
    public IConstructorBuilder DefineConstructor (MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes)
    {
      throw new NotImplementedException();
    }

    [CLSCompliant(false)]
    public IMethodBuilder DefineMethod (string name, MethodAttributes attributes, Type returnType, Type[] parameterTypes)
    {
      throw new NotImplementedException();
    }

    public void DefineMethodOverride (MethodInfo methodInfoBody, MethodInfo methodInfoDeclaration)
    {
      throw new NotImplementedException();
    }

    public Type CreateType ()
    {
      throw new NotImplementedException();
    }
  }
}