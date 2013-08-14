// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 

using System;
using System.Collections.Generic;
using System.Reflection;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection.Implementation.MemberFactory;
using Remotion.Utilities;
using Remotion.Development.UnitTesting.Enumerables;

namespace Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection
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
        IRelatedMethodFinder relatedMethodFinder = null,
        IInterfaceMappingComputer interfaceMappingComputer = null,
        IMutableMemberFactory mutableMemberFactory = null,
        bool copyCtorsFromBase = false)
    {
      baseType = baseType ?? typeof (UnspecifiedType);
      // Declaring type stays null.

      memberSelector = memberSelector ?? new MemberSelector (new BindingFlagsEvaluator());
      relatedMethodFinder = relatedMethodFinder ?? new RelatedMethodFinder();
      interfaceMappingComputer = interfaceMappingComputer ?? new InterfaceMappingComputer();
      mutableMemberFactory = mutableMemberFactory ?? new MutableMemberFactory (relatedMethodFinder);

      var proxyType = new MutableType (
          memberSelector, declaringType, baseType, name, @namespace, attributes, interfaceMappingComputer, mutableMemberFactory);
      if (copyCtorsFromBase)
        CopyConstructors (baseType, proxyType);

      return proxyType;
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

      return new MutableType (memberSelector, declaringType, null, name, @namespace, attributes, interfaceMappingComputer, mutableMemberFactory);
    }

    private static void CopyConstructors (Type baseType, MutableType proxyType)
    {
      var proxyTypeModelFactory = new MutableTypeFactory();
      proxyTypeModelFactory.Invoke<IEnumerable<Expression>> ("CopyConstructors", baseType, proxyType).ForceEnumeration();
    }

    public class UnspecifiedType { }
  }
}