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
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.Expressions;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection
{
  public static class MutableMethodInfoObjectMother
  {
    public static MutableMethodInfo Create (
        MutableType declaringType = null,
        string name = "UnspecifiedMethod",
        MethodAttributes attributes = (MethodAttributes) 7,
        Type returnType = null,
        IEnumerable<ParameterDeclaration> parameters = null,
        MethodInfo baseMethod = null,
        Expression body = null,
        IEnumerable<MutableGenericParameter> genericParameters = null)
    {
      declaringType = declaringType ?? MutableTypeObjectMother.Create();
      if (baseMethod != null)
        attributes = attributes.Set (MethodAttributes.Virtual);
      returnType = returnType ?? typeof (void);
      parameters = parameters ?? ParameterDeclaration.None;
      // baseMethod stays null.
      body = body == null && !attributes.IsSet (MethodAttributes.Abstract) ? ExpressionTreeObjectMother.GetSomeExpression (returnType) : body;
      var genericParas = (genericParameters ?? new MutableGenericParameter[0]).ToList();

      return new MutableMethodInfo (declaringType, name, attributes, genericParas, returnType, parameters, baseMethod, body);
    }
  }
}