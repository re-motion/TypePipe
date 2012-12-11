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
using Remotion.Collections;
using Remotion.Utilities;

namespace Remotion.TypePipe.Serialization.Implementation
{
  /// <summary>
  /// Implements <see cref="IObjectFactoryRegistry"/> by using a simple <see cref="Dictionary{TKey,TValue}"/>.
  /// </summary>
  public class ObjectFactoryRegistry : IObjectFactoryRegistry
  {
    private readonly IDataStore<string, IObjectFactory> _objectFactories = DataStoreFactory.CreateWithLocking<string, IObjectFactory>();

    public void Register (string factoryIdentifier, IObjectFactory objectFactory)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("factoryIdentifier", factoryIdentifier);
      ArgumentUtility.CheckNotNull ("objectFactory", objectFactory);

      if (_objectFactories.ContainsKey (factoryIdentifier))
      {
        var message = string.Format ("Another factory is already registered for identifier '{0}'.", factoryIdentifier);
        throw new InvalidOperationException (message);
      }

      _objectFactories.Add (factoryIdentifier, objectFactory);
    }

    public void Unregister (string factoryIdentifier)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("factoryIdentifier", factoryIdentifier);

      _objectFactories.Remove (factoryIdentifier);
    }

    public IObjectFactory Get (string factoryIdentifier)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("factoryIdentifier", factoryIdentifier);

      var objectFactory = _objectFactories.GetValueOrDefault (factoryIdentifier);

      if (objectFactory == null)
      {
        var message = string.Format ("No factory registered for identifier '{0}'.", factoryIdentifier);
        throw new InvalidOperationException (message);
      }

      return objectFactory;
    }
  }
}