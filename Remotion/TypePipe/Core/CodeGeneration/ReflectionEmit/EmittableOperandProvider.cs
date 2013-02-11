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
using System.Reflection;
using System.Reflection.Emit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Maps mutable reflection objects to associated emittable operands, which can be used for code generation by <see cref="ILGeneratorDecorator"/>.
  /// </summary>
  /// <remarks>
  /// This class is used to map instances of <see cref="ProxyType"/>, <see cref="MutableConstructorInfo"/>, etc. to the respective
  /// <see cref="TypeBuilder"/>, <see cref="ConstructorBuilder"/>, etc. objects. That way, <see cref="ILGeneratorDecorator"/> can resolve
  /// references to the mutable reflection objects when it emits code.
  /// </remarks>
  public class EmittableOperandProvider : IEmittableOperandProvider
  {
    private readonly Dictionary<ProxyType, Type> _mappedTypes = new Dictionary<ProxyType, Type>();
    private readonly Dictionary<MutableFieldInfo, FieldInfo> _mappedFields = new Dictionary<MutableFieldInfo, FieldInfo> ();
    private readonly Dictionary<MutableConstructorInfo, ConstructorInfo> _mappedConstructors = new Dictionary<MutableConstructorInfo, ConstructorInfo> ();
    private readonly Dictionary<MutableMethodInfo, MethodInfo> _mappedMethods = new Dictionary<MutableMethodInfo, MethodInfo> ();

    public void AddMapping (ProxyType mappedType, Type emittableType)
    {
      ArgumentUtility.CheckNotNull ("mappedType", mappedType);
      ArgumentUtility.CheckNotNull ("emittableType", emittableType);

      AddMapping (_mappedTypes, mappedType, emittableType);
    }

    public void AddMapping (MutableFieldInfo mappedField, FieldInfo emittableField)
    {
      ArgumentUtility.CheckNotNull ("mappedField", mappedField);
      ArgumentUtility.CheckNotNull ("emittableField", emittableField);

      AddMapping (_mappedFields, mappedField, emittableField);
    }

    public void AddMapping (MutableConstructorInfo mappedConstructor, ConstructorInfo emittableConstructor)
    {
      ArgumentUtility.CheckNotNull ("mappedConstructor", mappedConstructor);
      ArgumentUtility.CheckNotNull ("emittableConstructor", emittableConstructor);

      AddMapping (_mappedConstructors, mappedConstructor, emittableConstructor);
    }

    public void AddMapping (MutableMethodInfo mappedMethod, MethodInfo emittableMethod)
    {
      ArgumentUtility.CheckNotNull ("mappedMethod", mappedMethod);
      ArgumentUtility.CheckNotNull ("emittableMethod", emittableMethod);

      AddMapping (_mappedMethods, mappedMethod, emittableMethod);
    }

    public Type GetEmittableType (Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);

      return GetEmittableOperand (_mappedTypes, type, t => t is TypeBuilder || t.IsRuntimeType(), GetEmittableGenericType);
    }

    public FieldInfo GetEmittableField (FieldInfo field)
    {
      ArgumentUtility.CheckNotNull ("field", field);

      return GetEmittableOperand (
          _mappedFields,
          field,
          f => f is FieldBuilder || f.DeclaringType.IsRuntimeType(),
          f => GetEmittableMemberOfGenericType (f, (FieldOnTypeInstantiation fi) => fi.GenericField, TypeBuilder.GetField));
    }

    public ConstructorInfo GetEmittableConstructor (ConstructorInfo constructor)
    {
      ArgumentUtility.CheckNotNull ("constructor", constructor);

      return GetEmittableOperand (
          _mappedConstructors,
          constructor,
          c => c is ConstructorBuilder || c.DeclaringType.IsRuntimeType(),
          c => GetEmittableMemberOfGenericType (c, (ConstructorOnTypeInstantiation ci) => ci.GenericConstructor, TypeBuilder.GetConstructor));
    }

    public MethodInfo GetEmittableMethod (MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);

      return GetEmittableOperand (
          _mappedMethods,
          method,
          m => m is MethodBuilder || m.DeclaringType.IsRuntimeType(),
          m => GetEmittableMemberOfGenericType (m, (MethodOnTypeInstantiation mi) => mi.GenericMethod, TypeBuilder.GetMethod));
    }

    private static void AddMapping<TMutable, T> (Dictionary<TMutable, T> mapping, TMutable key, T value)
        where TMutable : T
        where T : MemberInfo
    {
      if (mapping.ContainsKey (key))
      {
        var message = string.Format ("{0} '{1}' is already mapped.", typeof (TMutable).Name, key.Name);
        var parameterName = "mapped" + typeof (T).Name.Replace ("Info", "");

        throw new ArgumentException (message, parameterName);
      }

      mapping.Add (key, value);
    }

    private static T GetEmittableOperand<T, TMutable, TBuilder> (
        Dictionary<TMutable, TBuilder> mapping, T operand, Predicate<T> isAlreadyEmittable, Func<T, T> genericEmittableOperandProvider)
        where TMutable : T
        where TBuilder : T
        where T : MemberInfo
    {
      if (isAlreadyEmittable (operand))
        return operand;

      var mutableOperand = operand as TMutable;
      if (mutableOperand == null)
        return genericEmittableOperandProvider (operand);

      TBuilder emittableOperand;
      if (mapping.TryGetValue (mutableOperand, out emittableOperand))
        return emittableOperand;

      var message = string.Format ("No emittable operand found for '{0}' of type '{1}'.", operand, operand.GetType().Name);
      throw new InvalidOperationException (message);
    }

    private Type GetEmittableGenericType (Type constructedType)
    {
      Assertion.IsTrue (constructedType.IsGenericType);

      var typeArguments = constructedType.GetGenericArguments().Select (GetEmittableType).ToArray();
      return constructedType.GetGenericTypeDefinition().MakeGenericType (typeArguments);
    }

    private T GetEmittableMemberOfGenericType<T, TMemberInstantiation> (
        T member, 
        Func<TMemberInstantiation, T> genericMemberAccessor, 
        Func<Type, T, T> typeBuilderMemberProvider)
        where TMemberInstantiation : T
        where T : MemberInfo
    {
      Assertion.IsTrue (member is TMemberInstantiation);

      var memberOnTypeInstantiation = (TMemberInstantiation) member;
      var emittableDeclaringType = GetEmittableGenericType (memberOnTypeInstantiation.DeclaringType);
      var genericMember = genericMemberAccessor (memberOnTypeInstantiation);

      return typeBuilderMemberProvider (emittableDeclaringType, genericMember);
    }
  }
}