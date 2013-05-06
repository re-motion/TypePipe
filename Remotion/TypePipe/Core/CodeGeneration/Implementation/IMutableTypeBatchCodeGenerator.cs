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
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.CodeGeneration.Implementation
{
  /// <summary>
  /// Defines an interface for classes generating types from a <see cref="ProxyTypeAssemblyContext"/>.
  /// </summary>
  public interface IMutableTypeBatchCodeGenerator
  {
    /// <summary>
    /// Generates <see cref="Type"/>s for a batch of <see cref="MutableType"/> instances based on the data specified by the participants.
    /// </summary>
    /// <remarks>This method may throw instances of <see cref="InvalidOperationException"/> and <see cref="NotSupportedException"/>.</remarks>
    /// <exception cref="InvalidOperationException">A requested operation is invalid with this configuration (user configuration or participants).</exception>
    /// <exception cref="NotSupportedException">A requested operation is not supported by the code generator.</exception>
    IEnumerable<KeyValuePair<MutableType, Type>> GenerateTypes (IEnumerable<MutableType> mutableTypes);
  }
}