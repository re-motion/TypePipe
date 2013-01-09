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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.StrongNaming
{
  /// <summary>
  /// TODO
  /// </summary>
  public class StrongNamingEmittableOperandProviderDecorator : IEmittableOperandProvider
  {
    private readonly IEmittableOperandProvider _emittableOperandProvider;

    public StrongNamingEmittableOperandProviderDecorator (IEmittableOperandProvider emittableOperandProvider)
    {
      ArgumentUtility.CheckNotNull ("emittableOperandProvider", emittableOperandProvider);

      _emittableOperandProvider = emittableOperandProvider;
    }

    public IEmittableOperandProvider InnerEmittableOperandProvider
    {
      get { return _emittableOperandProvider; }
    }

    public void AddMapping (MutableType mappedType, Type emittableType)
    {
      _emittableOperandProvider.AddMapping (mappedType, emittableType);
    }

    public void AddMapping (MutableFieldInfo mappedField, FieldInfo emittableField)
    {
      _emittableOperandProvider.AddMapping (mappedField, emittableField);
    }

    public void AddMapping (MutableConstructorInfo mappedConstructor, ConstructorInfo emittableConstructor)
    {
      _emittableOperandProvider.AddMapping (mappedConstructor, emittableConstructor);
    }

    public void AddMapping (MutableMethodInfo mappedMethod, MethodInfo emittableMethod)
    {
      _emittableOperandProvider.AddMapping (mappedMethod, emittableMethod);
    }

    public Type GetEmittableType (Type type)
    {
      return _emittableOperandProvider.GetEmittableType (type);
    }

    public FieldInfo GetEmittableField (FieldInfo field)
    {
      return _emittableOperandProvider.GetEmittableField (field);
    }

    public ConstructorInfo GetEmittableConstructor (ConstructorInfo constructor)
    {
      return _emittableOperandProvider.GetEmittableConstructor (constructor);
    }

    public MethodInfo GetEmittableMethod (MethodInfo method)
    {
      return _emittableOperandProvider.GetEmittableMethod (method);
    }

    public object GetEmittableOperand (object operand)
    {
      return _emittableOperandProvider.GetEmittableOperand (operand);
    }
  }
}