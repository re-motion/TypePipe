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
using Remotion.Collections;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Implements <see cref="ISubclassProxyBuilder"/> by building a subclass proxy using <see cref="ITypeBuilder"/> and related interfaces.
  /// Implements forward declarations of method and constructor bodies by deferring emission of code to the <see cref="Build"/> method.
  /// </summary>
  public class SubclassProxyBuilder : ISubclassProxyBuilder
  {
    private readonly IMemberEmitter _memberEmitter;
    private readonly IInitializationBuilder _initializationBuilder;
    private readonly IProxySerializationEnabler _proxySerializationEnabler;

    private readonly MemberEmitterContext _context;

    [CLSCompliant (false)]
    public SubclassProxyBuilder (
        IMemberEmitter memberEmitter,
        IInitializationBuilder initializationBuilder,
        IProxySerializationEnabler proxySerializationEnabler,
        ProxyType proxyType,
        ITypeBuilder typeBuilder,
        DebugInfoGenerator debugInfoGeneratorOrNull,
        IEmittableOperandProvider emittableOperandProvider,
        IMethodTrampolineProvider methodTrampolineProvider)
    {
      ArgumentUtility.CheckNotNull ("memberEmitter", memberEmitter);
      ArgumentUtility.CheckNotNull ("initializationBuilder", initializationBuilder);
      ArgumentUtility.CheckNotNull ("proxySerializationEnabler", proxySerializationEnabler);
      ArgumentUtility.CheckNotNull ("proxyType", proxyType);
      ArgumentUtility.CheckNotNull ("typeBuilder", typeBuilder);
      ArgumentUtility.CheckNotNull ("emittableOperandProvider", emittableOperandProvider);
      ArgumentUtility.CheckNotNull ("methodTrampolineProvider", methodTrampolineProvider);

      _memberEmitter = memberEmitter;
      _initializationBuilder = initializationBuilder;
      _proxySerializationEnabler = proxySerializationEnabler;

      _context = new MemberEmitterContext (proxyType, typeBuilder, debugInfoGeneratorOrNull, emittableOperandProvider, methodTrampolineProvider);
    }

    [CLSCompliant (false)]
    public IMemberEmitter MemberEmitter
    {
      get { return _memberEmitter; }
    }

    public IInitializationBuilder InitializationBuilder
    {
      get { return _initializationBuilder; }
    }

    public IProxySerializationEnabler ProxySerializationEnabler
    {
      get { return _proxySerializationEnabler; }
    }

    public MemberEmitterContext MemberEmitterContext
    {
      get { return _context; }
    }

    public Type Build (ProxyType proxyType)
    {
      ArgumentUtility.CheckNotNull ("proxyType", proxyType);

      if (proxyType.MutableTypeInitializer != null)
        _memberEmitter.AddConstructor (_context, proxyType.MutableTypeInitializer);

      var initializationMembers = _initializationBuilder.CreateInstanceInitializationMembers (proxyType);
      var initializationMethod = initializationMembers != null ? initializationMembers.Item2 : null;

      _proxySerializationEnabler.MakeSerializable (proxyType, initializationMethod);

      foreach (var customAttribute in proxyType.AddedCustomAttributes)
        _context.TypeBuilder.SetCustomAttribute (customAttribute);

      foreach (var ifc in proxyType.AddedInterfaces)
        _context.TypeBuilder.AddInterfaceImplementation (ifc);

      foreach (var field in proxyType.AddedFields)
        _memberEmitter.AddField (_context, field);
      foreach (var constructor in proxyType.AddedConstructors)
        WireAndAddConstructor (constructor, initializationMembers);
      foreach (var method in proxyType.AddedMethods)
        _memberEmitter.AddMethod (_context, method, method.Attributes);

      _context.PostDeclarationsActionManager.ExecuteAllActions();

      return _context.TypeBuilder.CreateType();
    }

    private void WireAndAddConstructor (MutableConstructorInfo constructor, Tuple<FieldInfo, MethodInfo> initializationMembers)
    {
      _initializationBuilder.WireConstructorWithInitialization (constructor, initializationMembers, _proxySerializationEnabler);
      _memberEmitter.AddConstructor (_context, constructor);
    }
  }
}