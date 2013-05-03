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
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Configuration;

namespace Remotion.TypePipe.Serialization
{
  // TODO 5552
  ///<remarks>
  /// <para>
  /// See <see cref="ITypeIdentifierProvider.GetFlatValueExpressionForSerialization"/> for a description on what a "flattened" value is. If 
  /// <see cref="ITypeIdentifierProvider.GetFlatValueExpressionForSerialization"/> returned an invalid value (e.g., one that contains cycles or <see cref="Type"/> members),
  /// the "ITypeIdentifierProvider.DeserializeFlattenedID" might encounter <see langword="null" /> values where none are expected.
  /// </para>
  /// </remarks>
  /// <seealso cref="PipelineSettings.EnableSerializationWithoutAssemblySaving"/>
  public interface IFlatValue
  {
    // TODO 5552
    object GetRealValue ();
  }
}