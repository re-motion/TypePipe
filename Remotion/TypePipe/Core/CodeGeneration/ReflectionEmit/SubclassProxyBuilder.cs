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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.BuilderAbstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Implements <see cref="ISubclassProxyBuilder"/> by building a subclass proxy using <see cref="ITypeBuilder"/> and related interfaces.
  /// Implements forward declarations of method and constructor bodies by deferring emission of code to the <see cref="Build"/> method.
  /// </summary>
  public class SubclassProxyBuilder : ISubclassProxyBuilder
  {
    private readonly ITypeBuilder _typeBuilder;
    private readonly IExpressionPreparer _expressionPreparer;
    private readonly ReflectionToBuilderMap _reflectionToBuilderMap;
    private readonly IILGeneratorFactory _ilGeneratorFactory;
    private readonly DebugInfoGenerator _debugInfoGenerator;

    private readonly List<Action> _buildActions = new List<Action>();

    private bool _hasBeenBuilt = false;

    [CLSCompliant (false)]
    public SubclassProxyBuilder (
        ITypeBuilder typeBuilder,
        IExpressionPreparer expressionPreparer,
        ReflectionToBuilderMap reflectionToBuilderMap,
        IILGeneratorFactory ilGeneratorFactory,
        DebugInfoGenerator debugInfoGeneratorOrNull)
    {
      ArgumentUtility.CheckNotNull ("typeBuilder", typeBuilder);
      ArgumentUtility.CheckNotNull ("expressionPreparer", expressionPreparer);
      ArgumentUtility.CheckNotNull ("reflectionToBuilderMap", reflectionToBuilderMap);
      ArgumentUtility.CheckNotNull ("ilGeneratorFactory", ilGeneratorFactory);

      _typeBuilder = typeBuilder;
      _expressionPreparer = expressionPreparer;
      _reflectionToBuilderMap = reflectionToBuilderMap;
      _ilGeneratorFactory = ilGeneratorFactory;
      _debugInfoGenerator = debugInfoGeneratorOrNull;
    }

    [CLSCompliant (false)]
    public ITypeBuilder TypeBuilder
    {
      get { return _typeBuilder; }
    }

    public IExpressionPreparer ExpressionPreparer
    {
      get { return _expressionPreparer; }
    }

    public ReflectionToBuilderMap ReflectionToBuilderMap
    {
      get { return _reflectionToBuilderMap; }
    }

    [CLSCompliant (false)]
    public IILGeneratorFactory ILGeneratorFactory
    {
      get { return _ilGeneratorFactory; }
    }

    public DebugInfoGenerator DebugInfoGenerator
    {
      get { return _debugInfoGenerator; }
    }

    public void HandleAddedInterface (Type addedInterface)
    {
      ArgumentUtility.CheckNotNull ("addedInterface", addedInterface);
      EnsureNotBuilt ();
      
      _typeBuilder.AddInterfaceImplementation (addedInterface);
    }

    public void HandleAddedField (MutableFieldInfo addedField)
    {
      ArgumentUtility.CheckNotNull ("addedField", addedField);
      EnsureNotBuilt ();

      var fieldBuilder = _typeBuilder.DefineField (addedField.Name, addedField.FieldType, addedField.Attributes);
      _reflectionToBuilderMap.AddMapping (addedField, fieldBuilder);

      foreach (var declaration in addedField.AddedCustomAttributeDeclarations)
      {
        var propertyArguments = declaration.NamedArguments.Where (na => na.MemberInfo.MemberType == MemberTypes.Property);
        var fieldArguments = declaration.NamedArguments.Where (na => na.MemberInfo.MemberType == MemberTypes.Field);

        var customAttributeBuilder = new CustomAttributeBuilder (
            declaration.AttributeConstructorInfo, 
            declaration.ConstructorArguments,
            propertyArguments.Select (namedArg => (PropertyInfo) namedArg.MemberInfo).ToArray(),
            propertyArguments.Select (namedArg => namedArg.Value).ToArray(),
            fieldArguments.Select (namedArg => (FieldInfo) namedArg.MemberInfo).ToArray(),
            fieldArguments.Select (namedArg => namedArg.Value).ToArray()
            );

        fieldBuilder.SetCustomAttribute (customAttributeBuilder);
      }
    }

    public void HandleAddedConstructor (MutableConstructorInfo addedConstructor)
    {
      ArgumentUtility.CheckNotNull ("addedConstructor", addedConstructor);
      EnsureNotBuilt ();

      if (!addedConstructor.IsNew)
        throw new ArgumentException ("The supplied constructor must be a new constructor.", "addedConstructor");

      AddConstructor (addedConstructor);
    }

    public void HandleAddedMethod (MutableMethodInfo addedMethod)
    {
      ArgumentUtility.CheckNotNull ("addedMethod", addedMethod);
      EnsureNotBuilt ();

      if (!addedMethod.IsNew)
        throw new ArgumentException ("The supplied method must be a new method.", "addedMethod");

      AddMethod (addedMethod, addedMethod.Name, addedMethod.Attributes, overriddenMethod: null);
    }

    public void HandleModifiedConstructor (MutableConstructorInfo modifiedConstructor)
    {
      ArgumentUtility.CheckNotNull ("modifiedConstructor", modifiedConstructor);
      EnsureNotBuilt ();

      if (modifiedConstructor.IsNew || !modifiedConstructor.IsModified)
        throw new ArgumentException ("The supplied constructor must be a modified existing constructor.", "modifiedConstructor");

      AddConstructor (modifiedConstructor);
    }

    public void HandleModifiedMethod (MutableMethodInfo modifiedMethod)
    {
      ArgumentUtility.CheckNotNull ("modifiedMethod", modifiedMethod);
      EnsureNotBuilt ();

      if (modifiedMethod.IsNew || !modifiedMethod.IsModified)
        throw new ArgumentException ("The supplied method must be a modified existing method.", "modifiedMethod");

      var explicitMethodOverrideName = modifiedMethod.DeclaringType.FullName + "." + modifiedMethod.Name;
      var explicitMethodOverrideAttributes = MethodAttributeUtility.ChangeVisibility (modifiedMethod.Attributes, MethodAttributes.Private);
      var overriddenMethodInfo = modifiedMethod.UnderlyingSystemMethodInfo;
      AddMethod (modifiedMethod, explicitMethodOverrideName, explicitMethodOverrideAttributes, overriddenMethodInfo);
    }

    public void AddConstructor (MutableConstructorInfo constructor)
    {
      ArgumentUtility.CheckNotNull ("constructor", constructor);
      EnsureNotBuilt ();

      var parameterTypes = GetParameterTypes (constructor);
      var ctorBuilder = _typeBuilder.DefineConstructor (constructor.Attributes, CallingConventions.HasThis, parameterTypes);
      _reflectionToBuilderMap.AddMapping (constructor, ctorBuilder);

      DefineParameters (ctorBuilder, constructor.GetParameters ());

      var body = _expressionPreparer.PrepareConstructorBody (constructor);
      RegisterBodyBuildAction (ctorBuilder, constructor.ParameterExpressions, body);
    }

    public Type Build ()
    {
      if (_hasBeenBuilt)
        throw new InvalidOperationException ("Build can only be called once.");
      
      _hasBeenBuilt = true;

      foreach (var action in _buildActions)
        action();

      return _typeBuilder.CreateType();
    }

    private void EnsureNotBuilt ()
    {
      if (_hasBeenBuilt)
        throw new InvalidOperationException ("Subclass proxy has already been built.");
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

    private void RegisterBodyBuildAction (IMethodBaseBuilder methodBuilder, IEnumerable<ParameterExpression> parameterExpressions, Expression body)
    {
      var bodyLambda = Expression.Lambda (body, parameterExpressions);

      // Bodies need to be generated after all other members have been declared (to allow bodies to reference new members in a circular way).
      _buildActions.Add (() => methodBuilder.SetBody (bodyLambda, _ilGeneratorFactory, _debugInfoGenerator));
    }

    private void AddMethod (MutableMethodInfo method, string name, MethodAttributes attributes, MethodInfo overriddenMethod)
    {
      var parameterTypes = GetParameterTypes (method);
      var methodBuilder = _typeBuilder.DefineMethod (name, attributes, method.ReturnType, parameterTypes);
      _reflectionToBuilderMap.AddMapping (method, methodBuilder);

      if (overriddenMethod != null)
        methodBuilder.DefineOverride (overriddenMethod);

      DefineParameters (methodBuilder, method.GetParameters ());

      var body = _expressionPreparer.PrepareMethodBody (method);
      RegisterBodyBuildAction (methodBuilder, method.ParameterExpressions, body);
    }
  }
}