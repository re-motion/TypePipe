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
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.TypeAssembly.Implementation
{
  /// <inheritdoc />
  public class ProxyTypeAssemblyContext : TypeAssemblyContextBase, IProxyTypeAssemblyContext
  {
    private readonly Type _requestedType;
    private readonly MutableType _proxyType;

    public ProxyTypeAssemblyContext (
        IMutableTypeFactory mutableTypeFactory,
        string participantConfigurationID,
        IParticipantState participantState,
        Type requestedType,
        MutableType proxyType)
        : base (mutableTypeFactory, participantConfigurationID, participantState)
    {
      ArgumentUtility.CheckNotNull ("requestedType", requestedType);
      ArgumentUtility.CheckNotNull ("proxyType", proxyType);

      _requestedType = requestedType;
      _proxyType = proxyType;
    }

    public Type RequestedType
    {
      get { return _requestedType; }
    }

    public MutableType ProxyType
    {
      get { return _proxyType; }
    }
  }
}