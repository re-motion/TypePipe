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
using Remotion.Utilities;

namespace Remotion.TypePipe
{
  /// <summary>
  /// Provides the global <see cref="IPipelineRegistry"/> <see cref="Instance"/> used by the <b>TypePipe</b> infrastructure, e.g. when deserializing an object.
  /// </summary>
  /// <threadsafety static="true" instance="true"/>
  public static class PipelineRegistry
  {
    private static readonly object s_instanceProviderLockObject = new object();
    private static Func<IPipelineRegistry> s_instanceProvider;

    /// <summary>
    /// Gets the global <see cref="IPipelineRegistry"/> instance registered via the <see cref="SetInstanceProvider"/> API.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// No <see cref="IPipelineRegistry"/> provider has been registered.<br/>- or -<br/>
    /// The provider registered with <see cref="SetInstanceProvider"/> returned <see langword="null" />.
    /// </exception>
    public static IPipelineRegistry Instance
    {
      get
      {
        Func<IPipelineRegistry> instanceProvider;
        lock (s_instanceProviderLockObject)
        {
          instanceProvider = s_instanceProvider;
        }

        if (instanceProvider == null)
        {
          throw new InvalidOperationException (
              "No instance provider was set for the PipelineRegistry's Instance property. "
              + "Use PipelineRegistry.SetInstanceProvider (() => thePipelineRegistry) during application startup to initialize the TypePipe infrastructure.");
        }

        var instance = instanceProvider();
        if (instance == null)
        {
          throw new InvalidOperationException (
              "The registered instance provider returned null. "
              + "Use PipelineRegistry.SetInstanceProvider (() => thePipelineRegistry) during application startup to initialize the TypePipe infrastructure.");
        }

        return instance;
      }
    }

    /// <summary>
    /// Gets a flag to test wether an <see cref="IPipelineRegistry"/> provider has been registered via <see cref="SetInstanceProvider"/>.
    /// </summary>
    public static bool HasInstanceProvider
    {
      get
      {
        lock (s_instanceProviderLockObject)
        {
          return s_instanceProvider != null;
        }
      }
    }

    /// <summary>
    /// Registers the a delegate that can be used to resolve the global <see cref="IPipelineRegistry"/> instance.
    /// </summary>
    /// <param name="pipelineRegistryProvider">
    /// The <see cref="Func{IPipelineRegistry}"/> delegate returns the global <see cref="IPipelineRegistry"/> instance.
    /// It must not be <see langword="null" /> and must not return a <see langword="null" /> value.
    /// </param>
    /// <remarks>
    /// The <paramref name="pipelineRegistryProvider"/> must not initialize the instance on the fly. 
    /// Instead, it must return the same instance during each invocation, 
    /// e.g. by capturing the <see cref="IPipelineRegistry"/> instance when the delegate is created as a closure 
    /// or by retrieving the <see cref="IPipelineRegistry"/> instance as a singleton from the application's IoC container.
    /// </remarks>
    public static void SetInstanceProvider ([NotNull] Func<IPipelineRegistry> pipelineRegistryProvider)
    {
      ArgumentUtility.CheckNotNull ("pipelineRegistryProvider", pipelineRegistryProvider);

      lock (s_instanceProviderLockObject)
      {
        s_instanceProvider = pipelineRegistryProvider;
      }
    }
  }
}