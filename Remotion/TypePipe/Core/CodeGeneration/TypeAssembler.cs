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
using Remotion.FunctionalProgramming;
using Remotion.Text;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration
{
  /// <summary>
  /// Provides functionality for assembling a type by orchestrating <see cref="IParticipant"/> instances and an instance of 
  /// <see cref="IMutableTypeBatchCodeGenerator"/>.
  /// Also calculates a compound cache key consisting of the requested type and the individual cache key parts returned from the 
  /// <see cref="ITypeIdentifierProvider"/>. The providers are retrieved from the participants exactly once at object creation.
  /// </summary>
  public class TypeAssembler : ITypeAssembler
  {
    private static readonly ConstructorInfo s_assembledTypeAttributeCtor =
        MemberInfoFromExpressionUtility.GetConstructor (() => new AssembledTypeAttribute());

    private readonly string _participantConfigurationID;
    private readonly ReadOnlyCollection<IParticipant> _participants;
    private readonly IMutableTypeFactory _mutableTypeFactory;
    private readonly IAssembledTypePreparer _assembledTypePreparer;
    private readonly IAssembledTypeIdentifierProvider _assembledTypeIdentifierProvider;

    public TypeAssembler (
        string participantConfigurationID,
        IEnumerable<IParticipant> participants,
        IMutableTypeFactory mutableTypeFactory,
        IAssembledTypePreparer assembledTypePreparer)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("participantConfigurationID", participantConfigurationID);
      ArgumentUtility.CheckNotNull ("participants", participants);
      ArgumentUtility.CheckNotNull ("mutableTypeFactory", mutableTypeFactory);
      ArgumentUtility.CheckNotNull ("assembledTypePreparer", assembledTypePreparer);

      _participantConfigurationID = participantConfigurationID;
      _participants = participants.ToList().AsReadOnly();
      _mutableTypeFactory = mutableTypeFactory;
      _assembledTypePreparer = assembledTypePreparer;

      _assembledTypeIdentifierProvider = new AssembledTypeIdentifierProvider (_participants);
    }

    public string ParticipantConfigurationID
    {
      get { return _participantConfigurationID; }
    }

    public ReadOnlyCollection<IParticipant> Participants
    {
      get { return _participants; }
    }

    public bool IsAssembledType (Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);

      return type.IsDefined (typeof (AssembledTypeAttribute), inherit: false);
    }

    public Type GetRequestedType (Type assembledType)
    {
      ArgumentUtility.CheckNotNull ("assembledType", assembledType);

      if (!IsAssembledType (assembledType))
        throw new ArgumentException ("The argument type is not an assembled type.", "assembledType");

      return assembledType.BaseType;
    }

    public object[] GetTypeID (Type requestedType)
    {
      // Using Debug.Assert because it will be compiled away.
      Debug.Assert (requestedType != null);

      return _assembledTypeIdentifierProvider.GetIdentifier (requestedType);
    }

    public object[] ExtractTypeID (Type assembledType)
    {
      ArgumentUtility.CheckNotNull ("assembledType", assembledType);

      return (object[]) _assembledTypePreparer.ExtractTypeID (assembledType);
    }

    public Type AssembleType (
        object[] typeID, Type requestedType, IDictionary<string, object> participantState, IMutableTypeBatchCodeGenerator codeGenerator)
    {
      ArgumentUtility.CheckNotNull ("typeID", typeID);
      ArgumentUtility.CheckNotNull ("requestedType", requestedType);
      ArgumentUtility.CheckNotNull ("participantState", participantState);
      ArgumentUtility.CheckNotNull ("codeGenerator", codeGenerator);

      if (!CheckIsSubclassable (requestedType))
        return requestedType;

      var typeModificationTracker = _mutableTypeFactory.CreateProxy (requestedType);
      var typeAssemblyContext = CreateTypeAssemblyContext (typeID, requestedType, typeModificationTracker.Type, participantState);

      foreach (var participant in _participants)
        participant.Participate (null, typeAssemblyContext);

      if (!typeModificationTracker.IsModified())
        return requestedType;

      var generatedTypeContext = GenerateTypes (typeAssemblyContext, codeGenerator);
      typeAssemblyContext.OnGenerationCompleted (generatedTypeContext);

      return generatedTypeContext.GetGeneratedType (typeAssemblyContext.ProxyType);
    }

    private TypeAssemblyContext CreateTypeAssemblyContext (
        object[] typeID, Type requestedType, MutableType proxyType, IDictionary<string, object> participantState)
    {
      var typeIDExpression = _assembledTypeIdentifierProvider.GetIdentifierExpression (typeID);
      return new TypeAssemblyContext (_participantConfigurationID, typeIDExpression, requestedType, proxyType, _mutableTypeFactory, participantState);
    }

    public void RebuildParticipantState (LoadedTypesContext loadedTypesContext)
    {
      ArgumentUtility.CheckNotNull ("loadedTypesContext", loadedTypesContext);

      foreach (var participant in _participants)
        participant.RebuildState (loadedTypesContext);
    }

    private bool CheckIsSubclassable (Type requestedType)
    {
      if (SubclassFilterUtility.IsSubclassable (requestedType))
        return true;

      foreach (var participant in _participants)
        participant.HandleNonSubclassableType (requestedType);

      return false;
    }

    private GeneratedTypeContext GenerateTypes (TypeAssemblyContext context, IMutableTypeBatchCodeGenerator codeGenerator)
    {
      var attribute = new CustomAttributeDeclaration (s_assembledTypeAttributeCtor, new object[0]);
      context.ProxyType.AddCustomAttribute (attribute);

      _assembledTypePreparer.AddTypeID (context.ProxyType, context.TypeID);

      return GenerateTypesWithDiagnostics (context, codeGenerator);
    }

    private GeneratedTypeContext GenerateTypesWithDiagnostics (TypeAssemblyContext context, IMutableTypeBatchCodeGenerator codeGenerator)
    {
      try
      {
        var mutableTypes = context.AdditionalTypes.Concat (context.ProxyType);
        var generatedTypes = codeGenerator.GenerateTypes (mutableTypes);

        return new GeneratedTypeContext (generatedTypes);
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