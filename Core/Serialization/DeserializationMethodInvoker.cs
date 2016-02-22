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
using System.Runtime.Serialization;
using Remotion.Utilities;

namespace Remotion.TypePipe.Serialization
{
  /// <summary>
  /// Delegates to an instance of <see cref="SerializationEventRaiser"/>.
  /// </summary>
  public class DeserializationMethodInvoker : IDeserializationMethodInvoker
  {
    private static readonly SerializationEventRaiser s_serializationEventRaiser = new SerializationEventRaiser();

    public void InvokeOnDeserializing (object instance, StreamingContext context)
    {
      ArgumentUtility.CheckNotNull ("instance", instance);

      s_serializationEventRaiser.InvokeAttributedMethod(instance, typeof(OnDeserializingAttribute), context);
    }

    public void InvokeOnDeserialized (object instance, StreamingContext context)
    {
      ArgumentUtility.CheckNotNull ("instance", instance);

      s_serializationEventRaiser.InvokeAttributedMethod(instance, typeof(OnDeserializedAttribute), context);
    }

    public void InvokeOnDeserialization (object instance, object sender)
    {
      ArgumentUtility.CheckNotNull ("instance", instance);
      // Sender may be null.

      s_serializationEventRaiser.RaiseDeserializationEvent (instance, sender);
    }
  }
}