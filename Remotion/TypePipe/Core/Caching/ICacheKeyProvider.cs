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
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.Caching
{
  /// <summary>
  /// This interface provides a cache key for a <see cref="IParticipant"/> depending on the requested type.
  /// Implementations should return non-equal cache keys if the participant applies different modifications to the <see cref="MutableType"/>.
  /// This might depend on participant configuation, context and user options.
  /// However, a cache key should not encode the requested type itself, as this is already handled by the pipeline caching facilities.
  /// </summary>
  /// <remarks>
  /// This interface must be implemented if the generated types cannot be cached unconditionally.
  /// If the generated types can be cached unconditionally <see cref="IParticipant.PartialCacheKeyProvider"/> should return <see langword="null"/>.
  /// </remarks>
  public interface ICacheKeyProvider
  {
    /// <summary>
    /// Gets a cache key used to identify the generated type for the provided requested type.
    /// It should include the <see cref="IParticipant"/> configuation and other data that might influence the modifications specified by a 
    /// participant. However, implementations should not encode the requested type itself, as this is already handled by the pipeline.
    /// </summary>
    /// <param name="requestedType">The requested type.</param>
    /// <returns>A cache key, or null if no specific caching information is required for the <paramref name="requestedType"/>..</returns>
    object GetCacheKey (Type requestedType);
  }
}