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
using Remotion.TypePipe.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.IntegrationTests
{
  public class ParticipantStub : IParticipant
  {
    private readonly ITypeIdentifierProvider _typeIdentifierProvider;
    private readonly Action<object, ITypeAssemblyContext> _participateAction;
    private readonly Action<LoadedTypesContext> _rebuildStateAction;
    private readonly Action<Type> _handleNonSubclassableTypeAction;
    private readonly Func<object, AdditionalTypeGenerationContext, Type> _getOrCreateAdditionalTypeFunc;

    public ParticipantStub (
        ITypeIdentifierProvider typeIdentifierProvider,
        Action<object, ITypeAssemblyContext> participateAction,
        Action<LoadedTypesContext> rebuildStateAction,
        Action<Type> handleNonSubclassableTypeAction,
        Func<object, AdditionalTypeGenerationContext, Type> getOrCreateAdditionalTypeFunc)
    {
      // Type identifier provider may be null.
      ArgumentUtility.CheckNotNull ("participateAction", participateAction);
      ArgumentUtility.CheckNotNull ("rebuildStateAction", rebuildStateAction);
      ArgumentUtility.CheckNotNull ("handleNonSubclassableTypeAction", handleNonSubclassableTypeAction);
      ArgumentUtility.CheckNotNull ("getOrCreateAdditionalTypeFunc", getOrCreateAdditionalTypeFunc);

      _typeIdentifierProvider = typeIdentifierProvider;
      _participateAction = participateAction;
      _rebuildStateAction = rebuildStateAction;
      _handleNonSubclassableTypeAction = handleNonSubclassableTypeAction;
      _getOrCreateAdditionalTypeFunc = getOrCreateAdditionalTypeFunc;
    }

    public ITypeIdentifierProvider PartialTypeIdentifierProvider
    {
      get { return _typeIdentifierProvider; }
    }

    public void Participate (object id, ITypeAssemblyContext typeAssemblyContext)
    {
      _participateAction (id, typeAssemblyContext);
    }

    public void RebuildState (LoadedTypesContext loadedTypesContext)
    {
      _rebuildStateAction (loadedTypesContext);
    }

    public void HandleNonSubclassableType (Type requestedType)
    {
      _handleNonSubclassableTypeAction (requestedType);
    }

    public Type GetOrCreateAdditionalType (object id, AdditionalTypeGenerationContext additionalTypeGenerationContext)
    {
      return _getOrCreateAdditionalTypeFunc (id, additionalTypeGenerationContext);
    }
  }
}