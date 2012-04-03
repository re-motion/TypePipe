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
  public class MutableFieldInfo : FieldInfo
  {
    private readonly Type _declaringType;
    private readonly Type _fieldType;
    private readonly string _name;
    private readonly FieldAttributes _attributes;
    private readonly List<CustomAttributeDeclaration> _addedCustomAttributeDeclarations = new List<CustomAttributeDeclaration>();

    public MutableFieldInfo (Type declaringType, Type fieldType, string name, FieldAttributes attributes)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("fieldType", fieldType);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);

      _declaringType = declaringType;
      _name = name;
      _fieldType = fieldType;
      _attributes = attributes;
    }

    public override Type DeclaringType
    {
      get { return _declaringType; }
    }

    public bool IsNewField
    {
      get { return true; }
    }

    public override Type FieldType
    {
      get { return _fieldType; }
    }

    public override string Name
    {
      get { return _name; }
    }

    public override FieldAttributes Attributes
    {
      get { return _attributes; }
    }

    public override string ToString ()
    {
      return FieldType + " " + Name;
    }

    public string ToDebugString ()
    {
      return string.Format ("MutableField = \"{0} {1}\", DeclaringType = \"{2}\"", FieldType.Name, Name, DeclaringType.Name);
    }

    public ReadOnlyCollection<CustomAttributeDeclaration> AddedCustomAttributeDeclarations
    {
      get { return _addedCustomAttributeDeclarations.AsReadOnly(); }
    }

    public void AddCustomAttribute (CustomAttributeDeclaration customAttributeDeclaration)
    {
      ArgumentUtility.CheckNotNull ("customAttributeDeclaration", customAttributeDeclaration);

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