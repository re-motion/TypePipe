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
using Remotion.TypePipe.MutableReflection.Descriptors;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.Caching
{
  /// <summary>
  /// Provides functionality for assembling a type by orchestrating <see cref="IParticipant"/> instances and an instance of 
  /// <see cref="ITypeModifier"/>.
  /// Also calculates a compound cache key consisting of the requested type and the individual cache key parts returned from the 
  /// <see cref="ICacheKeyProvider"/>. The providers are retrieved from the participants exactly once at object creation.
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

      _cacheKeyProviders = _participants.Select (p => p.PartialCacheKeyProvider).Where (ckp => ckp != null).ToArray();
    }

    public ReadOnlyCollection<ICacheKeyProvider> CacheKeyProviders
    {
      get { return _cacheKeyProviders.ToList().AsReadOnly(); }
    }

    public ICodeGenerator CodeGenerator
    {
      get { return _typeModifier.CodeGenerator; }
    }

    public Type AssembleType (Type requestedType)
    {
      var mutableType = CreateMutableType (requestedType);

      foreach (var participant in _participants)
        participant.ModifyType (mutableType);

      // TODO Review: Catch InvalidOperationException, NotSupportedException; add participant list to exception message, then throw. (Wrap inner exception.)
      return _typeModifier.ApplyModifications (mutableType);
    }

    public object[] GetCompoundCacheKey (Type requestedType, int freeSlotsAtStart)
    {
      ArgumentUtility.CheckNotNull ("requestedType", requestedType);

      // No LINQ for performance reasons.
      var offset = freeSlotsAtStart + 1;
      var compoundKey = new object[_cacheKeyProviders.Length + offset];
      compoundKey[freeSlotsAtStart] = requestedType;

      for (int i = 0; i < _cacheKeyProviders.Length; ++i)
        compoundKey[i + offset] = _cacheKeyProviders[i].GetCacheKey (requestedType);

      return compoundKey;
    }

    private MutableType CreateMutableType (Type requestedType)
    {
      var underlyingTypeDescriptor = TypeDescriptor.Create (requestedType);
      var memberSelector = new MemberSelector (new BindingFlagsEvaluator());
      var relatedMethodFinder = new RelatedMethodFinder();
      var interfaceMappingHelper = new InterfaceMappingComputer();
      var mutableMemberFactory = new MutableMemberFactory (memberSelector, relatedMethodFinder);

      return new MutableType (underlyingTypeDescriptor, memberSelector, relatedMethodFinder, interfaceMappingHelper, mutableMemberFactory);
    }
  }
}