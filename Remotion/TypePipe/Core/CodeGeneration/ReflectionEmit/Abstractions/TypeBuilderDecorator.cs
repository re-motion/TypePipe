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
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions
{
  /// <summary>
  /// Decorates an instance of <see cref="ITypeBuilder"/> to allow <see cref="CustomType"/>s to be used in signatures and 
  /// for checking strong-name compatibility.
  /// </summary>
  public class TypeBuilderDecorator : BuilderDecoratorBase, ITypeBuilder
  {
    private readonly ITypeBuilder _typeBuilder;

    [CLSCompliant (false)]
    public TypeBuilderDecorator (ITypeBuilder typeBuilder, IEmittableOperandProvider emittableOperandProvider)
        : base (typeBuilder, emittableOperandProvider)
    {
      _typeBuilder = typeBuilder;
    }

    [CLSCompliant (false)]
    public ITypeBuilder DecoratedTypeBuilder
    {
      get { return _typeBuilder; }
    }

    public void RegisterWith (IEmittableOperandProvider emittableOperandProvider, ProxyType type)
    {
      ArgumentUtility.CheckNotNull ("emittableOperandProvider", emittableOperandProvider);
      ArgumentUtility.CheckNotNull ("type", type);

      _typeBuilder.RegisterWith (emittableOperandProvider, type);
    }

    public void AddInterfaceImplementation (Type interfaceType)
    {
      ArgumentUtility.CheckNotNull ("interfaceType", interfaceType);

      var emittableInterfaceType = EmittableOperandProvider.GetEmittableType (interfaceType);
      _typeBuilder.AddInterfaceImplementation (emittableInterfaceType);
    }

    public IFieldBuilder DefineField (string name, Type type, FieldAttributes attributes)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("type", type);

      var emittableType = EmittableOperandProvider.GetEmittableType (type);
      var fieldBuilder = _typeBuilder.DefineField (name, emittableType, attributes);

      return new FieldBuilderDecorator (fieldBuilder, EmittableOperandProvider);
    }

    [CLSCompliant (false)]    
    public IConstructorBuilder DefineConstructor (MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes)
    {
      ArgumentUtility.CheckNotNull ("parameterTypes", parameterTypes);

      var emittableParameterTypes = parameterTypes.Select (EmittableOperandProvider.GetEmittableType).ToArray();
      var constructorBuilder = _typeBuilder.DefineConstructor (attributes, callingConvention, emittableParameterTypes);

      return new ConstructorBuilderDecorator (constructorBuilder, EmittableOperandProvider);
    }

    [CLSCompliant (false)]
    public IMethodBuilder DefineMethod (string name, MethodAttributes attributes)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);

      var methodBuilder = _typeBuilder.DefineMethod (name, attributes);
      return new MethodBuilderDecorator (methodBuilder, EmittableOperandProvider);
    }

    public void DefineMethodOverride (MethodInfo methodInfoBody, MethodInfo methodInfoDeclaration)
    {
      ArgumentUtility.CheckNotNull ("methodInfoBody", methodInfoBody);
      ArgumentUtility.CheckNotNull ("methodInfoDeclaration", methodInfoDeclaration);

      var emittableMethodInfoBody = EmittableOperandProvider.GetEmittableMethod (methodInfoBody);
      var emittableMethodInfoDeclaration = EmittableOperandProvider.GetEmittableMethod (methodInfoDeclaration);
      _typeBuilder.DefineMethodOverride (emittableMethodInfoBody, emittableMethodInfoDeclaration);
    }

    [CLSCompliant (false)]
    public IPropertyBuilder DefineProperty (
        string name, PropertyAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("returnType", returnType);
      ArgumentUtility.CheckNotNull ("parameterTypes", parameterTypes);

      var emittableReturnType = EmittableOperandProvider.GetEmittableType (returnType);
      var emittableParmeterTypes = parameterTypes.Select (EmittableOperandProvider.GetEmittableType).ToArray();
      var propertyBuilder = _typeBuilder.DefineProperty (name, attributes, callingConvention, emittableReturnType, emittableParmeterTypes);

      return new PropertyBuilderDecorator (propertyBuilder, EmittableOperandProvider);
    }

    [CLSCompliant (false)]
    public IEventBuilder DefineEvent (string name, EventAttributes attributes, Type eventtype)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("eventtype", eventtype);

      var emittableEventType = EmittableOperandProvider.GetEmittableType (eventtype);
      var eventBuilder = _typeBuilder.DefineEvent (name, attributes, emittableEventType);

      return new EventBuilderDecorator (eventBuilder, EmittableOperandProvider);
    }

    public Type CreateType ()
    {
      return _typeBuilder.CreateType();
    }
  }
}