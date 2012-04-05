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
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Implements the <see cref="ITypeModifier"/> interface using Reflection.Emit.
  /// </summary>
  /// <remarks>
  /// This class modifies the behavior of types by deriving runtime-generated subclass proxies that add or override members.
  /// </remarks>
  public class TypeModifier : ITypeModifier
  {
    private readonly ISubclassProxyBuilderFactory _handlerFactory;

    [CLSCompliant (false)]
    public TypeModifier (ISubclassProxyBuilderFactory handlerFactory)
    {
      ArgumentUtility.CheckNotNull ("handlerFactory", handlerFactory);

      _handlerFactory = handlerFactory;
    }

    public Type ApplyModifications (MutableType mutableType)
    {
      ArgumentUtility.CheckNotNull ("mutableType", mutableType);

      var builder = _handlerFactory.CreateBuilder (mutableType);

      // Ctors must be explicitly copied, because subclasses do not inherit the ctors from their base class.
      foreach (var clonedCtor in mutableType.ExistingConstructors.Where (ctor => !ctor.IsModified))
        builder.AddConstructor (clonedCtor);

      mutableType.Accept (builder);

      return builder.Build();
    }
  }
}
