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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection.Implementation
{
  public static class CustomFieldInfoObjectMother
  {
    public static CustomFieldInfo Create (
        CustomType declaringType = null,
        string name = "CustomField",
        Type type = null,
        FieldAttributes attributes = (FieldAttributes) 7,
        IEnumerable<ICustomAttributeData> customAttributes = null)
    {
      declaringType = declaringType ?? CustomTypeObjectMother.Create();
      type = type ?? ReflectionObjectMother.GetSomeType();
      customAttributes = customAttributes ?? new ICustomAttributeData[0];

      return new TestableCustomFieldInfo (declaringType, name, type, attributes) { CustomAttributeDatas = customAttributes };
    }
  }
}