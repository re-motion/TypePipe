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
  /// A custom <see cref="ConstructorInfo"/> that re-implements parts of the reflection API. Other classes may derive from this class to inherit the 
  /// implementation. Note that the equality members <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/> are implemented for
  /// reference equality.
  /// </summary>
  [DebuggerDisplay ("{ToDebugString(),nq}")]
  public abstract class CustomConstructorInfo : ConstructorInfo, ICustomAttributeDataProvider
  {
    private readonly CustomType _declaringType;
    private readonly MethodAttributes _attributes;

    protected CustomConstructorInfo (CustomType declaringType, MethodAttributes attributes)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      Assertion.IsTrue (attributes.IsSet (MethodAttributes.SpecialName | MethodAttributes.RTSpecialName));

      _declaringType = declaringType;
      _attributes = attributes;
    }

    public abstract IEnumerable<ICustomAttributeData> GetCustomAttributeData ();
    public abstract override ParameterInfo[] GetParameters ();

    public override Type DeclaringType
    {
      get { return _declaringType; }
    }

    public override string Name
    {
      get { return IsStatic ? TypeConstructorName : ConstructorName; }
    }

    public override MethodAttributes Attributes
    {
      get { return _attributes; }
    }

    public override CallingConventions CallingConvention
    {
      get { return IsStatic ? CallingConventions.Standard : CallingConventions.HasThis; }
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
      return SignatureDebugStringGenerator.GetConstructorSignature (this);
    }

    public string ToDebugString ()
    {
      return string.Format ("{0} = \"{1}\", DeclaringType = \"{2}\"", GetType().Name.Replace ("Info", ""), ToString(), DeclaringType);
    }

    #region Not Implemented from ConstructorInfo interface

    public override MethodImplAttributes GetMethodImplementationFlags ()
    {
      throw new NotImplementedException ();
    }

    #endregion

    #region Unsupported Members

    public override RuntimeMethodHandle MethodHandle
    {
      get { throw new NotSupportedException ("Property MethodHandle is not supported."); }
    }

    public override Type ReflectedType
    {
      get { throw new NotSupportedException ("Property ReflectedType is not supported."); }
    }

    public override object Invoke (object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
    {
      throw new NotSupportedException ("Method Invoke is not supported.");
    }

    public override object Invoke (BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
    {
      throw new NotSupportedException ("Method Invoke is not supported.");
    }

    #endregion
  }
}