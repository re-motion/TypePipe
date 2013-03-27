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
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Remotion.Collections;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Implements <see cref="IMutableTypeCodeGenerator"/> using <see cref="ITypeBuilder"/> and related interfaces.
  /// Implements forward declarations of types and method/constructor bodies by deferring code emission.
  /// This is necessary to allow the generation of types and method bodies which reference each other.
  /// </summary>
  public class MutableTypeCodeGenerator : IMutableTypeCodeGenerator
  {
    private readonly MutableType _mutableType;
    private readonly IReflectionEmitCodeGenerator _codeGenerator;
    private readonly IEmittableOperandProvider _emittableOperandProvider;
    private readonly IMemberEmitter _memberEmitter;
    private readonly IInitializationBuilder _initializationBuilder;
    private readonly IProxySerializationEnabler _proxySerializationEnabler;

    private int _state;
    private CodeGenerationContext _context;

    [CLSCompliant (false)]
    public MutableTypeCodeGenerator (
        MutableType mutableType,
        IReflectionEmitCodeGenerator codeGenerator,
        IEmittableOperandProvider emittableOperandProvider,
        IMemberEmitter memberEmitter,
        IInitializationBuilder initializationBuilder,
        IProxySerializationEnabler proxySerializationEnabler)
    {
      ArgumentUtility.CheckNotNull ("mutableType", mutableType);
      ArgumentUtility.CheckNotNull ("codeGenerator", codeGenerator);
      ArgumentUtility.CheckNotNull ("emittableOperandProvider", emittableOperandProvider);
      ArgumentUtility.CheckNotNull ("memberEmitter", memberEmitter);
      ArgumentUtility.CheckNotNull ("initializationBuilder", initializationBuilder);
      ArgumentUtility.CheckNotNull ("proxySerializationEnabler", proxySerializationEnabler);

      _mutableType = mutableType;
      _codeGenerator = codeGenerator;
      _emittableOperandProvider = emittableOperandProvider;
      _memberEmitter = memberEmitter;
      _initializationBuilder = initializationBuilder;
      _proxySerializationEnabler = proxySerializationEnabler;
    }

    public void DeclareType ()
    {
      EnsureState (0);

      var typeBuilder = _codeGenerator.DefineType (_mutableType.FullName, _mutableType.Attributes);
      typeBuilder.RegisterWith (_emittableOperandProvider, _mutableType);

      _context = new CodeGenerationContext (_mutableType, typeBuilder, _codeGenerator.DebugInfoGenerator, _emittableOperandProvider);
    }

    public void DefineTypeFacets ()
    {
      EnsureState (1);

      if (_mutableType.BaseType != null)
        _context.TypeBuilder.SetParent (_mutableType.BaseType);
      if (_mutableType.MutableTypeInitializer != null)
        _memberEmitter.AddConstructor (_context, _mutableType.MutableTypeInitializer);

      // Creation of initialization members must happen before interfaces, fields or methods are added.
      var initializationMembers = _initializationBuilder.CreateInitializationMembers (_mutableType);
      var initializationMethod = initializationMembers != null ? initializationMembers.Item2 : null;
      _proxySerializationEnabler.MakeSerializable (_mutableType, initializationMethod);

      foreach (var attribute in _mutableType.AddedCustomAttributes)
        _context.TypeBuilder.SetCustomAttribute (attribute);
      foreach (var ifc in _mutableType.AddedInterfaces)
        _context.TypeBuilder.AddInterfaceImplementation (ifc);
      foreach (var field in _mutableType.AddedFields)
        _memberEmitter.AddField (_context, field);
      foreach (var ctor in _mutableType.AddedConstructors)
        WireAndAddConstructor (_memberEmitter, _context, ctor, initializationMembers);
      foreach (var method in _mutableType.AddedMethods)
        _memberEmitter.AddMethod (_context, method);
      // Note that accessor methods must be added before their associated properties and events.
      foreach (var property in _mutableType.AddedProperties)
        _memberEmitter.AddProperty (_context, property);
      foreach (var evt in _mutableType.AddedEvents)
        _memberEmitter.AddEvent (_context, evt);
    }

    public Type CreateType ()
    {
      EnsureState (2);

      _context.PostDeclarationsActionManager.ExecuteAllActions();

      return _context.TypeBuilder.CreateType();
    }

    private void WireAndAddConstructor (
        IMemberEmitter member, CodeGenerationContext context, MutableConstructorInfo constructor, Tuple<FieldInfo, MethodInfo> initializationMembers)
    {
      _initializationBuilder.WireConstructorWithInitialization (constructor, initializationMembers, _proxySerializationEnabler);
      member.AddConstructor (context, constructor);
    }

    [AssertionMethod]
    private void EnsureState (int expectedState)
    {
      if (_state != expectedState)
        throw new InvalidOperationException (
            "Methods DeclareType, DefineTypeFacets and CreateType must be called exactly once and in the correct order.");

      _state++;
    }
  }
}