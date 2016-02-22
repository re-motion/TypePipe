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
using System.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.TypeAssembly.Implementation
{
  /// <summary>
  /// A base class for <see cref="ITypeAssemblyContext"/> implementers that provides the possibility to raise
  /// the <see cref="GenerationCompleted"/> event.
  /// </summary>
  public abstract class TypeAssemblyContextBase : ITypeAssemblyContext
  {
    private readonly IMutableTypeFactory _mutableTypeFactory;
    private readonly string _participantConfigurationID;
    private readonly IParticipantState _participantState;
    private readonly Dictionary<object, MutableType> _additionalTypes = new Dictionary<object, MutableType>();

    protected TypeAssemblyContextBase (IMutableTypeFactory mutableTypeFactory, string participantConfigurationID, IParticipantState participantState)
    {
      ArgumentUtility.CheckNotNull ("mutableTypeFactory", mutableTypeFactory);
      ArgumentUtility.CheckNotNullOrEmpty ("participantConfigurationID", participantConfigurationID);
      ArgumentUtility.CheckNotNull ("participantState", participantState);

      _mutableTypeFactory = mutableTypeFactory;
      _participantConfigurationID = participantConfigurationID;
      _participantState = participantState;
    }

    public event Action<GeneratedTypesContext> GenerationCompleted;

    public string ParticipantConfigurationID
    {
      get { return _participantConfigurationID; }
    }

    public IParticipantState ParticipantState
    {
      get { return _participantState; }
    }

    public IReadOnlyDictionary<object, MutableType> AdditionalTypes
    {
      get { return new ReadOnlyDictionary<object, MutableType> (_additionalTypes); }
    }

    public MutableType CreateAdditionalType (object additionalTypeID, string name, string @namespace, TypeAttributes attributes, Type baseType)
    {
      ArgumentUtility.CheckNotNull ("additionalTypeID", additionalTypeID);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      // Namespace may be null.
      // Base type may be null (for interfaces).

      var type = _mutableTypeFactory.CreateType (name, @namespace, attributes, baseType, null);
      _additionalTypes.Add (additionalTypeID, type);

      return type;
    }

    public MutableType CreateAddtionalProxyType (object additionalTypeID, Type baseType)
    {
      ArgumentUtility.CheckNotNull ("baseType", baseType);

      var type = _mutableTypeFactory.CreateProxy (baseType, ProxyKind.AdditionalType).Type;
      _additionalTypes.Add (additionalTypeID, type);

      return type;
    }

    public void OnGenerationCompleted (GeneratedTypesContext generatedTypesContext)
    {
      var handler = GenerationCompleted;
      if (handler != null)
        handler (generatedTypesContext);
    }
  }
}