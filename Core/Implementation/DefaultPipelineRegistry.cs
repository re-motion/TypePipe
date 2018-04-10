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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Remotion.Utilities;

namespace Remotion.TypePipe.Implementation
{
  /// <summary>
  /// A <see cref="IPipelineRegistry"/> implementation that registers the provided pipeline and sets it as the default pipeline.
  /// This ensures that the <see cref="DefaultPipelineRegistry.DefaultPipeline"/> property is populated.
  /// </summary>
  /// <threadsafety static="true" instance="true"/>
  public class DefaultPipelineRegistry : IPipelineRegistry
  {
    /// <summary>ConcurrentDictionary{string, IPipeline}</summary>
    /// <remarks>
    /// <see cref="Hashtable"/> was chosen over <see cref="ConcurrentDictionary{TKey,TValue}"/> due to performance considerations:
    /// When used in a multi-reader / single-writer setup with many reads but only few writes, the Hashtable allows 25% more reads per time unit 
    /// compared to the <see cref="ConcurrentDictionary{TKey,TValue}"/>. Test setup was a dictionary with 10 entries and a 36-characters string key.
    /// </remarks>
    private readonly Hashtable _pipelines = new Hashtable();

    public IPipeline DefaultPipeline { get; }

    public DefaultPipelineRegistry (IPipeline defaultPipeline)
    {
      ArgumentUtility.CheckNotNull ("defaultPipeline", defaultPipeline);

      Register (defaultPipeline);
      DefaultPipeline = defaultPipeline;
    }

    public void Register (IPipeline pipeline)
    {
      ArgumentUtility.CheckNotNull ("pipeline", pipeline);
      Assertion.IsNotNull (pipeline.ParticipantConfigurationID);

      lock (_pipelines.SyncRoot)
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

      if (participantConfigurationID == DefaultPipeline.ParticipantConfigurationID)
        throw new InvalidOperationException ("The default pipeline cannot be unregistered.");

      lock (_pipelines.SyncRoot)
      {
        _pipelines.Remove (participantConfigurationID);
      }
    }

    public IPipeline Get (string participantConfigurationID)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("participantConfigurationID", participantConfigurationID);

      // ReSharper disable once InconsistentlySynchronizedField
      // _pipeline is a Hashtable. Hashtable is threadsafe for multi-readers / single-writer.
      var pipeline = (IPipeline) _pipelines[participantConfigurationID];

      if (pipeline == null)
      {
        var message = string.Format ("No pipeline registered for identifier '{0}'.", participantConfigurationID);
        throw new InvalidOperationException (message);
      }

      return pipeline;
    }
  }
}