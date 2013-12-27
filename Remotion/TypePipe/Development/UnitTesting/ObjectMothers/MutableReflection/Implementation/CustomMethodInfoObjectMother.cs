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
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using System.Linq;

namespace Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection.Implementation
{
  public static class CustomMethodInfoObjectMother
  {
    public static CustomMethodInfo Create (
        CustomType declaringType = null,
        string name = "CustomMethod",
        MethodAttributes attributes = (MethodAttributes) 7,
        ParameterInfo returnParameter = null,
        IEnumerable<ParameterInfo> parameters = null,
        MethodInfo baseDefinition = null,
        IEnumerable<ICustomAttributeData> customAttributes = null,
        MethodInfo genericMethodDefintion = null,
        IEnumerable<Type> typeArguments = null)
    {
      declaringType = declaringType ?? CustomTypeObjectMother.Create();
      returnParameter = returnParameter ?? CustomParameterInfoObjectMother.Create (position: -1, type: typeof (void));
      parameters = parameters ?? new ParameterInfo[0];
      // Base definition stays null.
      customAttributes = customAttributes ?? new ICustomAttributeData[0];
      // Generic method definition stays null.
      var typeArgs = (typeArguments ?? Type.EmptyTypes).ToList ();

      return new TestableCustomMethodInfo (declaringType, name, attributes, genericMethodDefintion, typeArgs)
             {
                 ReturnParameter_ = returnParameter,
                 Parameters = parameters.ToArray(),
                 BaseDefinition = baseDefinition,
                 CustomAttributeDatas = customAttributes.ToArray()
             };
    }
  }
}