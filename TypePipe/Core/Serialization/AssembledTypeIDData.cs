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
using Remotion.TypePipe.Caching;
using Remotion.Utilities;

namespace Remotion.TypePipe.Serialization
{
  /// <summary>
  /// A data container for serialization that holds flattened serializable data from an <see cref="AssembledTypeID"/>.
  /// </summary>
  [Serializable]
  public class AssembledTypeIDData
  {
    private readonly string _requestedTypeAssemblyQualifiedName;
    private readonly IFlatValue[] _flattenedSerializableIDParts;

    public AssembledTypeIDData (string requestedTypeAssemblyQualifiedName, IFlatValue[] flattenedSerializableIDParts)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("requestedTypeAssemblyQualifiedName", requestedTypeAssemblyQualifiedName);
      ArgumentUtility.CheckNotNull ("flattenedSerializableIDParts", flattenedSerializableIDParts);

      _requestedTypeAssemblyQualifiedName = requestedTypeAssemblyQualifiedName;
      _flattenedSerializableIDParts = flattenedSerializableIDParts;
    }

    public string RequestedTypeAssemblyQualifiedName
    {
      get { return _requestedTypeAssemblyQualifiedName; }
    }

    public IFlatValue[] FlattenedSerializableIDParts
    {
      get { return _flattenedSerializableIDParts; }
    }

    public AssembledTypeID CreateTypeID ()
    {
      var requestedType = Type.GetType (_requestedTypeAssemblyQualifiedName, throwOnError: true);
      var parts = _flattenedSerializableIDParts.Select (v => v != null ? v.GetRealValue () : null).ToArray ();

      return new AssembledTypeID (requestedType, parts);
    }
  }
}