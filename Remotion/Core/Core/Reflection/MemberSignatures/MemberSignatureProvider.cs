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

namespace Remotion.Reflection.MemberSignatures
{
  /// <summary>
  /// Provides <see cref="IMemberSignature"/>s for <see cref="MemberInfo"/>s.
  /// </summary>
  public static class MemberSignatureProvider
  {
    public static IMemberSignature GetMemberSignature (MemberInfo memberInfo)
    {
      switch (memberInfo.MemberType)
      {
        case MemberTypes.Constructor:
        case MemberTypes.Method:
          return MethodSignature.Create ((MethodBase) memberInfo);
        case MemberTypes.Field:
          return FieldSignature.Create ((FieldInfo) memberInfo);
        case MemberTypes.Property:
          return PropertySignature.Create ((PropertyInfo) memberInfo);
        case MemberTypes.Event:
          return EventSignature.Create ((EventInfo) memberInfo);
        case MemberTypes.NestedType:
          return NestedTypeSignature.Create ((Type) memberInfo);

        default:
          var message = string.Format (
              "Cannot return a signature builder for member type '{0}'; only constructors, methods, fields, properties and events are supported.",
              memberInfo.MemberType);
          throw new NotSupportedException (message);
      }
    }
  }
}