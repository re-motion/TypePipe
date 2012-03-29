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
  /// Maps mutable reflection objects to their associated <code>Reflection.Emit</code> builder objects, which can be used for code generation
  /// by <see cref="ILGeneratorDecorator"/>.
  /// </summary>
  public class MutableReflectionObjectMap
  {
    private readonly Dictionary<MemberInfo, MemberInfo> _mapping = new Dictionary<MemberInfo, MemberInfo>();

    public void AddMapping (MutableType mutableType, TypeBuilder typeBuilder)
    {
      ArgumentUtility.CheckNotNull ("mutableType", mutableType);
      ArgumentUtility.CheckNotNull ("typeBuilder", typeBuilder);

      AddMappingInternal(mutableType, typeBuilder);
    }

    public TypeBuilder GetBuilder (MutableType mutableType)
    {
      ArgumentUtility.CheckNotNull ("mutableType", mutableType);

      return (TypeBuilder) GetInternal(mutableType);
    }

    public void AddMapping (MutableConstructorInfo mutableConstructorInfo, ConstructorBuilder constructorBuilder)
    {
      ArgumentUtility.CheckNotNull ("mutableConstructorInfo", mutableConstructorInfo);
      ArgumentUtility.CheckNotNull ("constructorBuilder", constructorBuilder);

      AddMappingInternal (mutableConstructorInfo, constructorBuilder);
    }

    public ConstructorBuilder GetBuilder (MutableConstructorInfo mutableConstructorInfo)
    {
      ArgumentUtility.CheckNotNull ("mutableConstructorInfo", mutableConstructorInfo);

      return (ConstructorBuilder) GetInternal (mutableConstructorInfo);
    }

    private void AddMappingInternal (MemberInfo mutableReflectionObject, MemberInfo reflectionEmitBuilder)
    {
      _mapping.Add (mutableReflectionObject, reflectionEmitBuilder);
    }

    private MemberInfo GetInternal (MemberInfo mutableType)
    {
      return _mapping[mutableType];
    }


    //public T GetBuilder<T> (T mutableReflectionObject)
    //    where T : MemberInfo
    //{
    //  ArgumentUtility.CheckNotNull ("mutableReflectionObject", mutableReflectionObject);

    //  return (T) _mapping[mutableReflectionObject];
    //}
    
  }
}