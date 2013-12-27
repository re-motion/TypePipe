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
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection.Implementation
{
  public static class ParameterOnCustomMemberObjectMother
  {
    public static ParameterOnCustomMember Create (
        MemberInfo member = null,
        int position = 7,
        string name = "param",
        Type type = null,
        ParameterAttributes attributes = (ParameterAttributes) 7)
    {
      member = member ?? NormalizingMemberInfoFromExpressionUtility.GetMethod (() => UnspecifiedMember());
      type = type ?? ReflectionObjectMother.GetSomeType();

      return new ParameterOnCustomMember (member, position, name, type, attributes);
    }

    private static void UnspecifiedMember () {}
  }
}