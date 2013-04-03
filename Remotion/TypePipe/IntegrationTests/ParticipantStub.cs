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
using Remotion.Utilities;

namespace Remotion.TypePipe.IntegrationTests
{
  public class ParticipantStub : IParticipant
  {
    private readonly ICacheKeyProvider _cacheKeyProvider;
    private readonly Action<ITypeAssemblyContext> _participateAction;
    private readonly Action<LoadedTypeContext> _rebuildStateAction;

    public ParticipantStub (
        ICacheKeyProvider cacheKeyProvider, Action<ITypeAssemblyContext> participateAction, Action<LoadedTypeContext> rebuildStateAction)
    
    {
      // Cache key provider may be null.
      ArgumentUtility.CheckNotNull ("participateAction", participateAction);
      ArgumentUtility.CheckNotNull ("rebuildStateAction", rebuildStateAction);

      _cacheKeyProvider = cacheKeyProvider;
      _participateAction = participateAction;
      _rebuildStateAction = rebuildStateAction;
    }

    public ICacheKeyProvider PartialCacheKeyProvider
    {
      get { return _cacheKeyProvider; }
    }

    public void Participate (ITypeAssemblyContext typeAssemblyContext)
    {
      _participateAction (typeAssemblyContext);
    }

    public void RebuildState (LoadedTypeContext loadedTypeContext)
    {
      _rebuildStateAction (loadedTypeContext);
    }
  }
}