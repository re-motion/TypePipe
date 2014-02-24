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
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Implements <see cref="IInitializationBuilder"/>.
  /// </summary>
  /// <threadsafety static="true" instance="true"/>
  public class InitializationBuilder : IInitializationBuilder
  {
    private static readonly MethodInfo s_interfaceMethod =
        MemberInfoFromExpressionUtility.GetMethod ((IInitializableObject obj) => obj.Initialize (InitializationSemantics.Construction));

    public InitializationBuilder ()
    {
    }

    public Tuple<FieldInfo, MethodInfo> CreateInitializationMembers (MutableType mutableType)
    {
      ArgumentUtility.CheckNotNull ("mutableType", mutableType);

      var initialization = mutableType.Initialization;
      if (initialization.Expressions.Count == 0)
        return null;

      mutableType.AddInterface (typeof (IInitializableObject));

      var counter = mutableType.AddField ("<tp>_ctorRunCounter", FieldAttributes.Private, typeof (int));
      var nonSerializedCtor = MemberInfoFromExpressionUtility.GetConstructor (() => new NonSerializedAttribute());
      counter.AddCustomAttribute (new CustomAttributeDeclaration (nonSerializedCtor, new object[0]));

      var initializationMethod = mutableType.AddExplicitOverride (s_interfaceMethod, ctx => CreateInitializationBody (ctx, initialization));

      return Tuple.Create<FieldInfo, MethodInfo> (counter, initializationMethod);
    }

    private Expression CreateInitializationBody (MethodBodyCreationContext ctx, InstanceInitialization initialization)
    {
      var replacements = new Dictionary<Expression, Expression> { { initialization.Semantics, ctx.Parameters[0] } };
      var initializations = initialization.Expressions.Select (e => e.Replace (replacements));

      return Expression.Block (typeof (void), initializations);
    }

    public void WireConstructorWithInitialization (
        MutableConstructorInfo constructor,
        Tuple<FieldInfo, MethodInfo> initializationMembers,
        IProxySerializationEnabler proxySerializationEnabler)
    {
      ArgumentUtility.CheckNotNull ("constructor", constructor);
      ArgumentUtility.CheckNotNull ("proxySerializationEnabler", proxySerializationEnabler);

      if (initializationMembers == null || proxySerializationEnabler.IsDeserializationConstructor (constructor))
        return;

      // We cannot protect the decrement with try-finally because the this pointer must be initialized before entering a try block.
      // Using *IncrementAssign and *DecrementAssign results in un-verifiable code (emits stloc.0).
      constructor.SetBody (
          ctx =>
          {
            var initilizationMethod = initializationMembers.Item2;
            var counter = Expression.Field (ctx.This, initializationMembers.Item1);
            var one = Expression.Constant (1);
            var zero = Expression.Constant (0);

            return Expression.Block (
                Expression.Assign (counter, Expression.Add (counter, one)),
                constructor.Body,
                Expression.Assign (counter, Expression.Subtract (counter, one)),
                Expression.IfThen (
                    Expression.Equal (counter, zero),
                    Expression.Call (ctx.This, initilizationMethod, Expression.Constant (InitializationSemantics.Construction))));
          });
    }
  }
}