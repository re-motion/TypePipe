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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Remotion.Utilities;

namespace Remotion.TypePipe.Serialization
{
  /// <summary>
  /// Provides helper methods for finding and invoking the serialization APIs identified via the <see cref="OnDeserializingAttribute"/> 
  /// or <see cref="OnDeserializedAttribute"/> and the <see cref="IDeserializationCallback"/> interface.
  /// </summary>
  public class SerializationEventRaiser
  {
    private readonly ConcurrentDictionary<Tuple<Type, Type>, IReadOnlyCollection<MethodInfo>> _attributedMethodCache =
        new ConcurrentDictionary<Tuple<Type, Type>, IReadOnlyCollection<MethodInfo>>();

    public SerializationEventRaiser ()
    {
    }

    public virtual void InvokeAttributedMethod (object deserializedObject, Type attributeType, StreamingContext context)
    {
      ArgumentUtility.CheckNotNull ("deserializedObject", deserializedObject);
      ArgumentUtility.CheckNotNull ("attributeType", attributeType);

      foreach (MethodInfo method in FindDeserializationMethodsWithCache (deserializedObject.GetType (), attributeType))
        method.Invoke (deserializedObject, new object[] { context });
    }

    protected virtual IReadOnlyCollection<MethodInfo> FindDeserializationMethodsWithCache (Type type, Type attributeType)
    {
      return _attributedMethodCache.GetOrAdd (Tuple.Create (type, attributeType),
          typeAndAttributeType => FindDeserializationMethodsNoCache (typeAndAttributeType.Item1, typeAndAttributeType.Item2).ToList());
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

      var objectAsDeserializationCallback = deserializedObject as IDeserializationCallback;
      if (objectAsDeserializationCallback != null)
        objectAsDeserializationCallback.OnDeserialization (sender);
    }
  }
}
