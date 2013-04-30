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
using System.Diagnostics;
using Remotion.Utilities;

namespace Remotion.TypePipe.Caching
{
  /// <summary>
  /// A data structure that identifies an assembled type.
  /// </summary>
  /// <remarks>
  /// Note that the length of the id parts (object array) is assumed to be equal.
  /// </remarks>
  [Serializable]
  public struct AssembledTypeID : IEquatable<AssembledTypeID>
  {
    private readonly Type _requestedType;
    private readonly object[] _parts;
    private readonly int _hashCode;

    public AssembledTypeID (Type requestedType, object[] parts)
    {
      // Using Debug.Assert because it will be compiled away.
      Debug.Assert (requestedType != null);
      Debug.Assert (parts != null);

      _requestedType = requestedType;
      _parts = parts;

      // Pre-compute hash code.
      _hashCode = requestedType.GetHashCode() ^ EqualityUtility.GetRotatedHashCode (_parts);
    }

    public Type RequestedType
    {
      get { return _requestedType; }
    }

    public object[] Parts
    {
      get { return _parts; }
    }

    public bool Equals (AssembledTypeID other)
    {
      // Using Debug.Assert because it will be compiled away.
      Debug.Assert (_parts.Length == other._parts.Length);

      if (_requestedType != other.RequestedType)
        return false;

      // ReSharper disable LoopCanBeConvertedToQuery // No LINQ for performance reasons.
      for (int i = 0; i < _parts.Length; ++i)
      {
        if (!Equals (_parts[i], other._parts[i]))
          return false;
      }

      return true;
    }

    public override bool Equals (object obj)
    {
      throw new NotSupportedException ("Should not be used for performance reasons.");
    }

    public override int GetHashCode ()
    {
      return _hashCode;
    }
  }
}