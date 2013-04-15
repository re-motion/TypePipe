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
using System.Reflection;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection.Implementation.MemberFactory;
using Remotion.Utilities;

namespace Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection
{
  public static class MutableTypeObjectMother
  {
    public static MutableType Create (
        Type baseType = null,
        string name = "MyMutableType",
        string @namespace = "MyNamespace",
        TypeAttributes attributes = TypeAttributes.Public | TypeAttributes.BeforeFieldInit,
        IMemberSelector memberSelector = null,
        IRelatedMethodFinder relatedMethodFinder = null,
        IInterfaceMappingComputer interfaceMappingComputer = null,
        IMutableMemberFactory mutableMemberFactory = null,
        bool copyCtorsFromBase = false)
    {
      baseType = baseType ?? typeof (UnspecifiedType);

      memberSelector = memberSelector ?? new MemberSelector (new BindingFlagsEvaluator());
      relatedMethodFinder = relatedMethodFinder ?? new RelatedMethodFinder();
      interfaceMappingComputer = interfaceMappingComputer ?? new InterfaceMappingComputer();
      mutableMemberFactory = mutableMemberFactory ?? new MutableMemberFactory (relatedMethodFinder);

      var proxyType = new MutableType (memberSelector, baseType, name, @namespace, attributes, interfaceMappingComputer, mutableMemberFactory);
      if (copyCtorsFromBase)
        CopyConstructors (baseType, proxyType);

      return proxyType;
    }

    public static MutableType CreateInterface (
        string name = "MyMutableInterface",
        string @namespace = "MyNamespace",
        TypeAttributes attributes = TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract,
        IMemberSelector memberSelector = null,
        IRelatedMethodFinder relatedMethodFinder = null,
        IInterfaceMappingComputer interfaceMappingComputer = null,
        IMutableMemberFactory mutableMemberFactory = null,
        bool copyCtorsFromBase = false)
    {
      memberSelector = memberSelector ?? new MemberSelector (new BindingFlagsEvaluator ());
      relatedMethodFinder = relatedMethodFinder ?? new RelatedMethodFinder ();
      interfaceMappingComputer = interfaceMappingComputer ?? new InterfaceMappingComputer ();
      mutableMemberFactory = mutableMemberFactory ?? new MutableMemberFactory (relatedMethodFinder);
      Assertion.IsTrue (attributes.IsSet (TypeAttributes.Interface | TypeAttributes.Abstract));

      return new MutableType (memberSelector, null, name, @namespace, attributes, interfaceMappingComputer, mutableMemberFactory);
    }

    private static void CopyConstructors (Type baseType, MutableType proxyType)
    {
      var proxyTypeModelFactory = new MutableTypeFactory();
      PrivateInvoke.InvokeNonPublicMethod (proxyTypeModelFactory, "CopyConstructors", baseType, proxyType);
    }

    public class UnspecifiedType { }
  }
}