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
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection
{
  public static class CustomAttributeDeclarationObjectMother
  {
    public static CustomAttributeDeclaration Create ()
    {
      return new CustomAttributeDeclaration (
          typeof (CustomAttribute).GetConstructor (new[] { typeof (int) }),
          new object[] { 7 },
          new NamedArgumentDeclaration (typeof (CustomAttribute).GetProperty ("Property"), "string"),
          new NamedArgumentDeclaration (typeof (CustomAttribute).GetField ("Field"), new object()));
    }

    public static CustomAttributeDeclaration Create (Type attributeType)
    {
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
      var ctor = attributeType.GetConstructor (bindingFlags, null, Type.EmptyTypes, null);
      Assertion.IsNotNull (ctor, "Custom attribute type does not define a default ctor.");

      return new CustomAttributeDeclaration (ctor, new object[0]);
    }

    public class CustomAttribute : Attribute
    {
      public object Field = null;

      public CustomAttribute (int arg)
      {
      }

      public string Property { get; set; }
    }
  }
}