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
  public class MutableConstructorInfo : CustomConstructorInfo, IMutableMethodBase
  {
    private readonly ReadOnlyCollection<MutableParameterInfo> _parameters;
    private readonly ReadOnlyCollection<ParameterExpression> _parameterExpressions;

    private readonly CustomAttributeContainer _customAttributeContainer = new CustomAttributeContainer();

    private Expression _body;

    public MutableConstructorInfo (
        ProxyType declaringType, MethodAttributes attributes, IEnumerable<ParameterDeclaration> parameters, Expression body)
        : base (declaringType, attributes)
    {
      ArgumentUtility.CheckNotNull ("parameters", parameters);
      ArgumentUtility.CheckNotNull ("body", body);
      Assertion.IsTrue (body.Type == typeof (void));

      var paras = parameters.ConvertToCollection();

      _parameters = paras.Select ((p, i) => new MutableParameterInfo (this, i, p.Name, p.Type, p.Attributes)).ToList().AsReadOnly();
      _parameterExpressions = paras.Select (p => p.Expression).ToList().AsReadOnly();
      _body = body;
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
      var context = new ConstructorBodyModificationContext ((ProxyType) DeclaringType, IsStatic, ParameterExpressions, _body, memberSelector);
      _body = BodyProviderUtility.GetTypedBody (typeof (void), bodyProvider, context);
    }

    public override ParameterInfo[] GetParameters ()
    {
      return _parameters.Cast<ParameterInfo>().ToArray();
    }

    public void AddCustomAttribute (CustomAttributeDeclaration customAttribute)
    {
      ArgumentUtility.CheckNotNull ("customAttribute", customAttribute);

      _customAttributeContainer.AddCustomAttribute (customAttribute);
    }

    public override IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      return _customAttributeContainer.AddedCustomAttributes.Cast<ICustomAttributeData>();
    }
  }
}