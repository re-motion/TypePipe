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

namespace Remotion.TypePipe.MutableReflection.ReflectionEmit
{
  /// <summary>
  /// Filters members based on their visibility. Only members that are accessibe from a subclass are retained.
  /// </summary>
  public class MemberFilter : IMemberFilter
  {
    public IEnumerable<FieldInfo> FilterFields (IEnumerable<FieldInfo> fieldInfos)
    {
      return fieldInfos.Where (fi => fi.IsPublic || fi.IsFamilyOrAssembly || fi.IsFamily);
    }

    public IEnumerable<ConstructorInfo> FilterConstructors (IEnumerable<ConstructorInfo> constructorInfos)
    {
      return FilterMethodBases (constructorInfos);
    }

    private IEnumerable<T> FilterMethodBases<T> (IEnumerable<T> methodBases)
      where T : MethodBase
    {
      return methodBases.Where (mb => mb.IsPublic || mb.IsFamilyOrAssembly || mb.IsFamily);
    }
  }
}