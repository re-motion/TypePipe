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

namespace Remotion.TypePipe.StrongNaming
{
  /// <summary>
  /// Specifies the compatibility of requested type modifications by a <see cref="IParticipant"/> with strong-naming.
  /// </summary>
  public enum StrongNameCompatibility
  {
    /// <summary>
    /// The requested modifications are compatible with strong-naming, i.e., only types from strong-named assemblies are used for modifications.
    /// </summary>
    Compatible,

    /// <summary>
    /// The requested modifications are not compatible with strong-naming, i.e., modifications include types from unsigned assemblies.
    /// </summary>
    Incompatible,

    /// <summary>
    /// The participant is unsure if the requested modifications are strong-name compatible.
    /// Note that this forces the pipeline to check strong-name compatibility which results in poorer performance if strong-naming is enabled.
    /// </summary>
    Unknown
  }
}