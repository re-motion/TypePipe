﻿// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using System.Linq;
using System.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.Serialization;
using Remotion.Utilities;

namespace Remotion.TypePipe.TypeAssembly.Implementation
{
  /// <summary>
  /// Provides functionality for assembling a type by orchestrating <see cref="IParticipant"/> instances and an instance of 
  /// <see cref="IMutableTypeBatchCodeGenerator"/>.
  /// Also calculates a compound cache key consisting of the requested type and the individual cache key parts returned from the 
  /// <see cref="ITypeIdentifierProvider"/>. The providers are retrieved from the participants exactly once at object creation.
  /// </summary>
  /// <threadsafety static="true" instance="true"/>
  public class TypeAssembler : ITypeAssembler
  {
    private static readonly ConstructorInfo s_assembledTypeAttributeCtor =
        MemberInfoFromExpressionUtility.GetConstructor (() => new AssembledTypeAttribute());

    private readonly string _participantConfigurationID;
    private readonly IReadOnlyCollection<IParticipant> _participants;
    private readonly IMutableTypeFactory _mutableTypeFactory;
    private readonly IAssembledTypeIdentifierProvider _assembledTypeIdentifierProvider;
    private readonly IComplexSerializationEnabler _complexSerializationEnabler;

    public TypeAssembler (
        string participantConfigurationID,
        IEnumerable<IParticipant> participants,
        IMutableTypeFactory mutableTypeFactory,
        IComplexSerializationEnabler complexSerializationEnabler)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("participantConfigurationID", participantConfigurationID);
      ArgumentUtility.CheckNotNull ("participants", participants);
      ArgumentUtility.CheckNotNull ("mutableTypeFactory", mutableTypeFactory);
      ArgumentUtility.CheckNotNull ("complexSerializationEnabler", complexSerializationEnabler);
      

      _participantConfigurationID = participantConfigurationID;
      _participants = participants.ToList().AsReadOnly();
      _mutableTypeFactory = mutableTypeFactory;
      _complexSerializationEnabler = complexSerializationEnabler;

      _assembledTypeIdentifierProvider = new AssembledTypeIdentifierProvider (_participants);
    }

    public string ParticipantConfigurationID
    {
      get { return _participantConfigurationID; }
    }

    public IReadOnlyCollection<IParticipant> Participants
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
      CheckIsAssembledType (assembledType);

