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
using Remotion.TypePipe.TypeAssembly;

namespace Remotion.TypePipe
{
  /// <summary>
  /// A base class for simple participants that do not need to implement the full <see cref="IParticipant"/> contract.
  /// Such a participant does not use context-sensitive data to generate types, i.e., the modifications only depend on the requested type.
  /// Furthermore it does not accumulate state, create additionaly types or handle non-subclassable requested types.
  /// If one of these features is required the participant may override the appropriate method in this base class or implement
  /// <see cref="IParticipant"/> directly.
  /// </summary>
  /// <threadsafety static="true" instance="true"/>
  public abstract class SimpleParticipantBase : IParticipant
  {
    public virtual ITypeIdentifierProvider PartialTypeIdentifierProvider
    {
      get { return null; }
    }

    public abstract void Participate (object id, IProxyTypeAssemblyContext proxyTypeAssemblyContext);

    public virtual void RebuildState (LoadedTypesContext loadedTypesContext)
    {
      // Does nothing.
    }

    public virtual Type GetOrCreateAdditionalType (object additionalTypeID, IAdditionalTypeAssemblyContext additionalTypeAssemblyContext)
    {
      return null; // Does nothing.
    }

    public virtual void HandleNonSubclassableType (Type requestedType)
    {
      // Does noting.
    }
  }
}