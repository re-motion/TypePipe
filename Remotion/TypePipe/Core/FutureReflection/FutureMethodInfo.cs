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

namespace Remotion.TypePipe.FutureReflection
{
  /// <summary>
  /// Represents a method that does not exist yet. This is used to represent methods yet to be generated within an expression tree.
  /// </summary>
  public class FutureMethodInfo : MethodInfo
  {
    private readonly Type _declaringType;
    private readonly MethodAttributes _methodAttributes;
    private readonly ParameterInfo[] _parameters;

    public FutureMethodInfo (Type declaringType, MethodAttributes methodAttributes, ParameterInfo[] parameters)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("parameters", parameters);

      _declaringType = declaringType;
      _methodAttributes = methodAttributes;
      _parameters = parameters;
    }

    public override Type DeclaringType
    {
      get { return _declaringType; }
    }

    public override MethodAttributes Attributes
    {
      get { return _methodAttributes; }
    }

    public override ParameterInfo[] GetParameters ()
    {
      return _parameters;
    }

    #region Not Implemented from MethodInfo interface

    public override object[] GetCustomAttributes (bool inherit)
    {
      throw new NotImplementedException();
    }

    public override bool IsDefined (Type attributeType, bool inherit)
    {
      throw new NotImplementedException();
    }

    public override MethodImplAttributes GetMethodImplementationFlags ()
    {
      throw new NotImplementedException();
    }

    public override object Invoke (object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    public override MethodInfo GetBaseDefinition ()
    {
      throw new NotImplementedException();
    }

    public override ICustomAttributeProvider ReturnTypeCustomAttributes
    {
      get { throw new NotImplementedException(); }
    }

    public override string Name
    {
      get { throw new NotImplementedException(); }
    }

    public override Type ReflectedType
    {
      get { throw new NotImplementedException(); }
    }

    public override RuntimeMethodHandle MethodHandle
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