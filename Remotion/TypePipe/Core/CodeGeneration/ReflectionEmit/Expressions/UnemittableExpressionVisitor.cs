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
using System.Diagnostics;
using System.Reflection;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Ast.Compiler;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.Expressions
{
  /// <summary>
  /// Replaces occurences of <see cref="ConstantExpression"/> that contain mutable members and other expressions that can not be emitted by 
  /// our customized <see cref="LambdaCompiler"/>.
  /// </summary>
  public class UnemittableExpressionVisitor : ExpressionVisitor
  {
    private static readonly MethodInfo s_createInstanceMethod =
        MemberInfoFromExpressionUtility.GetGenericMethodDefinition (() => Activator.CreateInstance<int>());

    private readonly CodeGenerationContext _context;
    private readonly IMethodTrampolineProvider _methodTrampolineProvider;

    public UnemittableExpressionVisitor (CodeGenerationContext context, IMethodTrampolineProvider methodTrampolineProvider)
    {
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("methodTrampolineProvider", methodTrampolineProvider);

      _context = context;
      _methodTrampolineProvider = methodTrampolineProvider;
    }

    protected internal override Expression VisitConstant (ConstantExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      if (node.Value == null)
        return base.VisitConstant (node);

      var emittableValue = GetEmittableValue (node.Value);
      if (emittableValue != node.Value)
      {
        if (!node.Type.IsInstanceOfType (emittableValue))
          throw NewNotSupportedExceptionWithDescriptiveMessage (node);

        return Expression.Constant (emittableValue, node.Type);
      }

      return base.VisitConstant (node);
    }

    protected internal override Expression VisitMethodCall (MethodCallExpression node)
    {
      return base.VisitMethodCall (node);
    }

    protected internal override Expression VisitNewArray (NewArrayExpression node)
    {
      if (node.Type is ArrayTypeBase && node.NodeType == ExpressionType.NewArrayBounds && node.Expressions.Count > 1)
      {
        var message =
            "The expression factories NewArrayBounds and NewArrayInit are not supported for multi-dimensional arrays. "
            + "To create a multi-dimensional array call the static method Array.CreateInstance and cast the result to the specific array type.";
        throw new NotSupportedException (message);
      }

      return base.VisitNewArray (node);
    }

    protected internal override Expression VisitNew (NewExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      var constructor = node.Constructor;
      if (constructor != null && constructor.DeclaringType is ArrayTypeBase)
      {
        var message =
            "Array constructors cannot be used directly in expression trees. For one-dimensional arrays use the NewArrayBounds or NewArrayInit "
            + "expression factories. For multi-dimensional arrays call the static method Array.CreateInstance and cast the result to "
            + "the specific array type.";
        throw new NotSupportedException (message);
      }

      if (constructor is GenericParameterDefaultConstructor)
      {
        var createInstance = s_createInstanceMethod.MakeTypePipeGenericMethod (node.Type);
        return Expression.Call (createInstance);
      }

      return base.VisitNew (node);
    }

    protected internal override Expression VisitUnary (UnaryExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      if (node.NodeType == ExpressionType.Convert)
      {
        var adaptedConvert = GetAdaptedConvertExpression (node);
        if (adaptedConvert != node)
          return Visit (adaptedConvert);
      }

      return base.VisitUnary (node);
    }

    protected internal override Expression VisitLambda (LambdaExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      var thisClosureVariable = Expression.Variable (_context.MutableType, "thisClosure");
      Func<Expression, Expression> lambdaPreparer = expr =>
      {
        if (expr is ThisExpression)
          return thisClosureVariable;

        var methodCall = expr as MethodCallExpression;
        if (methodCall != null && methodCall.Method is NonVirtualCallMethodInfoAdapter)
        {
          var method = ((NonVirtualCallMethodInfoAdapter) methodCall.Method).AdaptedMethod;
          var trampolineMethod = _methodTrampolineProvider.GetNonVirtualCallTrampoline (_context, method);
          return Expression.Call (thisClosureVariable, trampolineMethod, methodCall.Arguments);
        }

        return expr;
      };

      var newBody = node.Body.InlinedVisit (lambdaPreparer);
      if (newBody != node.Body)
      {
        var newLambda = node.Update (newBody, node.Parameters);
        var block = Expression.Block (
            new[] { thisClosureVariable },
            Expression.Assign (thisClosureVariable, new ThisExpression (_context.MutableType)),
            newLambda);

        return Visit (block);
      }

      return base.VisitLambda (node);
    }

    private object GetEmittableValue (object value)
    {
      var operandProvider = _context.EmittableOperandProvider;

      if (value is Type)
        return operandProvider.GetEmittableType ((Type) value);
      if (value is FieldInfo)
        return operandProvider.GetEmittableField ((FieldInfo) value);
      if (value is ConstructorInfo)
        return operandProvider.GetEmittableConstructor ((ConstructorInfo) value);
      if (value is MethodInfo)
        return operandProvider.GetEmittableMethod ((MethodInfo) value);

      return value;
    }

    private NotSupportedException NewNotSupportedExceptionWithDescriptiveMessage (ConstantExpression node)
    {
      var message =
          string.Format (
              "It is not supported to have a ConstantExpression of type '{0}' because instances of '{0}' exist only at code generation "
              + "time, not at runtime." + Environment.NewLine
              + "To embed a reference to a generated method or type, construct the ConstantExpression as follows: "
              + "Expression.Constant (myMutableMethod, typeof (MethodInfo)) or "
              + "Expression.Constant (myProxyType, typeof (Type))." + Environment.NewLine
              + "To embed a reference to another reflection object, embed a method call to the Reflection APIs, like this: " + Environment.NewLine
              + "Expression.Call (" + Environment.NewLine
              + "    Expression.Constant (myMutableField.DeclaringType, typeof (Type))," + Environment.NewLine
              + "    typeof (Type).GetMethod (\"GetField\", new[] {{ typeof (string), typeof (BindingFlags) }})," + Environment.NewLine
              + "    Expression.Constant (myMutableField.Name)," + Environment.NewLine
              + "    Expression.Constant (BindingFlags.NonPublic | BindingFlags.Instance)). (BindingFlags depend on the visibility of the member.)",
              node.Type.Name);

      return new NotSupportedException (message);
    }

    private Expression GetAdaptedConvertExpression (UnaryExpression node)
    {
      var operand = node.Operand;
      var toType = node.Type;
      var fromType = operand.Type;

      if (Equals (toType, fromType))
        return node;

      // Note that generic parameters can be instantiated with both - a refernce type or a value type.
      // The issue with this is that the IL instructions dealing with converting/boxing/unboxing are not symmetric.
      // The IL instruction <c>box</c> always leaves a reference type on the stack (box for value type, nop for reference types).
      // The outcome of <c>unbox.any</c>, however, depends on the type argument, which may be a generic parameter.
      // This means that we cannot determine the type of the value left on the evaluation stack at compile time.
      // Therefore, we box the value first (guaranteed boxed value) and immediatly unbox it again (guaranteed to work if cast is valid).
      // This is roughly equivalent why the (object) cast is necessary in the following C# snippets:
      //   T1 Method<T1, T2> (T2 t2) { return (T1) (object) t2; }

      if (toType.IsGenericParameter && fromType.IsGenericParameter)
        return BoxThenUnbox (operand, toType);
      if (toType.IsGenericParameter && IsReferenceType (fromType))
        return new UnboxExpression (operand, toType);
      if (IsReferenceType (toType) && fromType.IsGenericParameter)
        return new BoxAndCastExpression (operand, toType);
      if (toType.IsGenericParameter && fromType.IsValueType)
        Debug.Fail ("Convert expression that converts from value type to generic parameter is not valid (must convert via object).");
      if (toType.IsValueType && fromType.IsGenericParameter)
        Debug.Fail ("Convert expression that converts from generic parameter to value type is not valid (must convert via object).");

      return node;
    }

    private bool IsReferenceType (Type type)
    {
      return type.IsClass || type.IsInterface;
    }

    private UnboxExpression BoxThenUnbox (Expression operand, Type toType)
    {
      return new UnboxExpression (new BoxAndCastExpression (operand, typeof (object)), toType);
    }
  }
}