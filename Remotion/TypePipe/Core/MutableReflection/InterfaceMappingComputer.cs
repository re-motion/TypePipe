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
using Remotion.Utilities;
using System.Linq;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Implements <see cref="IInterfaceMappingComputer"/>.
  /// </summary>
  public class InterfaceMappingComputer : IInterfaceMappingComputer
  {
    private static readonly MemberNamedAndSignatureEqualityComparer s_memberNameAndSignatureComparer = new MemberNamedAndSignatureEqualityComparer();

    public InterfaceMapping ComputeMapping (
        MutableType mutableType,
        Func<Type, InterfaceMapping> interfacMappingProvider,
        Type interfaceType,
        IMutableMemberProvider<MethodInfo, MutableMethodInfo> mutableMethodProvider)
    {
      ArgumentUtility.CheckNotNull ("mutableType", mutableType);
      ArgumentUtility.CheckNotNull ("interfacMappingProvider", interfacMappingProvider);
      ArgumentUtility.CheckNotNull ("interfaceType", interfaceType);
      ArgumentUtility.CheckNotNull ("mutableMethodProvider", mutableMethodProvider);
      Assertion.IsTrue (interfaceType.IsInterface);
      Assertion.IsTrue (mutableType.GetInterfaces().Contains (interfaceType));

      var remainingInterfaceMethods = new HashSet<MethodInfo> (interfaceType.GetMethods());
      var mapping = new Dictionary<MethodInfo, MethodInfo>();

      // Explicit overrides in mutable type.
      foreach (var implementationMethod in mutableType.AllMutableMethods)
      {
        foreach (var explicitBaseDefinition in implementationMethod.AddedExplicitBaseDefinitions)
        {
          if (remainingInterfaceMethods.Remove (explicitBaseDefinition))
          {
            mapping.Add (explicitBaseDefinition, implementationMethod);

            if (remainingInterfaceMethods.Count == 0)
              return CreateInterfaceMapping (interfaceType, mutableType, mapping, mutableMethodProvider);
          }
        }
      }

      // TODO 5230: Adapt code when implementing MutableType.ReImplementInterface
      var isAddedInterface = mutableType.AddedInterfaces.Contains (interfaceType);
      return isAddedInterface
                 ? CreateForAdded (mutableType, interfaceType, mutableMethodProvider, remainingInterfaceMethods, mapping)
                 : CreateForExisting (mutableType, interfacMappingProvider, interfaceType, mutableMethodProvider, remainingInterfaceMethods, mapping);
    }

    private InterfaceMapping CreateForAdded (
        MutableType mutableType,
        Type interfaceType,
        IMutableMemberProvider<MethodInfo, MutableMethodInfo> mutableMethodProvider,
        HashSet<MethodInfo> remainingInterfaceMethods,
        Dictionary<MethodInfo, MethodInfo> mapping)
    {
      var remainingSignatures = remainingInterfaceMethods.ToDictionary (m => m, s_memberNameAndSignatureComparer);
      var allPublicMethods = mutableType.GetMethods(); // Interface methods must be public.
      
      // Serach methods (including base methods) that implicitly implement the added interface.
      foreach (var method in allPublicMethods)
      {
        MethodInfo interfaceMethod;
        if (remainingSignatures.TryGetValue (method, out interfaceMethod))
        {
          mapping.Add (interfaceMethod, method);
          remainingSignatures.Remove (method);

          if (remainingSignatures.Count == 0)
            return CreateInterfaceMapping (interfaceType, mutableType, mapping, mutableMethodProvider);
        }
      }

      var message = string.Format ("The added interface '{0}' is not fully implemented.", interfaceType.Name);
      throw new InvalidOperationException (message);
    }

    private InterfaceMapping CreateForExisting (
        MutableType mutableType,
        Func<Type, InterfaceMapping> interfacMappingProvider,
        Type interfaceType,
        IMutableMemberProvider<MethodInfo, MutableMethodInfo> mutableMethodProvider,
        HashSet<MethodInfo> remainingInterfaceMethods,
        Dictionary<MethodInfo, MethodInfo> mapping)
    {
      var underlyingMapping = interfacMappingProvider (interfaceType);

      // Interface map from underlying type.
      for (int i = 0; i < underlyingMapping.InterfaceMethods.Length; i++)
      {
        var interfaceMethod = underlyingMapping.InterfaceMethods[i];
        if (remainingInterfaceMethods.Remove (interfaceMethod))
        {
          mapping.Add (interfaceMethod, underlyingMapping.TargetMethods[i]);

          if (remainingInterfaceMethods.Count == 0)
            return CreateInterfaceMapping (interfaceType, mutableType, mapping, mutableMethodProvider);
        }
      }

      throw new Exception ("Unreachable code");
    }

    private InterfaceMapping CreateInterfaceMapping (
        Type interfaceType,
        MutableType targetType,
        Dictionary<MethodInfo, MethodInfo> interfaceMap,
        IMutableMemberProvider<MethodInfo, MutableMethodInfo> mutableMethodProvider)
    {
      var mapping = new InterfaceMapping
                    {
                        InterfaceType = interfaceType,
                        TargetType = targetType,
                        InterfaceMethods = new MethodInfo[interfaceMap.Count],
                        TargetMethods = new MethodInfo[interfaceMap.Count]
                    };

      int i = 0;
      foreach (var entry in interfaceMap)
      {
        mapping.InterfaceMethods[i] = entry.Key;
        mapping.TargetMethods[i] = mutableMethodProvider.GetMutableMember (entry.Value) ?? entry.Value;
        i++;
      }

      return mapping;
    }
  }
}