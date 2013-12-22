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

namespace Remotion.TypePipe.IntegrationTests
{
  public class ParticipantStub : IParticipant
  {
    private readonly ITypeIdentifierProvider _typeIdentifierProvider;
    private readonly Action<object, IProxyTypeAssemblyContext> _participateAction;
    private readonly Func<Type, object>_getAdditionalTypeIDFunc;
    private readonly Action<Type> _handleNonSubclassableTypeAction;
    private readonly Func<object, IAdditionalTypeAssemblyContext, Type> _getOrCreateAdditionalTypeFunc;

    public ParticipantStub (
        ITypeIdentifierProvider typeIdentifierProvider,
        Action<object, IProxyTypeAssemblyContext> participateAction,
        Func<Type, object> getAdditionalTypeIDFunc,
        Action<Type> handleNonSubclassableTypeAction,
        Func<object, IAdditionalTypeAssemblyContext, Type> getOrCreateAdditionalTypeFunc)
    {
      // Type identifier provider may be null.
      if (participateAction == null)
        throw new ArgumentNullException ("participateAction");
      if (getAdditionalTypeIDFunc == null)
        throw new ArgumentNullException ("getAdditionalTypeIDFunc");
      if (handleNonSubclassableTypeAction == null)
        throw new ArgumentNullException ("handleNonSubclassableTypeAction");
      if (getOrCreateAdditionalTypeFunc == null)
        throw new ArgumentNullException ("getOrCreateAdditionalTypeFunc");

      _typeIdentifierProvider = typeIdentifierProvider;
      _participateAction = participateAction;
      _getAdditionalTypeIDFunc = getAdditionalTypeIDFunc;
      _handleNonSubclassableTypeAction = handleNonSubclassableTypeAction;
      _getOrCreateAdditionalTypeFunc = getOrCreateAdditionalTypeFunc;
    }

    public ITypeIdentifierProvider PartialTypeIdentifierProvider
    {
      get { return _typeIdentifierProvider; }
    }

    public void Participate (object id, IProxyTypeAssemblyContext proxyTypeAssemblyContext)
    {
      _participateAction (id, proxyTypeAssemblyContext);
    }

    public object GetAdditionalTypeID (Type additionalType)
    {
      return _getAdditionalTypeIDFunc (additionalType);
    }

    public void HandleNonSubclassableType (Type nonSubclassableRequestedType)
    {
      _handleNonSubclassableTypeAction (nonSubclassableRequestedType);
    }

    public Type GetOrCreateAdditionalType (object additionalTypeID, IAdditionalTypeAssemblyContext additionalTypeAssemblyContext)
    {
      return _getOrCreateAdditionalTypeFunc (additionalTypeID, additionalTypeAssemblyContext);
    }
  }
}