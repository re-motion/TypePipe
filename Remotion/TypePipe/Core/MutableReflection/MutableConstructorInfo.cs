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
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;
using Remotion.FunctionalProgramming;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Represents a <see cref="ConstructorInfo"/> that can be modified.
  /// </summary>
  [DebuggerDisplay ("{ToDebugString(),nq}")]
  public class MutableConstructorInfo : ConstructorInfo, IMutableMethodBase
  {
    private readonly MutableType _declaringType;
    private readonly MethodAttributes _attributes;
    private readonly ReadOnlyCollection<MutableParameterInfo> _parameters;
    private readonly ReadOnlyCollection<ParameterExpression> _parameterExpressions;

    private readonly CustomAttributeContainer _customAttributeContainer = new CustomAttributeContainer();

    private Expression _body;

    public MutableConstructorInfo (
        MutableType declaringType, MethodAttributes attributes, IEnumerable<ParameterDeclaration> parameters, Expression body)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("parameters", parameters);
      ArgumentUtility.CheckNotNull ("body", body);
      Assertion.IsTrue (body.Type == typeof (void));

      var parameterDeclarations = parameters.ConvertToCollection();

      _declaringType = declaringType;
      _attributes = attributes;
      _parameters = parameterDeclarations.Select ((p, i) => new MutableParameterInfo (this, i, p.Name, p.Type, p.Attributes)).ToList().AsReadOnly();
      _parameterExpressions = parameterDeclarations.Select (p => Expression.Parameter (p.Type, p.Name)).ToList().AsReadOnly();
      _body = body;
    }

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

    public ReadOnlyCollection<CustomAttributeDeclaration> AddedCustomAttributes
    {
      get { return _customAttributeContainer.AddedCustomAttributes; }
    }

    public ReadOnlyCollection<MutableParameterInfo> MutableParameters
    {
      get { return _parameters; }
    }

    public ReadOnlyCollection<ParameterExpression> ParameterExpressions
    {
      get { return _parameterExpressions; }
    }

    public Expression Body
    {
      get { return _body; }
    }

    public void SetBody (Func<ConstructorBodyModificationContext, Expression> bodyProvider)
    {
      ArgumentUtility.CheckNotNull ("bodyProvider", bodyProvider);

      var memberSelector = new MemberSelector (new BindingFlagsEvaluator());
      var context = new ConstructorBodyModificationContext (_declaringType, ParameterExpressions, _body, memberSelector);
      _body = BodyProviderUtility.GetTypedBody (typeof (void), bodyProvider, context);
    }

    public override ParameterInfo[] GetParameters ()
    {
      return _parameters.Cast<ParameterInfo>().ToArray();
    }

    public void AddCustomAttribute (CustomAttributeDeclaration customAttributeDeclaration)
    {
      ArgumentUtility.CheckNotNull ("customAttributeDeclaration", customAttributeDeclaration);

      _customAttributeContainer.AddCustomAttribute (customAttributeDeclaration);
    }

    public IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      return _customAttributeContainer.AddedCustomAttributes.Cast<ICustomAttributeData>();
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
      return string.Format ("MutableConstructor = \"{0}\", DeclaringType = \"{1}\"", ToString (), DeclaringType);
    }

    #region Not Implemented from ConstructorInfo interface

    public override MethodImplAttributes GetMethodImplementationFlags ()
    {
      // TODO
      throw new NotImplementedException();
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