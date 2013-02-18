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
using Remotion.Collections;
using Remotion.Reflection.MemberSignatures;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;
using Remotion.FunctionalProgramming;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Represents a <see cref="MethodInfo"/> that can be modified.
  /// </summary>
  public class MutableMethodInfo : CustomMethodInfo, IMutableMethodBase
  {
    private readonly MutableParameterInfo _returnParameter;
    private readonly ReadOnlyCollection<MutableParameterInfo> _parameters;
    private readonly ReadOnlyCollection<ParameterExpression> _parameterExpressions;
    private readonly MethodInfo _baseMethod;

    private readonly CustomAttributeContainer _customAttributeContainer = new CustomAttributeContainer();
    private readonly HashSet<MethodInfo> _addedExplicitBaseDefinitions = new HashSet<MethodInfo>();

    private Expression _body;

    public MutableMethodInfo (
        ProxyType declaringType,
        string name,
        MethodAttributes attributes,
        Type returnType,
        IEnumerable<ParameterDeclaration> parameters,
        MethodInfo baseMethod,
        Expression body)
        : base (declaringType, name, attributes)
    {
      ArgumentUtility.CheckNotNull ("returnType", returnType);
      ArgumentUtility.CheckNotNull ("parameters", parameters);
      Assertion.IsTrue (baseMethod == null || (baseMethod.IsVirtual && attributes.IsSet (MethodAttributes.Virtual)));
      Assertion.IsTrue (body != null || attributes.IsSet (MethodAttributes.Abstract));
      Assertion.IsTrue (body == null || returnType.IsAssignableFromFast (body.Type));

      var paras = parameters.ConvertToCollection();

      _returnParameter = new MutableParameterInfo (this, -1, null, returnType, ParameterAttributes.None);
      _parameters = paras.Select ((p, i) => new MutableParameterInfo (this, i, p.Name, p.Type, p.Attributes)).ToList().AsReadOnly();
      _parameterExpressions = paras.Select (p => p.Expression).ToList().AsReadOnly();
      _baseMethod = baseMethod;
      _body = body;
    }

    public override Type ReturnType
    {
      get { return _returnParameter.ParameterType; }
    }

    public override ParameterInfo ReturnParameter
    {
      get { return _returnParameter; }
    }

    public MutableParameterInfo MutableReturnParameter
    {
      get { return _returnParameter; }
    }

    public ReadOnlyCollection<MutableParameterInfo> MutableParameters
    {
      get { return _parameters; }
    }

    public ReadOnlyCollection<ParameterExpression> ParameterExpressions
    {
      get { return _parameterExpressions; }
    }

    public override MethodAttributes Attributes
    {
      get
      {
        if (_body != null)
          return base.Attributes.Unset (MethodAttributes.Abstract);

        return base.Attributes;
      }
    }

    public MethodInfo BaseMethod
    {
      get { return _baseMethod; }
    }

    public ReadOnlyCollection<CustomAttributeDeclaration> AddedCustomAttributes
    {
      get { return _customAttributeContainer.AddedCustomAttributes; }
    }

    public Expression Body
    {
      get
      {
        if (IsAbstract)
          throw new InvalidOperationException ("An abstract method has no body.");

        return _body;
      }
    }

    /// <summary>
    /// Returns all root <see cref="MethodInfo"/> instances that were added via <see cref="AddExplicitBaseDefinition"/>.
    /// </summary>
    public ReadOnlyCollectionDecorator<MethodInfo> AddedExplicitBaseDefinitions
    {
      get { return _addedExplicitBaseDefinitions.AsReadOnly (); }
    }

    public override MethodInfo GetBaseDefinition ()
    {
      return BaseMethod != null ? BaseMethod.GetBaseDefinition() : this;
    }

    public override ParameterInfo[] GetParameters ()
    {
      return _parameters.Cast<ParameterInfo>().ToArray();
    }

    /// <summary>
    /// Adds an explicit base definition, i.e., a root <see cref="MethodInfo"/> explicitly overridden by this <see cref="MethodInfo"/>.
    /// </summary>
    /// <param name="overriddenMethodBaseDefinition">The overridden method base definition.</param>
    /// <remarks>
    /// This method does not affect <see cref="GetBaseDefinition"/> or <see cref="BaseMethod"/>, both of which only return implicitly overridden 
    /// methods. Methods can override both a single method implicitly and multiple methods explicitly.
    /// </remarks>
    public void AddExplicitBaseDefinition (MethodInfo overriddenMethodBaseDefinition)
    {
      ArgumentUtility.CheckNotNull ("overriddenMethodBaseDefinition", overriddenMethodBaseDefinition);

      if (!IsVirtual)
      {
        // TODO 4695: Adapt message
        var message = string.Format ("Cannot add an explicit base definition to the non-virtual method '{0}'.", Name);
        throw new NotSupportedException (message);
      }

      if (!overriddenMethodBaseDefinition.IsVirtual || overriddenMethodBaseDefinition.IsFinal)
        throw new ArgumentException ("Method must be virtual and non-final.", "overriddenMethodBaseDefinition");

      if (!MethodSignature.AreEqual (this, overriddenMethodBaseDefinition))
        throw new ArgumentException ("Method signatures must be equal.", "overriddenMethodBaseDefinition");

      if (!overriddenMethodBaseDefinition.DeclaringType.IsAssignableFromFast (DeclaringType))
        throw new ArgumentException ("The overridden method must be from the same type hierarchy.", "overriddenMethodBaseDefinition");

      if (overriddenMethodBaseDefinition.GetBaseDefinition () != overriddenMethodBaseDefinition)
      {
        throw new ArgumentException (
            "The given method must be a root method definition. (Use GetBaseDefinition to get a root method.)",
            "overriddenMethodBaseDefinition");
      }

      // TODO: check all mutable methods not just the current one
      if (_addedExplicitBaseDefinitions.Contains (overriddenMethodBaseDefinition))
        throw new InvalidOperationException ("The given method has already been added to the list of explicit base definitions.");

      _addedExplicitBaseDefinitions.Add (overriddenMethodBaseDefinition);
    }

    public void SetBody (Func<MethodBodyModificationContext, Expression> bodyProvider)
    {
      ArgumentUtility.CheckNotNull ("bodyProvider", bodyProvider);

      var memberSelector = new MemberSelector (new BindingFlagsEvaluator());
      var context = new MethodBodyModificationContext (
          (ProxyType) DeclaringType, IsStatic, _parameterExpressions, ReturnType, _baseMethod, _body, memberSelector);
      var newBody = BodyProviderUtility.GetTypedBody (ReturnType, bodyProvider, context);

      _body = newBody;
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