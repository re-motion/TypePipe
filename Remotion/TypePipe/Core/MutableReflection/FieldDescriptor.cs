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
using System.Collections.ObjectModel;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Defines the characteristics of a field.
  /// </summary>
  /// <remarks>
  /// This is used by <see cref="MutableFieldInfo"/> to represent the original field, before any mutations.
  /// </remarks>
  public class FieldDescriptor : DescriptorBase<FieldInfo>
  {
    public static FieldDescriptor Create (string name, Type type, FieldAttributes attributes)
    {
      ArgumentUtility.CheckNotNull ("type", type);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);

      return new FieldDescriptor (null, type, name, attributes, EmptyCustomAttributeDataProvider);
    }

    public static FieldDescriptor Create (FieldInfo underlyingField)
    {
      ArgumentUtility.CheckNotNull ("underlyingField", underlyingField);

      var customAttributeDataProvider = GetCustomAttributeProvider (underlyingField);

      return new FieldDescriptor (
          underlyingField, underlyingField.FieldType, underlyingField.Name, underlyingField.Attributes, customAttributeDataProvider);
    }

    private readonly FieldAttributes _attributes;
    private readonly Type _type;

    private FieldDescriptor (
        FieldInfo underlyingField,
        Type fieldType,
        string name,
        FieldAttributes attributes,
        Func<ReadOnlyCollection<ICustomAttributeData>> customAttributeDataProvider)
      : base (underlyingField, name, customAttributeDataProvider)
    {
      Assertion.IsNotNull (fieldType);
      Assertion.IsNotNull (name);

      _type = fieldType;
      _attributes = attributes;
    }

    public Type Type
    {
      get { return _type; }
    }

    public FieldAttributes Attributes
    {
      get { return _attributes; }
    }
  }
}