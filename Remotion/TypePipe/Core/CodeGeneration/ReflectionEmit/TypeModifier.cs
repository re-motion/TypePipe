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
using System.Reflection;
using System.Runtime.CompilerServices;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.BuilderAbstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
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
    private readonly IModuleBuilder _moduleBuilder;
    private readonly ISubclassProxyNameProvider _subclassProxyNameProvider;
    private readonly DebugInfoGenerator _debugInfoGenerator;

    [CLSCompliant (false)]
    public TypeModifier (
        IModuleBuilder moduleBuilder,
        ISubclassProxyNameProvider subclassProxyNameProvider,
        DebugInfoGenerator debugInfoGeneratorOrNull)
    {
      ArgumentUtility.CheckNotNull ("moduleBuilder", moduleBuilder);
      ArgumentUtility.CheckNotNull ("subclassProxyNameProvider", subclassProxyNameProvider);

      _moduleBuilder = moduleBuilder;
      _subclassProxyNameProvider = subclassProxyNameProvider;
      _debugInfoGenerator = debugInfoGeneratorOrNull;
    }

    [CLSCompliant (false)]
    public IModuleBuilder ModuleBuilder
    {
      get { return _moduleBuilder; }
    }

    public ISubclassProxyNameProvider SubclassProxyNameProvider
    {
      get { return _subclassProxyNameProvider; }
    }

    public DebugInfoGenerator DebugInfoGenerator
    {
      get { return _debugInfoGenerator; }
    }

    public Type ApplyModifications (MutableType mutableType)
    {
      ArgumentUtility.CheckNotNull ("mutableType", mutableType);

      var subclassProxyName = _subclassProxyNameProvider.GetSubclassProxyName (mutableType);
      var typeBuilder = _moduleBuilder.DefineType (
          subclassProxyName,
          TypeAttributes.Public | TypeAttributes.BeforeFieldInit,
          mutableType.UnderlyingSystemType);

      var mutableReflectionObjectMap = new ReflectionToBuilderMap ();
      mutableReflectionObjectMap.AddMapping (mutableType, typeBuilder);

      var ilGeneratorFactory = new ILGeneratorDecoratorFactory (new OffsetTrackingILGeneratorFactory (), mutableReflectionObjectMap);
      var modificationHandler = new TypeModificationHandler (
          typeBuilder, new ExpandingExpressionPreparer(), mutableReflectionObjectMap, ilGeneratorFactory, _debugInfoGenerator);

      // Ctors must be explicitly copied, because subclasses do not inherit the ctors from their base class.
      foreach (var clonedCtor in mutableType.ExistingConstructors.Where (ctor => !ctor.IsModified))
        modificationHandler.HandleUnmodifiedConstructor (clonedCtor);

      mutableType.Accept (modificationHandler);

      return typeBuilder.CreateType();
    }
  }
}