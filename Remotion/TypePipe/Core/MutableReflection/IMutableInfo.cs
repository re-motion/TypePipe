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

using System.Collections.ObjectModel;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Defines a common interface for mutable members (e.g. <see cref="MutableFieldInfo"/>, <see cref="MutableMethodInfo"/>, etc.) and
  /// mutable parameters (<see cref="MutableParameterInfo"/>).
  /// </summary>
  public interface IMutableInfo : ITypePipeCustomAttributeProvider
  {
    bool IsNew { get; }
    bool IsModified { get; }

    bool CanAddCustomAttributeData { get; }
    ReadOnlyCollection<CustomAttributeDeclaration> AddedCustomAttributeDeclarations { get; }

    void AddCustomAttribute (CustomAttributeDeclaration customAttributeDeclaration);
  }
}