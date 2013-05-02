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
using Remotion.TypePipe.Caching;

namespace Remotion.TypePipe.Implementation
{
  /// <summary>
  /// Provides functionality for retrieving assembled types and analyzing them.
  /// </summary>
  public interface IReflectionService
  {
    /// <summary>
    /// Determines whether or not a <see cref="Type"/> is an assembled type.
    /// </summary>
    /// <param name="type">A type.</param>
    /// <returns>
    ///   <c>true</c> if the type is an assembled type; otherwise, <c>false</c>.
    /// </returns>
    bool IsAssembledType (Type type);

    /// <summary>
    /// Gets the type that triggered the generation of an assembled type.
    /// </summary>
    /// <param name="assembledType">An assembled type.</param>
    /// <returns>The requested type for the assembled type.</returns>
    /// <exception cref="ArgumentException">If the argument type is not an assembled type.</exception>
    Type GetRequestedType (Type assembledType);

    /// <summary>
    /// Gets a cached or newly generates an assembled type for a requested type.
    /// Because an assembled type is not uniquely identified by its requested type alone, this method may return different assembled types if the
    /// participant configuration changes between calls.
    /// </summary>
    /// <remarks>
    /// Note that this method triggers code generation if the respective assembled type is not yet present in the cache.
    /// </remarks>
    /// <param name="requestedType">A requested type.</param>
    /// <returns>An assembled type for the requested type.</returns>
    Type GetAssembledType (Type requestedType);

    /// <summary>
    /// Gets a cached or newly generates an assembled type for the specified <see cref="AssembledTypeID"/>.
    /// Because an assembled type is uniquely identified by its <paramref name="typeID"/>, this method always returns the same assembled type even
    /// if the participant configuration changes between calls.
    /// </summary>
    /// <remarks>
    /// Note that this method triggers code generation if the respective assembled type is not yet present in the cache.
    /// </remarks>
    /// <param name="typeID">An assembled type identifier.</param>
    /// <returns>The assembled type for the specified identifier.</returns>
    Type GetAssembledType (AssembledTypeID typeID);
  }
}