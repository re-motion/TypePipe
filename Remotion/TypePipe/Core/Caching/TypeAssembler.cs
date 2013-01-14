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
using Remotion.Text;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.MutableReflection;
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

      return ApplyModificationsWithDiagnostics (mutableType);
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

    // TODO Create this method with injected ProxyTypeFactory.
    private MutableType CreateMutableType (Type requestedType)
    {
      var memberSelector = new MemberSelector (new BindingFlagsEvaluator());
      var relatedMethodFinder = new RelatedMethodFinder();
      var interfaceMappingComputer = new InterfaceMappingComputer();
      var mutableMemberFactory = new MutableMemberFactory (memberSelector, relatedMethodFinder);

      // TODO test.
      // TODO name = xxxProxyYY
      // TODO fullname
      // TODO attributes
      return new MutableType (
          null,
          requestedType,
          requestedType.Name,
          requestedType.Namespace,
          requestedType.FullName,
          requestedType.Attributes,
          requestedType.GetInterfaceMap,
          memberSelector,
          interfaceMappingComputer,
          mutableMemberFactory);
    }

    private Type ApplyModificationsWithDiagnostics (MutableType mutableType)
    {
      try
      {
        return _typeModifier.ApplyModifications (mutableType);
      }
      catch (InvalidOperationException ex)
      {
        throw new InvalidOperationException (BuildExceptionMessage (mutableType, ex), ex);
      }
      catch (NotSupportedException ex)
      {
        throw new NotSupportedException (BuildExceptionMessage (mutableType, ex), ex);
      }
    }

    private string BuildExceptionMessage (MutableType mutableType, SystemException exception)
    {
      var participantList = SeparatedStringBuilder.Build (", ", _participants, p => "'" + p.GetType().Name + "'");
      return string.Format (
          "An error occurred during code generation for '{0}': {1} "
          + "The following participants are currently configured and may have caused the error: {2}.",
          mutableType.Name,
          exception.Message,
          participantList);
    }
  }
}