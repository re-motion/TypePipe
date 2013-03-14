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
using Microsoft.Scripting.Ast;
using Remotion.Collections;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Implements <see cref="IInitializationBuilder"/>.
  /// </summary>
  public class InitializationBuilder : IInitializationBuilder
  {
    public Tuple<FieldInfo, MethodInfo> CreateInitializationMembers (MutableType mutableType)
    {
      ArgumentUtility.CheckNotNull ("mutableType", mutableType);

      if (mutableType.Initializations.Count == 0)
        return null;

      mutableType.AddInterface (typeof (IInitializableObject));

      var counter = mutableType.AddField ("<tp>_ctorRunCounter", FieldAttributes.Private, typeof (int));
      var nonSerializedCtor = MemberInfoFromExpressionUtility.GetConstructor (() => new NonSerializedAttribute());
      counter.AddCustomAttribute (new CustomAttributeDeclaration (nonSerializedCtor, new object[0]));

      var interfaceMethod = MemberInfoFromExpressionUtility.GetMethod ((IInitializableObject obj) => obj.Initialize());
      var body = Expression.Block (interfaceMethod.ReturnType, mutableType.Initializations);
      var initializationMethod = mutableType.AddExplicitOverride (interfaceMethod, ctx => body);

      return Tuple.Create<FieldInfo, MethodInfo> (counter, initializationMethod);
    }

    public void WireConstructorWithInitialization (
        MutableConstructorInfo constructor, Tuple<FieldInfo, MethodInfo> initializationMembers, IProxySerializationEnabler proxySerializationEnabler)
    {
      ArgumentUtility.CheckNotNull ("constructor", constructor);
      // initializationMembers may be null
      ArgumentUtility.CheckNotNull ("proxySerializationEnabler", proxySerializationEnabler);

      if (initializationMembers == null || proxySerializationEnabler.IsDeserializationConstructor (constructor))
        return;

      // We cannot protect the decrement with try-finally because the this pointer must be initialized before entering a try block.
      // Using *IncrementAssign and *DecrementAssign results in un-verifiable code (emits stloc.0).
      constructor.SetBody (
          ctx =>
          {
            var counter = Expression.Field (ctx.This, initializationMembers.Item1);
            var one = Expression.Constant (1);

            return Expression.Block (
                Expression.Assign (counter, Expression.Add (counter, one)),
                constructor.Body,
                Expression.Assign (counter, Expression.Subtract (counter, one)),
                Expression.IfThen (
                    Expression.Equal (counter, Expression.Constant (0)),
                    Expression.Call (ctx.This, initializationMembers.Item2)));
          });
    }
  }
}