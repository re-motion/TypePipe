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
using System.Linq;
using Remotion.Collections;
using Remotion.Utilities;

namespace Remotion.TypePipe.StrongNaming
{
  public class StrongNameTypeVerifier : IStrongNameTypeVerifier
  {
    private readonly IDataStore<Type, bool> _cache = DataStoreFactory.Create<Type, bool> (new ReferenceEqualityComparer<Type>());

    private readonly IStrongNameAssemblyVerifier _assemblyVerifier;

    public StrongNameTypeVerifier (IStrongNameAssemblyVerifier assemblyVerifier)
    {
      ArgumentUtility.CheckNotNull ("assemblyVerifier", assemblyVerifier);

      _assemblyVerifier = assemblyVerifier;
    }

    public bool IsStrongNamed (Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);

      return _cache.GetOrCreateValue (type, CalculateIsStrongNamed);
    }

    public void SetStrongNamed (Type type, bool isStrongNamed)
    {
      ArgumentUtility.CheckNotNull ("type", type);

      _cache[type] = isStrongNamed;
    }

    private bool CalculateIsStrongNamed (Type type)
    {
      return _assemblyVerifier.IsStrongNamed (type.Assembly) && type.GetGenericArguments().All (IsStrongNamed);
    }
  }
}