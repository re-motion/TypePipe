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
using Remotion.TypePipe.Configuration;

namespace Remotion.TypePipe
{
  /// <summary>
  /// Creates instances of <see cref="IPipeline"/>.
  /// </summary>
  /// <threadsafety static="true" instance="true"/>
  public interface IPipelineFactory
  {
    /// <summary>
    /// Creates an <see cref="IPipeline"/> with the given participant configuration ID containing the specified participants and optionally a
    /// custom configuration provider. If the configuration provider is omitted, the <c>App.config</c>-based configuration provider
    /// (<see cref="AppConfigBasedSettingsProvider"/>) is used.
    /// </summary>
    /// <remarks>
    /// <see cref="IPipeline"/> instances with equal participant configuration IDs must generate equivalent types.
    /// </remarks>
    /// <param name="participantConfigurationID">The participant configuration ID.</param>
    /// <param name="settings">The pipeline settings.</param>
    /// <param name="participants">The participants that should be used by this object factory.</param>
    /// <returns>An new instance of <see cref="IPipeline"/>.</returns>
    IPipeline Create (string participantConfigurationID, PipelineSettings settings, IEnumerable<IParticipant> participants);
  }
}