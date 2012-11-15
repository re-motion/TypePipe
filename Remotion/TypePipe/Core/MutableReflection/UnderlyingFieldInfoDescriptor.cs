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
  public class UnderlyingFieldInfoDescriptor : UnderlyingInfoDescriptorBase<FieldInfo>
  {
    public static UnderlyingFieldInfoDescriptor Create (Type fieldType, string name, FieldAttributes attributes)
    {
      ArgumentUtility.CheckNotNull ("fieldType", fieldType);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);

      return new UnderlyingFieldInfoDescriptor (null, fieldType, name, attributes, EmptyCustomAttributeDataProvider);
    }

    public static UnderlyingFieldInfoDescriptor Create (FieldInfo originalField)
    {
      ArgumentUtility.CheckNotNull ("originalField", originalField);

      var customAttributeDataProvider = GetCustomAttributeProvider (originalField);

      return new UnderlyingFieldInfoDescriptor (
          originalField, originalField.FieldType, originalField.Name, originalField.Attributes, customAttributeDataProvider);
    }

    private readonly FieldInfo _underlyingSystemFieldInfo;
    private readonly FieldAttributes _attributes;
    private readonly Type _type;

    private UnderlyingFieldInfoDescriptor (
        FieldInfo underlyingSystemFieldInfo,
        Type fieldType,
        string name,
        FieldAttributes attributes,
        Func<ReadOnlyCollection<ICustomAttributeData>> customAttributeDataProvider)
      : base (underlyingSystemFieldInfo, name, customAttributeDataProvider)
    {
      Assertion.IsNotNull (fieldType);
      Assertion.IsNotNull (name);

      _underlyingSystemFieldInfo = underlyingSystemFieldInfo;
      _type = fieldType;
      _attributes = attributes;
    }

    public FieldInfo UnderlyingSystemFieldInfo
    {
      get { return _underlyingSystemFieldInfo; }
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