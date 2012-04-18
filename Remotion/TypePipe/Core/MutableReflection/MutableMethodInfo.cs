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
using Microsoft.Scripting.Ast;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Represents a method that does not exist yet. This is used to represent methods yet to be generated within an expression tree.
  /// </summary>
  [DebuggerDisplay ("{ToDebugString(),nq}")]
  public class MutableMethodInfo : MethodInfo, IMutableMethodBase
  {
    private readonly MutableType _declaringType;
    private readonly UnderlyingMethodInfoDescriptor _underlyingMethodInfoDescriptor;
    private readonly ReadOnlyCollection<MutableParameterInfo> _parameters;

    private Expression _body;

    public MutableMethodInfo (MutableType declaringType, UnderlyingMethodInfoDescriptor underlyingMethodInfoDescriptor)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("underlyingMethodInfoDescriptor", underlyingMethodInfoDescriptor);

      _declaringType = declaringType;
      _underlyingMethodInfoDescriptor = underlyingMethodInfoDescriptor;

      _parameters = _underlyingMethodInfoDescriptor.ParameterDeclarations
          .Select ((pd, i) => MutableParameterInfo.CreateFromDeclaration (this, i, pd))
          .ToList()
          .AsReadOnly();

      _body = _underlyingMethodInfoDescriptor.Body;
    }

    public override Type DeclaringType
    {
      get { return _declaringType; }
    }

    MutableType IMutableMethodBase.DeclaringType
    {
      get { return _declaringType; }
    }

    public MethodInfo UnderlyingSystemMethodInfo
    {
      get { return _underlyingMethodInfoDescriptor.UnderlyingSystemMethodBase ?? this; }
    }

    public bool IsNew
    {
      get { return _underlyingMethodInfoDescriptor.UnderlyingSystemMethodBase == null; }
    }

    public bool IsModified
    {
      get { return _body != _underlyingMethodInfoDescriptor.Body; }
    }

    public override string Name
    {
      get { return _underlyingMethodInfoDescriptor.Name; }
    }

    public override MethodAttributes Attributes
    {
      get { return _underlyingMethodInfoDescriptor.Attributes; }
    }

    public override CallingConventions CallingConvention
    {
      get { return IsStatic ? CallingConventions.Standard : CallingConventions.HasThis; }
    }

    public override Type ReturnType
    {
      get { return _underlyingMethodInfoDescriptor.ReturnType; }
    }

    public IEnumerable<ParameterExpression> ParameterExpressions
    {
      get { return _underlyingMethodInfoDescriptor.ParameterDeclarations.Select (pd => pd.Expression); }
    }

    public Expression Body
    {
      get { return _body; }
    }

    public bool CanSetBody
    {
      // TODO 4695
      get { return IsNew || IsVirtual; }
    }

    public void SetBody (Func<MethodBodyModificationContext, Expression> bodyProvider)
    {
      ArgumentUtility.CheckNotNull ("bodyProvider", bodyProvider);

      if (!CanSetBody)
      {
        var message = string.Format ("The body of the existing non-virtual method '{0}' cannot be replaced.", Name);
        throw new NotSupportedException (message);
      }

      var context = new MethodBodyModificationContext (_declaringType, ParameterExpressions, _body, IsStatic);
      _body = BodyProviderUtility.GetTypedBody (ReturnType, bodyProvider, context);
    }

    public override string ToString ()
    {
      return SignatureDebugStringGenerator.GetMethodSignatureString (this);
    }

    public string ToDebugString ()
    {
      return string.Format ("MutableMethod = \"{0}\", DeclaringType = \"{1}\"", ToString(), DeclaringType.Name);
    }

    public override ParameterInfo[] GetParameters ()
    {
      return _parameters.ToArray();
    }

    #region Not YET Implemented from MethodInfo interface

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

    public override Type[] GetGenericArguments ()
    {
      throw new NotImplementedException ();
    }

    public override MethodInfo GetGenericMethodDefinition ()
    {
      throw new NotImplementedException ();
    }

    public override MethodInfo MakeGenericMethod (params Type[] typeArguments)
    {
      throw new NotImplementedException ();
    }

    public override ParameterInfo ReturnParameter
    {
      get { throw new NotImplementedException (); }
    }

    public override bool IsGenericMethodDefinition
    {
      get { return base.IsGenericMethodDefinition; }
    }

    public override bool ContainsGenericParameters
    {
      get { return base.ContainsGenericParameters; }
    }

    public override bool IsGenericMethod
    {
      get { return base.IsGenericMethod; }
    }

    public override MethodBody GetMethodBody ()
    {
      throw new NotImplementedException ();
    }

    #endregion

    #region Unsupported Members

    public override int MetadataToken
    {
      get { throw new NotSupportedException ("Property MutableMethodInfo.MetadataToken is not supported."); }
    }

    public override Module Module
    {
      get { throw new NotSupportedException ("Property MutableMethodInfo.Module is not supported."); }
    }

    #endregion
  }
}