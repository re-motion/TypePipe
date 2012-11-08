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
using System.Linq;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe
{
  /// <summary>
  /// Provides functionality for assembling a type by orchestrating <see cref="IParticipant"/> instances and an instance of 
  /// <see cref="ITypeModifier"/>.
  /// Can also calculate a <see cref="CompoundCacheKey"/> that contains the individual <see cref="CacheKey"/>s from the 
  /// <see cref="ICacheKeyProvider"/>s which in turn are retrieved from the participants.
  /// </summary>
  public class TypeAssembler : ITypeAssembler
  {
    private readonly ReadOnlyCollection<IParticipant> _participants;
    private readonly ITypeModifier _typeModifier;
    // Array for performance reasons.
    private readonly ICacheKeyProvider[] _cacheKeyProviders;

    public TypeAssembler (IEnumerable<IParticipant> participants, ITypeModifier typeModifier)
    {
      ArgumentUtility.CheckNotNull ("participants", participants);
      ArgumentUtility.CheckNotNull ("typeModifier", typeModifier);

      _participants = participants.ToList().AsReadOnly();
      _typeModifier = typeModifier;
      _cacheKeyProviders = _participants.Select (p => p.GetCacheKeyProvider()).Where (ckp => ckp != null).ToArray();
    }

    public ReadOnlyCollection<IParticipant> Participants
    {
      get { return _participants; }
    }

    public ITypeModifier TypeModifier
    {
      get { return _typeModifier; }
    }

    public ReadOnlyCollection<ICacheKeyProvider> CacheKeyProviders
    {
      get { return _cacheKeyProviders.ToList().AsReadOnly(); }
    }

    public Type AssembleType (Type requestedType)
    {
      var mutableType = CreateMutableType (requestedType);

      foreach (var participant in _participants)
        participant.ModifyType (mutableType);

      return _typeModifier.ApplyModifications (mutableType);
    }

    public CompoundCacheKey GetCompoundCacheKey (Type requestedType)
    {
      ArgumentUtility.CheckNotNull ("requestedType", requestedType);

      // No LINQ for performance reasons.
      var cacheKeys = new CacheKey[_cacheKeyProviders.Length];
      for (int i = 0; i < cacheKeys.Length; ++i)
        cacheKeys[i] = _cacheKeyProviders[i].GetCacheKey (requestedType);

      return new CompoundCacheKey (requestedType, cacheKeys);
    }

    private MutableType CreateMutableType (Type requestedType)
    {
      var underlyingTypeDescriptor = UnderlyingTypeDescriptor.Create (requestedType);
      var memberSelector = new MemberSelector (new BindingFlagsEvaluator());
      var relatedMethodFinder = new RelatedMethodFinder();
      var mutableMemberFactory = new MutableMemberFactory (memberSelector, relatedMethodFinder);

      return new MutableType (underlyingTypeDescriptor, memberSelector, relatedMethodFinder, mutableMemberFactory);
    }
  }
}