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
using System.Linq;
using System.Reflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Represents a <see cref="FieldInfo"/> that can be modified.
  /// </summary>
  public class MutableFieldInfo : CustomFieldInfo, IMutableMember
  {
    private readonly CustomAttributeContainer _customAttributeContainer = new CustomAttributeContainer();

    public MutableFieldInfo (MutableType declaringType, string name, Type type, FieldAttributes attributes)
        : base (declaringType, name, type, attributes)
    {
    }

    public MutableType MutableDeclaringType
    {
      get { return (MutableType) DeclaringType; }
    }

    public override FieldAttributes Attributes
    {
      get
      {
        if (_customAttributeContainer.AddedCustomAttributes.Any (a => a.Type == typeof (NonSerializedAttribute)))
          return base.Attributes | FieldAttributes.NotSerialized;

        return base.Attributes;
      }
    }

    public IReadOnlyCollection<CustomAttributeDeclaration> AddedCustomAttributes
    {
      get { return _customAttributeContainer.AddedCustomAttributes; }
    }

    public void AddCustomAttribute (CustomAttributeDeclaration customAttribute)
    {
      ArgumentUtility.CheckNotNull ("customAttribute", customAttribute);

      _customAttributeContainer.AddCustomAttribute (customAttribute);
    }

    public override IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      return _customAttributeContainer.AddedCustomAttributes.Cast<ICustomAttributeData>();
    }
  }
}