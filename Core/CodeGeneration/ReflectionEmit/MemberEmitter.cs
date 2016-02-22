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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Generics;
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

      var parameterTypes = GetParameterTypes (constructor);
      var ctorBuilder = context.TypeBuilder.DefineConstructor (constructor.Attributes, constructor.CallingConvention, parameterTypes);
      ctorBuilder.RegisterWith (context.EmittableOperandProvider, constructor);

      DefineParameters (ctorBuilder, constructor);
      DefineCustomAttributes (ctorBuilder, constructor);

      var bodyBuildAction = CreateBodyBuildAction (context, ctorBuilder, constructor.ParameterExpressions, constructor.Body);
      context.PostDeclarationsActionManager.AddAction (bodyBuildAction);
    }

    public void AddMethod (CodeGenerationContext context, MutableMethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("method", method);

      var methodBuilder = context.TypeBuilder.DefineMethod (method.Name, method.Attributes);
      methodBuilder.RegisterWith (context.EmittableOperandProvider, method);
      context.MethodBuilders.Add (method, methodBuilder);

      // Generic parameters must be defined before the signature as generic parameters may be used in the signature.
      DefineGenericParameters (context, methodBuilder, method);

      methodBuilder.SetReturnType (method.ReturnType);
      methodBuilder.SetParameters (GetParameterTypes (method));
      
      DefineParameter (methodBuilder, method.MutableReturnParameter);
      DefineParameters (methodBuilder, method);
      DefineCustomAttributes (methodBuilder, method);

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
        propertyBuilder.SetGetMethod (context.MethodBuilders[getMethod]);
      if (setMethod != null)
        propertyBuilder.SetSetMethod (context.MethodBuilders[setMethod]);
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

      eventBuilder.SetAddOnMethod (context.MethodBuilders[addMethod]);
      eventBuilder.SetRemoveOnMethod (context.MethodBuilders[removeMethod]);
      if (raiseMethod != null)
        eventBuilder.SetRaiseMethod (context.MethodBuilders[raiseMethod]);
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

    private void DefineGenericParameters (CodeGenerationContext context, IMethodBuilder methodBuilder, MutableMethodInfo method)
    {
      if (!method.IsGenericMethodDefinition)
        return;

      var genericParameterNames = method.MutableGenericParameters.Select (p => p.Name).ToArray();
      var genericParametersBuilders = methodBuilder.DefineGenericParameters (genericParameterNames);

      foreach (var pair in genericParametersBuilders.Zip (method.MutableGenericParameters, (b, g) => new { Builder = b, GenericParameter = g }))
      {
        pair.Builder.RegisterWith (context.EmittableOperandProvider, pair.GenericParameter);
        DefineGenericParameter (pair.Builder, pair.GenericParameter);
      }
    }

    private void DefineGenericParameter (IGenericTypeParameterBuilder genericTypeParameterBuilder, MutableGenericParameter genericParameter)
    {
      // The following differs from just calling genericParameter.GetInterfaces() as it does not repeat the interfaces of the base type.
      var interfaceConstraints = genericParameter.GetGenericParameterConstraints().Where (g => g.IsInterface).ToArray();

      genericTypeParameterBuilder.SetGenericParameterAttributes (genericParameter.GenericParameterAttributes);
      genericTypeParameterBuilder.SetBaseTypeConstraint (genericParameter.BaseType);
      genericTypeParameterBuilder.SetInterfaceConstraints (interfaceConstraints);

      DefineCustomAttributes(genericTypeParameterBuilder, genericParameter);
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