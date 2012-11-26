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
using System.Reflection;
using Remotion.Utilities;
using Remotion.FunctionalProgramming;
using System.Linq;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Provides useful methods for investigating property overrides.
  /// </summary>
  public class RelatedPropertyFinder : IRelatedPropertyFinder
  {
    public PropertyInfo GetBaseProperty (PropertyInfo property)
    {
      ArgumentUtility.CheckNotNull ("property", property);

      Assertion.IsNotNull (property.DeclaringType);
      var baseTypeSequence = property.DeclaringType.BaseType.CreateSequence (t => t.BaseType);
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
      var baseProperties = from t in baseTypeSequence
                           from p in t.GetProperties (bindingFlags)
                           where IsBaseProperty (p, property)
                           select p;
      return baseProperties.FirstOrDefault();
    }

    private bool IsBaseProperty (PropertyInfo baseCandidateProperty, PropertyInfo property)
    {
      var getter = property.GetGetMethod (true);
      var setter = property.GetSetMethod (true);
      var baseCandidateGetter = baseCandidateProperty.GetGetMethod (true);
      var baseCandidateSetter = baseCandidateProperty.GetSetMethod (true);

      return SafeHasSameBaseDefinition (getter, baseCandidateGetter) || SafeHasSameBaseDefinition (setter, baseCandidateSetter);
    }

    private bool SafeHasSameBaseDefinition (MethodInfo a, MethodInfo b)
    {
      return a != null && b != null && a.GetBaseDefinition() == b.GetBaseDefinition();
    }
  }
}