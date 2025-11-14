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
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions
{
  /// <summary>
  /// Adapts <see cref="TypeBuilder"/> with the <see cref="ITypeBuilder"/> interface.
  /// </summary>
  public class TypeBuilderAdapter : BuilderAdapterBase, ITypeBuilder
  {
    private readonly TypeBuilder _typeBuilder;

    public TypeBuilderAdapter (TypeBuilder typeBuilder)
        : base (ArgumentUtility.CheckNotNull (nameof(typeBuilder), typeBuilder).SetCustomAttribute)
    {
      _typeBuilder = typeBuilder;
    }

    public void RegisterWith (IEmittableOperandProvider emittableOperandProvider, MutableType type)
    {
      ArgumentUtility.CheckNotNull (nameof(emittableOperandProvider), emittableOperandProvider);
      ArgumentUtility.CheckNotNull (nameof(type), type);

      emittableOperandProvider.AddMapping (type, _typeBuilder);
    }

    public void SetParent (Type parent)
    {
      ArgumentUtility.CheckNotNull (nameof(parent), parent);

      _typeBuilder.SetParent (parent);
    }

    public void AddInterfaceImplementation (Type interfaceType)
    {
      ArgumentUtility.CheckNotNull (nameof(interfaceType), interfaceType);

      _typeBuilder.AddInterfaceImplementation (interfaceType);
    }

    [CLSCompliant (false)]
    public ITypeBuilder DefineNestedType (string name, TypeAttributes attributes)
    {
      ArgumentUtility.CheckNotNullOrEmpty (nameof(name), name);

      var typeBuilder = _typeBuilder.DefineNestedType (name, attributes);
      return new TypeBuilderAdapter (typeBuilder);
    }

    [CLSCompliant (false)]
    public IFieldBuilder DefineField (string name, Type type, FieldAttributes attributes)
    {
      ArgumentUtility.CheckNotNullOrEmpty (nameof(name), name);
      ArgumentUtility.CheckNotNull (nameof(type), type);

      var fieldBuilder = _typeBuilder.DefineField (name, type, attributes);
      return new FieldBuilderAdapter (fieldBuilder);
    }

    [CLSCompliant (false)]
    public IConstructorBuilder DefineConstructor (MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes)
    {
      ArgumentUtility.CheckNotNull (nameof(parameterTypes), parameterTypes);

      var constructorBuilder = _typeBuilder.DefineConstructor (attributes, callingConvention, parameterTypes);
      return new ConstructorBuilderAdapter (constructorBuilder);
    }

    [CLSCompliant (false)]
    public IMethodBuilder DefineMethod (string name, MethodAttributes attributes)
    {
      ArgumentUtility.CheckNotNullOrEmpty (nameof(name), name);

      var methodBuilder = _typeBuilder.DefineMethod (name, attributes);
      return new MethodBuilderAdapter (methodBuilder);
    }

    public void DefineMethodOverride (MethodInfo methodInfoBody, MethodInfo methodInfoDeclaration)
    {
      ArgumentUtility.CheckNotNull (nameof(methodInfoBody), methodInfoBody);
      ArgumentUtility.CheckNotNull (nameof(methodInfoDeclaration), methodInfoDeclaration);

      _typeBuilder.DefineMethodOverride (methodInfoBody, methodInfoDeclaration);
    }

    [CLSCompliant (false)]
    public IPropertyBuilder DefineProperty (
        string name, PropertyAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
    {
      ArgumentUtility.CheckNotNullOrEmpty (nameof(name), name);
      ArgumentUtility.CheckNotNull (nameof(returnType), returnType);
      ArgumentUtility.CheckNotNull (nameof(parameterTypes), parameterTypes);

      // We need to use the complex overload which takes a CallingConventions parameter here to correctly emit 'instance properties'.
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

      return new PropertyBuilderAdapter (propertyBuilder);
    }

    [CLSCompliant (false)]
    public IEventBuilder DefineEvent (string name, EventAttributes attributes, Type eventtype)
    {
      ArgumentUtility.CheckNotNullOrEmpty (nameof(name), name);
      ArgumentUtility.CheckNotNull (nameof(eventtype), eventtype);

      var eventBuilder = _typeBuilder.DefineEvent (name, attributes, eventtype);
      return new EventBuilderAdapter (eventBuilder);
    }

    public Type CreateType ()
    {
      return _typeBuilder.CreateType();
    }
  }
}