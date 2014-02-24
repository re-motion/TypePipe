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
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection.Implementation;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection.Implementation.MemberFactory;
using Remotion.Utilities;

namespace Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection
{
  public static class MutableTypeObjectMother
  {
    public static MutableType Create (
        Type baseType = null,
        string name = "MyMutableType",
        string @namespace = "MyNamespace",
        TypeAttributes attributes = TypeAttributes.Public | TypeAttributes.BeforeFieldInit,
        MutableType declaringType = null,
        IMemberSelector memberSelector = null,
        IInterfaceMappingComputer interfaceMappingComputer = null,
        IMutableMemberFactory mutableMemberFactory = null,
        bool copyCtorsFromBase = false)
    {
      baseType = baseType ?? typeof (UnspecifiedType);
      // Declaring type stays null.

      memberSelector = memberSelector ?? new MemberSelector (new BindingFlagsEvaluator());
      interfaceMappingComputer = interfaceMappingComputer ?? new InterfaceMappingComputer();
      mutableMemberFactory = mutableMemberFactory ?? new MutableMemberFactory (new RelatedMethodFinder());

      var mutableType = new MutableType (declaringType, baseType, name, @namespace, attributes, interfaceMappingComputer, mutableMemberFactory);
      mutableType.SetMemberSelector (memberSelector);

      if (copyCtorsFromBase)
        CopyConstructors (baseType, mutableType);

      return mutableType;
    }

    public static MutableType CreateInterface (
        string name = "MyMutableInterface",
        string @namespace = "MyNamespace",
        TypeAttributes attributes = TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract,
        MutableType declaringType = null,
        IMemberSelector memberSelector = null,
        IRelatedMethodFinder relatedMethodFinder = null,
        IInterfaceMappingComputer interfaceMappingComputer = null,
        IMutableMemberFactory mutableMemberFactory = null)
    {
      // Declaring type stays null.

      memberSelector = memberSelector ?? new MemberSelector (new BindingFlagsEvaluator());
      relatedMethodFinder = relatedMethodFinder ?? new RelatedMethodFinder();
      interfaceMappingComputer = interfaceMappingComputer ?? new InterfaceMappingComputer();
      mutableMemberFactory = mutableMemberFactory ?? new MutableMemberFactory (relatedMethodFinder);
      Assertion.IsTrue (attributes.IsSet (TypeAttributes.Interface | TypeAttributes.Abstract));

      var mutableType = new MutableType (declaringType, null, name, @namespace, attributes, interfaceMappingComputer, mutableMemberFactory);
      mutableType.SetMemberSelector (memberSelector);

      return mutableType;
    }


    private static void CopyConstructors (Type baseType, MutableType proxyType)
    {
      var proxyTypeModelFactory = new MutableTypeFactory();
      var result = (IEnumerable<Expression>) PrivateInvoke.InvokeNonPublicMethod (proxyTypeModelFactory, "CopyConstructors", baseType, proxyType);
      // ReSharper disable ReturnValueOfPureMethodIsNotUsed
      result.ToArray();
      // ReSharper restore ReturnValueOfPureMethodIsNotUsed
    }

    public class UnspecifiedType { }
  }
}