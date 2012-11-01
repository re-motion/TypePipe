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
using System.Linq;
using System.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Provides method stubs for performing non virtual calls to virtual methods as it is needed for base calls.
  /// </summary>
  /// <remarks>This class is used by <see cref="UnemittableExpressionVisitor"/>.</remarks>
  public class MethodTrampolineProvider : IMethodTrampolineProvider
  {
    private readonly ITypeBuilder _typeBuilder;

    private readonly Dictionary<MethodInfo, MethodInfo> _trampolines =
        new Dictionary<MethodInfo, MethodInfo> (MemberInfoEqualityComparer<MethodInfo>.Instance);
        // TODO 4972: Use TypeEqualityComparer.

    [CLSCompliant (false)]
    public MethodTrampolineProvider (ITypeBuilder typeBuilder)
    {
      ArgumentUtility.CheckNotNull ("typeBuilder", typeBuilder);

      _typeBuilder = typeBuilder;
    }

    [CLSCompliant (false)]
    public ITypeBuilder TypeBuilder
    {
      get { return _typeBuilder; }
    }

    public MethodInfo GetNonVirtualCallTrampoline (MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);

      MethodInfo trampoline;
      if (!_trampolines.TryGetValue (method, out trampoline))
      {
        trampoline = CreateNonVirtualCallTrampoline (method);
        _trampolines.Add (method, trampoline);
      }

      return trampoline;
    }

    private MethodInfo CreateNonVirtualCallTrampoline (MethodInfo method)
    {
      var trampolineName = string.Format ("{0}.{1}_NonVirtualCallTrampoline", method.DeclaringType.FullName, method.Name);
      var parameterTypes = method.GetParameters().Select (p => p.ParameterType).ToArray();
      var methodBuilder = _typeBuilder.DefineMethod (trampolineName, MethodAttributes.Private, method.ReturnType, parameterTypes);

      foreach (var parameter in method.GetParameters())
        methodBuilder.DefineParameter (parameter.Position + 1, parameter.Attributes, parameter.Name);

      // TODO 4876: Break abstraction?
      //var dummy = new EmittableOperandProvider();
      //MutableMethodInfo mutableMethod = null;
      //methodBuilder.RegisterWith (dummy, mutableMethod);
      //return dummy.GetEmittableMethod (mutableMethod);

      return methodBuilder.GetInternalMethodBuilder();
    }
  }
}