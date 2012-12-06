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
using Remotion.Utilities;

namespace Remotion.TypePipe.Serialization.Implementation
{
  /// <summary>
  /// Acts as a common base for the .NET deserialization surrogates.
  /// </summary>
  [Serializable]
  public abstract class SerializationSurrogateBase : ISerializable, IObjectReference
  {
    private readonly IObjectFactoryRegistry _registry = SafeServiceLocator.Current.GetInstance<IObjectFactoryRegistry>();

    private readonly SerializationInfo _serializationInfo;

    protected SerializationSurrogateBase (SerializationInfo serializationInfo, StreamingContext streamingContext)
    {
      ArgumentUtility.CheckNotNull ("serializationInfo", serializationInfo);

      _serializationInfo = serializationInfo;
    }

    public SerializationInfo SerializationInfo
    {
      get { return _serializationInfo; }
    }

    public void GetObjectData (SerializationInfo info, StreamingContext context)
    {
      throw new NotSupportedException ("This method should not be called.");
    }

    public object GetRealObject (StreamingContext context)
    {
      var underlyingTypeName = (string) SerializationInfo.GetValue (SerializationParticipant.UnderlyingTypeKey, typeof (string));
      var factoryIdentifier = (string) SerializationInfo.GetValue (SerializationParticipant.FactoryIdentifierKey, typeof (string));

      var underlyingType = Type.GetType (underlyingTypeName, throwOnError: true);
      var factory = _registry.Get (factoryIdentifier);

      return CreateRealObject (factory, underlyingType, context);
    }

    protected abstract object CreateRealObject (IObjectFactory objectFactory, Type underlyingType, StreamingContext context);
  }
}