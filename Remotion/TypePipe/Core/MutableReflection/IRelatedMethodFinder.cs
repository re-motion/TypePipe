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
using Remotion.Reflection.MemberSignatures;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Defines an interface for classes providing methods for investigating method overrides.
  /// </summary>
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
    /// <returns>The most derived virtual method matching the given parameters.</returns>
    /// <remarks>
    /// The returned <see cref="MethodInfo"/> has its <see cref="MemberInfo.ReflectedType"/> set to its <see cref="MemberInfo.DeclaringType"/>.
    /// </remarks>
    MethodInfo GetMostDerivedVirtualMethod (string name, MethodSignature signature, Type typeToStartSearch);

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
  }
}