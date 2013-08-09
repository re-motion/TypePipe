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
using System.Reflection;
using System.Runtime.CompilerServices;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// A data structure that is used as a key for caching <see cref="ICustomAttributeData"/> intances.
  /// </summary>
  /// <remarks>
  /// Note that the implementation of this struct is critical for performance.
  /// </remarks>
  public struct CustomAttributeDataCacheKey : IEquatable<CustomAttributeDataCacheKey>
  {
    private readonly MemberInfo _member;
    private readonly bool _inherit;

    private readonly int _hashCode;

    public CustomAttributeDataCacheKey (MemberInfo member, bool inherit)
    {
      // Using Assertion.DebugAssert because it will be compiled away.
      Assertion.DebugAssert (member != null);
      Assertion.DebugAssert (!(member is IMutableMember));

      _member = member;
      _inherit = inherit;

      // Pre-compute hash code.
      _hashCode = EqualityUtility.GetRotatedHashCode (RuntimeHelpers.GetHashCode (member), inherit);
    }

    public bool Equals (CustomAttributeDataCacheKey other)
    {
      return object.ReferenceEquals (_member, other._member)
             && _inherit == other._inherit;
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