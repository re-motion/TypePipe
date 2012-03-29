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
using Microsoft.Scripting.Ast;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.BuilderAbstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;
using System.Linq;

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
      CopyConstructorsFromBaseClass (mutableType, typeBuilder, ilGeneratorFactory, mutableReflectionObjectMap);

      var modificationHandler = new TypeModificationHandler (
          typeBuilder, new ExpandingExpressionPreparer(), mutableReflectionObjectMap, ilGeneratorFactory, _debugInfoGenerator);
      mutableType.Accept (modificationHandler);

      return typeBuilder.CreateType();
    }

    private void CopyConstructorsFromBaseClass (
        MutableType mutableType,
        ITypeBuilder typeBuilder,
        IILGeneratorFactory ilGeneratorFactory,
        ReflectionToBuilderMap reflectionToBuilderMap)
    {
      foreach (var clonedCtor in mutableType.ExistingConstructors)
      {
        // Prevent loosening of visibility if the ctor visibility is FamilyOrAssembly (change to Family because the assembly of the generated 
        // subclass is different from the assembly of the original class).
        var attributes = clonedCtor.IsFamilyOrAssembly ? ChangeVisibility (clonedCtor.Attributes, MethodAttributes.Family) : clonedCtor.Attributes;

        var parameterTypes = clonedCtor.GetParameters().Select (pi => pi.ParameterType).ToArray();
        var ctorBuilder = typeBuilder.DefineConstructor (attributes, CallingConventions.HasThis, parameterTypes);
        reflectionToBuilderMap.AddMapping (clonedCtor, ctorBuilder);

        foreach (MutableParameterInfo parameterInfo in clonedCtor.GetParameters())
          ctorBuilder.DefineParameter (parameterInfo.Position + 1, parameterInfo.Attributes, parameterInfo.Name);

        var parameterExpressions =
            clonedCtor.GetParameters().Select (paramInfo => Expression.Parameter (paramInfo.ParameterType, paramInfo.Name)).ToArray();
        var baseCallExpression = Expression.Call (
            new TypeAsUnderlyingSystemTypeExpression (new ThisExpression (mutableType)),
            new ConstructorAsMethodInfoAdapter (clonedCtor.UnderlyingSystemConstructorInfo),
            parameterExpressions.Cast<Expression>());
        var body = Expression.Lambda (baseCallExpression, parameterExpressions);

        ctorBuilder.SetBody (body, ilGeneratorFactory, _debugInfoGenerator);
      }
    }

    private MethodAttributes ChangeVisibility (MethodAttributes originalAttributes, MethodAttributes newVisibility)
    {
      return (originalAttributes & ~MethodAttributes.MemberAccessMask) | newVisibility;
    }
  }
}