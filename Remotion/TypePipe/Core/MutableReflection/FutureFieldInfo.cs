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
using System.Globalization;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Represents a field that does not exist yet. This is used to represent fields yet to be generated within an expression tree.
  /// </summary>
  public class FutureFieldInfo : FieldInfo
  {
    private readonly Type _declaringType;
    private readonly string _name;
    private readonly Type _fieldType;
    private readonly FieldAttributes _fieldAttributes;

    public FutureFieldInfo (Type declaringType, string name, Type fieldType, FieldAttributes fieldAttributes)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("fieldType", fieldType);

      _declaringType = declaringType;
      _name = name;
      _fieldType = fieldType;
      _fieldAttributes = fieldAttributes;
    }

    public override Type DeclaringType
    {
      get { return _declaringType; }
    }

    public override string Name
    {
      get { return _name; }
    }

    public override FieldAttributes Attributes
    {
      get { return _fieldAttributes; }
    }

    public override Type FieldType
    {
      get { return _fieldType; }
    }

    #region Not Implemented from FieldInfo interface

    public override object[] GetCustomAttributes (bool inherit)
    {
      throw new NotImplementedException();
    }

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

    public override object[] GetCustomAttributes (Type attributeType, bool inherit)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}