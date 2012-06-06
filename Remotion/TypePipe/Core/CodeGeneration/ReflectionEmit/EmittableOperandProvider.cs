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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;
using Remotion.Collections;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Maps reflection objects to associated emittable operands, which can be used for code generation by <see cref="ILGeneratorDecorator"/>.
  /// </summary>
  /// <remarks>
  /// This class is mainly used to map instances of <see cref="MutableType"/>, <see cref="MutableConstructorInfo"/>, etc. to the respective
  /// <see cref="TypeBuilder"/>, <see cref="ConstructorBuilder"/>, etc. objects. That way, <see cref="ILGeneratorDecorator"/> can resolve
  /// references to the mutable Reflection objects when it emits code.
  /// </remarks>
  public class EmittableOperandProvider : IEmittableOperandProvider
  {
    private readonly Dictionary<Type, Type> _mappedTypes = new Dictionary<Type, Type> ();
    private readonly Dictionary<FieldInfo, FieldInfo> _mappedFieldInfos = new Dictionary<FieldInfo, FieldInfo> ();
    private readonly Dictionary<ConstructorInfo, ConstructorInfo> _mappedConstructorInfos = new Dictionary<ConstructorInfo, ConstructorInfo> ();
    private readonly Dictionary<MethodInfo, MethodInfo> _mappedMethodInfos = new Dictionary<MethodInfo, MethodInfo> ();

    public void AddMapping (Type mappedType, Type emittableType)
    {
      ArgumentUtility.CheckNotNull ("mappedType", mappedType);
      ArgumentUtility.CheckNotNull ("emittableType", emittableType);

      AddMapping (_mappedTypes, mappedType, emittableType);
    }

    public void AddMapping (FieldInfo mappedField, FieldInfo emittableField)
    {
      ArgumentUtility.CheckNotNull ("mappedField", mappedField);
      ArgumentUtility.CheckNotNull ("emittableField", emittableField);

      AddMapping (_mappedFieldInfos, mappedField, emittableField);
    }

    public void AddMapping (ConstructorInfo mappedConstructor, ConstructorInfo emittableConstructor)
    {
      ArgumentUtility.CheckNotNull ("mappedConstructor", mappedConstructor);
      ArgumentUtility.CheckNotNull ("emittableConstructor", emittableConstructor);

      AddMapping (_mappedConstructorInfos, mappedConstructor, emittableConstructor);
    }

    public void AddMapping (MethodInfo mappedMethod, MethodInfo emittableMethod)
    {
      ArgumentUtility.CheckNotNull ("mappedMethod", mappedMethod);
      ArgumentUtility.CheckNotNull ("emittableMethod", emittableMethod);

      AddMapping (_mappedMethodInfos, mappedMethod, emittableMethod);
    }

    public Type GetEmittableType (Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);

      return GetEmittableOperand (_mappedTypes, type, typeof (MutableType));
    }

    public FieldInfo GetEmittableField (FieldInfo fieldInfo)
    {
      ArgumentUtility.CheckNotNull ("fieldInfo", fieldInfo);

      return GetEmittableOperand (_mappedFieldInfos, fieldInfo, typeof (FieldInfo));
    }

    public ConstructorInfo GetEmittableConstructor (ConstructorInfo constructorInfo)
    {
      ArgumentUtility.CheckNotNull ("constructorInfo", constructorInfo);

      return GetEmittableOperand (_mappedConstructorInfos, constructorInfo, typeof (ConstructorInfo));
    }

    public MethodInfo GetEmittableMethod (MethodInfo methodInfo)
    {
      ArgumentUtility.CheckNotNull ("methodInfo", methodInfo);

      return GetEmittableOperand (_mappedMethodInfos, methodInfo, typeof (MutableMethodInfo));
    }

    public object GetEmittableOperand (object operand)
    {
      ArgumentUtility.CheckNotNull ("operand", operand);

      if (operand is Type)
        return GetEmittableType ((Type) operand);
      if (operand is FieldInfo)
        return GetEmittableField ((FieldInfo) operand);
      if (operand is ConstructorInfo)
        return GetEmittableConstructor ((ConstructorInfo) operand);
      if (operand is MethodInfo)
        return GetEmittableMethod ((MethodInfo) operand);

      return operand;
    }

    private void AddMapping<T> (Dictionary<T, T> mapping, T key, T value)
    {
      string itemNameType = typeof (T).Name;
      if (mapping.ContainsKey (key))
      {
        var message = string.Format ("{0} is already mapped.", itemNameType);
        throw new ArgumentException (message, "mapped" + itemNameType);
      }

      mapping.Add (key, value);
    }

    private T GetEmittableOperand<T> (Dictionary<T, T> mapping, T mappedItem, Type mutableMemberType)
        where T: class
    {
      var operand = mapping.GetValueOrDefault (mappedItem);
      if (operand != null)
        return operand;

      Assertion.IsTrue (mappedItem.GetType() != mutableMemberType, "Wrapped object must not be of type {0}.", mutableMemberType);
      return mappedItem;
    }
  }
}