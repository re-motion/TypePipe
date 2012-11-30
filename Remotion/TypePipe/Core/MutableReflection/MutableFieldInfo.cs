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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Remotion.TypePipe.MutableReflection.Descriptors;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Represents a <see cref="FieldInfo"/> that can be modified.
  /// </summary>
  [DebuggerDisplay ("{ToDebugString(),nq}")]
  public class MutableFieldInfo : FieldInfo, IMutableMember
  {
    private readonly MutableType _declaringType;
    private readonly FieldDescriptor _descriptor;
    private readonly List<CustomAttributeDeclaration> _addedCustomAttributeDeclarations = new List<CustomAttributeDeclaration>();
    private readonly DoubleCheckedLockingContainer<ReadOnlyCollection<ICustomAttributeData>> _customAttributeDatas;

    public MutableFieldInfo (MutableType declaringType, FieldDescriptor descriptor)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("descriptor", descriptor);

      _declaringType = declaringType;
      _descriptor = descriptor;

      _customAttributeDatas = new DoubleCheckedLockingContainer<ReadOnlyCollection<ICustomAttributeData>> (descriptor.CustomAttributeDataProvider);
    }

    public override Type DeclaringType
    {
      get { return _declaringType; }
    }

    public FieldInfo UnderlyingSystemFieldInfo
    {
      get { return _descriptor.UnderlyingSystemInfo ?? this; }
    }

    public bool IsNew
    {
      get { return _descriptor.UnderlyingSystemInfo == null; }
    }

    public bool IsModified
    {
      get { return _addedCustomAttributeDeclarations.Count != 0; }
    }

    public override Type FieldType
    {
      get { return _descriptor.Type; }
    }

    public override string Name
    {
      get { return _descriptor.Name; }
    }

    public override FieldAttributes Attributes
    {
      get { return _descriptor.Attributes; }
    }

    public ReadOnlyCollection<CustomAttributeDeclaration> AddedCustomAttributeDeclarations
    {
      get { return _addedCustomAttributeDeclarations.AsReadOnly(); }
    }

    public override string ToString ()
    {
      return SignatureDebugStringGenerator.GetFieldSignature (this);
    }

    public string ToDebugString ()
    {
      return string.Format ("MutableField = \"{0}\", DeclaringType = \"{1}\"", ToString(), DeclaringType);
    }

    public IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      // TODO: 4695
      Assertion.IsTrue (IsNew || _addedCustomAttributeDeclarations.Count == 0);

      return IsNew ? AddedCustomAttributeDeclarations.Cast<ICustomAttributeData>() : _customAttributeDatas.Value;
    }

    public override object[] GetCustomAttributes (bool inherit)
    {
      return TypePipeCustomAttributeImplementationUtility.GetCustomAttributes (this, inherit);
    }

    public override object[] GetCustomAttributes (Type attributeType, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("attributeType", attributeType);

      return TypePipeCustomAttributeImplementationUtility.GetCustomAttributes (this, attributeType, inherit);
    }

    public override bool IsDefined (Type attributeType, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("attributeType", attributeType);

      return TypePipeCustomAttributeImplementationUtility.IsDefined (this, attributeType, inherit);
    }

    public void AddCustomAttribute (CustomAttributeDeclaration customAttributeDeclaration)
    {
      ArgumentUtility.CheckNotNull ("customAttributeDeclaration", customAttributeDeclaration);

      // TODO: 4695
      if (!IsNew)
        throw new NotSupportedException ("Adding attributes to existing fields is not supported.");

      _addedCustomAttributeDeclarations.Add (customAttributeDeclaration);
    }

    #region Not Implemented from FieldInfo interface

    public override object GetValue (object obj)
    {
      throw new NotImplementedException();
    }

    public override void SetValue (object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    public override Type ReflectedType
    {
      get { throw new NotImplementedException(); }
    }

    public override RuntimeFieldHandle FieldHandle
    {
      get { throw new NotImplementedException(); }
    }

    #endregion
  }
}