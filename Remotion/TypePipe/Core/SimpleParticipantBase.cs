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
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.Implementation;

namespace Remotion.TypePipe
{
  // TODO 5370
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

    public virtual Type GetOrCreateAdditionalType (object id, AdditionalTypeAssemblyContext additionalTypeAssemblyContext)
    {
      return null;
    }

    public virtual void HandleNonSubclassableType (Type requestedType)
    {
      // Does noting.
    }
  }
}