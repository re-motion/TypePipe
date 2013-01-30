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
using System.Reflection;
using Remotion.Reflection.MemberSignatures;
using Remotion.ServiceLocation;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// Defines an interface for classes providing methods for investigating method overrides.
  /// This is used by <see cref="ProxyType"/>.
  /// </summary>
  [ConcreteImplementation (typeof (RelatedMethodFinder))]
  // TODO review: remove again
  public interface IRelatedMethodFinder
  {
    /// <summary>
    /// Gets the most derived virtual method with the given <paramref name="name"/> and <paramref name="signature"/>, starting from the
    /// given <paramref name="typeToStartSearch"/> (then searching up the type hierarchy). This is the method that would be implicitly overridden
    /// by a <see cref="MethodAttributes.ReuseSlot"/> method with equivalent <paramref name="name"/> and <paramref name="signature"/> in a type 
    /// derived from <paramref name="typeToStartSearch"/>.
    /// </summary>
    /// <param name="name">The method name.</param>
    /// <param name="signature">The method signature.</param>
    /// <param name="typeToStartSearch">The type to start the search from.</param>
    /// <returns>The most derived virtual method matching the given parameters, or <see langword="null"/> if no such method exists.</returns>
    /// <remarks>
    /// The returned <see cref="MethodInfo"/> has its <see cref="MemberInfo.ReflectedType"/> set to its <see cref="MemberInfo.DeclaringType"/>.
    /// </remarks>
    MethodInfo GetMostDerivedVirtualMethod (string name, MethodSignature signature, Type typeToStartSearch);

    /// <summary>
    /// Gets the most derived method that implicitly overrides the given <paramref name="baseDefinition"/> starting from the given
    /// <paramref name="typeToStartSearch"/> (then searching up the type hierarchy).
    /// </summary>
    /// <param name="baseDefinition">The base definition to search overrides for.</param>
    /// <param name="typeToStartSearch">The type to start the search from.</param>
    /// <returns>The most derived override of <paramref name="baseDefinition"/>, or <paramref name="baseDefinition"/> if there is no override.</returns>
    /// <remarks>
    /// The returned <see cref="MethodInfo"/> has its <see cref="MemberInfo.ReflectedType"/> set to its <see cref="MemberInfo.DeclaringType"/>.
    /// </remarks>
    MethodInfo GetMostDerivedOverride (MethodInfo baseDefinition, Type typeToStartSearch);

    /// <summary>
    /// Gets the method directly overridden by the given <paramref name="method"/>. If the method does not override another method (because it is not
    /// <see cref="MethodAttributes.Virtual"/>, <see cref="MethodAttributes.NewSlot"/>, or simply because there is no overridable method in the
    /// base type chain), it returns <see langword="null"/>.
    /// </summary>
    /// <param name="method">The method to get the base method for.</param>
    /// <returns>The directly overridden method, or <see langword="null" /> if no such method exists.</returns>
    /// <remarks>
    /// The returned <see cref="MethodInfo"/> has its <see cref="MemberInfo.ReflectedType"/> set to its <see cref="MemberInfo.DeclaringType"/>.
    /// </remarks>
    MethodInfo GetBaseMethod (MethodInfo method);

    /// <summary>
    /// Determines if <paramref name="baseDefinition"/> is shadowed by one of the methods in <paramref name="shadowingCandidates"/>.
    /// </summary>
    /// <param name="baseDefinition">The base definition which might be shadowed.</param>
    /// <param name="shadowingCandidates">The methods to be considered for determining the shadowing status.</param>
    /// <returns><see langword="true"/> if <paramref name="baseDefinition"/> is shadowed, <see langword="false"/> otherwise.</returns>
    bool IsShadowed(MethodInfo baseDefinition, IEnumerable<MethodInfo> shadowingCandidates);

    /// <summary>
    /// Gets the single <see cref="MutableMethodInfo"/> in <paramref name="overrideCandidates"/> overriding <paramref name="baseDefinition"/> either
    /// implicitly or explicitly. If <paramref name="baseDefinition"/> is not overridden by any method in <paramref name="overrideCandidates"/>,
    /// <see langword="null"/> is returned.
    /// </summary>
    /// <param name="baseDefinition">The base definition to search an override for.</param>
    /// <param name="overrideCandidates">The methods included in the search for an override.</param>
    /// <returns>The directly overridden method, or <see langword="null" /> if no such method exists.</returns>
    MutableMethodInfo GetOverride (MethodInfo baseDefinition, IEnumerable<MutableMethodInfo> overrideCandidates);
  }
}