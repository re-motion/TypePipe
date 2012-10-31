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

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions
{
  /// <summary>
  /// Represents a field on a constructed type.
  /// </summary>
  public class FieldOnTypeInstantiation : FieldInfo
  {
    private readonly Type _constructedDeclaringType;
    private readonly FieldInfo _genericField;

    public FieldOnTypeInstantiation (Type constructedDeclaringType, FieldInfo genericField)
    {
      ArgumentUtility.CheckNotNull ("constructedDeclaringType", constructedDeclaringType);
      ArgumentUtility.CheckNotNull ("genericField", genericField);

      _constructedDeclaringType = constructedDeclaringType;
      _genericField = genericField;
    }

    public override Type DeclaringType
    {
      get { return _constructedDeclaringType; }
    }

    public FieldInfo GenericField
    {
      get { return _genericField; }
    }

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

    public override string Name
    {
      get { throw new NotImplementedException(); }
    }

    public override Type ReflectedType
    {
      get { throw new NotImplementedException(); }
    }

    public override Type FieldType
    {
      get { throw new NotImplementedException(); }
    }

    public override FieldAttributes Attributes
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
  }
}