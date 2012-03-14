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
using Remotion.FunctionalProgramming;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Represents a property that does not exist yet. This is used to represent properties yet to be generated within an expression tree.
  /// </summary>
  public class MutablePropertyInfo : PropertyInfo
  {
    private readonly Type _declaringType;
    private readonly Type _propertyType;
    private readonly Maybe<MethodInfo> _getMethod;
    private readonly Maybe<MethodInfo> _setMethod;

    // TODO: Discuss usage of Maybe with MK.
    public MutablePropertyInfo (Type declaringType, Type propertyType, Maybe<MethodInfo> getMethod, Maybe<MethodInfo> setMethod)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("propertyType", propertyType);
      ArgumentUtility.CheckNotNull ("getMethod", getMethod);
      ArgumentUtility.CheckNotNull ("setMethod", setMethod);

      if (!getMethod.HasValue && !setMethod.HasValue)
        throw new ArgumentException ("At least one of the accessors must be specified.");

      _declaringType = declaringType;
      _propertyType = propertyType;
      _getMethod = getMethod;
      _setMethod = setMethod;
    }

    public override Type DeclaringType
    {
      get { return _declaringType; }
    }

    public override Type PropertyType
    {
      get { return _propertyType; }
    }

    public override MethodInfo GetGetMethod (bool nonPublic)
    {
      return _getMethod.ValueOrDefault();
    }

    public override MethodInfo GetSetMethod (bool nonPublic)
    {
      return _setMethod.ValueOrDefault();
    }

    public override bool CanRead
    {
      get { throw new NotImplementedException (); }
    }

    public override bool CanWrite
    {
      get { return true; }
    }

    #region Not Implemented from PropertyInfo interface

    public override object[] GetCustomAttributes (bool inherit)
    {
      throw new NotImplementedException();
    }

    public override bool IsDefined (Type attributeType, bool inherit)
    {
      throw new NotImplementedException();
    }

    public override object GetValue (object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    public override void SetValue (object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    public override MethodInfo[] GetAccessors (bool nonPublic)
    {
      throw new NotImplementedException();
    }

    public override ParameterInfo[] GetIndexParameters ()
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

    public override PropertyAttributes Attributes
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