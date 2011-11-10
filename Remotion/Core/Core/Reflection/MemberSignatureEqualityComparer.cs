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
    public bool Equals (MemberInfo x, MemberInfo y)
    {
      ArgumentUtility.CheckNotNull ("x", x);
      ArgumentUtility.CheckNotNull ("y", y);

      var signatureBuilderX = GetSignatureStringBuilder (x.MemberType);
      var signatureBuilderY = GetSignatureStringBuilder (y.MemberType);

      if (signatureBuilderX != signatureBuilderY)
        return false;

      var signatureX = signatureBuilderX.BuildSignatureString (x);
      var signatureY = signatureBuilderY.BuildSignatureString (y);

      return signatureX == signatureY;
    }
    
    public int GetHashCode (MemberInfo obj)
    {
      ArgumentUtility.CheckNotNull ("obj", obj);

      var signatureBuilder = GetSignatureStringBuilder (obj.MemberType);
      var signatureString = signatureBuilder.BuildSignatureString (obj);
      return signatureString.GetHashCode ();
    }

    private IMemberSignatureStringBuilder GetSignatureStringBuilder (MemberTypes memberType)
    {
      try
      {
        return MemberSignatureStringBuilderProvider.GetSignatureBuilder (memberType);
      }
      catch (NotSupportedException ex)
      {
        var message = String.Format (
              "MemberSignatureEqualityComparer does not support member type '{0}', only methods, properties, and events are supported.", 
              memberType);
          throw new NotSupportedException (message, ex);
      }
    }
  }
}
