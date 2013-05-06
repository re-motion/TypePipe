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
using Remotion.TypePipe.CodeGeneration.Implementation.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.TypePipe.CodeGeneration.Implementation.ReflectionEmit
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

    private readonly IDelegateProvider _delegateProvider;

    public EmittableOperandProvider (IDelegateProvider delegateProvider)
    {
      ArgumentUtility.CheckNotNull ("delegateProvider", delegateProvider);

      _delegateProvider = delegateProvider;
    }

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

      var emittable = GetDirectlyEmittableOperand (_mappedTypes, type);
      if (emittable != null)
        return emittable;

      var typeInstantiation = type as TypeInstantiation;
      if (typeInstantiation != null)
        return GetEmittableTypeInstantiation (typeInstantiation);

      var delegateTypePlaceholder = type as DelegateTypePlaceholder;
      if (delegateTypePlaceholder != null)
        return _delegateProvider.GetDelegateType (delegateTypePlaceholder.ReturnType, delegateTypePlaceholder.ParameterTypes);

      Assertion.IsTrue (type is ByRefType || type is VectorType || type is MultiDimensionalArrayType);

      var emittableElementType = GetEmittableType (type.GetElementType());
      if (type is ByRefType)
        return emittableElementType.MakeByRefType();
      if (type is VectorType)
        return emittableElementType.MakeArrayType();
      else // MultiDimensionalArrayType
        return emittableElementType.MakeArrayType (type.GetArrayRank());
    }

    public FieldInfo GetEmittableField (FieldInfo field)
    {
      ArgumentUtility.CheckNotNull ("field", field);

      var emittable = GetDirectlyEmittableOperand (_mappedFields, field);
      if (emittable != null)
        return emittable;

      return GetEmittableMemberInstantiation ((FieldOnTypeInstantiation) field, fi => fi.FieldOnGenericType, TypeBuilder.GetField);
    }

    public ConstructorInfo GetEmittableConstructor (ConstructorInfo constructor)
    {
      ArgumentUtility.CheckNotNull ("constructor", constructor);

      var emittable = GetDirectlyEmittableOperand (_mappedConstructors, constructor);
      if (emittable != null)
        return emittable;

      return GetEmittableMemberInstantiation ((ConstructorOnTypeInstantiation) constructor, ci => ci.ConstructorOnGenericType, TypeBuilder.GetConstructor);
    }

    public MethodInfo GetEmittableMethod (MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);

      var emittable = GetDirectlyEmittableOperand (_mappedMethods, method);
      if (emittable != null)
        return emittable;

      var methodInstantiation = method as MethodInstantiation;
      if (methodInstantiation != null)
        return GetEmittableMethodInstantiation (methodInstantiation);
      else
        return GetEmittableMemberInstantiation ((MethodOnTypeInstantiation) method, mi => mi.MethodOnGenericType, TypeBuilder.GetMethod);
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

    private T GetDirectlyEmittableOperand<T> (Dictionary<T, T> mapping, T operand)
        where T : MemberInfo
    {
      if (IsEmittable (operand))
        return operand;

      if (operand is IMutableInfo)
        return mapping[operand];

      return null;
    }

    private bool IsEmittable (MemberInfo member)
    {
      Debug.Assert (member is Type || member is FieldInfo || member is ConstructorInfo || member is MethodInfo);

      return !(member is CustomType) && !(member is CustomFieldInfo) && !(member is CustomConstructorInfo) && !(member is CustomMethodInfo);
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
      var emittableGenericMethodDefinition = GetEmittableMethod (methodInstantiation.GetGenericMethodDefinition());
      var emittableTypeArguments = methodInstantiation.GetGenericArguments().Select (GetEmittableType).ToArray();

      // Should *not* be MakeTypePipeGenericMethod.
      return emittableGenericMethodDefinition.MakeGenericMethod (emittableTypeArguments);
    }

    private T GetEmittableMemberInstantiation<T, TInstantiation> (
        TInstantiation memberInstantiation, Func<TInstantiation, T> genericMemberProvider, Func<Type, T, T> emittableMemberInstantiationProvider)
        where T : MemberInfo
        where TInstantiation : T
    {
      var emittableDeclaringType = GetEmittableTypeInstantiation ((TypeInstantiation) memberInstantiation.DeclaringType);
      var genericMember = genericMemberProvider (memberInstantiation);

      return emittableMemberInstantiationProvider (emittableDeclaringType, genericMember);
    }
  }
}