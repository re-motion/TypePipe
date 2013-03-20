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
using Remotion.Utilities;
using System.Linq;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// A context that holds the completed types after code generation. This class can be used to retrieve a generated <see cref="MemberInfo"/>
  /// with the corresponding <see cref="IMutableMember"/>.
  /// </summary>
  public class GeneratedTypeContext
  {
    private readonly Type _requestedType;
    private readonly Type _proxyType;
    private readonly ReadOnlyCollection<Type> _additionalTypes;

    public GeneratedTypeContext (Type requestedType, Type proxyType, IEnumerable<Type> additionalTypes)
    {
      ArgumentUtility.CheckNotNull ("requestedType", requestedType);
      ArgumentUtility.CheckNotNull ("proxyType", proxyType);
      ArgumentUtility.CheckNotNull ("additionalTypes", additionalTypes);

      _requestedType = requestedType;
      _proxyType = proxyType;
      _additionalTypes = additionalTypes.ToList().AsReadOnly();
    }

    public Type RequestedType
    {
      get { return _requestedType; }
    }

    public Type ProxyType
    {
      get { return _proxyType; }
    }

    public ReadOnlyCollection<Type> AdditionalTypes
    {
      get { return _additionalTypes; }
    }

    public MemberInfo GetGeneratedMember (IMutableMember mutableMember)
    {
      ArgumentUtility.CheckNotNull ("mutableMember", mutableMember);

      return null;
    }
  }
}