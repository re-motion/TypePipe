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
using Remotion.ServiceLocation;
using Remotion.TypePipe.Caching;

namespace Remotion.TypePipe.CodeGeneration
{
  /// <summary>
  /// Generates types for requested types and computes compound cache keys to enabled efficient caching of generated types.
  /// </summary>
  [ConcreteImplementation (typeof (TypeAssembler))]
  public interface ITypeAssembler
  {
    string ParticipantConfigurationID { get; }
    ICodeGenerator CodeGenerator { get; }

    /// <summary>
    /// Computes a compound cache key consisting of the individual cache key parts from the <see cref="ICacheKeyProvider"/>s of the participants.
    /// The return value of this method is an object array for performance reasons.
    /// </summary>
    /// <param name="cacheKeyProviderMethod">
    ///   A function delegate that computes one part of the cache key by calling the appropriate method on <see cref="ICacheKeyProvider"/>.
    /// </param>
    /// <param name="type">The requested or generated type.</param>
    /// <param name="freeSlotsAtStart">Number of slots beginning at the start of the array which are reserved for use by the caller.</param>
    /// <returns>The compound cache key.</returns>
    object[] GetCompoundCacheKey (Func<ICacheKeyProvider, Type, object> cacheKeyProviderMethod, Type type, int freeSlotsAtStart);

    Type AssembleType (Type requestedType, IDictionary<string, object> participantState);
  }
}