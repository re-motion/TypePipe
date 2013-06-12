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
using System.Diagnostics;
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
    private readonly IDeserializationMethodInvoker _deserializationMethodInvoker = new DeserializationMethodInvoker();

    private readonly SerializationInfo _serializationInfo;
    private readonly StreamingContext _streamingContext;

    private object _instance;

    // ReSharper disable UnusedParameter.Local
    protected ObjectDeserializationProxyBase (SerializationInfo serializationInfo, StreamingContext streamingContext)
    // ReSharper restore UnusedParameter.Local
    {
      ArgumentUtility.CheckNotNull ("serializationInfo", serializationInfo);

      _serializationInfo = serializationInfo;
      _streamingContext = streamingContext;
    }

    public void GetObjectData (SerializationInfo info, StreamingContext context)
    {
      throw new NotSupportedException ("This method should not be called.");
    }

    public object GetRealObject (StreamingContext context)
    {
      Debug.Assert (context.Equals (_streamingContext));

      // Do not move this code into the constructor (although it belongs there logically).
      // Reason: The deserialization constructor is called by .NET infrastructure via reflection. If we create the instance in the constructor,
      // we get an TargetInvocationException instead of our hand-crafted exceptions if something goes wrong.

      if (_instance != null)
        return _instance;

      var participantConfigurationID = (string) _serializationInfo.GetValue (ComplexSerializationEnabler.ParticipantConfigurationID, typeof (string));
      var assembledTypeIDData = (AssembledTypeIDData) _serializationInfo.GetValue (ComplexSerializationEnabler.AssembledTypeIDData, typeof (AssembledTypeIDData));

      var pipeline = _registry.Get (participantConfigurationID);
      var typeID = assembledTypeIDData.CreateTypeID();

      _instance = CreateRealObject (pipeline, typeID);

      return _instance;
    }

    public void OnDeserialization (object sender)
    {
      // sender may be null.

      _deserializationMethodInvoker.InvokeOnDeserialized (_instance, _streamingContext);
      _deserializationMethodInvoker.InvokeOnDeserialization (_instance, sender);
    }

    private object CreateRealObject (IPipeline pipeline, AssembledTypeID typeID)
    {
      var assembledType = pipeline.ReflectionService.GetAssembledType (typeID);
      var instance = FormatterServices.GetUninitializedObject (assembledType);

      // Call methods with [OnDeserializing] which setup default values for fields ...
      _deserializationMethodInvoker.InvokeOnDeserializing (instance, _streamingContext);
      // ... but those default values may be overridden by deserialized values.
      PopulateInstance (instance, _serializationInfo, _streamingContext, typeID.RequestedType.Name);

      return instance;
    }

    protected abstract void PopulateInstance (
        object instance, SerializationInfo serializationInfo, StreamingContext streamingContext, string requestedTypeName);
  }
}