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
using System.Globalization;
using System.Linq;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Represents a method that does not exist yet. This is used to represent methods yet to be generated within an expression tree.
  /// </summary>
  public class MutableMethodInfo : MethodInfo
  {
    private readonly Type _declaringType;
    private readonly string _name;
    private readonly MethodAttributes _methodAttributes;
    private readonly Type _returnType;
    private readonly ReadOnlyCollection<MutableParameterInfo> _parameters;

    public MutableMethodInfo (
        Type declaringType,
        string name,
        MethodAttributes methodAttributes,
        Type returnType,
        IEnumerable<ParameterDeclaration> parameterDeclarations)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("returnType", returnType);
      ArgumentUtility.CheckNotNull ("parameterDeclarations", parameterDeclarations);

      _declaringType = declaringType;
      _name = name;
      _methodAttributes = methodAttributes;
      _returnType = returnType;
      _parameters = parameterDeclarations.Select ((pd, i) => MutableParameterInfo.CreateFromDeclaration (this, i, pd)).ToList().AsReadOnly();
    }

    public override Type DeclaringType
    {
      get { return _declaringType; }
    }

    public override string Name
    {
      get { return _name; }
    }

    public override MethodAttributes Attributes
    {
      get { return _methodAttributes; }
    }

    public override Type ReturnType
    {
      get { return _returnType; }
    }

    public override ParameterInfo[] GetParameters ()
    {
      return _parameters.ToArray();
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