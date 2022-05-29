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
using Remotion.Utilities;

// ReSharper disable once CheckNamespace
namespace Remotion.Development.UnitTesting.PEVerifyPathSources
{
  partial class CompoundPEVerifyPathSource : IPEVerifyPathSource
  {
    private readonly IPEVerifyPathSource[] _sources;

    public CompoundPEVerifyPathSource (params IPEVerifyPathSource[] sources)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("sources", sources);
      _sources = sources;
    }

    public string GetPEVerifyPath (PEVerifyVersion version)
    {
      return _sources.Select (source => source.GetPEVerifyPath (version)).FirstOrDefault (path => path != null);
    }

    public string GetLookupDiagnostics (PEVerifyVersion version)
    {
      return string.Join (Environment.NewLine, _sources.Select (source => source.GetLookupDiagnostics (version)));
    }
  }
}