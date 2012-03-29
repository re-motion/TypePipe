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
  /// Implements <see cref="ITypeModificationHandler"/> by applying the modifications made to a <see cref="MutableType"/> to a subclass proxy.
  /// </summary>
  public class TypeModificationHandler : ITypeModificationHandler
  {
    private readonly ITypeBuilder _subclassProxyBuilder;
    private readonly IExpressionPreparer _expressionPreparer;
    private readonly MutableReflectionObjectMap _mutableReflectionObjectMap;
    private readonly IILGeneratorFactory _ilGeneratorFactory;
    private readonly DebugInfoGenerator _debugInfoGenerator;

    [CLSCompliant (false)]
    public TypeModificationHandler (
        ITypeBuilder subclassProxyBuilder,
        IExpressionPreparer expressionPreparer,
        MutableReflectionObjectMap mutableReflectionObjectMap,
        IILGeneratorFactory ilGeneratorFactory,
        DebugInfoGenerator debugInfoGeneratorOrNull)
    {
      ArgumentUtility.CheckNotNull ("subclassProxyBuilder", subclassProxyBuilder);
      ArgumentUtility.CheckNotNull ("expressionPreparer", expressionPreparer);
      ArgumentUtility.CheckNotNull ("mutableReflectionObjectMap", mutableReflectionObjectMap);
      ArgumentUtility.CheckNotNull ("ilGeneratorFactory", ilGeneratorFactory);

      _subclassProxyBuilder = subclassProxyBuilder;
      _expressionPreparer = expressionPreparer;
      _mutableReflectionObjectMap = mutableReflectionObjectMap;
      _ilGeneratorFactory = ilGeneratorFactory;
      _debugInfoGenerator = debugInfoGeneratorOrNull;
    }

    [CLSCompliant (false)]
    public ITypeBuilder SubclassProxyBuilder
    {
      get { return _subclassProxyBuilder; }
    }

    public IExpressionPreparer ExpressionPreparer
    {
      get { return _expressionPreparer; }
    }

    public MutableReflectionObjectMap MutableReflectionObjectMap
    {
      get { return _mutableReflectionObjectMap; }
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
      _subclassProxyBuilder.AddInterfaceImplementation (addedInterface);
    }

    public void HandleAddedField (MutableFieldInfo addedField)
    {
      ArgumentUtility.CheckNotNull ("addedField", addedField);
      var fieldBuilder = _subclassProxyBuilder.DefineField (addedField.Name, addedField.FieldType, addedField.Attributes);
      // TODO 4686: Add mapping addedField => fieldBuilder to MutableReflectionObjectMap.

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

      var parameterTypes = addedConstructor.GetParameters().Select (pe => pe.ParameterType).ToArray();
      var ctorBuilder = _subclassProxyBuilder.DefineConstructor (addedConstructor.Attributes, addedConstructor.CallingConvention, parameterTypes);
      // TODO 4686: Add mapping addedConstructor => ctorBuilder to MutableReflectionObjectMap.
      
      var body = _expressionPreparer.PrepareConstructorBody (addedConstructor);
      var bodyLambda = Expression.Lambda (Expression.Block (typeof (void), body), addedConstructor.ParameterExpressions);
      ctorBuilder.SetBody (bodyLambda, _ilGeneratorFactory, _debugInfoGenerator);
    }
  }
}