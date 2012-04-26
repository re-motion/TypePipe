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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Remotion.Text;
using Remotion.Utilities;

namespace Remotion.Reflection.MemberSignatures
{
  /// <summary>
  /// Represents a property signature and allows signatures to be compared to each other.
  /// </summary>
  public class PropertySignature : IEquatable<PropertySignature>
  {
    public static PropertySignature Create (PropertyInfo propertyInfo)
    {
      ArgumentUtility.CheckNotNull ("propertyInfo", propertyInfo);

      var propertyType = propertyInfo.PropertyType;
      var indexParameterTypes = propertyInfo.GetIndexParameters ().Select (p => p.ParameterType);

      return new PropertySignature (propertyType, indexParameterTypes);
    }

    private readonly Type _propertyType;
    private readonly ReadOnlyCollection<Type> _indexParameterTypes;

    public PropertySignature (Type propertyType, IEnumerable<Type> indexParameterTypes)
    {
      ArgumentUtility.CheckNotNull ("propertyType", propertyType);
      ArgumentUtility.CheckNotNull ("indexParameterTypes", indexParameterTypes);

      _propertyType = propertyType;
      _indexParameterTypes = indexParameterTypes.ToList().AsReadOnly();
    }

    public Type PropertyType
    {
      get { return _propertyType; }
    }

    public ReadOnlyCollection<Type> IndexParameterTypes
    {
      get { return _indexParameterTypes; }
    }

    public override string ToString ()
    {
      return string.Format ("{0}({1})", PropertyType, SeparatedStringBuilder.Build (",", IndexParameterTypes));
    }

    public virtual bool Equals (PropertySignature other)
    {
      return !ReferenceEquals (other, null)
          && PropertyType == other.PropertyType
          && IndexParameterTypes.SequenceEqual (other.IndexParameterTypes);
    }

    public sealed override bool Equals (object obj)
    {
      if (obj == null || obj.GetType () != GetType ())
        return false;

      var other = (PropertySignature) obj;
      return Equals (other);
    }

    public override int GetHashCode ()
    {
      return PropertyType.GetHashCode() ^ EqualityUtility.GetRotatedHashCode (IndexParameterTypes);
    }
  }
}