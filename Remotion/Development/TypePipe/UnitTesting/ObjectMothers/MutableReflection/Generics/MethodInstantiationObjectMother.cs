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
using System.Linq;
using System.Reflection;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Generics;

namespace Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection.Generics
{
  public static class MethodInstantiationObjectMother
  {
    public static MethodInstantiation Create (MethodInfo genericMethodDefinition = null, IEnumerable<Type> typeArguments = null)
    {
      genericMethodDefinition = genericMethodDefinition
                                ?? NormalizingMemberInfoFromExpressionUtility.GetGenericMethodDefinition (() => GenericMethod (7));
      typeArguments = typeArguments ?? genericMethodDefinition.GetGenericArguments().Select (a => ReflectionObjectMother.GetSomeType());
      var instantiationInfo = new MethodInstantiationInfo (genericMethodDefinition, typeArguments);

      return new MethodInstantiation (instantiationInfo);
    }

    private static void GenericMethod<T> (T t) {}
  }
}