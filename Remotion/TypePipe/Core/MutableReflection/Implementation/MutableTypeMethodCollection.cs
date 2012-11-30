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
using System.Linq;
using System.Reflection;
using Remotion.Collections;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// A container storing methods for <see cref="MutableType"/>. See also <see cref="MutableTypeMemberCollection{TMemberInfo,TMutableMemberInfo}"/>.
  /// </summary>
  public class MutableTypeMethodCollection : MutableTypeMemberCollection<MethodInfo, MutableMethodInfo>
  {
    public MutableTypeMethodCollection (
        MutableType declaringType, IEnumerable<MethodInfo> existingMembers, Func<MethodInfo, MutableMethodInfo> mutableMemberProvider)
        : base (declaringType, existingMembers, mutableMemberProvider)
    {
    }

    protected override IEnumerable<MethodInfo> FilterBaseMembers (IEnumerable<MethodInfo> baseMembers)
    {
      var overridenBaseDefinitions = new Set<MethodInfo> (AddedMembers.Select (mi => mi.GetBaseDefinition()));
      return baseMembers.Where (m => !overridenBaseDefinitions.Contains (m.GetBaseDefinition()));
    }
  }
}