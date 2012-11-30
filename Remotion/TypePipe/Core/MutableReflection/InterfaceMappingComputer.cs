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
        IMutableMemberProvider<MethodInfo, MutableMethodInfo> mutableMethodProvider,
        Type interfaceType,
        bool allowPartialInterfaceMapping)
    {
      ArgumentUtility.CheckNotNull ("mutableType", mutableType);
      ArgumentUtility.CheckNotNull ("interfacMappingProvider", interfacMappingProvider);
      ArgumentUtility.CheckNotNull ("mutableMethodProvider", mutableMethodProvider);
      ArgumentUtility.CheckNotNull ("interfaceType", interfaceType);

      if (!interfaceType.IsInterface)
        throw new ArgumentException ("Type passed must be an interface.", "interfaceType");
      if (!mutableType.GetInterfaces().Contains (interfaceType))
        throw new ArgumentException ("Interface not found.", "interfaceType");

      var remainingInterfaceMethods = new HashSet<MethodInfo> (interfaceType.GetMethods());
      var explicitImplementations = new Dictionary<MethodInfo, MutableMethodInfo>();

      foreach (var method in mutableType.AllMutableMethods)
      {
        foreach (var explicitBaseDefinition in method.AddedExplicitBaseDefinitions)
        {
          if (remainingInterfaceMethods.Remove (explicitBaseDefinition))
          {
            explicitImplementations.Add (explicitBaseDefinition, method);

            if (remainingInterfaceMethods.Count == 0)
            {
              // Keys and Values collections are guaranteed to have matching order.
              var interfaceMethods = explicitImplementations.Keys.ToArray();
              var targetMethods = explicitImplementations.Values.Cast<MethodInfo>().ToArray();

              return CreateInterfaceMapping (interfaceType, mutableType, interfaceMethods, targetMethods);
            }
          }
        }
      }

      // TODO 5230: Adapt code when implementing MutableType.ReImplementInterface
      var isAddedInterface = mutableType.AddedInterfaces.Contains (interfaceType);
      return isAddedInterface
                 ? CreateForAdded (mutableType, interfaceType, explicitImplementations, allowPartialInterfaceMapping)
                 : CreateForExisting (mutableType, interfacMappingProvider, interfaceType, explicitImplementations, mutableMethodProvider);
    }

    private InterfaceMapping CreateForAdded (
        MutableType mutableType,
        Type interfaceType,
        Dictionary<MethodInfo, MutableMethodInfo> explicitImplementations,
        bool allowPartialInterfaceMapping)
    {
      // Only public virtual methods may implicitly implement interfaces, ignore shadowed methods. (ECMA-335, 6th edition, II.12.2) 
      var candidates = mutableType
          .CreateSequence<Type> (t => t.BaseType)
          .SelectMany (t => t.GetMethods (BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where (m => m.IsVirtual))
          .Cast<MemberInfo>().Distinct (s_memberNameAndSignatureComparer).Cast<MethodInfo>() // TODO 5057 (cleanup code when upgrading to 4.x)
          .ToDictionary (m => new { m.Name, Signature = MethodSignature.Create (m) });

      var interfaceMethods = interfaceType.GetMethods().ToArray();
      var targetMethods = interfaceMethods
          .Select (
              m =>
              explicitImplementations.GetValueOrDefault (m) ?? candidates.GetValueOrDefault (new { m.Name, Signature = MethodSignature.Create (m) }))
          .ToArray();

      if (targetMethods.Contains (null) && !allowPartialInterfaceMapping)
      {
        var missingMethods = interfaceMethods.Zip (targetMethods).Where (t => t.Item2 == null).Select (t => t.Item1);
        var message = string.Format (
            "The added interface '{0}' is not fully implemented. The following methods have no implementation: {1}.",
            interfaceType.Name,
            SeparatedStringBuilder.Build (", ", missingMethods, m => "'" + m.Name + "'"));
        throw new InvalidOperationException (message);
      }

      return CreateInterfaceMapping (interfaceType, mutableType, interfaceMethods, targetMethods);
    }

    private InterfaceMapping CreateForExisting (
        MutableType mutableType,
        Func<Type, InterfaceMapping> interfacMappingProvider,
        Type interfaceType,
        Dictionary<MethodInfo, MutableMethodInfo> explicitImplementations,
        IMutableMemberProvider<MethodInfo, MutableMethodInfo> mutableMethodProvider)
    {
      var mapping = interfacMappingProvider (interfaceType);
      mapping.TargetType = mutableType;

      for (int i = 0; i < mapping.InterfaceMethods.Length; i++)
      {
        var interfaceMethod = mapping.InterfaceMethods[i];
        var targetMethod = mapping.TargetMethods[i];

        MutableMethodInfo explicitImplementation;
        if (explicitImplementations.TryGetValue (interfaceMethod, out explicitImplementation))
          mapping.TargetMethods[i] = explicitImplementation;
        else
          mapping.TargetMethods[i] = mutableMethodProvider.GetMutableMember (targetMethod) ?? targetMethod;
      }

      return mapping;
    }

    private InterfaceMapping CreateInterfaceMapping (
        Type interfaceType, MutableType targetType, MethodInfo[] interfaceMethods, MethodInfo[] targetMethods)
    {
      return new InterfaceMapping
             {
                 InterfaceType = interfaceType,
                 TargetType = targetType,
                 InterfaceMethods = interfaceMethods,
                 TargetMethods = targetMethods
             };
    }
  }
}