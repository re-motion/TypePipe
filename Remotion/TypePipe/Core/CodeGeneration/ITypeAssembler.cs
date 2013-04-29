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
using System.Collections.ObjectModel;
using Remotion.TypePipe.Implementation;

namespace Remotion.TypePipe.CodeGeneration
{
  /// <summary>
  /// Generates types for requested types and computes compound identifiers to enabled efficient caching of generated types.
  /// </summary>
  public interface ITypeAssembler
  {
    string ParticipantConfigurationID { get; }
    ReadOnlyCollection<IParticipant> Participants { get; }

    bool IsAssembledType (Type type);
    Type GetRequestedType (Type assembledType);

    /// <summary>
    /// Computes a compound identifier consisting of the individual identifier parts returned from the
    /// <see cref="IParticipant.PartialTypeIdentifierProvider"/> of the participants.
    /// The return value of this method is an object array for performance reasons.
    /// </summary>
    /// <param name="requestedType">The requested type.</param>
    /// <param name="freeSlotsAtStart">Number of slots beginning at the start of the array which are reserved for use by the caller.</param>
    /// <returns>The compound identifier.</returns>
    object[] GetCompoundID (Type requestedType, int freeSlotsAtStart);

    IEnumerable<object> ExtractCompoundID (Type assembledType);

    Type AssembleType (Type requestedType, IDictionary<string, object> participantState, IMutableTypeBatchCodeGenerator codeGenerator);

    void RebuildParticipantState (LoadedTypesContext loadedTypesContext);
  }
}