﻿// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using System.Threading;
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
    private readonly object _lock = new object();
    private readonly Dictionary<string, IPipeline> _pipelines = new Dictionary<string, IPipeline>();

    private IPipeline _defaultPipeline;

    public DefaultPipelineRegistry (IPipeline defaultPipeline)
    {
      ArgumentUtility.CheckNotNull ("defaultPipeline", defaultPipeline);

      SetDefaultPipeline (defaultPipeline);
    }

    public IPipeline DefaultPipeline
    {
      get
      {
        // Reading the _defaultPipeline field with just a volatle read instead of inside a lock-statement is considered sufficient 
        // because there is only one locked write access to the field in the rest of the code (SetDefaultPipleline).
        // Note that SetDefaultPipeline(...) might update the _defaultPipeline field before updating the _pipelines dictionary. An access to 
        // get_DefaultPipeline would then see this updated value before SetDefaultPipeline has returned. This potential race condition is 
        // considered irrelevant to the correct operation of the system, the caller would simply see the new default a little bit sooner.
        var defaultPipeline = Volatile.Read (ref _defaultPipeline);

        return defaultPipeline;
      }
    }

    public void SetDefaultPipeline (IPipeline defaultPipeline)
    {
      ArgumentUtility.CheckNotNull ("defaultPipeline", defaultPipeline);

      lock (_lock)
      {
        _pipelines[defaultPipeline.ParticipantConfigurationID] = defaultPipeline;

        // See also get_DefaultPipeline for notes on thread-safety for the _defaultPipeline field.
        _defaultPipeline = defaultPipeline;
      }
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

      lock (_lock)
      {
        if (participantConfigurationID == _defaultPipeline.ParticipantConfigurationID)
          throw new InvalidOperationException ("The default pipeline cannot be unregistered.");

        _pipelines.Remove (participantConfigurationID);
      }
    }

    public IPipeline Get (string participantConfigurationID)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("participantConfigurationID", participantConfigurationID);

      lock (_lock)
      {
        IPipeline pipeline;
        if (_pipelines.TryGetValue (participantConfigurationID, out pipeline))
          return pipeline;
      }

      var message = string.Format ("No pipeline registered for identifier '{0}'.", participantConfigurationID);
      throw new InvalidOperationException (message);
    }
  }
}