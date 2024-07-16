﻿// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using System.Linq;
using System.Reflection;
using System.Threading;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection.Implementation.MemberFactory;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// Creates <see cref="MutableType"/> instances from the specified data or the given base type.
  /// </summary>
  /// <threadsafety static="true" instance="true" />
  public class MutableTypeFactory : IMutableTypeFactory
  {
    /// <summary>
    /// Assembly number counter. Will be incremented using <see cref="Interlocked.Increment(ref int)"/>.
    /// </summary>
    private int _counter;

    public MutableType CreateType (string name, string @namespace, TypeAttributes attributes, Type baseType, MutableType declaringType)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      // Name space may be null.
      // Base type may be null (for interfaces).
      // Declaring type may be null.

      var isInterface = attributes.IsSet (TypeAttributes.Interface);
      if (!isInterface && baseType == null)
        throw new ArgumentException ("Base type cannot be null.", "baseType");
      if (isInterface && baseType != null)
        throw new ArgumentException (string.Format ("Base type must be null for interfaces. Type: '{0}'", baseType.FullName), "baseType");

      if (baseType != null && !IsValidBaseType (baseType))
      {
        throw new ArgumentException (
            string.Format (
                "Base type must not be sealed, an interface, an array, a byref type, a pointer, a generic parameter, "
                + "contain generic parameters and must have an accessible constructor. Type: '{0}'",
                baseType.FullName),
            "baseType");
      }

      return CreateMutableType (name, @namespace, attributes, baseType, declaringType);
    }

    public ITypeModificationTracker CreateProxy (Type baseType, ProxyKind proxyKind)
    {
      ArgumentUtility.CheckNotNull ("baseType", baseType);

      var incrementedCounter = Interlocked.Increment (ref _counter);

      var name = string.Format ("{0}_{1}Proxy_{2}", baseType.Name, proxyKind, incrementedCounter);
      var attributes = TypeAttributes.Public | TypeAttributes.BeforeFieldInit | (baseType.IsTypePipeSerializable() ? TypeAttributes.Serializable : 0);

      var proxyType = CreateType (name, baseType.Namespace, attributes, baseType, null);
      var constructorBodies = CopyConstructors (baseType, proxyType);

      return new ProxyTypeModificationTracker (proxyType, constructorBodies);
    }

    private bool IsValidBaseType (Type baseType)
    {
      return SubclassFilterUtility.IsSubclassable (baseType) && !baseType.ContainsGenericParameters;
    }

    private MutableType CreateMutableType (string name, string @namespace, TypeAttributes attributes, Type baseType, MutableType declaringType)
    {
      var interfaceMappingComputer = new InterfaceMappingComputer();
      var mutableMemberFactory = new MutableMemberFactory (new RelatedMethodFinder());

      return new MutableType (declaringType, baseType, name, @namespace, attributes, interfaceMappingComputer, mutableMemberFactory);
    }

    private Expression[] CopyConstructors (Type baseType, MutableType proxyType)
    {
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
      var accessibleInstanceCtors = baseType.GetConstructors (bindingFlags).Where (SubclassFilterUtility.IsVisibleFromSubclass);

      var attributes = MethodAttributes.Public | MethodAttributes.HideBySig;
      var copiedConstructors = accessibleInstanceCtors.Select (ctor => CopyConstructor (proxyType, attributes, ctor));
      return copiedConstructors.Select (ctor => ctor.Body).ToArray();
    }

    private static MutableConstructorInfo CopyConstructor (MutableType proxyType, MethodAttributes attributes, ConstructorInfo constructor)
    {
      var parameters = constructor.GetParameters().Select (p => new ParameterDeclaration (p.ParameterType, p.Name, p.Attributes));
      return proxyType.AddConstructor (attributes, parameters, ctx => ctx.CallBaseConstructor (ctx.Parameters));
    }
  }
}