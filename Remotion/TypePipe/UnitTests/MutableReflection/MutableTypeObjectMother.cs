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
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class MutableTypeObjectMother
  {
    public static ProxyType Create (
        Type baseType = null,
        string name = "Proxy",
        string @namespace = "My",
        string fullname = "My.Proxy",
        TypeAttributes attributes = TypeAttributes.Public | TypeAttributes.BeforeFieldInit,
        IMemberSelector memberSelector = null,
        IRelatedMethodFinder relatedMethodFinder = null,
        IInterfaceMappingComputer interfaceMappingComputer = null,
        IMutableMemberFactory mutableMemberFactory = null)
    {
      baseType = baseType ?? ReflectionObjectMother.GetSomeSubclassableType();
      memberSelector = memberSelector ?? new MemberSelector (new BindingFlagsEvaluator());
      relatedMethodFinder = relatedMethodFinder ?? new RelatedMethodFinder();
      interfaceMappingComputer = interfaceMappingComputer ?? new InterfaceMappingComputer();
      mutableMemberFactory = mutableMemberFactory ?? new MutableMemberFactory (memberSelector, relatedMethodFinder);

      return new ProxyType (
          baseType,
          name,
          @namespace,
          fullname,
          attributes,
          memberSelector,
          interfaceMappingComputer,
          mutableMemberFactory);
    }
  }
}