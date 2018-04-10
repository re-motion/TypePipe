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
using JetBrains.Annotations;

namespace Remotion.TypePipe
{
  /// <summary>
  /// Allows the registration of an <see cref="IPipeline"/> instance under a given participant configuration ID and retrieving that pipeline later
  /// via the same identifier.
  /// </summary>
  /// <remarks>
  /// <para>
  /// An instance of this interface should be registered with the <see cref="PipelineRegistry"/>. This allows the TypePipe infrastructure to access
  /// the registered <see cref="IPipelineRegistry"/> instance from within deserialization code. It can also be used to register and resolve the 
  /// <see cref="IPipeline"/> instances used throughout an application's code. Note: If the application uses an IoC container to resolve the dependencies, 
  /// the delegate registered with the <see cref="PipelineRegistry"/>'s <see cref="PipelineRegistry.SetInstanceProvider"/> method should redirect 
  /// to the IoC container to retrieve the <see cref="IPipelineRegistry"/> in order to keep the pipeline registrations in sync.
  /// </para>
  /// <para>
  /// This interface also enables serialization of object instances without the need to save the generated types to disk.
  /// To accomplish this register compatible pipelines under the same participant configuration ID in the deserializing as well as the
  /// serializing app domain. <i>Compatible pipelines</i> refers to pipelines with equivalent participant configurations, i.e., pipelines that
  /// generate equivalent types for a requested type.
  /// </para>
  /// </remarks>
  /// <threadsafety static="true" instance="true"/>
  public interface IPipelineRegistry
  {
    /// <summary>
    /// Gets the default pipeline.
    /// </summary>
    /// <exception cref="InvalidOperationException">If no default pipeline was specified.</exception>
    [NotNull]
    IPipeline DefaultPipeline { get; }

    /// <summary>
    /// Registers an <see cref="IPipeline"/> under its <see cref="IPipeline.ParticipantConfigurationID"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">If a factory is already registered under the specified identifier.</exception>
    /// <param name="pipeline">The object factory to register.</param>
    void Register ([NotNull] IPipeline pipeline);

    /// <summary>
    /// Unregisters the <see cref="IPipeline"/> instance that is currently registered under the specified participant configuration ID.
    /// No exception is thrown if no factory is registered under the given identifier.
    /// </summary>
    /// <param name="participantConfigurationID">The participant configuration ID.</param>
    void Unregister ([NotNull] string participantConfigurationID);

    /// <summary>
    /// Retrieves the <see cref="IPipeline"/> instance that is registered under the specified participant configuration ID.
    /// </summary>
    /// <exception cref="InvalidOperationException">If no factory is registered under the specified identifier.</exception>
    /// <param name="participantConfigurationID">The participant configuration ID.</param>
    /// <returns>The registered pipeline.</returns>
    [NotNull]
    IPipeline Get ([NotNull] string participantConfigurationID);
  }
}