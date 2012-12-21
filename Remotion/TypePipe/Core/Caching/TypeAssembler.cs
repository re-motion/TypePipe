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
using Remotion.Collections;
using Remotion.Text;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Descriptors;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.StrongNaming;
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
    private readonly IMutableTypeAnalyzer _mutableTypeAnalyzer;
    private readonly ITypeModifier _typeModifier;
    // Array for performance reasons.
    private readonly ICacheKeyProvider[] _cacheKeyProviders;

    public TypeAssembler (IEnumerable<IParticipant> participants, IMutableTypeAnalyzer mutableTypeAnalyzer, ITypeModifier typeModifier)
    {
      ArgumentUtility.CheckNotNull ("participants", participants);
      ArgumentUtility.CheckNotNull ("mutableTypeAnalyzer", mutableTypeAnalyzer);
      ArgumentUtility.CheckNotNull ("typeModifier", typeModifier);

      _participants = participants.ToList().AsReadOnly();
      _mutableTypeAnalyzer = mutableTypeAnalyzer;
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

      var participantCompatibilities = _participants.Select (p => Tuple.Create (p, p.ModifyType (mutableType))).ToList();

      if (_typeModifier.CodeGenerator.IsStrongNamingEnabled)
        CheckCompatibility (mutableType, participantCompatibilities);

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

    private void CheckCompatibility (MutableType mutableType, List<Tuple<IParticipant, StrongNameCompatibility>> compatibilities)
    {
      var incompatibleParticipants = compatibilities.Where (t => t.Item2 == StrongNameCompatibility.Incompatible).Select (t => t.Item1).ToList();
      if (incompatibleParticipants.Count > 0)
        throw NewInvalidOperationException ("", incompatibleParticipants);

      var unknownParticipants = compatibilities.Where (t => t.Item2 == StrongNameCompatibility.Unknown).Select (t => t.Item1).ToList();
      if (unknownParticipants.Count > 0 && !_mutableTypeAnalyzer.IsStrongNameCompatible (mutableType))
        throw NewInvalidOperationException ("at least one of ", unknownParticipants);
    }

    private InvalidOperationException NewInvalidOperationException (string participantMeaning, List<IParticipant> offendingParticipants)
    {
      var participantList = SeparatedStringBuilder.Build (", ", offendingParticipants, p => string.Format ("'{0}'", p.GetType().Name));
      var message = string.Format (
          "Strong-naming is enabled but {0}the following participants requested incompatible type modifications: {1}.",
          participantMeaning,
          participantList);
      return new InvalidOperationException (message);
    }
  }
}