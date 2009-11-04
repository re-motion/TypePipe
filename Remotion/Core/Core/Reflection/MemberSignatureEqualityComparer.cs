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
using System.Collections.Generic;
using System.Reflection;
using Remotion.Reflection.SignatureStringBuilding;
using Remotion.Utilities;

namespace Remotion.Reflection
{
  /// <summary>
  /// Compares the signatures of two members for equality. Two member signatures compare equal if and only if both their member types and
  /// their signature strings (<see cref="N:Remotion.Reflection.SignatureStringBuilding"/>) are equal. This comparer does not support comparing
  /// <see langword="null" /> values.
  /// </summary>
  public class MemberSignatureEqualityComparer : IEqualityComparer<MemberInfo>
  {
    private static readonly MethodSignatureStringBuilder s_methodSignatureBuilder = new MethodSignatureStringBuilder ();
    private static readonly PropertySignatureStringBuilder s_propertySignatureBuilder = new PropertySignatureStringBuilder ();
    private static readonly EventSignatureStringBuilder s_eventSignatureBuilder = new EventSignatureStringBuilder ();
    
    public bool Equals (MemberInfo x, MemberInfo y)
    {
      ArgumentUtility.CheckNotNull ("x", x);
      ArgumentUtility.CheckNotNull ("y", y);

      var signatureBuilderX = GetSignatureBuilder (x.MemberType);
      var signatureBuilderY = GetSignatureBuilder (y.MemberType);

      if (signatureBuilderX != signatureBuilderY)
        return false;

      var signatureX = signatureBuilderX.BuildSignatureString (x);
      var signatureY = signatureBuilderY.BuildSignatureString (y);

      return signatureX == signatureY;
    }

    public int GetHashCode (MemberInfo obj)
    {
      ArgumentUtility.CheckNotNull ("obj", obj);

      var signatureBuilder = GetSignatureBuilder (obj.MemberType);
      var signatureString = signatureBuilder.BuildSignatureString (obj);
      return signatureString.GetHashCode ();
    }

    private IMemberSignatureStringBuilder GetSignatureBuilder (MemberTypes memberType)
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
              "MemberSignatureEqualityComparer does not support member type '{0}', only methods, properties, and events are supported.", 
              memberType);
          throw new NotSupportedException (message);
      }
    }
  }
}
