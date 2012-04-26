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
using Remotion.Utilities;

namespace Remotion.Reflection.MemberSignatures
{
  /// <summary>
  /// Compares two members for equality by considering only the signatures, but not the names of the members. This comparer does not support comparing
  /// <see langword="null" /> values.
  /// </summary>
  public class MemberSignatureEqualityComparer : IEqualityComparer<MemberInfo>
  {
    public bool Equals (MemberInfo x, MemberInfo y)
    {
      ArgumentUtility.CheckNotNull ("x", x);
      ArgumentUtility.CheckNotNull ("y", y);

      var signatureX = GetMemberSignature (x);
      var signatureY = GetMemberSignature (y);

      return signatureX.Equals (signatureY);
    }

    public int GetHashCode (MemberInfo obj)
    {
      ArgumentUtility.CheckNotNull ("obj", obj);

      var signature = GetMemberSignature (obj);
      return signature.GetHashCode();
    }

    private IMemberSignature GetMemberSignature (MemberInfo x)
    {
      try
      {
        return MemberSignatureProvider.GetMemberSignature (x);
      }
      catch (NotSupportedException ex)
      {
        var message = string.Format (
            "MemberSignatureEqualityComparer does not support member type '{0}', "
            + "only constructors, methods, properties, events and fields are supported.",
            x.MemberType);
        throw new NotSupportedException (message, ex);
      }
    }
  }
}