      return assembledType.BaseType;
    }

    public AssembledTypeID ComputeTypeID (Type requestedType)
    {
      ArgumentUtility.DebugCheckNotNull ("requestedType", requestedType);

      return _assembledTypeIdentifierProvider.ComputeTypeID (requestedType);
    }

    public AssembledTypeID ExtractTypeID (Type assembledType)
    {
      ArgumentUtility.CheckNotNull ("assembledType", assembledType);
      CheckIsAssembledType (assembledType);

      return _assembledTypeIdentifierProvider.ExtractTypeID (assembledType);
    }

    public TypeAssemblyResult AssembleType (AssembledTypeID typeID, IParticipantState participantState, IMutableTypeBatchCodeGenerator codeGenerator)
    {
      ArgumentUtility.CheckNotNull ("typeID", typeID);
      ArgumentUtility.CheckNotNull ("participantState", participantState);
      ArgumentUtility.CheckNotNull ("codeGenerator", codeGenerator);

      var requestedType = typeID.RequestedType;
      CheckRequestedType (requestedType);

      if (ShortCircuitTypeAssembly (requestedType))
        return new TypeAssemblyResult (requestedType);

      var typeModificationTracker = _mutableTypeFactory.CreateProxy (requestedType, ProxyKind.AssembledType);
      var context = new ProxyTypeAssemblyContext (
          _mutableTypeFactory, _participantConfigurationID, participantState, requestedType, typeModificationTracker.Type);

      foreach (var participant in _participants)
      {
        var idPart = _assembledTypeIdentifierProvider.GetPart (typeID, participant);
        participant.Participate (idPart, context);
      }

      if (!typeModificationTracker.IsModified())
        return new TypeAssemblyResult (requestedType);

      var generatedTypesContext = GenerateTypes (typeID, context, codeGenerator);
      context.OnGenerationCompleted (generatedTypesContext);

      return new TypeAssemblyResult (
          generatedTypesContext.GetGeneratedType (context.ProxyType),
          context.AdditionalTypes.ToDictionary (kvp => kvp.Key, kvp => generatedTypesContext.GetGeneratedType (kvp.Value)));
    }

    public TypeAssemblyResult AssembleAdditionalType (
        object additionalTypeID,
        IParticipantState participantState,
        IMutableTypeBatchCodeGenerator codeGenerator)
    {
      ArgumentUtility.CheckNotNull ("additionalTypeID", additionalTypeID);
      ArgumentUtility.CheckNotNull ("participantState", participantState);
      ArgumentUtility.CheckNotNull ("codeGenerator", codeGenerator);

      var context = new AdditionalTypeAssemblyContext (_mutableTypeFactory, _participantConfigurationID, participantState);
      var additionalType = _participants.Select (p => p.GetOrCreateAdditionalType (additionalTypeID, context)).FirstOrDefault (t => t != null);
      if (additionalType == null)
        throw new NotSupportedException ("No participant provided an additional type for the given identifier.");

      var generatedTypesContext = GenerateTypesWithDiagnostics (codeGenerator, context.AdditionalTypes.Values, additionalTypeID.ToString());
      context.OnGenerationCompleted (generatedTypesContext);

      Type resultType;
      if (additionalType is MutableType)
        resultType = generatedTypesContext.GetGeneratedType ((MutableType) additionalType);
      else
        resultType = additionalType;

      return new TypeAssemblyResult (
          resultType,
          context.AdditionalTypes.ToDictionary (kvp => kvp.Key, kvp => generatedTypesContext.GetGeneratedType (kvp.Value)));
    }

    public object GetAdditionalTypeID (Type additionalType)
    {
      ArgumentUtility.CheckNotNull ("additionalType", additionalType);

      var ids = _participants.Select (p => p.GetAdditionalTypeID (additionalType)).Where (t => t != null).ToList();
      if (ids.Count > 1)
      {
        throw new InvalidOperationException (
            string.Format ("More than one participant returned an ID for the additional type '{0}'", additionalType.Name));
      }

      return ids.SingleOrDefault();
    }

    private void CheckRequestedType (Type requestedType)
    {
      if (IsAssembledType (requestedType))
      {
        var message = string.Format ("The provided requested type '{0}' is already an assembled type.", requestedType.Name);
        throw new ArgumentException (message);
      }
    }

    private bool ShortCircuitTypeAssembly (Type requestedType)
    {
      var isNonSubclassable = !SubclassFilterUtility.IsSubclassable (requestedType);
      if (isNonSubclassable)
      {
        foreach (var participant in _participants)
          participant.HandleNonSubclassableType (requestedType);
      }

      return isNonSubclassable;
    }

    private GeneratedTypesContext GenerateTypes (AssembledTypeID typeID, ProxyTypeAssemblyContext context, IMutableTypeBatchCodeGenerator codeGenerator)
    {
      // Add [AssembledType] attribute.
      var attribute = new CustomAttributeDeclaration (s_assembledTypeAttributeCtor, new object[0]);
      context.ProxyType.AddCustomAttribute (attribute);

      // Add '__typeID' and initialization code.
      _assembledTypeIdentifierProvider.AddTypeID (context.ProxyType, typeID);

      // Enable complex serialization.
      _complexSerializationEnabler.MakeSerializable (context.ProxyType, _participantConfigurationID,_assembledTypeIdentifierProvider, typeID);

      var mutableTypes = context.AdditionalTypes.Values.Concat (new[] { context.ProxyType });
      return GenerateTypesWithDiagnostics (codeGenerator, mutableTypes, context.RequestedType.Name);
    }

    private GeneratedTypesContext GenerateTypesWithDiagnostics (
        IMutableTypeBatchCodeGenerator codeGenerator, IEnumerable<MutableType> mutableTypes, string generationSubjectName)
    {
      try
      {
        var generatedTypes = codeGenerator.GenerateTypes (mutableTypes);
        return new GeneratedTypesContext (generatedTypes);
      }
      catch (InvalidOperationException ex)
      {
        throw new InvalidOperationException (BuildExceptionMessage (generationSubjectName, ex), ex);
      }
      catch (NotSupportedException ex)
      {
        throw new NotSupportedException (BuildExceptionMessage (generationSubjectName, ex), ex);
      }
    }

    private string BuildExceptionMessage (string generationSubjectName, Exception exception)
    {
      var participantList = string.Join (", ", _participants.Select (p => "'" + p.GetType().Name + "'"));
      return string.Format (
          "An error occurred during code generation for '{0}':{1}{2}{3}"
          + "The following participants are currently configured and may have caused the error: {4}.",
          generationSubjectName,
          Environment.NewLine,
          exception.Message,
          Environment.NewLine,
          participantList);
    }

    private void CheckIsAssembledType (Type assembledType)
    {
      if (!IsAssembledType (assembledType))
      {
        var message = string.Format ("The argument type '{0}' is not an assembled type.", assembledType.Name);
        throw new ArgumentException (message, "assembledType");
      }
    }
  }
}