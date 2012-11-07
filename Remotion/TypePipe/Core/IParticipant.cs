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
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe
{
  /// <summary>
  /// Participates in the assembly of a type by calling mutating members of <see cref="MutableType"/>.
  /// Framework authors implement this interface in order to specify their code generation needs.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Instances of this interface can be configured in the pipeline.
  /// Every participant has the chance the specify type modifications via the provided <see cref="MutableType"/> instance.
  /// The <see cref="MutableType"/> is a representation of the type to be generated for the requested type.
  /// In addition, it contains all modifications applied by preceding participants in the pipeline.
  /// </para>
  /// <para>
  /// A participant must also provide an instance of <see cref="ICacheKeyProvider"/> if generated types cannot be cached unconditionally, i.e.,
  /// the modifications depend solely on the requested type.
  /// If generated types can be cached unconditionally, the participant should return <see langword="null"/> from <see cref="GetCacheKeyProvider"/>.
  /// </para>
  /// </remarks>
  public interface IParticipant
  {
    ICacheKeyProvider GetCacheKeyProvider ();

    void ModifyType (MutableType mutableType);
  }
}