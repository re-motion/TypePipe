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
using Microsoft.Scripting.Ast;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Emits members for mutable reflection objects. Note that accessor methods must be added before their associated properties and events.
  /// </summary>
  public class MemberEmitter : IMemberEmitter
  {
    private readonly Dictionary<MutableMethodInfo, IMethodBuilder> _methodMapping = new Dictionary<MutableMethodInfo, IMethodBuilder>();
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

    public void AddField (CodeGenerationContext context, MutableFieldInfo field)
    {
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("field", field);

      var fieldBuilder = context.TypeBuilder.DefineField (field.Name, field.FieldType, field.Attributes);
      fieldBuilder.RegisterWith (context.EmittableOperandProvider, field);

      DefineCustomAttributes (fieldBuilder, field);
    }

    public void AddConstructor (CodeGenerationContext context, MutableConstructorInfo constructor)
    {
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("constructor", constructor);

      var callingConvention = constructor.IsStatic ? CallingConventions.Standard : CallingConventions.HasThis;
      var parameterTypes = GetParameterTypes (constructor);
      var ctorBuilder = context.TypeBuilder.DefineConstructor (constructor.Attributes, callingConvention, parameterTypes);
      ctorBuilder.RegisterWith (context.EmittableOperandProvider, constructor);

      DefineCustomAttributes (ctorBuilder, constructor);
      DefineParameters (ctorBuilder, constructor);

      var bodyBuildAction = CreateBodyBuildAction (context, ctorBuilder, constructor.ParameterExpressions, constructor.Body);
      context.PostDeclarationsActionManager.AddAction (bodyBuildAction);
    }

    public void AddMethod (CodeGenerationContext context, MutableMethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("method", method);

      var parameterTypes = GetParameterTypes (method);
      var methodBuilder = context.TypeBuilder.DefineMethod (method.Name, method.Attributes, method.ReturnType, parameterTypes);
      methodBuilder.RegisterWith (context.EmittableOperandProvider, method);
      _methodMapping.Add (method, methodBuilder);

      DefineCustomAttributes (methodBuilder, method);
      DefineParameter (methodBuilder, method.MutableReturnParameter);
      DefineParameters (methodBuilder, method);

      if (!method.IsAbstract)
      {
        var bodyBuildAction = CreateBodyBuildAction (context, methodBuilder, method.ParameterExpressions, method.Body);
        context.PostDeclarationsActionManager.AddAction (bodyBuildAction);
      }

      var explicitOverrideAction = CreateExplicitOverrideBuildAction (context, method);
      context.PostDeclarationsActionManager.AddAction (explicitOverrideAction);
    }

    public void AddProperty (CodeGenerationContext context, MutablePropertyInfo property)
    {
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("property", property);

      var getMethod = property.MutableGetMethod;
      var setMethod = property.MutableSetMethod;
      Assertion.IsTrue (getMethod == null || setMethod == null || getMethod.CallingConvention == setMethod.CallingConvention);

      var callingConvention = (getMethod ?? setMethod).CallingConvention;
      var indexParameterTypes = property.GetIndexParameters().Select (p => p.ParameterType).ToArray();
      var propertyBuilder = context.TypeBuilder.DefineProperty (
          property.Name, property.Attributes, callingConvention, property.PropertyType, indexParameterTypes);

      DefineCustomAttributes (propertyBuilder, property);

      if (getMethod != null)
        propertyBuilder.SetGetMethod (_methodMapping[getMethod]);
      if (setMethod != null)
        propertyBuilder.SetSetMethod (_methodMapping[setMethod]);
    }

    public void AddEvent (CodeGenerationContext context, MutableEventInfo event_)
    {
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("event_", event_);

      var addMethod = event_.MutableAddMethod;
      var removeMethod = event_.MutableRemoveMethod;
      var raiseMethod = event_.MutableRaiseMethod;

      var eventBuilder = context.TypeBuilder.DefineEvent (event_.Name, event_.Attributes, event_.EventHandlerType);

      DefineCustomAttributes (eventBuilder, event_);

      eventBuilder.SetAddOnMethod (_methodMapping[addMethod]);
      eventBuilder.SetRemoveOnMethod (_methodMapping[removeMethod]);
      eventBuilder.SetRaiseMethod (_methodMapping[raiseMethod]);
    }

    private void DefineCustomAttributes (ICustomAttributeTargetBuilder customAttributeTargetBuilder, IMutableInfo mutableInfo)
    {
      foreach (var declaration in mutableInfo.AddedCustomAttributes)
        customAttributeTargetBuilder.SetCustomAttribute (declaration);
    }

    private Type[] GetParameterTypes (MethodBase methodBase)
    {
      return methodBase.GetParameters().Select (pe => pe.ParameterType).ToArray();
    }

    private void DefineParameters (IMethodBaseBuilder methodBaseBuilder, IMutableMethodBase mutableMethodBase)
    {
      foreach (var parameter in mutableMethodBase.MutableParameters)
        DefineParameter (methodBaseBuilder, parameter);
    }

    private void DefineParameter (IMethodBaseBuilder methodBaseBuilder, MutableParameterInfo parameter)
    {
      var parameterBuilder = methodBaseBuilder.DefineParameter (parameter.Position + 1, parameter.Attributes, parameter.Name);
      DefineCustomAttributes (parameterBuilder, parameter);
    }

    private Action CreateBodyBuildAction (
        CodeGenerationContext context,
        IMethodBaseBuilder methodBuilder,
        IEnumerable<ParameterExpression> parameterExpressions,
        Expression unpreparedBody)
    {
      // Bodies need to be generated after all other members have been declared (to allow bodies to reference new members).
      return () =>
      {
        var body = _expressionPreparer.PrepareBody (context, unpreparedBody);
        var bodyLambda = Expression.Lambda (body, parameterExpressions);
        methodBuilder.SetBody (bodyLambda, _ilGeneratorFactory, context.DebugInfoGenerator);
      };
    }

    private Action CreateExplicitOverrideBuildAction (CodeGenerationContext context, MutableMethodInfo overridingMethod)
    {
      return () =>
      {
        var emittableOverridingMethod = context.EmittableOperandProvider.GetEmittableMethod (overridingMethod);

        foreach (var overriddenMethod in overridingMethod.AddedExplicitBaseDefinitions)
        {
          var emittableOverriddenMethod = context.EmittableOperandProvider.GetEmittableMethod (overriddenMethod);
          context.TypeBuilder.DefineMethodOverride (emittableOverridingMethod, emittableOverriddenMethod);
        }
      };
    }
  }
}