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
using System.Linq;
using System.Reflection;
using Remotion.FunctionalProgramming;
using Remotion.Reflection.MemberSignatures;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// Provides useful methods for investigating method overrides.
  /// </summary>
  public class RelatedMethodFinder : IRelatedMethodFinder
  {
    /// <inheritdoc />
    public MethodInfo GetMostDerivedVirtualMethod (string name, MethodSignature signature, Type typeToStartSearch)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("signature", signature);
      ArgumentUtility.CheckNotNull ("typeToStartSearch", typeToStartSearch);

      Func<MethodInfo, bool> predicate = m => m.IsVirtual && m.Name == name && MethodSignature.Create (m).Equals (signature);
      return FirstOrDefaultFromOrderedBaseMethods (typeToStartSearch, predicate);
    }

    /// <inheritdoc />
    public MethodInfo GetMostDerivedOverride (MethodInfo baseDefinition, Type typeToStartSearch)
    {
      ArgumentUtility.CheckNotNull ("baseDefinition", baseDefinition);
      ArgumentUtility.CheckNotNull ("typeToStartSearch", typeToStartSearch);
      Assertion.IsTrue (baseDefinition == baseDefinition.GetBaseDefinition());

      Func<MethodInfo, bool> predicate = m => m.GetBaseDefinition().Equals (baseDefinition);
      return FirstOrDefaultFromOrderedBaseMethods (typeToStartSearch, predicate);
    }

    /// <inheritdoc />
    public MethodInfo GetBaseMethod (MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);
      Assertion.IsNotNull (method.DeclaringType);

      var baseDefinition = method.GetBaseDefinition();
      if (baseDefinition.DeclaringType.BaseType == null)
        return null;

      return GetMostDerivedOverride (baseDefinition, method.DeclaringType.BaseType);
    }

    /// <inheritdoc />
    public bool IsShadowed(MethodInfo baseDefinition, IEnumerable<MethodInfo> shadowingCandidates)
    {
      ArgumentUtility.CheckNotNull ("baseDefinition", baseDefinition);
      ArgumentUtility.CheckNotNull ("shadowingCandidates", shadowingCandidates);
      Assertion.IsTrue (baseDefinition == baseDefinition.GetBaseDefinition ());

      return shadowingCandidates.Any (
          m => m.Name == baseDefinition.Name
               && MethodSignature.AreEqual (m, baseDefinition)
               && baseDefinition.DeclaringType.IsAssignableFromFast (m.DeclaringType.BaseType)
               && m.GetBaseDefinition() != baseDefinition);
    }

    /// <inheritdoc />
    public MutableMethodInfo GetOverride (MethodInfo baseDefinition, IEnumerable<MutableMethodInfo> overrideCandidates)
    {
      ArgumentUtility.CheckNotNull ("baseDefinition", baseDefinition);
      ArgumentUtility.CheckNotNull ("overrideCandidates", overrideCandidates);
      Assertion.IsTrue (baseDefinition == baseDefinition.GetBaseDefinition());

      return overrideCandidates.SingleOrDefault (
          m => m.GetBaseDefinition().Equals (baseDefinition)
               || m.AddedExplicitBaseDefinitions.Contains (baseDefinition));
    }

    private MethodInfo FirstOrDefaultFromOrderedBaseMethods (Type typeToStartSearch, Func<MethodInfo, bool> predicate)
    {
      var baseTypeSequence = typeToStartSearch.CreateSequence (t => t.BaseType);
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

      return baseTypeSequence.SelectMany (type => type.GetMethods (bindingFlags)).FirstOrDefault (predicate);
    }
  }
}