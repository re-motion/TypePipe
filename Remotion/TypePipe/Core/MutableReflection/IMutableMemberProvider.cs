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

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// An interfaces that provides mutable members for standard reflection objects.
  /// </summary>
  /// <typeparam name="TMemberInfo">The type of the member info.</typeparam>
  /// <typeparam name="TMutableMemberInfo">The type of the mutable member info.</typeparam>
  public interface IMutableMemberProvider<in TMemberInfo, out TMutableMemberInfo>
      where TMemberInfo : MemberInfo
      where TMutableMemberInfo : TMemberInfo
  {
    /// <summary>
    /// Gets the mutable version for a standard reflection object.
    /// Note that no object is created if the mutable version does not exist yet.
    /// </summary>
    /// <param name="member">The member.</param>
    /// <returns>The mutable version of the member, or <see langword="null"/>.</returns>
    TMutableMemberInfo GetMutableMember (TMemberInfo member);
  }
}