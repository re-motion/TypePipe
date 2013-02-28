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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  public static class TypeInstantiationObjectMother
  {
    public static TypeInstantiation Create (
        Type genericTypeDefinition = null,
        IEnumerable<Type> typeArguments = null,
        Dictionary<TypeInstantiationInfo, TypeInstantiation> instantiationContext = null,
        IMemberSelector memberSelector = null)
    {
      genericTypeDefinition = genericTypeDefinition ?? typeof (MyGenericType<>);
      typeArguments = typeArguments ?? genericTypeDefinition.GetGenericArguments().Select (a => ReflectionObjectMother.GetSomeType());
      var instantiationInfo = new TypeInstantiationInfo (genericTypeDefinition, typeArguments);
      instantiationContext = instantiationContext ?? new Dictionary<TypeInstantiationInfo, TypeInstantiation>();
      memberSelector = memberSelector ?? new MemberSelector (new BindingFlagsEvaluator());

      return new TypeInstantiation (memberSelector, instantiationInfo, instantiationContext);
    }

    private class MyGenericType<T> {}
  }
}