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
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// A custom <see cref="FieldInfo"/> that re-implements parts of the reflection API. Other classes may derive from this class to inherit the 
  /// implementation. Note that the equality members <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/> are implemented for
  /// reference equality.
  /// </summary>
  [DebuggerDisplay ("{ToDebugString(),nq}")]
  public abstract class CustomFieldInfo : FieldInfo, ICustomAttributeDataProvider
  {
    private readonly CustomType _declaringType;
    private readonly string _name;
    private readonly Type _type;
    private readonly FieldAttributes _attributes;

    protected CustomFieldInfo (CustomType declaringType, string name, Type type, FieldAttributes attributes)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("type", type);
      Assertion.IsTrue (type != typeof (void));

      _declaringType = declaringType;
      _name = name;
      _type = type;
      _attributes = attributes;
    }

    public abstract IEnumerable<ICustomAttributeData> GetCustomAttributeData ();

    public override Type DeclaringType
    {
      get { return _declaringType; }
    }

    public override string Name
    {
      get { return _name; }
    }

    public override Type FieldType
    {
      get { return _type; }
    }

    public override FieldAttributes Attributes
    {
      get { return _attributes; }
    }

    public IEnumerable<ICustomAttributeData> GetCustomAttributeData (bool inherit)
    {
      return TypePipeCustomAttributeData.GetCustomAttributes (this, inherit);
    }

    public override object[] GetCustomAttributes (bool inherit)
    {
      return CustomAttributeFinder.GetCustomAttributes (this, inherit);
    }

    public override object[] GetCustomAttributes (Type attributeType, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("attributeType", attributeType);

      return CustomAttributeFinder.GetCustomAttributes (this, attributeType, inherit);
    }

    public override bool IsDefined (Type attributeType, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("attributeType", attributeType);

      return CustomAttributeFinder.IsDefined (this, attributeType, inherit);
    }

    public override string ToString ()
    {
      return SignatureDebugStringGenerator.GetFieldSignature (this);
    }

    public string ToDebugString ()
    {
      return string.Format ("{0} = \"{1}\", DeclaringType = \"{2}\"", GetType().Name.Replace ("Info", ""), ToString(), DeclaringType);
    }

    #region Unsupported Members

    public override RuntimeFieldHandle FieldHandle
    {
      get { throw new NotSupportedException ("Property FieldHandle is not supported."); }
    }

    public override Type ReflectedType
    {
      get { throw new NotSupportedException ("Property ReflectedType is not supported."); }
    }

    public override object GetValue (object obj)
    {
      throw new NotSupportedException ("Method GetValue is not supported.");
    }

    public override void SetValue (object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
    {
      throw new NotSupportedException ("Method SetValue is not supported.");
    }

    #endregion
  }
}