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
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration
{
  /// <summary>
  /// Provides functionality for assembling a type by orchestrating <see cref="IParticipant"/> instances and an instance of 
  /// <see cref="ITypeAssemblyContextCodeGenerator"/>.
  /// Also calculates a compound cache key consisting of the requested type and the individual cache key parts returned from the 
  /// <see cref="ICacheKeyProvider"/>. The providers are retrieved from the participants exactly once at object creation.
  /// </summary>
  public class TypeAssembler : ITypeAssembler
  {
    private readonly ReadOnlyCollection<IParticipant> _participants;
    private readonly IMutableTypeFactory _mutableTypeFactory;
    private readonly ITypeAssemblyContextCodeGenerator _typeAssemblyContextCodeGenerator;
    // Array for performance reasons.
    private readonly ICacheKeyProvider[] _cacheKeyProviders;

    public TypeAssembler (
        IEnumerable<IParticipant> participants, IMutableTypeFactory mutableTypeFactory, ITypeAssemblyContextCodeGenerator typeAssemblyContextCodeGenerator)
    {
      ArgumentUtility.CheckNotNull ("participants", participants);
      ArgumentUtility.CheckNotNull ("mutableTypeFactory", mutableTypeFactory);
      ArgumentUtility.CheckNotNull ("typeAssemblyContextCodeGenerator", typeAssemblyContextCodeGenerator);

      _participants = participants.ToList().AsReadOnly();
      _mutableTypeFactory = mutableTypeFactory;
      _typeAssemblyContextCodeGenerator = typeAssemblyContextCodeGenerator;

      _cacheKeyProviders = _participants.Select (p => p.PartialCacheKeyProvider).Where (ckp => ckp != null).ToArray();
    }

    public ReadOnlyCollection<ICacheKeyProvider> CacheKeyProviders
    {
      get { return _cacheKeyProviders.ToList().AsReadOnly(); }
    }

    public ICodeGenerator CodeGenerator
    {
      get { return _typeAssemblyContextCodeGenerator.CodeGenerator; }
    }

    public Type AssembleType (Type requestedType, IDictionary<string, object> participantState)
    {
      var typeAssemblyContext = new TypeAssemblyContext (_mutableTypeFactory, requestedType, participantState);

      foreach (var participant in _participants)
        participant.Participate (typeAssemblyContext);

      var generatedTypeContext = GenerateTypesWithDiagnostics (typeAssemblyContext);
      typeAssemblyContext.OnGenerationCompleted (generatedTypeContext);

      return (Type) generatedTypeContext.GetGeneratedMember (typeAssemblyContext.ProxyType);
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

    private GeneratedTypeContext GenerateTypesWithDiagnostics (TypeAssemblyContext typeAssemblyContext)
    {
      try
      {
        return _typeAssemblyContextCodeGenerator.GenerateTypes (typeAssemblyContext);
      }
      catch (InvalidOperationException ex)
      {
        throw new InvalidOperationException (BuildExceptionMessage (typeAssemblyContext.RequestedType, ex), ex);
      }
      catch (NotSupportedException ex)
      {
        throw new NotSupportedException (BuildExceptionMessage (typeAssemblyContext.RequestedType, ex), ex);
      }
    }

    private string BuildExceptionMessage (Type requestedType, Exception exception)
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