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
  /// Implements <see cref="ISubclassProxyBuilder"/> by building a subclass proxy using <see cref="ITypeBuilder"/> and related interfaces.
  /// Implements forward declarations of method and constructor bodies by deferring emission of code to the <see cref="Build"/> method.
  /// </summary>
  public class SubclassProxyBuilder : ISubclassProxyBuilder
  {
    private readonly ITypeBuilder _typeBuilder;
    private readonly IExpressionPreparer _expressionPreparer;
    private readonly EmittableOperandProvider _emittableOperandProvider;
    private readonly IILGeneratorFactory _ilGeneratorFactory;
    private readonly DebugInfoGenerator _debugInfoGenerator;

    private readonly List<Action> _buildActions = new List<Action>();

    private bool _hasBeenBuilt = false;

    [CLSCompliant (false)]
    public SubclassProxyBuilder (
        ITypeBuilder typeBuilder,
        IExpressionPreparer expressionPreparer,
        EmittableOperandProvider emittableOperandProvider,
        IILGeneratorFactory ilGeneratorFactory,
        DebugInfoGenerator debugInfoGeneratorOrNull)
    {
      ArgumentUtility.CheckNotNull ("typeBuilder", typeBuilder);
      ArgumentUtility.CheckNotNull ("expressionPreparer", expressionPreparer);
      ArgumentUtility.CheckNotNull ("emittableOperandProvider", emittableOperandProvider);
      ArgumentUtility.CheckNotNull ("ilGeneratorFactory", ilGeneratorFactory);

      _typeBuilder = typeBuilder;
      _expressionPreparer = expressionPreparer;
      _emittableOperandProvider = emittableOperandProvider;
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

    public EmittableOperandProvider EmittableOperandProvider
    {
      get { return _emittableOperandProvider; }
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

    public void HandleAddedInterface (Type interfaceType)
    {
      ArgumentUtility.CheckNotNull ("interfaceType", interfaceType);
      EnsureNotBuilt ();
      
      _typeBuilder.AddInterfaceImplementation (interfaceType);
    }

    public void HandleAddedField (MutableFieldInfo field)
    {
      ArgumentUtility.CheckNotNull ("field", field);
      EnsureNotBuilt ();
      CheckMemberState (field, "field", isNew: true, isModified: null);

      var fieldBuilder = _typeBuilder.DefineField (field.Name, field.FieldType, field.Attributes);
      _emittableOperandProvider.AddMapping (field, fieldBuilder.GetEmittableOperand());

      foreach (var declaration in field.AddedCustomAttributeDeclarations)
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

    public void HandleAddedConstructor (MutableConstructorInfo constructor)
    {
      ArgumentUtility.CheckNotNull ("constructor", constructor);
      EnsureNotBuilt ();
      CheckMemberState (constructor, "constructor", isNew: true, isModified: null);

      AddConstructor (constructor);
    }

    public void HandleAddedMethod (MutableMethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);
      EnsureNotBuilt ();
      CheckMemberState (method, "method", isNew: true, isModified: null);

      AddMethod (method, method.Name, method.Attributes, overriddenMethod: null);
    }

    public void HandleModifiedConstructor (MutableConstructorInfo constructor)
    {
      ArgumentUtility.CheckNotNull ("constructor", constructor);
      EnsureNotBuilt ();
      CheckMemberState (constructor, "constructor", isNew: false, isModified: true);

      AddConstructor (constructor);
    }

    public void HandleModifiedMethod (MutableMethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);
      EnsureNotBuilt ();
      CheckMemberState (method, "method", isNew: false, isModified: true);

      var explicitMethodOverrideName = method.DeclaringType.FullName + "." + method.Name;
      var explicitMethodOverrideAttributes = MethodAttributeUtility.ChangeVisibility (method.Attributes, MethodAttributes.Private);
      var overriddenMethodInfo = method.UnderlyingSystemMethodInfo;
      AddMethod (method, explicitMethodOverrideName, explicitMethodOverrideAttributes, overriddenMethodInfo);
    }

    public void HandleUnmodifiedField (MutableFieldInfo field)
    {
      ArgumentUtility.CheckNotNull ("field", field);
      EnsureNotBuilt();
      CheckMemberState (field, "field", isNew: false, isModified: false);

      _emittableOperandProvider.AddMapping (field, new EmittableField (field.UnderlyingSystemFieldInfo));
    }

    public void HandleUnmodifiedConstructor (MutableConstructorInfo constructor)
    {
      ArgumentUtility.CheckNotNull ("constructor", constructor);
      EnsureNotBuilt();
      CheckMemberState (constructor, "constructor", isNew: false, isModified: false);

      // Ctors must be explicitly copied, because subclasses do not inherit the ctors from their base class.
      AddConstructor (constructor);
    }

    public void HandleUnmodifiedMethod (MutableMethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);
      EnsureNotBuilt ();
      CheckMemberState (method, "method", isNew: false, isModified: false);

      _emittableOperandProvider.AddMapping (method, new EmittableMethod (method.UnderlyingSystemMethodInfo));
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

    private void CheckMemberState (IMutableMember member, string memberType, bool isNew, bool? isModified)
    {
      if (member.IsNew != isNew || (isModified.HasValue && member.IsModified != isModified.Value))
      {
        var modifiedOrUnmodifiedOrEmpty = isModified.HasValue ? (isModified.Value ? "modified " : "unmodified ") : "";
        var newOrExisting = isNew ? "new" : "existing";
        var message = string.Format ("The supplied {0} must be a {1}{2} {0}.", memberType, modifiedOrUnmodifiedOrEmpty, newOrExisting);
        throw new ArgumentException (message, memberType);
      }
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

    private void AddConstructor (MutableConstructorInfo constructor)
    {
      ArgumentUtility.CheckNotNull ("constructor", constructor);
      EnsureNotBuilt ();

      var parameterTypes = GetParameterTypes (constructor);
      var ctorBuilder = _typeBuilder.DefineConstructor (constructor.Attributes, CallingConventions.HasThis, parameterTypes);
      _emittableOperandProvider.AddMapping (constructor, ctorBuilder.GetEmittableOperand ());

      DefineParameters (ctorBuilder, constructor.GetParameters ());

      var body = _expressionPreparer.PrepareConstructorBody (constructor);
      RegisterBodyBuildAction (ctorBuilder, constructor.ParameterExpressions, body);
    }

    private void AddMethod (MutableMethodInfo method, string name, MethodAttributes attributes, MethodInfo overriddenMethod)
    {
      var parameterTypes = GetParameterTypes (method);
      var methodBuilder = _typeBuilder.DefineMethod (name, attributes, method.ReturnType, parameterTypes);
      _emittableOperandProvider.AddMapping (method, methodBuilder.GetEmittableOperand());

      if (overriddenMethod != null)
        methodBuilder.DefineOverride (overriddenMethod);

      DefineParameters (methodBuilder, method.GetParameters ());

      var body = _expressionPreparer.PrepareMethodBody (method);
      RegisterBodyBuildAction (methodBuilder, method.ParameterExpressions, body);
    }

    private void RegisterBodyBuildAction (IMethodBaseBuilder methodBuilder, IEnumerable<ParameterExpression> parameterExpressions, Expression body)
    {
      var bodyLambda = Expression.Lambda (body, parameterExpressions);

      // Bodies need to be generated after all other members have been declared (to allow bodies to reference new members in a circular way).
      _buildActions.Add (() => methodBuilder.SetBody (bodyLambda, _ilGeneratorFactory, _debugInfoGenerator));
    }
  }
}