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
using Remotion.ServiceLocation;
using Remotion.TypePipe.Caching;
using Remotion.Utilities;

namespace Remotion.TypePipe.Serialization
{
  /// <summary>
  /// A common base class for objects used as placeholders in the .NET deserialization process.
  /// </summary>
  /// <remarks>
  /// This class uses the metadata in the <see cref="SerializationInfo"/> that was added by the <see cref="ComplexSerializationEnabler"/> to 
  /// regenerate a suitable type for deserialization.
  /// </remarks>
  public abstract class ObjectDeserializationProxyBase : ISerializable, IObjectReference, IDeserializationCallback
  {
    private readonly IPipelineRegistry _registry = SafeServiceLocator.Current.GetInstance<IPipelineRegistry>();

    private readonly object _instance;

    protected ObjectDeserializationProxyBase (SerializationInfo serializationInfo, StreamingContext streamingContext)
    {
      ArgumentUtility.CheckNotNull ("serializationInfo", serializationInfo);

      _instance = CreateRealObject (serializationInfo, streamingContext);
    }

    public void GetObjectData (SerializationInfo info, StreamingContext context)
    {
      throw new NotSupportedException ("This method should not be called.");
    }

    public object GetRealObject (StreamingContext context)
    {
      return _instance;
    }

    public void OnDeserialization (object sender)
    {
      // TODO ????: OnDeserialized

      //SerializationImplementer.RaiseOnDeserialized (_instance, );
      //SerializationImplementer.RaiseOnDeserialization (_instance, sender);

      var deserializationCallback = _instance as IDeserializationCallback;
      if (deserializationCallback != null)
        deserializationCallback.OnDeserialization (sender);
    }

    private object CreateRealObject (SerializationInfo serializationInfo, StreamingContext streamingContext)
    {
      var participantConfigurationID = (string) serializationInfo.GetValue(ComplexSerializationEnabler.ParticipantConfigurationID, typeof(string));
      var assembledTypeIDData = (AssembledTypeIDData) serializationInfo.GetValue(ComplexSerializationEnabler.AssembledTypeIDData, typeof(AssembledTypeIDData));

      var pipeline = _registry.Get(participantConfigurationID);
      var typeID = assembledTypeIDData.CreateTypeID();

      return CreateRealObject (pipeline, typeID, serializationInfo, streamingContext);
    }

    protected abstract object CreateRealObject (IPipeline pipeline, AssembledTypeID typeID, SerializationInfo serializationInfo, StreamingContext streamingContext);
  }
}