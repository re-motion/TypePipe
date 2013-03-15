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
using Remotion.Utilities;

namespace Remotion.TypePipe.Caching
{
  /// <summary>
  /// Provides functionality for assembling a type by orchestrating <see cref="IParticipant"/> instances and an instance of 
  /// <see cref="ITypeContextCodeGenerator"/>.
  /// Also calculates a compound cache key consisting of the requested type and the individual cache key parts returned from the 
  /// <see cref="ICacheKeyProvider"/>. The providers are retrieved from the participants exactly once at object creation.
  /// </summary>
  public class TypeAssembler : ITypeAssembler
  {
    private readonly ReadOnlyCollection<IParticipant> _participants;
    private readonly IMutableTypeFactory _mutableTypeFactory;
    private readonly ITypeContextCodeGenerator _typeContextCodeGenerator;
    // Array for performance reasons.
    private readonly ICacheKeyProvider[] _cacheKeyProviders;

    public TypeAssembler (
        IEnumerable<IParticipant> participants, IMutableTypeFactory mutableTypeFactory, ITypeContextCodeGenerator typeContextCodeGenerator)
    {
      ArgumentUtility.CheckNotNull ("participants", participants);
      ArgumentUtility.CheckNotNull ("mutableTypeFactory", mutableTypeFactory);
      ArgumentUtility.CheckNotNull ("typeContextCodeGenerator", typeContextCodeGenerator);

      _participants = participants.ToList().AsReadOnly();
      _mutableTypeFactory = mutableTypeFactory;
      _typeContextCodeGenerator = typeContextCodeGenerator;

      _cacheKeyProviders = _participants.Select (p => p.PartialCacheKeyProvider).Where (ckp => ckp != null).ToArray();
    }

    public ReadOnlyCollection<ICacheKeyProvider> CacheKeyProviders
    {
      get { return _cacheKeyProviders.ToList().AsReadOnly(); }
    }

    public ICodeGenerator CodeGenerator
    {
      get { return _typeContextCodeGenerator.CodeGenerator; }
    }

    public Type AssembleType (Type requestedType)
    {
      var typeContext = new TypeContext (_mutableTypeFactory, requestedType);

      foreach (var participant in _participants)
        participant.Modify (typeContext);

      return ApplyModificationsWithDiagnostics (typeContext);
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

    private Type ApplyModificationsWithDiagnostics (TypeContext typeContext)
    {
      try
      {
        return _typeContextCodeGenerator.GenerateProxy (typeContext);
      }
      catch (InvalidOperationException ex)
      {
        throw new InvalidOperationException (BuildExceptionMessage (typeContext.RequestedType, ex), ex);
      }
      catch (NotSupportedException ex)
      {
        throw new NotSupportedException (BuildExceptionMessage (typeContext.RequestedType, ex), ex);
      }
    }

    private string BuildExceptionMessage (Type requestedType, SystemException exception)
    {
      var participantList = SeparatedStringBuilder.Build (", ", _participants, p => "'" + p.GetType().Name + "'");
      return string.Format (
          "An error occurred during code generation for '{0}':{1}{2}{3}"
          + "The following participants are currently configured and may have caused the error: {4}.",
          requestedType.Name,
          Environment.NewLine,
          exception.Message,
          Environment.NewLine,
          participantList);
    }
  }
}