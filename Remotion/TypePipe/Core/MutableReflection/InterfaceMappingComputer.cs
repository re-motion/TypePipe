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

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Implements <see cref="IInterfaceMappingComputer"/>, computing <see cref="InterfaceMapping"/> instances for a <see cref="MutableType"/>.
  /// </summary>
  public class InterfaceMappingComputer : IInterfaceMappingComputer
  {
    private static readonly MemberNamedAndSignatureEqualityComparer s_memberNameAndSignatureComparer = new MemberNamedAndSignatureEqualityComparer();

    public InterfaceMapping ComputeMapping (
        MutableType mutableType,
        Func<Type, InterfaceMapping> interfacMappingProvider,
        Type interfaceType,
        IMutableMemberProvider<MethodInfo, MutableMethodInfo> mutableMethodProvider,
        bool allowPartialInterfaceMapping)
    {
      ArgumentUtility.CheckNotNull ("mutableType", mutableType);
      ArgumentUtility.CheckNotNull ("interfacMappingProvider", interfacMappingProvider);
      ArgumentUtility.CheckNotNull ("interfaceType", interfaceType);
      ArgumentUtility.CheckNotNull ("mutableMethodProvider", mutableMethodProvider);

      if (!interfaceType.IsInterface)
        throw new ArgumentException ("Type passed must be an interface.", "interfaceType");
      if (!mutableType.GetInterfaces().Contains (interfaceType))
        throw new ArgumentException ("Interface not found.", "interfaceType");

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
                 ? CreateForAdded (mutableType, interfaceType, mutableMethodProvider, remainingInterfaceMethods, mapping, allowPartialInterfaceMapping)
                 : CreateForExisting (mutableType, interfacMappingProvider, interfaceType, mutableMethodProvider, remainingInterfaceMethods, mapping);
    }

    private InterfaceMapping CreateForAdded (
        MutableType mutableType,
        Type interfaceType,
        IMutableMemberProvider<MethodInfo, MutableMethodInfo> mutableMethodProvider,
        HashSet<MethodInfo> remainingInterfaceMethods,
        Dictionary<MethodInfo, MethodInfo> mapping, // TODO Review: Dict<MethodInfo, MutableMethodInfo>, rename to explicitImplementations
        bool allowPartialInterfaceMapping)
    {
      // TODO Review: Use Dictionary<anonymous tuple <string, MethodSignature>, MethodInfo>.
      var remainingSignatures = remainingInterfaceMethods.ToDictionary (m => m, s_memberNameAndSignatureComparer);
      // Only public virtual methods may implicitly implement interfaces. (ECMA-335, 6th edition, II.12.2) 
      var allPublicMethods = mutableType.GetMethods (); // TODO Review: virtual!

      // TODO Review: Consider this:
      //var publicMethodsByNameAndSignature = allPublicMethods.ToDictionary (m => m, s_memberNameAndSignatureComparer);
      // var interfaceMethods = ...ToArray();
      // var targetMethods = interfaceMethods.Select (m => mapping.GetValueOrDefault (m) ?? publicMethodsByNameAndSignature[m]).ToArray(); <- Plus error detection

      //// Combine mapping and remainingMapping, check for nulls if not allowed.

      // Search methods (including base methods) that implicitly implement the added interface.
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

      if (allowPartialInterfaceMapping)
      {
        foreach (var remainingMethod in remainingSignatures.Keys.Cast<MethodInfo>())
          mapping.Add (remainingMethod, null);

        return CreateInterfaceMapping (interfaceType, mutableType, mapping, mutableMethodProvider);
      }

      // TODO Review: Refactor so that the standard case returns at the end of the method, not the error case.
      var missingImplementations = SeparatedStringBuilder.Build (", ", remainingSignatures.Keys, m => m.Name);
      var message = string.Format (
          "The added interface '{0}' is not fully implemented. The following methods have no implementation: {1}",
          interfaceType.Name,
          missingImplementations);
      throw new InvalidOperationException (message);
    }

    private InterfaceMapping CreateForExisting (
        MutableType mutableType,
        Func<Type, InterfaceMapping> interfacMappingProvider,
        Type interfaceType,
        IMutableMemberProvider<MethodInfo, MutableMethodInfo> mutableMethodProvider,
        HashSet<MethodInfo> remainingInterfaceMethods,
        Dictionary<MethodInfo, MethodInfo> mapping) // TODO Review: Dict<MethodInfo, MutableMethodInfo>, rename to explicitImplementations
    {
      var underlyingMapping = interfacMappingProvider (interfaceType);

      // TODO Review:
      //for (int i = 0; i < underlyingMapping.InterfaceMethods.Length; i++)
      //{
      //  var interfaceMethod = underlyingMapping.InterfaceMethods[i];
      //  if (mapping.Contains (interfaceMethod))
      //    underlyingMapping.TargetMethods[i] = mapping[interfaceMethod]; <- use GetValueOrDefault or TryGetValue
      //}

      //return underlyingMapping; <- rename to just 'mapping'

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
        mapping.TargetMethods[i] = entry.Value == null ? null : mutableMethodProvider.GetMutableMember (entry.Value) ?? entry.Value;
        i++;
      }

      return mapping;
    }
  }
}