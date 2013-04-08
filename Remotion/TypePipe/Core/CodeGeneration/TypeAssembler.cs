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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Remotion.Text;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration
{
  /// <summary>
  /// Provides functionality for assembling a type by orchestrating <see cref="IParticipant"/> instances and an instance of 
  /// <see cref="IMutableTypeBatchCodeGenerator"/>.
  /// Also calculates a compound cache key consisting of the requested type and the individual cache key parts returned from the 
  /// <see cref="ICacheKeyProvider"/>. The providers are retrieved from the participants exactly once at object creation.
  /// </summary>
  public class TypeAssembler : ITypeAssembler
  {
    private static readonly ConstructorInfo s_proxyTypeAttributeCtor = MemberInfoFromExpressionUtility.GetConstructor (() => new ProxyTypeAttribute());

    private readonly string _participantConfigurationID;
    private readonly ReadOnlyCollection<IParticipant> _participants;
    private readonly IMutableTypeFactory _mutableTypeFactory;
    // Array for performance reasons.
    private readonly ICacheKeyProvider[] _cacheKeyProviders;

    public TypeAssembler (string participantConfigurationID, IEnumerable<IParticipant> participants, IMutableTypeFactory mutableTypeFactory)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("participantConfigurationID", participantConfigurationID);
      ArgumentUtility.CheckNotNull ("participants", participants);
      ArgumentUtility.CheckNotNull ("mutableTypeFactory", mutableTypeFactory);

      _participantConfigurationID = participantConfigurationID;
      _participants = participants.ToList().AsReadOnly();
      _mutableTypeFactory = mutableTypeFactory;

      _cacheKeyProviders = _participants.Select (p => p.PartialCacheKeyProvider).Where (ckp => ckp != null).ToArray();
    }

    public string ParticipantConfigurationID
    {
      get { return _participantConfigurationID; }
    }

    public object[] GetCompoundCacheKey (Func<ICacheKeyProvider, Type, object> cacheKeyProviderMethod, Type type, int freeSlotsAtStart)
    {
      // Using Debug.Assert because it will be compiled away.
      Debug.Assert (cacheKeyProviderMethod != null);
      Debug.Assert (type != null);
      Debug.Assert (freeSlotsAtStart >= 0);

      var compoundKey = new object[_cacheKeyProviders.Length + freeSlotsAtStart];

      // No LINQ for performance reasons.
      for (int i = 0; i < _cacheKeyProviders.Length; ++i)
        compoundKey[freeSlotsAtStart + i] = cacheKeyProviderMethod (_cacheKeyProviders[i], type);

      return compoundKey;
    }

    public Type AssembleType (
        Type requestedType, IDictionary<string, object> participantState, IMutableTypeBatchCodeGenerator mutableTypeBatchCodeGenerator)
    {
      ArgumentUtility.CheckNotNull ("requestedType", requestedType);
      ArgumentUtility.CheckNotNull ("participantState", participantState);
      ArgumentUtility.CheckNotNull ("mutableTypeBatchCodeGenerator", mutableTypeBatchCodeGenerator);

      var typeAssemblyContext = CreateTypeAssemblyContext (requestedType, participantState);

      foreach (var participant in _participants)
        participant.Participate (typeAssemblyContext);

      var generatedTypeContext = GenerateTypesWithDiagnostics (mutableTypeBatchCodeGenerator, typeAssemblyContext);
      typeAssemblyContext.OnGenerationCompleted (generatedTypeContext);

      return generatedTypeContext.GetGeneratedType (typeAssemblyContext.ProxyType);
    }

    public bool IsAssembledType (Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);

      return type.IsDefined (typeof (ProxyTypeAttribute), inherit: false);
    }

    public void RebuildParticipantState (LoadedTypesContext loadedTypesContext)
    {
      ArgumentUtility.CheckNotNull ("loadedTypesContext", loadedTypesContext);

      foreach (var participant in _participants)
        participant.RebuildState (loadedTypesContext);
    }

    private TypeAssemblyContext CreateTypeAssemblyContext (Type requestedType, IDictionary<string, object> participantState)
    {
      var proxyType = _mutableTypeFactory.CreateProxy (requestedType);
      var proxyTypeAttribute = new CustomAttributeDeclaration (s_proxyTypeAttributeCtor, new object[0]);
      proxyType.AddCustomAttribute (proxyTypeAttribute);

      return new TypeAssemblyContext (_participantConfigurationID, requestedType, proxyType, _mutableTypeFactory, participantState);
    }

    private GeneratedTypeContext GenerateTypesWithDiagnostics (IMutableTypeBatchCodeGenerator codeGenerator, TypeAssemblyContext context)
    {
      try
      {
        return codeGenerator.GenerateTypes (context);
      }
      catch (InvalidOperationException ex)
      {
        throw new InvalidOperationException (BuildExceptionMessage (context.RequestedType, ex), ex);
      }
      catch (NotSupportedException ex)
      {
        throw new NotSupportedException (BuildExceptionMessage (context.RequestedType, ex), ex);
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