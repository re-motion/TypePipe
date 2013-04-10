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

namespace Remotion.TypePipe.Implementation
{
  /// <summary>
  /// Implements <see cref="IPipelineRegistry"/> by using a thread-safe <see cref="IDataStore{TKey,TValue}"/>.
  /// </summary>
  public class PipelineRegistry : IPipelineRegistry
  {
    private const string c_defaultPipelineKey = "<default>";

    private readonly IDataStore<string, IPipeline> _pipelines = DataStoreFactory.CreateWithLocking<string, IPipeline>();

    public IPipeline DefaultPipeline
    {
      get { return _pipelines.GetValueOrDefault (c_defaultPipelineKey); }
    }

    public void Register (IPipeline pipeline)
    {
      ArgumentUtility.CheckNotNull ("pipeline", pipeline);
      Assertion.IsNotNull (pipeline.ParticipantConfigurationID);

      // Cannot use ContainsKey/Add combination as this would introduce a race condition.
      try
      {
        _pipelines.Add (pipeline.ParticipantConfigurationID, pipeline);
      }
      catch (ArgumentException)
      {
        var message = string.Format ("Another factory is already registered for identifier '{0}'.", pipeline.ParticipantConfigurationID);
        throw new InvalidOperationException (message);
      }
    }

    public void Unregister (string participantConfigurationID)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("participantConfigurationID", participantConfigurationID);

      _pipelines.Remove (participantConfigurationID);
    }

    public IPipeline Get (string participantConfigurationID)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("participantConfigurationID", participantConfigurationID);

      var pipeline = _pipelines.GetValueOrDefault (participantConfigurationID);

      if (pipeline == null)
      {
        var message = string.Format ("No factory registered for identifier '{0}'.", participantConfigurationID);
        throw new InvalidOperationException (message);
      }

      return pipeline;
    }

    public void SetDefaultPipeline (string participantConfigurationID)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("participantConfigurationID", participantConfigurationID);

      // TODO 5515: Race condition.
      var defaultPipeline = Get (participantConfigurationID);
      _pipelines[c_defaultPipelineKey] = defaultPipeline;
    }
  }
}