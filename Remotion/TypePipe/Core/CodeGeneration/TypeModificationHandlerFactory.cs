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
using System.Linq;
using System.Runtime.CompilerServices;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.BuilderAbstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration
{
  /// <summary>
  /// Creates <see cref="TypeModificationHandler"/> instances.
  /// </summary>
  // TODO 4745: SubclassProxyBuilderFactory
  public class TypeModificationHandlerFactory : IDisposableTypeModificationHandlerFactory
  {
    private readonly IExpressionPreparer _expressionPreparer;
    private readonly DebugInfoGenerator _debugInfoGenerator;

    public TypeModificationHandlerFactory (IExpressionPreparer expressionPreparer, DebugInfoGenerator debugInfoGeneratorOrNull)
    {
      ArgumentUtility.CheckNotNull ("expressionPreparer", expressionPreparer);

      _expressionPreparer = expressionPreparer;
      _debugInfoGenerator = debugInfoGeneratorOrNull;
    }

    public IExpressionPreparer ExpressionPreparer
    {
      get { return _expressionPreparer; }
    }

    public DebugInfoGenerator DebugInfoGenerator
    {
      get { return _debugInfoGenerator; }
    }

    [CLSCompliant (false)]
    public IDisposableTypeModificationHandler CreateHandler (
        MutableType mutableType,
        ITypeBuilder subclassProxyTypeBuilder,
        ReflectionToBuilderMap reflectionToBuilderMap,
        IILGeneratorFactory ilGeneratorFactory)
    {
      var handler = new TypeModificationHandler (subclassProxyTypeBuilder, _expressionPreparer, reflectionToBuilderMap, ilGeneratorFactory, _debugInfoGenerator);

      // Ctors must be explicitly copied, because subclasses do not inherit the ctors from their base class.
      foreach (var clonedCtor in mutableType.ExistingConstructors.Where (ctor => !ctor.IsModified))
        handler.HandleUnmodifiedConstructor (clonedCtor);

      return handler;
    }
  }
}