// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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

    public static IMemberSignatureStringBuilder GetSignatureBuilder (MemberTypes memberType)
    {
      switch (memberType)
      {
        case MemberTypes.Method:
          return s_methodSignatureBuilder;
        case MemberTypes.Property:
          return s_propertySignatureBuilder;
        case MemberTypes.Event:
          return s_eventSignatureBuilder;
        default:
          var message = string.Format (
              "Cannot return a signature builder for member type '{0}'; only methods, properties, and events are supported.", 
              memberType);
          throw new NotSupportedException (message);
      }
    }
  }
}