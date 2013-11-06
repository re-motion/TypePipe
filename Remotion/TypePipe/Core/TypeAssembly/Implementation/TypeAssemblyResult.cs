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
using Remotion.Collections;
using Remotion.Utilities;

namespace Remotion.TypePipe.TypeAssembly.Implementation
{
  public class TypeAssemblyResult
  {
    // TODO RM-5895: test

    private static readonly IReadOnlyDictionary<object, Type> s_emptyDictionary = new Dictionary<object, Type>().AsReadOnly();

    private readonly Type _type;
    private readonly IReadOnlyDictionary<object, Type> _additionalTypes;

    public TypeAssemblyResult (Type type)
        : this (type, s_emptyDictionary)
    {
    }

    public TypeAssemblyResult (Type type, IReadOnlyDictionary<object, Type> additionalTypes)
    {
      ArgumentUtility.CheckNotNull ("type", type);
      ArgumentUtility.CheckNotNull ("additionalTypes", additionalTypes);

      _type = type;
      _additionalTypes = additionalTypes;
    }

    public Type Type
    {
      get { return _type; }
    }

    public IReadOnlyDictionary<object, Type> AdditionalTypes
    {
      get { return _additionalTypes; }
    }
  }
}