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

namespace Remotion.TypePipe.CodeGeneration
{
  /// <summary>
  /// This context allows <see cref="IParticipant"/>s to specify their code generation needs.
  /// Holds the <see cref="RequestedType"/> and <see cref="ProxyType"/> and allows generation of additional types.
  /// </summary>
  /// <remarks>
  /// The <see cref="ProxyType"/> represents the proxy type to be generated for the <see cref="RequestedType"/> including the modifications
  /// applied by preceding participants.
  /// Its mutating members (e.g. <see cref="MutableType.AddMethod"/>) can be used to specify the needed modifications.
  /// </remarks>
  public interface IProxyTypeAssemblyContext : ITypeAssemblyContext
  {
    /// <summary>
    /// The original <see cref="Type"/> that was requested by the user through an instance of <see cref="IPipeline"/>.
    /// </summary>
    Type RequestedType { get; }

    /// <summary>
    /// The mutable proxy type that was created by the pipeline for the <see cref="RequestedType"/>.
    /// </summary>
    MutableType ProxyType { get; }
  }
}