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
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection.ReflectionEmit;
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
        MutableType mutableType,
        ITypeBuilder typeBuilder,
        DebugInfoGenerator debugInfoGeneratorOrNull,
        IEmittableOperandProvider emittableOperandProvider,
        IMethodTrampolineProvider methodTrampolineProvider)
    {
      ArgumentUtility.CheckNotNull ("memberEmitter", memberEmitter);
      ArgumentUtility.CheckNotNull ("initializationBuilder", initializationBuilder);
      ArgumentUtility.CheckNotNull ("proxySerializationEnabler", proxySerializationEnabler);
      ArgumentUtility.CheckNotNull ("mutableType", mutableType);
      ArgumentUtility.CheckNotNull ("typeBuilder", typeBuilder);
      ArgumentUtility.CheckNotNull ("emittableOperandProvider", emittableOperandProvider);
      ArgumentUtility.CheckNotNull ("methodTrampolineProvider", methodTrampolineProvider);

      _memberEmitter = memberEmitter;
      _initializationBuilder = initializationBuilder;
      _proxySerializationEnabler = proxySerializationEnabler;

      _context = new MemberEmitterContext (mutableType, typeBuilder, debugInfoGeneratorOrNull, emittableOperandProvider, methodTrampolineProvider);
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

    public Type Build (MutableType mutableType)
    {
      ArgumentUtility.CheckNotNull ("mutableType", mutableType);

      var typeInitializer = _initializationBuilder.CreateTypeInitializer (mutableType);
      if (typeInitializer != null)
        _memberEmitter.AddConstructor (_context, typeInitializer);

      var initializationMembers = _initializationBuilder.CreateInstanceInitializationMembers (mutableType);
      var initializationMethod = initializationMembers != null ? initializationMembers.Item2 : null;

      _proxySerializationEnabler.MakeSerializable (mutableType, initializationMethod);

      foreach (var customAttribute in mutableType.AddedCustomAttributes)
        _context.TypeBuilder.SetCustomAttribute (customAttribute);

      foreach (var ifc in mutableType.AddedInterfaces)
        _context.TypeBuilder.AddInterfaceImplementation (ifc);

      foreach (var field in mutableType.AddedFields)
        _memberEmitter.AddField (_context, field);
      foreach (var constructor in mutableType.AddedConstructors)
        WireAndAddConstructor (constructor, initializationMembers);
      foreach (var method in mutableType.AddedMethods)
        _memberEmitter.AddMethod (_context, method, method.Attributes);

      // TODO: Copy ctors from base class.

      _context.PostDeclarationsActionManager.ExecuteAllActions();

      return _context.TypeBuilder.CreateType();
    }

    private void WireAndAddConstructor (MutableConstructorInfo constructor, Tuple<FieldInfo, MethodInfo> initializationMembers)
    {
      _initializationBuilder.WireConstructorWithInitialization (constructor, initializationMembers, _proxySerializationEnabler);
      _memberEmitter.AddConstructor (_context, constructor);
    }

    private void AddConstructorIfVisibleFromSubclass (MutableConstructorInfo constructor, Tuple<FieldInfo, MethodInfo> initializationMembers)
    {
      // Ctors must be explicitly copied, because subclasses do not inherit the ctors from their base class.
      if (SubclassFilterUtility.IsVisibleFromSubclass (constructor))
        WireAndAddConstructor (constructor, initializationMembers);
    }

    private void AddMethodAsImplicitOverride (MutableMethodInfo method)
    {
      // Modified methods are added as implicit method overrides for the underlying method.
      var attributes = MethodOverrideUtility.GetAttributesForImplicitOverride (method);
      _memberEmitter.AddMethod (_context, method, attributes);
    }
  }
}