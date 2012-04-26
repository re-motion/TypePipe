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
using Remotion.Utilities;

namespace Remotion.Reflection.MemberSignatures
{
  /// <summary>
  /// Represents a field signature and allows signatures to be compared to each other.
  /// </summary>
  public class FieldSignature : IEquatable<FieldSignature>
  {
    public static FieldSignature Create (FieldInfo fieldInfo)
    {
      ArgumentUtility.CheckNotNull ("fieldInfo", fieldInfo);
      return new FieldSignature (fieldInfo.FieldType);
    }

    private readonly Type _fieldType;

    public FieldSignature (Type fieldType)
    {
      _fieldType = fieldType;
    }

    public Type FieldType
    {
      get { return _fieldType; }
    }

    public override string ToString ()
    {
      return FieldType.ToString();
    }

    public virtual bool Equals (FieldSignature other)
    {
      return !ReferenceEquals (other, null)
          && FieldType == other.FieldType;
    }

    public sealed override bool Equals (object obj)
    {
      if (obj == null || obj.GetType () != GetType ())
        return false;

      var other = (FieldSignature) obj;
      return Equals (other);
    }

    public override int GetHashCode ()
    {
      return FieldType.GetHashCode ();
    }
  }
}