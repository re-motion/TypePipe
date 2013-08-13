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
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection
{
  public static class MutableConstructorInfoObjectMother
  {
    public static MutableConstructorInfo Create (
        MutableType declaringType = null,
        MethodAttributes attributes = (MethodAttributes) 7,
        IEnumerable<ParameterDeclaration> parameters = null,
        Expression body = null)
    {
      declaringType = declaringType ?? MutableTypeObjectMother.Create();
      attributes |= MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
      parameters = parameters ?? ParameterDeclaration.None;
      body = body ?? Expression.Empty();

      return new MutableConstructorInfo (declaringType, attributes, parameters, body);
    }
  }
}