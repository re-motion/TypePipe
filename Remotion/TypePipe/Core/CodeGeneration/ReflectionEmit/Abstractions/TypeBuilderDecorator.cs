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
using System.Linq;

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
      ArgumentUtility.CheckNotNull ("emittableOperandProvider", emittableOperandProvider);
      ArgumentUtility.CheckNotNull ("type", type);

      _typeBuilder.RegisterWith (emittableOperandProvider, type);
    }

    public void AddInterfaceImplementation (Type interfaceType)
    {
      ArgumentUtility.CheckNotNull ("interfaceType", interfaceType);

      var emittableInterfaceType = _emittableOperandProvider.GetEmittableType (interfaceType);
      _typeBuilder.AddInterfaceImplementation (emittableInterfaceType);
    }

    public IFieldBuilder DefineField (string name, Type type, FieldAttributes attributes)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("type", type);

      var emittableType = _emittableOperandProvider.GetEmittableType (type);
      var fieldBuilder = _typeBuilder.DefineField (name, emittableType, attributes);

      return new FieldBuilderDecorator (fieldBuilder, _emittableOperandProvider);
    }

    [CLSCompliant (false)]    
    public IConstructorBuilder DefineConstructor (MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes)
    {
      ArgumentUtility.CheckNotNull ("parameterTypes", parameterTypes);

      var emittableParameterTypes = parameterTypes.Select (_emittableOperandProvider.GetEmittableType).ToArray();
      var constructorBuilder = _typeBuilder.DefineConstructor (attributes, callingConvention, emittableParameterTypes);

      return new ConstructorBuilderDecorator (constructorBuilder, _emittableOperandProvider);
    }

    [CLSCompliant(false)]
    public IMethodBuilder DefineMethod (string name, MethodAttributes attributes, Type returnType, Type[] parameterTypes)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("returnType", returnType);
      ArgumentUtility.CheckNotNull ("parameterTypes", parameterTypes);

      var emittableReturnType = _emittableOperandProvider.GetEmittableType (returnType);
      var emittableParameterTypes = parameterTypes.Select (_emittableOperandProvider.GetEmittableType).ToArray ();
      var methodBuilder = _typeBuilder.DefineMethod (name, attributes, emittableReturnType, emittableParameterTypes);

      return new MethodBuilderDecorator (methodBuilder, _emittableOperandProvider);
    }

    public void DefineMethodOverride (MethodInfo methodInfoBody, MethodInfo methodInfoDeclaration)
    {
      ArgumentUtility.CheckNotNull ("methodInfoBody", methodInfoBody);
      ArgumentUtility.CheckNotNull ("methodInfoDeclaration", methodInfoDeclaration);

      var emittableMethodInfoBody = _emittableOperandProvider.GetEmittableMethod (methodInfoBody);
      var emittableMethodInfoDeclaration = _emittableOperandProvider.GetEmittableMethod (methodInfoDeclaration);
      _typeBuilder.DefineMethodOverride (emittableMethodInfoBody, emittableMethodInfoDeclaration);
    }

    public Type CreateType ()
    {
      return _typeBuilder.CreateType();
    }
  }
}