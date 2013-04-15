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
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection.Generics
{
  public static class MutableGenericParameterObjectMother
  {
    public static MutableGenericParameter Create (
        int position = 7,
        string name = "T",
        string @namespace = "MyNs",
        GenericParameterAttributes genericParameterAttributes = GenericParameterAttributes.None,
        IEnumerable<Type> constraints = null,
        IMemberSelector memberSelector = null)
    {
      constraints = constraints ?? Type.EmptyTypes;
      memberSelector = memberSelector ?? new MemberSelector (new BindingFlagsEvaluator());

      var genericParameter = new MutableGenericParameter (memberSelector, position, name, @namespace, genericParameterAttributes);
      genericParameter.SetGenericParameterConstraints (constraints);

      return genericParameter;
    }

    public static MutableGenericParameter[] CreateMultiple (int count)
    {
      return Enumerable.Range (1, count).Select (i => Create (name: "p" + i)).ToArray ();
    }
  }
}