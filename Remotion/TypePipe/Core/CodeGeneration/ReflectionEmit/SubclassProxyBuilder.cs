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
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Ast;
using Remotion.TypePipe.Caching;
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
    private readonly IMemberEmitter _memberEmitter;

    private readonly DeferredActionManager _postDeclarationsActions = new DeferredActionManager();
    private readonly MemberEmitterContext _context;

    private bool _hasBeenBuilt = false;

    [CLSCompliant (false)]
    public SubclassProxyBuilder (
        MutableType mutableType,
        ITypeBuilder typeBuilder,
        DebugInfoGenerator debugInfoGeneratorOrNull,
        IEmittableOperandProvider emittableOperandProvider,
        IMethodTrampolineProvider methodTrampolineProvider,
        IMemberEmitter memberEmitter)
    {
      ArgumentUtility.CheckNotNull ("mutableType", mutableType);
      ArgumentUtility.CheckNotNull ("typeBuilder", typeBuilder);
      ArgumentUtility.CheckNotNull ("emittableOperandProvider", emittableOperandProvider);
      ArgumentUtility.CheckNotNull ("methodTrampolineProvider", methodTrampolineProvider);
      ArgumentUtility.CheckNotNull ("memberEmitter", memberEmitter);

      _memberEmitter = memberEmitter;
      _context = new MemberEmitterContext (
          mutableType, typeBuilder, debugInfoGeneratorOrNull, emittableOperandProvider, methodTrampolineProvider, _postDeclarationsActions);
    }

    [CLSCompliant (false)]
    public IMemberEmitter MemberEmitter
    {
      get { return _memberEmitter; }
    }

    public MemberEmitterContext MemberEmitterContext
    {
      get { return _context; }
    }

    public void HandleTypeInitializations (ReadOnlyCollection<Expression> initializationExpressions)
    {
      ArgumentUtility.CheckNotNull ("initializationExpressions", initializationExpressions);
      EnsureNotBuilt();

      if (initializationExpressions.Count == 0)
        return;

      var attributes = MethodAttributes.Private | MethodAttributes.Static;
      var parameters = UnderlyingParameterInfoDescriptor.EmptyParameters;
      var body = Expression.Block (typeof (void), initializationExpressions);
      var descriptor = UnderlyingConstructorInfoDescriptor.Create (attributes, parameters, body);
      var typeInitializer = new MutableConstructorInfo (_context.MutableType, descriptor);

      HandleAddedConstructor (typeInitializer);
    }

    public void HandleInstanceInitializations (ReadOnlyCollection<Expression> initializationExpressions)
    {
      ArgumentUtility.CheckNotNull ("initializationExpressions", initializationExpressions);
      EnsureNotBuilt();

      if (initializationExpressions.Count == 0)
        return;

      HandleAddedInterface (typeof (IInitializableObject));

      var interfaceMethod = MemberInfoFromExpressionUtility.GetMethod ((IInitializableObject obj) => obj.Initialize());
      var name = MethodOverrideUtility.GetNameForExplicitOverride (interfaceMethod);
      var attributes = MethodOverrideUtility.GetAttributesForExplicitOverride (interfaceMethod).Unset (MethodAttributes.Abstract);
      var returnType = typeof (void);
      var parameters = UnderlyingParameterInfoDescriptor.EmptyParameters;
      var body = Expression.Block (returnType, initializationExpressions);
      var descriptor = UnderlyingMethodInfoDescriptor.Create (name, attributes, returnType, parameters, null, false, false, false, body);
      var initializeMethod = new MutableMethodInfo (_context.MutableType, descriptor);

      HandleAddedMethod (initializeMethod);

      // Manually add explicit override because we cannot use MutableMethodInfo.AddExplicitBaseDefinition().
      var emittableMethod = _context.EmittableOperandProvider.GetEmittableMethod (initializeMethod);
      _context.TypeBuilder.DefineMethodOverride (emittableMethod, interfaceMethod);
    }

    public void HandleAddedInterface (Type addedInterface)
    {
      ArgumentUtility.CheckNotNull ("addedInterface", addedInterface);
      EnsureNotBuilt ();

      _context.TypeBuilder.AddInterfaceImplementation (addedInterface);
    }

    public void HandleAddedField (MutableFieldInfo field)
    {
      ArgumentUtility.CheckNotNull ("field", field);
      EnsureNotBuilt ();
      CheckMemberState (field, "field", isNew: true, isModified: null);

      _memberEmitter.AddField (_context, field);
    }

    public void HandleAddedConstructor (MutableConstructorInfo constructor)
    {
      ArgumentUtility.CheckNotNull ("constructor", constructor);
      EnsureNotBuilt ();
      CheckMemberState (constructor, "constructor", isNew: true, isModified: null);

      _memberEmitter.AddConstructor (_context, constructor);
    }

    public void HandleAddedMethod (MutableMethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);
      EnsureNotBuilt ();
      CheckMemberState (method, "method", isNew: true, isModified: null);

      _memberEmitter.AddMethod (_context, method, method.Attributes);
    }

    public void HandleModifiedConstructor (MutableConstructorInfo constructor)
    {
      ArgumentUtility.CheckNotNull ("constructor", constructor);
      EnsureNotBuilt ();
      CheckMemberState (constructor, "constructor", isNew: false, isModified: true);

      _memberEmitter.AddConstructor (_context, constructor);
    }

    public void HandleModifiedMethod (MutableMethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);
      EnsureNotBuilt ();
      CheckMemberState (method, "method", isNew: false, isModified: true);

      // Modified methods are added as implicit method overrides for the underlying method.
      var attributes = MethodOverrideUtility.GetAttributesForImplicitOverride (method);
      _memberEmitter.AddMethod (_context, method, attributes);
    }

    public void HandleUnmodifiedField (MutableFieldInfo field)
    {
      ArgumentUtility.CheckNotNull ("field", field);
      EnsureNotBuilt();
      CheckMemberState (field, "field", isNew: false, isModified: false);

      _context.EmittableOperandProvider.AddMapping (field, field.UnderlyingSystemFieldInfo);
    }

    public void HandleUnmodifiedConstructor (MutableConstructorInfo constructor)
    {
      ArgumentUtility.CheckNotNull ("constructor", constructor);
      EnsureNotBuilt();
      CheckMemberState (constructor, "constructor", isNew: false, isModified: false);

      if (SubclassFilterUtility.IsVisibleFromSubclass (constructor))
        // Ctors must be explicitly copied, because subclasses do not inherit the ctors from their base class.
        _memberEmitter.AddConstructor (_context, constructor);
    }

    public void HandleUnmodifiedMethod (MutableMethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);
      EnsureNotBuilt ();
      CheckMemberState (method, "method", isNew: false, isModified: false);

      _context.EmittableOperandProvider.AddMapping (method, method.UnderlyingSystemMethodInfo);
    }

    public Type Build ()
    {
      EnsureNotBuilt();
      _hasBeenBuilt = true;

      _postDeclarationsActions.ExecuteAllActions();

      return _context.TypeBuilder.CreateType ();
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