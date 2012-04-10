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

namespace Remotion.Reflection.SignatureStringBuilding
{
  /// <summary>
  /// Returns the right implementation of <see cref="IMemberSignatureStringBuilder"/> for a given <see cref="MemberTypes"/> value.
  /// </summary>
  public static class MemberSignatureStringBuilderProvider
  {
    private static readonly MethodSignatureStringBuilder s_methodSignatureBuilder = new MethodSignatureStringBuilder ();
    private static readonly PropertySignatureStringBuilder s_propertySignatureBuilder = new PropertySignatureStringBuilder ();
    private static readonly EventSignatureStringBuilder s_eventSignatureBuilder = new EventSignatureStringBuilder ();
    private static readonly FieldSignatureStringBuilder s_fieldSignatureBuilder = new FieldSignatureStringBuilder ();

    public static IMemberSignatureStringBuilder GetSignatureBuilder (MemberTypes memberType)
    {
      switch (memberType)
      {
        case MemberTypes.Constructor:
        case MemberTypes.Method:
          return s_methodSignatureBuilder;
        case MemberTypes.Property:
          return s_propertySignatureBuilder;
        case MemberTypes.Event:
          return s_eventSignatureBuilder;
        case MemberTypes.Field:
          return s_fieldSignatureBuilder;
        default:
          var message = string.Format (
              "Cannot return a signature builder for member type '{0}'; only constructors, methods, properties, events and fields are supported.", 
              memberType);
          throw new NotSupportedException (message);
      }
    }
  }
}