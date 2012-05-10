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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.MutableReflection;
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
    private readonly ITypeBuilder _typeBuilder;
    private readonly DebugInfoGenerator _debugInfoGenerator;
    private readonly IEmittableOperandProvider _emittableOperandProvider;
    private readonly IMemberEmitter _memberEmitter;

    private readonly DeferredActionManager _postDeclarationsActions = new DeferredActionManager();

    private bool _hasBeenBuilt = false;

    [CLSCompliant (false)]
    public SubclassProxyBuilder (
        ITypeBuilder typeBuilder,
        DebugInfoGenerator debugInfoGeneratorOrNull, 
        IEmittableOperandProvider emittableOperandProvider,
        IMemberEmitter memberEmitter)
    {
      ArgumentUtility.CheckNotNull ("typeBuilder", typeBuilder);
      ArgumentUtility.CheckNotNull ("emittableOperandProvider", emittableOperandProvider);
      ArgumentUtility.CheckNotNull ("memberEmitter", memberEmitter);

      _typeBuilder = typeBuilder;
      _debugInfoGenerator = debugInfoGeneratorOrNull;
      _emittableOperandProvider = emittableOperandProvider;
      _memberEmitter = memberEmitter;
    }

    [CLSCompliant (false)]
    public ITypeBuilder TypeBuilder
    {
      get { return _typeBuilder; }
    }

    public DebugInfoGenerator DebugInfoGenerator
    {
      get { return _debugInfoGenerator; }
    }

    public IEmittableOperandProvider EmittableOperandProvider
    {
      get { return _emittableOperandProvider; }
    }

    [CLSCompliant (false)]
    public IMemberEmitter MemberEmitter
    {
      get { return _memberEmitter; }
    }

    public void HandleAddedInterface (Type addedInterface)
    {
      ArgumentUtility.CheckNotNull ("addedInterface", addedInterface);
      EnsureNotBuilt ();

      _typeBuilder.AddInterfaceImplementation (addedInterface);
    }

    public void HandleAddedField (MutableFieldInfo field)
    {
      ArgumentUtility.CheckNotNull ("field", field);
      EnsureNotBuilt ();
      CheckMemberState (field, "field", isNew: true, isModified: null);

      _memberEmitter.AddField (_typeBuilder, _emittableOperandProvider, field);
    }

    public void HandleAddedConstructor (MutableConstructorInfo constructor)
    {
      ArgumentUtility.CheckNotNull ("constructor", constructor);
      EnsureNotBuilt ();
      CheckMemberState (constructor, "constructor", isNew: true, isModified: null);

      _memberEmitter.AddConstructor (_typeBuilder, _debugInfoGenerator, _emittableOperandProvider,_postDeclarationsActions, constructor);
    }

    public void HandleAddedMethod (MutableMethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);
      EnsureNotBuilt ();
      CheckMemberState (method, "method", isNew: true, isModified: null);

      _memberEmitter.AddMethod (
          _typeBuilder, _debugInfoGenerator, _emittableOperandProvider, _postDeclarationsActions, method, method.Name, method.Attributes);
    }

    public void HandleModifiedConstructor (MutableConstructorInfo constructor)
    {
      ArgumentUtility.CheckNotNull ("constructor", constructor);
      EnsureNotBuilt ();
      CheckMemberState (constructor, "constructor", isNew: false, isModified: true);

      _memberEmitter.AddConstructor (_typeBuilder, _debugInfoGenerator, _emittableOperandProvider, _postDeclarationsActions, constructor);
    }

    public void HandleModifiedMethod (MutableMethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);
      EnsureNotBuilt ();
      CheckMemberState (method, "method", isNew: false, isModified: true);

      // Modified methods are added as explicit method overrides for the underlying method
      var explicitMethodOverrideName = method.DeclaringType.FullName + "." + method.Name;
      var explicitMethodOverrideAttributes = MethodAttributeUtility.ChangeVisibility (method.Attributes, MethodAttributes.Private);

      _memberEmitter.AddMethod (
          _typeBuilder,
          _debugInfoGenerator,
          _emittableOperandProvider,
          _postDeclarationsActions,
          method,
          explicitMethodOverrideName,
          explicitMethodOverrideAttributes);
      
      var emittableMethod = _emittableOperandProvider.GetEmittableMethod (method);
      _typeBuilder.DefineMethodOverride (emittableMethod, method.UnderlyingSystemMethodInfo);
    }

    public void HandleUnmodifiedField (MutableFieldInfo field)
    {
      ArgumentUtility.CheckNotNull ("field", field);
      EnsureNotBuilt();
      CheckMemberState (field, "field", isNew: false, isModified: false);

      _emittableOperandProvider.AddMapping (field, field.UnderlyingSystemFieldInfo);
    }

    public void HandleUnmodifiedConstructor (MutableConstructorInfo constructor)
    {
      ArgumentUtility.CheckNotNull ("constructor", constructor);
      EnsureNotBuilt();
      CheckMemberState (constructor, "constructor", isNew: false, isModified: false);

      if (SubclassFilterUtility.IsVisibleFromSubclass (constructor))
        // Ctors must be explicitly copied, because subclasses do not inherit the ctors from their base class.
        _memberEmitter.AddConstructor (_typeBuilder, _debugInfoGenerator, _emittableOperandProvider, _postDeclarationsActions, constructor);
    }

    public void HandleUnmodifiedMethod (MutableMethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);
      EnsureNotBuilt ();
      CheckMemberState (method, "method", isNew: false, isModified: false);

      _emittableOperandProvider.AddMapping (method, method.UnderlyingSystemMethodInfo);
    }

    public Type Build ()
    {
      if (_hasBeenBuilt)
        throw new InvalidOperationException ("Build can only be called once.");
      
      _hasBeenBuilt = true;

      _postDeclarationsActions.ExecuteAllActions();
      return _typeBuilder.CreateType();
    }

    private void EnsureNotBuilt ()
    {
      if (_hasBeenBuilt)
        throw new InvalidOperationException ("Subclass proxy has already been built.");
    }

    private void CheckMemberState (IMutableMember member, string memberType, bool isNew, bool? isModified)
    {
      if (member.IsNew != isNew || (isModified.HasValue && member.IsModified != isModified.Value))
      {
        var modifiedOrUnmodifiedOrEmpty = isModified.HasValue ? (isModified.Value ? "modified " : "unmodified ") : "";
        var newOrExisting = isNew ? "new" : "existing";
        var message = string.Format ("The supplied {0} must be a {1}{2} {0}.", memberType, modifiedOrUnmodifiedOrEmpty, newOrExisting);
        throw new ArgumentException (message, memberType);
      }
    }
  }
}