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
using Remotion.ServiceLocation;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.TypeAssembly;

namespace Remotion.TypePipe
{
  /// <summary>
  /// Participates in the assembly of a type by calling mutating members of <see cref="MutableType"/>.
  /// Framework authors implement this interface in order to take part in the code generation process.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Instances of this interface are used to populate <see cref="IPipeline.Participants"/>.
  /// Every participant has the chance the specify type modifications via the provided <see cref="MutableType"/> instance.
  /// The <see cref="MutableType"/> is a representation of the type to be generated for the requested type.
  /// In addition, it contains all modifications applied by preceding participants in the pipeline.
  /// </para>
  /// <para>
  /// Note that implementations of this interface may be shared across multiple <see cref="IPipeline"/> instances.
  /// Participants therefore must not hold any mutable state directly.
  /// If there is the need to hold state a participant should use <see cref="ITypeAssemblyContext.State"/>.
  /// </para>
  /// </remarks>
  [ConcreteImplementation ("Remotion.Mixins.CodeGeneration.TypePipe.MixinParticipant, Remotion.Mixins, "
                           + "Version=<version>, Culture=neutral, PublicKeyToken=<publicKeyToken>", ignoreIfNotFound: true, Position = 1)]
  [ConcreteImplementation ("Remotion.Data.DomainObjects.Infrastructure.TypePipe.DomainObjectParticipant, Remotion.Data.DomainObjects, "
                           + "Version=<version>, Culture=neutral, PublicKeyToken=<publicKeyToken>", ignoreIfNotFound: true, Position = 2)]
  public interface IParticipant
  {
    /// <summary>
    /// A participant must provide a <see cref="ITypeIdentifierProvider"/> if generated types cannot be cached unconditionally, i.e.,
    /// the modifications depend not solely on the requested type.
    /// If generated types can be cached unconditionally, <see cref="PartialTypeIdentifierProvider"/> should return <see langword="null"/>.
    /// </summary>
    /// <value>
    /// The partial type identifier provider, or <see langword="null"/>.
    /// </value>
    ITypeIdentifierProvider PartialTypeIdentifierProvider { get; }

    /// <summary>
    /// This method allows participants to specify their code generation needs.
    /// The provided <see cref="IProxyTypeAssemblyContext"/> contains the type requested by the user and the mutable proxy type that was created for it by
    /// the pipeline.
    /// The <paramref name="id"/> identifies the type being assembled. It was created by calling <see cref="ITypeIdentifierProvider.GetID"/> on
    /// <see cref="PartialTypeIdentifierProvider"/>; it is <see langword="null"/> if there is no such provider.
    /// The participant must consider this identifier when generating types and must not use any additional ambient configuration data.
    /// </summary>
    /// <param name="id">The identifier returned by the type identifier provider or <see langword="null"/> if there is no such provider.</param>
    /// <param name="proxyTypeAssemblyContext">The type assembly context.</param>
    void Participate (object id, IProxyTypeAssemblyContext proxyTypeAssemblyContext);

    /// <summary>
    /// This method allows participants to react when the pipeline loads a set of types from a previously flushed assembly.
    /// </summary>
    /// <param name="loadedTypesContext">The loaded types context.</param>
    void RebuildState (LoadedTypesContext loadedTypesContext);

    /// <summary>
    /// Gets or creates an additional type for the specified identifier. If the participant can not interpret the <paramref name="additionalTypeID"/>
    /// it should return <see langword="null"/>.
    /// </summary>
    /// <param name="additionalTypeID">The additional type identifier.</param>
    /// <param name="additionalTypeAssemblyContext">The additional type assembly context.</param>
    /// <returns>An additional type for the specified identifier; or <see langword="null"/>.</returns>
    /// <remarks>
    /// A participant may retrieve a cached additional type from the state cache available via <see cref="ITypeAssemblyContext.State"/> on
    /// <paramref name="additionalTypeAssemblyContext"/> or create a new <see cref="MutableType"/> and return it.
    /// </remarks>
    Type GetOrCreateAdditionalType (object additionalTypeID, IAdditionalTypeAssemblyContext additionalTypeAssemblyContext);

    /// <summary>
    /// This method is called for requested types that are not subclassable and therefore not processed by the pipeline
    /// (i.e., <see cref="Participate"/> is never called for such types).
    /// Participants may use this method to log diagnostic and report errors even for types that are not processed by the pipeline.
    /// </summary>
    /// <param name="requestedType">The non-subclassable requested type.</param>
    void HandleNonSubclassableType (Type requestedType);
  }
}