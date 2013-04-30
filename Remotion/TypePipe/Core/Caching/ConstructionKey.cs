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
using System.Diagnostics;
using Remotion.Utilities;

namespace Remotion.TypePipe.Caching
{
  /// <summary>
  /// A data structure that can be used as a key for constructor delegates.
  /// </summary>
  public struct ConstructionKey : IEquatable<ConstructionKey>
  {
    private static readonly CompoundIdentifierEqualityComparer s_typeKeyComparer = new CompoundIdentifierEqualityComparer();

    private readonly object[] _typeID;
    private readonly Type _delegateType;
    private readonly bool _allowNonPublic;

    private readonly int _hashCode;

    public ConstructionKey (object[] typeID, Type delegateType, bool allowNonPublic)
    {
      // Using Debug.Assert because it will be compiled away.
      Debug.Assert (typeID != null);
      Debug.Assert (delegateType != null);

      _typeID = typeID;
      _delegateType = delegateType;
      _allowNonPublic = allowNonPublic;

      // Pre-compute hash code.
      _hashCode = EqualityUtility.GetRotatedHashCode (s_typeKeyComparer.GetHashCode (typeID), delegateType, allowNonPublic);
    }

    public object[] TypeID
    {
      get { return _typeID; }
    }

    public Type DelegateType
    {
      get { return _delegateType; }
    }

    public bool AllowNonPublic
    {
      get { return _allowNonPublic; }
    }

    public bool Equals (ConstructionKey other)
    {
      return s_typeKeyComparer.Equals (_typeID, other._typeID)
             && _delegateType == other._delegateType
             && _allowNonPublic == other._allowNonPublic;
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