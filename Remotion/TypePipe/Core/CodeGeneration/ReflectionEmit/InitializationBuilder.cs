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
    public MutableConstructorInfo CreateTypeInitializer (MutableType mutableType)
    {
      ArgumentUtility.CheckNotNull ("mutableType", mutableType);

      if (mutableType.TypeInitializations.Count == 0)
        return null;

      var attributes = MethodAttributes.Private | MethodAttributes.Static;
      var body = Expression.Block (typeof (void), mutableType.TypeInitializations);
      var descriptor = ConstructorDescriptor.Create (attributes, ParameterDescriptor.EmptyParameters, body);
      var typeInitializer = new MutableConstructorInfo (mutableType, descriptor);

      return typeInitializer;
    }

    public Tuple<FieldInfo, MethodInfo> CreateInstanceInitializationMembers (MutableType mutableType)
    {
      ArgumentUtility.CheckNotNull ("mutableType", mutableType);

      if (mutableType.InstanceInitializations.Count == 0)
        return null;

      mutableType.AddInterface (typeof (IInitializableObject));

      var counter = mutableType.AddField ("_<TypePipe-generated>_ctorRunCounter", typeof (int), FieldAttributes.Private);

      var interfaceMethod = MemberInfoFromExpressionUtility.GetMethod ((IInitializableObject obj) => obj.Initialize ());
      var name = MethodOverrideUtility.GetNameForExplicitOverride (interfaceMethod);
      var attributes = MethodOverrideUtility.GetAttributesForExplicitOverride (interfaceMethod).Unset (MethodAttributes.Abstract);
      var parameters = ParameterDeclaration.CreateForEquivalentSignature (interfaceMethod);
      var body = Expression.Block (interfaceMethod.ReturnType, mutableType.InstanceInitializations);
      var initMethod = mutableType.AddMethod (name, attributes, interfaceMethod.ReturnType, parameters, ctx => body);
      initMethod.AddExplicitBaseDefinition (interfaceMethod);

      return Tuple.Create<FieldInfo, MethodInfo> (counter, initMethod);
    }

    public void WireConstructorWithInitialization (MutableConstructorInfo constructor, Tuple<FieldInfo, MethodInfo> initializationMembers)
    {
      ArgumentUtility.CheckNotNull ("constructor", constructor);

      if (initializationMembers == null)
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