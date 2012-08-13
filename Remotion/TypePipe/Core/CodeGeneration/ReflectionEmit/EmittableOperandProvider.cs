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
    private readonly Dictionary<MutableType, Type> _mappedTypes = new Dictionary<MutableType, Type> ();
    private readonly Dictionary<MutableFieldInfo, FieldInfo> _mappedFieldInfos = new Dictionary<MutableFieldInfo, FieldInfo> ();
    private readonly Dictionary<MutableConstructorInfo, ConstructorInfo> _mappedConstructorInfos = new Dictionary<MutableConstructorInfo, ConstructorInfo> ();
    private readonly Dictionary<MutableMethodInfo, MethodInfo> _mappedMethodInfos = new Dictionary<MutableMethodInfo, MethodInfo> ();

    public void AddMapping (MutableType mappedType, Type emittableType)
    {
      ArgumentUtility.CheckNotNull ("mappedType", mappedType);
      ArgumentUtility.CheckNotNull ("emittableType", emittableType);

      AddMapping (_mappedTypes, mappedType, emittableType);
    }

    public void AddMapping (MutableFieldInfo mappedField, FieldInfo emittableField)
    {
      ArgumentUtility.CheckNotNull ("mappedField", mappedField);
      ArgumentUtility.CheckNotNull ("emittableField", emittableField);

      AddMapping (_mappedFieldInfos, mappedField, emittableField);
    }

    public void AddMapping (MutableConstructorInfo mappedConstructor, ConstructorInfo emittableConstructor)
    {
      ArgumentUtility.CheckNotNull ("mappedConstructor", mappedConstructor);
      ArgumentUtility.CheckNotNull ("emittableConstructor", emittableConstructor);

      AddMapping (_mappedConstructorInfos, mappedConstructor, emittableConstructor);
    }

    public void AddMapping (MutableMethodInfo mappedMethod, MethodInfo emittableMethod)
    {
      ArgumentUtility.CheckNotNull ("mappedMethod", mappedMethod);
      ArgumentUtility.CheckNotNull ("emittableMethod", emittableMethod);

      AddMapping (_mappedMethodInfos, mappedMethod, emittableMethod);
    }

    public Type GetEmittableType (Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);

      return GetEmittableOperand (_mappedTypes, type);
    }

    public FieldInfo GetEmittableField (FieldInfo fieldInfo)
    {
      ArgumentUtility.CheckNotNull ("fieldInfo", fieldInfo);

      return GetEmittableOperand (_mappedFieldInfos, fieldInfo);
    }

    public ConstructorInfo GetEmittableConstructor (ConstructorInfo constructorInfo)
    {
      ArgumentUtility.CheckNotNull ("constructorInfo", constructorInfo);

      return GetEmittableOperand (_mappedConstructorInfos, constructorInfo);
    }

    public MethodInfo GetEmittableMethod (MethodInfo methodInfo)
    {
      ArgumentUtility.CheckNotNull ("methodInfo", methodInfo);

      return GetEmittableOperand (_mappedMethodInfos, methodInfo);
    }

    public object GetEmittableOperand (object operand)
    {
      ArgumentUtility.CheckNotNull ("operand", operand);

      if (operand is MutableType)
        return GetEmittableType ((MutableType) operand);
      if (operand is MutableFieldInfo)
        return GetEmittableField ((MutableFieldInfo) operand);
      if (operand is MutableConstructorInfo)
        return GetEmittableConstructor ((MutableConstructorInfo) operand);
      if (operand is MutableMethodInfo)
        return GetEmittableMethod ((MutableMethodInfo) operand);

      return operand;
    }

    private void AddMapping<TMapped, TEmittable> (Dictionary<TMapped, TEmittable> mapping, TMapped key, TEmittable value)
    {
      if (mapping.ContainsKey (key))
      {
        var itemTypeName = typeof (TEmittable).Name;
        var message = itemTypeName + " is already mapped.";
        var parameterName = "mapped" + itemTypeName.Replace ("Info", "");

        throw new ArgumentException (message, parameterName);
      }

      mapping.Add (key, value);
    }

    private TBase GetEmittableOperand<TMutable, TBase> (Dictionary<TMutable, TBase> mapping, TBase operandToBeEmitted)
        where TMutable: TBase
    {
      if (!(operandToBeEmitted is TMutable))
        return operandToBeEmitted;

      try
      {
        return mapping[(TMutable) operandToBeEmitted];
      }
      catch (KeyNotFoundException exception)
      {
        var message = string.Format ("No emittable operand found for '{0}' of type '{1}'.", operandToBeEmitted, operandToBeEmitted.GetType().Name);
        throw new InvalidOperationException (message, exception);
      }
    }
  }
}