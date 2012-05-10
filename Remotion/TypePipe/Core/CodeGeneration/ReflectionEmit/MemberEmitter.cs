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
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Ast;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Emits members for mutable reflection objects.
  /// </summary>
  public class MemberEmitter : IMemberEmitter
  {
    private readonly IExpressionPreparer _expressionPreparer;
    private readonly IILGeneratorFactory _ilGeneratorFactory;

    [CLSCompliant(false)]
    public MemberEmitter (IExpressionPreparer expressionPreparer, IILGeneratorFactory ilGeneratorFactory)
    {
      ArgumentUtility.CheckNotNull ("expressionPreparer", expressionPreparer);
      ArgumentUtility.CheckNotNull ("ilGeneratorFactory", ilGeneratorFactory);

      _expressionPreparer = expressionPreparer;
      _ilGeneratorFactory = ilGeneratorFactory;
    }

    public IExpressionPreparer ExpressionPreparer
    {
      get { return _expressionPreparer; }
    }

    [CLSCompliant (false)]
    public IILGeneratorFactory ILGeneratorFactory
    {
      get { return _ilGeneratorFactory; }
    }

    [CLSCompliant (false)]
    public void AddField (ITypeBuilder typeBuilder, IEmittableOperandProvider emittableOperandProvider, MutableFieldInfo field)
    {
      ArgumentUtility.CheckNotNull ("typeBuilder", typeBuilder);
      ArgumentUtility.CheckNotNull ("emittableOperandProvider", emittableOperandProvider);
      ArgumentUtility.CheckNotNull ("field", field);

      var fieldBuilder = typeBuilder.DefineField (field.Name, field.FieldType, field.Attributes);
      fieldBuilder.RegisterWith (emittableOperandProvider, field);

      foreach (var declaration in field.AddedCustomAttributeDeclarations)
      {
        var propertyArguments = declaration.NamedArguments.Where (na => na.MemberInfo.MemberType == MemberTypes.Property);
        var fieldArguments = declaration.NamedArguments.Where (na => na.MemberInfo.MemberType == MemberTypes.Field);

        var customAttributeBuilder = new CustomAttributeBuilder (
            declaration.AttributeConstructorInfo,
            declaration.ConstructorArguments,
            propertyArguments.Select (namedArg => (PropertyInfo) namedArg.MemberInfo).ToArray (),
            propertyArguments.Select (namedArg => namedArg.Value).ToArray (),
            fieldArguments.Select (namedArg => (FieldInfo) namedArg.MemberInfo).ToArray (),
            fieldArguments.Select (namedArg => namedArg.Value).ToArray ()
            );

        fieldBuilder.SetCustomAttribute (customAttributeBuilder);
      }
    }

    [CLSCompliant (false)]
    public void AddConstructor (
        ITypeBuilder typeBuilder,
        DebugInfoGenerator debugInfoGeneratorOrNull,
        IEmittableOperandProvider emittableOperandProvider,
        DeferredActionManager postDeclarationsActionManager,
        MutableConstructorInfo constructor)
    {
      ArgumentUtility.CheckNotNull ("typeBuilder", typeBuilder);
      ArgumentUtility.CheckNotNull ("emittableOperandProvider", emittableOperandProvider);
      ArgumentUtility.CheckNotNull ("constructor", constructor);
      ArgumentUtility.CheckNotNull ("postDeclarationsActionManager", postDeclarationsActionManager);

      var parameterTypes = GetParameterTypes (constructor);
      var ctorBuilder = typeBuilder.DefineConstructor (constructor.Attributes, CallingConventions.HasThis, parameterTypes);
      ctorBuilder.RegisterWith (emittableOperandProvider, constructor);

      DefineParameters (ctorBuilder, constructor.GetParameters ());

      var body = _expressionPreparer.PrepareConstructorBody (constructor);
      var bodyBuildAction = CreateBodyBuildAction (ctorBuilder, debugInfoGeneratorOrNull, constructor.ParameterExpressions, body);
      postDeclarationsActionManager.AddAction (bodyBuildAction);
    }

    [CLSCompliant (false)]
    public void AddMethod (
        ITypeBuilder typeBuilder,
        DebugInfoGenerator debugInfoGeneratorOrNull,
        IEmittableOperandProvider emittableOperandProvider,
        DeferredActionManager postDeclarationsActionManager,
        MutableMethodInfo method,
        string name,
        MethodAttributes attributes)
    {
      ArgumentUtility.CheckNotNull ("typeBuilder", typeBuilder);
      ArgumentUtility.CheckNotNull ("emittableOperandProvider", emittableOperandProvider);
      ArgumentUtility.CheckNotNull ("method", method);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("postDeclarationsActionManager", postDeclarationsActionManager);

      var parameterTypes = GetParameterTypes (method);
      var methodBuilder = typeBuilder.DefineMethod (name, attributes, method.ReturnType, parameterTypes);
      methodBuilder.RegisterWith (emittableOperandProvider, method);

      DefineParameters (methodBuilder, method.GetParameters ());

      var body = _expressionPreparer.PrepareMethodBody (method);
      var bodyBuildAction = CreateBodyBuildAction (methodBuilder, debugInfoGeneratorOrNull, method.ParameterExpressions, body);
      postDeclarationsActionManager.AddAction (bodyBuildAction);

      var explicitOverrideAction = CreateExplicitOverrideBuildAction (typeBuilder, emittableOperandProvider, method);
      postDeclarationsActionManager.AddAction (explicitOverrideAction);
    }

    private Type[] GetParameterTypes (MethodBase methodBase)
    {
      return methodBase.GetParameters ().Select (pe => pe.ParameterType).ToArray ();
    }

    private void DefineParameters (IMethodBaseBuilder methodBuilder, ParameterInfo[] parameterInfos)
    {
      foreach (var parameterInfo in parameterInfos)
        methodBuilder.DefineParameter (parameterInfo.Position + 1, parameterInfo.Attributes, parameterInfo.Name);
    }

    private Action CreateBodyBuildAction (
        IMethodBaseBuilder methodBuilder,
        DebugInfoGenerator debugInfoGenerator,
        IEnumerable<ParameterExpression> parameterExpressions,
        Expression body)
    {
      var bodyLambda = Expression.Lambda (body, parameterExpressions);

      // Bodies need to be generated after all other members have been declared (to allow bodies to reference new members in a circular way).
      return () => methodBuilder.SetBody (bodyLambda, _ilGeneratorFactory, debugInfoGenerator);
    }

    private Action CreateExplicitOverrideBuildAction (
        ITypeBuilder typeBuilder, IEmittableOperandProvider emittableOperandProvider, MutableMethodInfo overridingMethod)
    {
      return () =>
      {
        var emittableOverridingMethod = emittableOperandProvider.GetEmittableMethod (overridingMethod);
        foreach (var overriddenMethod in overridingMethod.AddedExplicitBaseDefinitions)
        {
          var emittableOverriddenMethod = emittableOperandProvider.GetEmittableMethod (overriddenMethod);
          typeBuilder.DefineMethodOverride (emittableOverridingMethod, emittableOverriddenMethod);
        }
      };
    }
  }
}