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
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class ProxyTypeObjectMother
  {
    public static ProxyType Create (
        Type baseType = null,
        string name = "Proxy",
        string @namespace = "My",
        string fullName = "My.Proxy",
        TypeAttributes attributes = TypeAttributes.Public | TypeAttributes.BeforeFieldInit,
        IMemberSelector memberSelector = null,
        IUnderlyingTypeFactory underlyingTypeFactory = null,
        IRelatedMethodFinder relatedMethodFinder = null,
        IInterfaceMappingComputer interfaceMappingComputer = null,
        IMutableMemberFactory mutableMemberFactory = null,
        bool copyCtorsFromBase = false)
    {
      baseType = baseType ?? typeof (UnspecifiedType);
      memberSelector = memberSelector ?? new MemberSelector (new BindingFlagsEvaluator());
      underlyingTypeFactory = underlyingTypeFactory ?? new ThrowingUnderlyingTypeFactory();

      relatedMethodFinder = relatedMethodFinder ?? new RelatedMethodFinder();
      interfaceMappingComputer = interfaceMappingComputer ?? new InterfaceMappingComputer();
      mutableMemberFactory = mutableMemberFactory ?? new MutableMemberFactory (memberSelector, relatedMethodFinder);

      var proxyType = new ProxyType (
          memberSelector,
          underlyingTypeFactory,
          baseType,
          name,
          @namespace,
          fullName,
          attributes,
          interfaceMappingComputer,
          mutableMemberFactory);

      if (copyCtorsFromBase)
        CopyConstructors (baseType, proxyType);

      return proxyType;
    }

    private static void CopyConstructors (Type baseType, ProxyType proxyType)
    {
      var proxyTypeModelFactory = new ProxyTypeModelFactory (new ThrowingUnderlyingTypeFactory());
      PrivateInvoke.InvokeNonPublicMethod (proxyTypeModelFactory, "CopyConstructors", baseType, proxyType);
    }

    public class UnspecifiedType { }
  }
}