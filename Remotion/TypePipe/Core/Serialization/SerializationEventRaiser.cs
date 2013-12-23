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
using System.Runtime.Serialization;
using Remotion.Collections;
using Remotion.Utilities;

namespace Remotion.TypePipe.Serialization
{
  public class SerializationEventRaiser
  {
    private readonly LockingCacheDecorator<Tuple<Type, Type>, List<MethodInfo>> _attributedMethodCache =
        CacheFactory.CreateWithLocking<Tuple<Type, Type>, List<MethodInfo>>();

    public virtual void InvokeAttributedMethod (object deserializedObject, Type attributeType, StreamingContext context)
    {
      ArgumentUtility.CheckNotNull ("deserializedObject", deserializedObject);
      ArgumentUtility.CheckNotNull ("attributeType", attributeType);

      foreach (MethodInfo method in FindDeserializationMethodsWithCache (deserializedObject.GetType (), attributeType))
        method.Invoke (deserializedObject, new object[] { context });
    }

    protected virtual List<MethodInfo> FindDeserializationMethodsWithCache (Type type, Type attributeType)
    {
      return _attributedMethodCache.GetOrCreateValue (Tuple.Create (type, attributeType), delegate (Tuple<Type, Type> typeAndAttributeType) {
          return new List<MethodInfo> (FindDeserializationMethodsNoCache (typeAndAttributeType.Item1, typeAndAttributeType.Item2)); });
    }

    protected virtual IEnumerable<MethodInfo> FindDeserializationMethodsNoCache (Type type, Type attributeType)
    {
      ArgumentUtility.CheckNotNull ("type", type);
      ArgumentUtility.CheckNotNull ("attributeType", attributeType);

      for (Type currentType = type; currentType != null; currentType = currentType.BaseType)
      {
        foreach (
            MethodInfo method in
                currentType.GetMethods (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
          if (method.IsDefined (attributeType, false))
            yield return method;
        }
      }
    }

    public virtual void RaiseDeserializationEvent (object deserializedObject, object sender)
    {
      ArgumentUtility.CheckNotNull ("deserializedObject", deserializedObject);

      IDeserializationCallback objectAsDeserializationCallback = deserializedObject as IDeserializationCallback;
      if (objectAsDeserializationCallback != null)
        objectAsDeserializationCallback.OnDeserialization (sender);
    }
  }
}
