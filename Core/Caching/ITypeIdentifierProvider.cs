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
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.Serialization;

namespace Remotion.TypePipe.Caching
{
  /// <summary>
  /// This interface provides an identifier for a <see cref="IParticipant"/> from the requested or generated type.
  /// Implementations should return non-equal identifiers if the participant applies different modifications to the <see cref="MutableType"/>.
  /// This might depend on participant configuation, context and user options.
  /// However, a identifier should not encode the requested type itself, as this is already handled by the pipeline caching facilities.
  /// </summary>
  /// <remarks>
  /// This interface must be implemented if the generated types cannot be cached unconditionally.
  /// If the generated types can be cached unconditionally <see cref="IParticipant.PartialTypeIdentifierProvider"/> should return <see langword="null"/>.
  /// Furthermore, the returned identifier must be serializable if the <see cref="ComplexSerializationEnabler"/> is used.
  /// </remarks>
  public interface ITypeIdentifierProvider
  {
    /// <summary>
    /// Gets an identifier used to identify the assembled type for the provided requested type.
    /// The implementation of this method should be fast as it is invoked everytime the pipeline creates an object.
    /// </summary>
    /// <remarks>
    /// The identifier should include the configuration of this <see cref="IParticipant"/> and other data that might influence the modifications
    /// specified by the <see cref="IParticipant"/>.
    /// Implementations should not encode the requested type itself, as this is already handled by the pipeline.
    /// </remarks>
    /// <param name="requestedType">The requested type.</param>
    /// <returns>
    /// An identifier, or <see langword="null"/> if no specific caching information is required for the <paramref name="requestedType"/>.
    /// </returns>
    object GetID (Type requestedType);

    /// <summary>
    /// Gets an expression that re-builds the specified identifier in code.
    /// </summary>
    /// <param name="id">An identifier previously returned by <see cref="GetID"/>.</param>
    /// <returns>An expression that builds an identifier equivalent to <paramref name="id"/>.</returns>
    /// <remarks>This method is not called if <see cref="GetID"/> returned <see langword="null"/>.</remarks>
    Expression GetExpression (object id);

    /// <summary>
    /// Gets an expression of type <see cref="IFlatValue"/> that re-builds a flattened serializable representation of the specified identifier
    /// in code. On deserialization <see cref="IFlatValue.GetRealValue"/> is called on the deserialized flat value to create an identifier that is
    /// equivalent to the original identifier.
    /// </summary>
    /// <param name="id">An identifier previously returned by <see cref="GetID"/>.</param>
    /// <returns>An expression of type <see cref="IFlatValue"/> that builds a flattened serializable representation of <paramref name="id"/>.</returns>
    /// <remarks>
    /// <para>
    /// This method is only called when using the complex serialization strategy and <see cref="GetID"/> did not return <see langword="null"/>.
    /// </para>
    /// <para>
    /// A "flattened" value is a value that is immediately deserializable in one step. It must not contain any cycles or objects
    /// that implement <see cref="IObjectReference"/>, such as <see cref="Type"/>, <see cref="MethodInfo"/>, etc. Instead of such objects, include
    /// some simple identifier in the flattened value, e.g., <see cref="Type.AssemblyQualifiedName"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="IFlatValue"/>
    /// <seealso cref="PipelineSettings.EnableSerializationWithoutAssemblySaving"/>
    Expression GetFlatValueExpressionForSerialization (object id);
  }
}