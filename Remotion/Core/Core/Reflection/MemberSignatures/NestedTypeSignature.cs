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
using Remotion.Utilities;

namespace Remotion.Reflection.MemberSignatures
{
  /// <summary>
  /// Represents a nested type signature and allows signatures to be compared to each other.
  /// </summary>
  public class NestedTypeSignature : IMemberSignature, IEquatable<NestedTypeSignature>
  {
    public static NestedTypeSignature Create (Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);
      Assertion.IsNotNull (type.DeclaringType);

      return new NestedTypeSignature (type.GetGenericArguments().Length);
    }

    private readonly int _genericParameterCount;

    public NestedTypeSignature (int genericParameterCount)
    {
      _genericParameterCount = genericParameterCount;
    }

    public int GenericParameterCount
    {
      get { return _genericParameterCount; }
    }

    public override string ToString ()
    {
      return "`" + GenericParameterCount;
    }

    public bool Equals (NestedTypeSignature other)
    {
      return !ReferenceEquals (other, null)
             && GenericParameterCount == other.GenericParameterCount;
    }

    public sealed override bool Equals (object obj)
    {
      if (obj == null || obj.GetType () != GetType ())
        return false;

      var other = (NestedTypeSignature) obj;
      return Equals (other);
    }

    bool IEquatable<IMemberSignature>.Equals (IMemberSignature other)
    {
      return Equals (other);
    }


    public override int GetHashCode ()
    {
      return GenericParameterCount.GetHashCode();
    }
  }
}