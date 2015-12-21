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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.Dlr.Runtime.CompilerServices;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// This class holds information needed by the code generation.
  /// </summary>
  public class CodeGenerationContext
  {
    private readonly IDictionary<MutableMethodInfo, IMethodBuilder> _methodBuilders = new Dictionary<MutableMethodInfo, IMethodBuilder> ();
    private readonly DeferredActionManager _postDeclarationsActionManager = new DeferredActionManager ();
    private readonly IDictionary<MethodInfo, MethodInfo> _trampolineMethods =
        new Dictionary<MethodInfo, MethodInfo> (MemberInfoEqualityComparer<MethodInfo>.Instance);

    private readonly MutableType _mutableType;
    private readonly ITypeBuilder _typeBuilder;
    private readonly DebugInfoGenerator _debugInfoGenerator;
    private readonly IEmittableOperandProvider _emittableOperandProvider;

    [CLSCompliant (false)]
    public CodeGenerationContext (
        MutableType mutableType, ITypeBuilder typeBuilder, DebugInfoGenerator debugInfoGeneratorOrNull, IEmittableOperandProvider emittableOperandProvider)
    {
      ArgumentUtility.CheckNotNull ("mutableType", mutableType);
      ArgumentUtility.CheckNotNull ("typeBuilder", typeBuilder);
      ArgumentUtility.CheckNotNull ("emittableOperandProvider", emittableOperandProvider);

      _mutableType = mutableType;
      _typeBuilder = typeBuilder;
      _debugInfoGenerator = debugInfoGeneratorOrNull;
      _emittableOperandProvider = emittableOperandProvider;
    }

    public MutableType MutableType
    {
      get { return _mutableType; }
    }

    [CLSCompliant (false)]
    public ITypeBuilder TypeBuilder
    {
      get { return _typeBuilder; }
    }

    [CLSCompliant (false)]
    public IDictionary<MutableMethodInfo, IMethodBuilder> MethodBuilders
    {
      get { return _methodBuilders; }
    }

    public DebugInfoGenerator DebugInfoGenerator
    {
      get { return _debugInfoGenerator; }
    }

    public IEmittableOperandProvider EmittableOperandProvider
    {
      get { return _emittableOperandProvider; }
    }

    public IDictionary<MethodInfo, MethodInfo> TrampolineMethods
    {
      get { return _trampolineMethods; }
    }

    public DeferredActionManager PostDeclarationsActionManager
    {
      get { return _postDeclarationsActionManager; }
    }
  }
}