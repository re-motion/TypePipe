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
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Represents a field that does not exist yet. This is used to represent fields yet to be generated within an expression tree.
  /// </summary>
  [DebuggerDisplay ("{ToDebugString(),nq}")]
  public class MutableFieldInfo : FieldInfo, IMutableMember
  {
    private readonly MutableType _declaringType;
    private readonly UnderlyingFieldInfoDescriptor _underlyingFieldInfoDescriptor;
    private readonly List<CustomAttributeDeclaration> _addedCustomAttributeDeclarations = new List<CustomAttributeDeclaration>();

    public MutableFieldInfo (MutableType declaringType, UnderlyingFieldInfoDescriptor underlyingFieldInfoDescriptor)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("underlyingFieldInfoDescriptor", underlyingFieldInfoDescriptor);

      _declaringType = declaringType;
      _underlyingFieldInfoDescriptor = underlyingFieldInfoDescriptor;
    }

    public override Type DeclaringType
    {
      get { return _declaringType; }
    }

    MutableType IMutableMember.DeclaringType
    {
      get { return _declaringType; }
    }

    public FieldInfo UnderlyingSystemFieldInfo
    {
      get { return _underlyingFieldInfoDescriptor.UnderlyingSystemFieldInfo ?? this; }
    }

    public bool IsNew
    {
      get { return _underlyingFieldInfoDescriptor.UnderlyingSystemFieldInfo == null; }
    }

    public bool IsModified
    {
      get { return _addedCustomAttributeDeclarations.Count != 0; }
    }

    public override Type FieldType
    {
      get { return _underlyingFieldInfoDescriptor.FieldType; }
    }

    public override string Name
    {
      get { return _underlyingFieldInfoDescriptor.Name; }
    }

    public override FieldAttributes Attributes
    {
      get { return _underlyingFieldInfoDescriptor.Attributes; }
    }

    public override string ToString ()
    {
      return SignatureDebugStringGenerator.GetFieldSignature (this);
    }

    public string ToDebugString ()
    {
      return string.Format ("MutableField = \"{0}\", DeclaringType = \"{1}\"", ToString(), DeclaringType.Name);
    }

    public ReadOnlyCollection<CustomAttributeDeclaration> AddedCustomAttributeDeclarations
    {
      get { return _addedCustomAttributeDeclarations.AsReadOnly(); }
    }

    public void AddCustomAttribute (CustomAttributeDeclaration customAttributeDeclaration)
    {
      ArgumentUtility.CheckNotNull ("customAttributeDeclaration", customAttributeDeclaration);

      if (!IsNew)
        throw new NotSupportedException ("Adding attributes to existing fields is not supported.");

      _addedCustomAttributeDeclarations.Add (customAttributeDeclaration);
    }

    public override object[] GetCustomAttributes (bool inherit)
    {
      return AddedCustomAttributeDeclarations
          .Select (attr => attr.CreateInstance())
          .ToArray();
    }

    public override object[] GetCustomAttributes (Type attributeType, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("attributeType", attributeType);
      return AddedCustomAttributeDeclarations
          .Where (attr => attributeType.IsAssignableFrom (attr.AttributeConstructorInfo.DeclaringType))
          .Select (attr => attr.CreateInstance())
          .ToArray();
    }

    #region Not Implemented from FieldInfo interface

    public override bool IsDefined (Type attributeType, bool inherit)
    {
      throw new NotImplementedException();
    }

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