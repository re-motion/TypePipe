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
using System.Runtime.Serialization;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Configuration;

namespace Remotion.TypePipe.Serialization
{
  /// <summary>
  /// Represents a flat value that knows how to re-construct its real value. Participants must return serializable expressions of this type from
  /// <see cref="ITypeIdentifierProvider.GetFlatValueExpressionForSerialization"/> in order to allow serialization of instances without the need of
  /// saving the generated assembly to disk.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A "flattened" value is a value that is immediately deserializable in one step. It must not contain any cycles or objects
  /// that implement <see cref="IObjectReference"/>, such as <see cref="Type"/>, <see cref="MethodInfo"/>, etc. Instead of such objects, include
  /// some simple identifier in the flattened value, e.g., <see cref="Type.AssemblyQualifiedName"/>.
  /// </para>
  /// <para>
  /// If <see cref="ITypeIdentifierProvider.GetFlatValueExpressionForSerialization"/> returned an invalid value (e.g., one that contains cycles
  /// or <see cref="Type"/> members), the method <see cref="GetRealValue"/> might encounter <see langword="null"/> values where none are expected.
  /// </para>
  /// </remarks>
  /// <seealso cref="ITypeIdentifierProvider.GetFlatValueExpressionForSerialization"/>
  /// <seealso cref="PipelineSettings.EnableSerializationWithoutAssemblySaving"/>
  public interface IFlatValue
  {
    /// <summary>
    /// Re-constructs the real value from its flat representation.
    /// </summary>
    /// <returns>The real value.</returns>
    object GetRealValue ();
  }
}