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
using Remotion.Collections;
using Remotion.Utilities;

namespace Remotion.TypePipe.Serialization.Implementation
{
  /// <summary>
  /// Implements <see cref="IObjectFactoryRegistry"/> by using a thread-safe <see cref="IDataStore{TKey,TValue}"/>.
  /// </summary>
  public class ObjectFactoryRegistry : IObjectFactoryRegistry
  {
    private readonly IDataStore<string, IObjectFactory> _objectFactories = DataStoreFactory.CreateWithLocking<string, IObjectFactory>();

    public void Register (IObjectFactory objectFactory)
    {
      ArgumentUtility.CheckNotNull ("objectFactory", objectFactory);
      Assertion.IsNotNull (objectFactory.ParticipantConfigurationID);

      // Cannot use ContainsKey/Add combination as this would introduce a race condition.
      try
      {
        _objectFactories.Add (objectFactory.ParticipantConfigurationID, objectFactory);
      }
      catch (ArgumentException)
      {
        var message = string.Format ("Another factory is already registered for identifier '{0}'.", objectFactory.ParticipantConfigurationID);
        throw new InvalidOperationException (message);
      }
    }

    public void Unregister (string participantConfigurationID)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("participantConfigurationID", participantConfigurationID);

      _objectFactories.Remove (participantConfigurationID);
    }

    public IObjectFactory Get (string participantConfigurationID)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("participantConfigurationID", participantConfigurationID);

      var objectFactory = _objectFactories.GetValueOrDefault (participantConfigurationID);

      if (objectFactory == null)
      {
        var message = string.Format ("No factory registered for identifier '{0}'.", participantConfigurationID);
        throw new InvalidOperationException (message);
      }

      return objectFactory;
    }
  }
}