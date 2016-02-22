// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.MemberSignatures
{
  /// <summary>
  /// Represents a property signature and allows signatures to be compared to each other.
  /// </summary>
  public class PropertySignature : IMemberSignature, IEquatable<PropertySignature>
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
      return string.Format ("{0}({1})", PropertyType, string.Join (",", IndexParameterTypes));
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

    bool IEquatable<IMemberSignature>.Equals (IMemberSignature other)
    {
      return Equals (other);
    }

    public override int GetHashCode ()
    {
      return PropertyType.GetHashCode() ^ EqualityUtility.GetRotatedHashCode (IndexParameterTypes);
    }
  }
}