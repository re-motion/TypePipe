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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection
{
  public class MutablePropertyInfoObjectMother
  {
    public static MutablePropertyInfo Create (
        MutableType declaringType = null,
        string name = "UnspecifiedProperty",
        PropertyAttributes attributes = PropertyAttributes.None,
        MutableMethodInfo getMethod = null,
        MutableMethodInfo setMethod = null)
    {
      declaringType = declaringType ?? MutableTypeObjectMother.Create();
      if (getMethod == null && setMethod == null)
        getMethod = MutableMethodInfoObjectMother.Create (declaringType, "Getter", returnType: typeof (int));

      return new MutablePropertyInfo (declaringType, name, attributes, getMethod, setMethod);
    }

    public static MutablePropertyInfo CreateReadWrite (
        MutableType declaringType = null, string name = "UnspecifiedProperty", PropertyAttributes attributes = PropertyAttributes.None, Type type = null)
    {
      declaringType = declaringType ?? MutableTypeObjectMother.Create();
      type = type ?? ReflectionObjectMother.GetSomeType();

      var getMethod = MutableMethodInfoObjectMother.Create (returnType: type);
      var setMethod = MutableMethodInfoObjectMother.Create (parameters: new[] { ParameterDeclarationObjectMother.Create (type) });

      return new MutablePropertyInfo (declaringType, name, attributes, getMethod, setMethod);
    }
  }
}