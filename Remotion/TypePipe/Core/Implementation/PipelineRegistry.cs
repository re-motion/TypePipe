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

namespace Remotion.TypePipe.Implementation
{
  /// <summary>
  /// A <see cref="IPipelineRegistry"/> implementation that sets the <see cref="PipelineRegistry.DefaultPipeline"/> on creation containing the
  /// specified participants.
  /// </summary>
  public class PipelineRegistry : IPipelineRegistry
  {
    private readonly object _lock = new object();
    private readonly IDataStore<string, IPipeline> _pipelines = DataStoreFactory.CreateWithLocking<string, IPipeline>();

    private IPipeline _defaultPipeline;

    public PipelineRegistry (IEnumerable<IParticipant> defaultPipelineParticipants)
    {
      ArgumentUtility.CheckNotNull ("defaultPipelineParticipants", defaultPipelineParticipants);

      _defaultPipeline = PipelineFactory.Create ("<default participant configuration>", defaultPipelineParticipants);
    }

    public IPipeline DefaultPipeline
    {
      get { return _defaultPipeline; }
      set { _defaultPipeline = ArgumentUtility.CheckNotNull ("value", value); }
    }

    public void Register (IPipeline pipeline)
    {
      ArgumentUtility.CheckNotNull ("pipeline", pipeline);
      Assertion.IsNotNull (pipeline.ParticipantConfigurationID);

      lock (_lock)
      {
        if (_pipelines.ContainsKey (pipeline.ParticipantConfigurationID))
        {
          var message = string.Format ("Another pipeline is already registered for identifier '{0}'.", pipeline.ParticipantConfigurationID);
          throw new InvalidOperationException (message);
        }

        _pipelines.Add (pipeline.ParticipantConfigurationID, pipeline);
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
        throw new InvalidOperationException (string.Format ("No pipeline registered for identifier '{0}'.", participantConfigurationID));

      return pipeline;
    }
  }
}