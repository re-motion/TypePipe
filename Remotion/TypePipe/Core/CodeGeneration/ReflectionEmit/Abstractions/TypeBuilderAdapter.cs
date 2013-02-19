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
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;
using Remotion.Collections;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions
{
  /// <summary>
  /// Adapts <see cref="TypeBuilder"/> with the <see cref="ITypeBuilder"/> interface.
  /// </summary>
  public class TypeBuilderAdapter : BuilderAdapterBase, ITypeBuilder
  {
    private readonly Dictionary<IMethodBuilder, MethodBuilder> _methodMapping = new Dictionary<IMethodBuilder, MethodBuilder>();
    private readonly TypeBuilder _typeBuilder;

    public TypeBuilderAdapter (TypeBuilder typeBuilder)
        : base (ArgumentUtility.CheckNotNull ("typeBuilder", typeBuilder).SetCustomAttribute)
    {
      _typeBuilder = typeBuilder;
    }

    public void RegisterWith (IEmittableOperandProvider emittableOperandProvider, ProxyType type)
    {
      ArgumentUtility.CheckNotNull ("emittableOperandProvider", emittableOperandProvider);
      ArgumentUtility.CheckNotNull ("type", type);

      emittableOperandProvider.AddMapping (type, _typeBuilder);
    }

    public void AddInterfaceImplementation (Type interfaceType)
    {
      ArgumentUtility.CheckNotNull ("interfaceType", interfaceType);
      _typeBuilder.AddInterfaceImplementation (interfaceType);
    }

    [CLSCompliant (false)]
    public IFieldBuilder DefineField (string name, Type type, FieldAttributes attributes)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("type", type);

      var fieldBuilder = _typeBuilder.DefineField (name, type, attributes);
      return new FieldBuilderAdapter (fieldBuilder);
    }

    [CLSCompliant (false)]
    public IConstructorBuilder DefineConstructor (MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes)
    {
      ArgumentUtility.CheckNotNull ("parameterTypes", parameterTypes);

      var constructorBuilder = _typeBuilder.DefineConstructor (attributes, callingConvention, parameterTypes);
      return new ConstructorBuilderAdapter (constructorBuilder);
    }

    [CLSCompliant (false)]
    public IMethodBuilder DefineMethod (string name, MethodAttributes attributes, Type returnType, Type[] parameterTypes)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("returnType", returnType);
      ArgumentUtility.CheckNotNull ("parameterTypes", parameterTypes);

      var methodBuilder = _typeBuilder.DefineMethod (name, attributes, returnType, parameterTypes);
      var adapter = new MethodBuilderAdapter (methodBuilder);
      _methodMapping.Add (adapter, methodBuilder);

      return adapter;
    }

    public void DefineMethodOverride (MethodInfo methodInfoBody, MethodInfo methodInfoDeclaration)
    {
      ArgumentUtility.CheckNotNull ("methodInfoBody", methodInfoBody);
      ArgumentUtility.CheckNotNull ("methodInfoDeclaration", methodInfoDeclaration);

      _typeBuilder.DefineMethodOverride (methodInfoBody, methodInfoDeclaration);
    }

    [CLSCompliant (false)]
    public IPropertyBuilder DefineProperty (string name, PropertyAttributes attributes, Type returnType, Type[] parameterTypes)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("returnType", returnType);
      ArgumentUtility.CheckNotNull ("parameterTypes", parameterTypes);

      var propertyBuilder = _typeBuilder.DefineProperty (name, attributes, returnType, parameterTypes);
      return new PropertyBuilderAdapter (propertyBuilder, _methodMapping.AsReadOnly());
    }

    [CLSCompliant (false)]
    public IPropertyBuilder DefineProperty (
        string name, PropertyAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("returnType", returnType);
      ArgumentUtility.CheckNotNull ("parameterTypes", parameterTypes);

      var propertyBuilder = _typeBuilder.DefineProperty (
          name,
          attributes,
          callingConvention,
          returnType,
          returnTypeRequiredCustomModifiers: null,
          returnTypeOptionalCustomModifiers: null,
          parameterTypes: parameterTypes,
          parameterTypeRequiredCustomModifiers: null,
          parameterTypeOptionalCustomModifiers: null);

      return new PropertyBuilderAdapter (propertyBuilder, _methodMapping.AsReadOnly());
    }

    public Type CreateType ()
    {
      return _typeBuilder.CreateType();
    }
  }
}