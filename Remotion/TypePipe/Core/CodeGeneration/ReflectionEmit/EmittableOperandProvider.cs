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
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Maps mutable reflection objects to associated emittable operands, which can be used for code generation by <see cref="ILGeneratorDecorator"/>.
  /// </summary>
  /// <remarks>
  /// This class is used to map instances of <see cref="MutableType"/>, <see cref="MutableConstructorInfo"/>, etc. to the respective
  /// <see cref="TypeBuilder"/>, <see cref="ConstructorBuilder"/>, etc. objects. That way, <see cref="ILGeneratorDecorator"/> can resolve
  /// references to the mutable reflection objects when it emits code.
  /// </remarks>
  public class EmittableOperandProvider : IEmittableOperandProvider
  {
    private readonly Dictionary<Type, Type> _mappedTypes = new Dictionary<Type, Type>();
    private readonly Dictionary<FieldInfo, FieldInfo> _mappedFields = new Dictionary<FieldInfo, FieldInfo>();
    private readonly Dictionary<ConstructorInfo, ConstructorInfo> _mappedConstructors = new Dictionary<ConstructorInfo, ConstructorInfo>();
    private readonly Dictionary<MethodInfo, MethodInfo> _mappedMethods = new Dictionary<MethodInfo, MethodInfo>();

    public void AddMapping (MutableType mappedType, Type emittableType)
    {
      ArgumentUtility.CheckNotNull ("mappedType", mappedType);
      ArgumentUtility.CheckNotNull ("emittableType", emittableType);

      AddMapping (_mappedTypes, mappedType, emittableType);
    }

    public void AddMapping (MutableGenericParameter mappedGenericParameter, Type emittableGenericParameter)
    {
      ArgumentUtility.CheckNotNull ("mappedGenericParameter", mappedGenericParameter);
      ArgumentUtility.CheckNotNull ("emittableGenericParameter", emittableGenericParameter);

      AddMapping (_mappedTypes, mappedGenericParameter, emittableGenericParameter);
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

      Func<Type, Type> emittableInstantiationProvider = t =>
      {
        var typeInstantiation = t as TypeInstantiation;
        if (typeInstantiation != null)
          return GetEmittableTypeInstantiation (typeInstantiation);

        var emittableElementType = GetEmittableType (t.GetElementType());
        if (t is ByRefType)
          return emittableElementType.MakeByRefType();
        if (t is VectorType)
          return emittableElementType.MakeArrayType();
        else // (t is MultiDimensionalArrayType)
          return emittableElementType.MakeArrayType (t.GetArrayRank());
      };

      return GetEmittableOperand (_mappedTypes, type, IsEmittable, emittableInstantiationProvider);
    }

    public FieldInfo GetEmittableField (FieldInfo field)
    {
      ArgumentUtility.CheckNotNull ("field", field);

      return GetEmittableOperand (
          _mappedFields,
          field,
          IsEmittable,
          f => GetEmittableMemberInstantiation (f, fi => ((FieldOnTypeInstantiation) fi).FieldOnGenericType, TypeBuilder.GetField));
    }

    public ConstructorInfo GetEmittableConstructor (ConstructorInfo constructor)
    {
      ArgumentUtility.CheckNotNull ("constructor", constructor);

      return GetEmittableOperand (
          _mappedConstructors,
          constructor,
          IsEmittable,
          c => GetEmittableMemberInstantiation (c, ci => ((ConstructorOnTypeInstantiation) ci).ConstructorOnGenericType, TypeBuilder.GetConstructor));
    }

    public MethodInfo GetEmittableMethod (MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);

      Func<MethodInfo, MethodInfo> emittableInstantiationProvider = m =>
      {
        var methodInstantiation = m as MethodInstantiation;
        if (methodInstantiation != null)
          return GetEmittableMethodInstantiation (methodInstantiation);
        else
          return GetEmittableMemberInstantiation (m, mi => ((MethodOnTypeInstantiation) mi).MethodOnGenericType, TypeBuilder.GetMethod);
      };

      return GetEmittableOperand (_mappedMethods, method, IsEmittable, emittableInstantiationProvider);
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

    private bool IsEmittable (Type type)
    {
      return !(type is CustomType);
    }

    private bool IsEmittable (MemberInfo member)
    {
      Debug.Assert (member is FieldInfo || member is ConstructorInfo || member is MethodInfo);

      return !(member is CustomFieldInfo) && !(member is CustomConstructorInfo) && !(member is CustomMethodInfo);
    }

    private T GetEmittableOperand<T> (Dictionary<T, T> mapping, T operand, Predicate<T> isAlreadyEmittable, Func<T, T> emittableInstantiationProvider)
    {
      if (isAlreadyEmittable (operand))
        return operand;

      if (operand is IMutableInfo)
        return mapping[operand];
      else
        return emittableInstantiationProvider (operand);
    }

    private Type GetEmittableTypeInstantiation (TypeInstantiation typeInstantiation)
    {
      var genericTypeDefinition = typeInstantiation.GetGenericTypeDefinition();
      var emittableTypeArguments = typeInstantiation.GetGenericArguments().Select (GetEmittableType).ToArray();
      Assertion.IsNotNull (genericTypeDefinition);

      // Should *not* be MakeTypePipeGenericType.
      return genericTypeDefinition.MakeGenericType (emittableTypeArguments);
    }

    private MethodInfo GetEmittableMethodInstantiation (MethodInstantiation methodInstantiation)
    {
      var emittableGenericMethodDefinition = GetEmittableMethod (methodInstantiation.GetGenericMethodDefinition ());
      var emittableTypeArguments = methodInstantiation.GetGenericArguments().Select (GetEmittableType).ToArray();

      // Should *not* be MakeTypePipeGenericMethod.
      return emittableGenericMethodDefinition.MakeGenericMethod (emittableTypeArguments);
    }

    private T GetEmittableMemberInstantiation<T, TInstantiation> (
        TInstantiation memberInstantiation, Func<TInstantiation, T> genericMemberAccessor, Func<Type, T, T> emittableMemberProvider)
        where T : MemberInfo
        where TInstantiation : T
    {
      var emittableDeclaringType = GetEmittableTypeInstantiation ((TypeInstantiation) memberInstantiation.DeclaringType);
      var genericMember = genericMemberAccessor (memberInstantiation);

      return emittableMemberProvider (emittableDeclaringType, genericMember);
    }
  }
}