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
using Remotion.Text;
using Remotion.Utilities;
using System.Linq;
using Remotion.Collections;
using Remotion.FunctionalProgramming;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// Implements <see cref="IInterfaceMappingComputer"/>, computing <see cref="InterfaceMapping"/> instances for a <see cref="MutableType"/>.
  /// </summary>
  public class InterfaceMappingComputer : IInterfaceMappingComputer
  {
    public InterfaceMapping ComputeMapping (
        MutableType mutableType, Func<Type, InterfaceMapping> interfacMappingProvider, Type interfaceType, bool allowPartialInterfaceMapping)
    {
      ArgumentUtility.CheckNotNull ("mutableType", mutableType);
      ArgumentUtility.CheckNotNull ("interfacMappingProvider", interfacMappingProvider);
      ArgumentUtility.CheckNotNull ("interfaceType", interfaceType);

      if (!interfaceType.IsInterface)
        throw new ArgumentException ("Type passed must be an interface.", "interfaceType");
      if (!mutableType.GetInterfaces ().Contains (interfaceType))
        throw new ArgumentException ("Interface not found.", "interfaceType");

      var mapping = mutableType.AddedInterfaces.Contains (interfaceType)
                        ? CreateForAdded (mutableType, interfaceType)
                        : CreateForExisting (mutableType, interfacMappingProvider, interfaceType);

      var targetMethods = mapping.TargetMethods;
      var interfaceMethods = mapping.InterfaceMethods;

      // Explicit implementations overrule implicit implementations.
      var explicitImplementations = mutableType.AddedMethods
          .SelectMany (m => m.AddedExplicitBaseDefinitions.Select (b => new { Base = b, Override = m }))
          .ToDictionary (t => t.Base, t => (MethodInfo) t.Override);

      for (int i = 0; i < targetMethods.Length; i++)
        targetMethods[i] = explicitImplementations.GetValueOrDefault (interfaceMethods[i], targetMethods[i]);

      if (targetMethods.Contains (null) && !allowPartialInterfaceMapping)
      {
        var missingMethods = interfaceMethods.Zip (targetMethods).Where (t => t.Item2 == null).Select (t => t.Item1);
        var message = string.Format (
            "The added interface '{0}' is not fully implemented. The following methods have no implementation: {1}.",
            interfaceType.Name,
            SeparatedStringBuilder.Build (", ", missingMethods, m => "'" + m.Name + "'"));
        throw new InvalidOperationException (message);
      }

      return mapping;
    }

    private InterfaceMapping CreateForAdded (MutableType mutableType, Type interfaceType)
    {
      // Only public virtual methods may implicitly implement interfaces, ignore shadowed methods. (ECMA-335, 6th edition, II.12.2) 
      var implementationCandidates = mutableType.GetMethods (BindingFlags.Public | BindingFlags.Instance)
          .Where (m => m.IsVirtual)
          .ToLookup (m => new { m.Name, Signature = MethodSignature.Create (m) });
      var interfaceMethods = interfaceType.GetMethods();
      var targetMethods = interfaceMethods
          .Select (m => GetMostDerivedOrDefault (implementationCandidates[new { m.Name, Signature = MethodSignature.Create (m) }]))
          .ToArray();

      return new InterfaceMapping
             {
                 InterfaceType = interfaceType,
                 TargetType = mutableType,
                 InterfaceMethods = interfaceMethods,
                 TargetMethods = targetMethods
             };
    }

    private MethodInfo GetMostDerivedOrDefault (IEnumerable<MethodInfo> candidates)
    {
      MethodInfo mostDerived = null;
      foreach (var method in candidates)
      {
        if (mostDerived == null || mostDerived.DeclaringType.IsTypePipeAssignableFrom (method.DeclaringType))
          mostDerived = method;
      }

      return mostDerived;
    }

    private InterfaceMapping CreateForExisting (MutableType mutableType, Func<Type, InterfaceMapping> interfacMappingProvider, Type interfaceType)
    {
      var mapping = interfacMappingProvider (interfaceType);
      mapping.TargetType = mutableType;

      for (int i = 0; i < mapping.InterfaceMethods.Length; i++)
      {
        var baseImplementation = mapping.TargetMethods[i];

        // 1) Base implementation override.  2) Base implementation.
        mapping.TargetMethods[i] = mutableType.AddedMethods.SingleOrDefault (m => baseImplementation.Equals (m.BaseMethod)) ?? baseImplementation;
      }

      return mapping;
    }
  }
}