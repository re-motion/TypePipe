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
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.StrongNaming
{
  /// <summary>
  /// Determines wheter <see cref="Type"/> is strong-named.
  /// </summary>
  public interface IStrongNamedTypeVerifier
  {
    bool IsStrongNamed (Type type);
  }

  public class StrongNamedTypeVerifier : IStrongNamedTypeVerifier
  {
    private readonly IStrongNamedAssemblyVerifier _assemblyVerifier;

    private readonly Dictionary<Assembly, bool> _cache = new Dictionary<Assembly, bool>();

    public StrongNamedTypeVerifier (IStrongNamedAssemblyVerifier assemblyVerifier)
    {
      ArgumentUtility.CheckNotNull ("assemblyVerifier", assemblyVerifier);

      _assemblyVerifier = assemblyVerifier;
    }

    // TODO Review: Make private, move IsStrongNamed (Type) from StrongNameAnalyzer to here.
    public bool IsStrongNamed (Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);

      var assembly = type.Assembly;

      bool isSigned;
      if (!_cache.TryGetValue (assembly, out isSigned))
      {
        isSigned = _assemblyVerifier.IsStrongNamed (assembly);
        _cache.Add (assembly, isSigned);
      }

      return isSigned;
    }
  }
}