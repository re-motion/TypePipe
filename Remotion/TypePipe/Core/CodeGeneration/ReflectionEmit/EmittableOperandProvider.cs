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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;
using Remotion.Collections;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Maps reflection objects to their associated builder objects, which can be used for code generation by <see cref="ILGeneratorDecorator"/>.
  /// </summary>
  /// <remarks>
  /// This class is mainly used to map instances of <see cref="MutableType"/>, <see cref="MutableConstructorInfo"/>, etc. to the respective
  /// <see cref="ITypeBuilder"/>, <see cref="IConstructorBuilder"/>, etc. objects. That way, <see cref="ILGeneratorDecorator"/> can resolve
  /// references to the mutable Reflection objects when it emits code.
  /// </remarks>
  public class EmittableOperandProvider
  {
    private readonly Dictionary<Type, IEmittableOperand> _mappedTypes = new Dictionary<Type, IEmittableOperand> ();
    private readonly Dictionary<FieldInfo, IEmittableOperand> _mappedFieldInfos = new Dictionary<FieldInfo, IEmittableOperand> ();
    private readonly Dictionary<ConstructorInfo, IEmittableOperand> _mappedConstructorInfos = new Dictionary<ConstructorInfo, IEmittableOperand> ();
    private readonly Dictionary<MethodInfo, IEmittableMethodOperand> _mappedMethodInfos = new Dictionary<MethodInfo, IEmittableMethodOperand> ();

    [CLSCompliant (false)]
    public void AddMapping (Type mappedType, IEmittableOperand typeOperand)
    {
      ArgumentUtility.CheckNotNull ("mappedType", mappedType);
      ArgumentUtility.CheckNotNull ("typeOperand", typeOperand);

      AddMapping (_mappedTypes, mappedType, typeOperand);
    }

    [CLSCompliant (false)]
    public void AddMapping (FieldInfo mappedFieldInfo, IEmittableOperand fieldOperand)
    {
      ArgumentUtility.CheckNotNull ("mappedFieldInfo", mappedFieldInfo);
      ArgumentUtility.CheckNotNull ("fieldOperand", fieldOperand);

      AddMapping (_mappedFieldInfos, mappedFieldInfo, fieldOperand);
    }

    [CLSCompliant (false)]
    public void AddMapping (ConstructorInfo mappedConstructorInfo, IEmittableOperand constructorOperand)
    {
      ArgumentUtility.CheckNotNull ("mappedConstructorInfo", mappedConstructorInfo);
      ArgumentUtility.CheckNotNull ("constructorOperand", constructorOperand);

      AddMapping (_mappedConstructorInfos, mappedConstructorInfo, constructorOperand);
    }

    [CLSCompliant (false)]
    public void AddMapping (MethodInfo mappedMethodInfo, IEmittableMethodOperand methodOperand)
    {
      ArgumentUtility.CheckNotNull ("mappedMethodInfo", mappedMethodInfo);
      ArgumentUtility.CheckNotNull ("methodOperand", methodOperand);

      AddMapping (_mappedMethodInfos, mappedMethodInfo, methodOperand);
    }

    [CLSCompliant (false)]
    public IEmittableOperand GetEmittableOperand (Type mappedType)
    {
      ArgumentUtility.CheckNotNull ("mappedType", mappedType);

      return GetEmittableOperand (_mappedTypes, mappedType, typeof (MutableType), t => new EmittableType (mappedType));
    }

    [CLSCompliant (false)]
    public IEmittableOperand GetEmittableOperand (FieldInfo mappedFieldInfo)
    {
      ArgumentUtility.CheckNotNull ("mappedFieldInfo", mappedFieldInfo);

      return GetEmittableOperand (_mappedFieldInfos, mappedFieldInfo, typeof (FieldInfo), fi => new EmittableField (mappedFieldInfo));
    }

    [CLSCompliant (false)]
    public IEmittableOperand GetEmittableOperand (ConstructorInfo mappedConstructorInfo)
    {
      ArgumentUtility.CheckNotNull ("mappedConstructorInfo", mappedConstructorInfo);

      return GetEmittableOperand (
          _mappedConstructorInfos, mappedConstructorInfo, typeof (ConstructorInfo), ci => new EmittableConstructor (mappedConstructorInfo));
    }

    [CLSCompliant (false)]
    public IEmittableMethodOperand GetEmittableOperand (MethodInfo mappedMethodInfo)
    {
      ArgumentUtility.CheckNotNull ("mappedMethodInfo", mappedMethodInfo);

      return GetEmittableOperand (_mappedMethodInfos, mappedMethodInfo, typeof (MutableMethodInfo), mi => new EmittableMethod (mappedMethodInfo));
    }

    private void AddMapping<TKey, TValue> (Dictionary<TKey, TValue> mapping, TKey mappedItem, TValue builder)
    {
      string itemNameType = typeof (TKey).Name;
      if (mapping.ContainsKey (mappedItem))
      {
        var message = string.Format ("{0} is already mapped.", itemNameType);
        throw new ArgumentException (message, "mapped" + itemNameType);
      }

      mapping.Add (mappedItem, builder);
    }

    private TValue GetEmittableOperand<TKey, TValue> (
        Dictionary<TKey, TValue> mapping, TKey mappedItem, Type mutableMemberType, Func<TKey, TValue> wrapperCreator)
        where TValue: class
    {
      var operand = mapping.GetValueOrDefault (mappedItem);
      if (operand != null)
        return operand;

      Assertion.IsTrue (mappedItem.GetType() != mutableMemberType, "Wrapped object must not be of type {0}.", mutableMemberType);
      return wrapperCreator (mappedItem);
    }
  }
}