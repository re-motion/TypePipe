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
using Remotion.Reflection;
using Remotion.ServiceLocation;
using Remotion.Utilities;

namespace Remotion.TypePipe.Serialization.Implementation
{
  /// <summary>
  /// Acts as a helper for the .NET deserialization process of modified types.
  /// </summary>
  [Serializable]
  public class SerializationSurrogate : ISerializable, IObjectReference
  {
    private readonly IObjectFactoryRegistry _registry = SafeServiceLocator.Current.GetInstance<IObjectFactoryRegistry>();

    private readonly SerializationInfo _serializationInfo;
    private readonly StreamingContext _streamingContext;

    public SerializationSurrogate (SerializationInfo serializationInfo, StreamingContext streamingContext)
    {
      ArgumentUtility.CheckNotNull ("serializationInfo", serializationInfo);

      _serializationInfo = serializationInfo;
      _streamingContext = streamingContext;
    }

    public SerializationInfo SerializationInfo
    {
      get { return _serializationInfo; }
    }

    public StreamingContext StreamingContext
    {
      get { return _streamingContext; }
    }

    public void GetObjectData (SerializationInfo info, StreamingContext context)
    {
      throw new NotSupportedException("This method should not be called.");
    }

    public object GetRealObject (StreamingContext context)
    {
      // TODO 5223: Which StreamingContext should we pass to the deserialization constructor?
      var underlyingTypeName = (string) _serializationInfo.GetValue (SerializationParticipant.UnderlyingTypeKey, typeof (string));
      var factoryIdentifier = (string) _serializationInfo.GetValue (SerializationParticipant.FactoryIdentifierKey, typeof (string));

      var underlyingType = Type.GetType (underlyingTypeName, throwOnError: true);
      var factory = _registry.Get (factoryIdentifier);

      var paramList = ParamList.Create (_serializationInfo, _streamingContext);
      return factory.CreateObject (underlyingType, paramList, allowNonPublicConstructor: true);
    }
  }
}