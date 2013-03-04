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
using System.Linq;
using System.Reflection;
using Microsoft.Scripting.Ast;
using Remotion.FunctionalProgramming;
using Remotion.Reflection.MemberSignatures;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Implementation.MemberFactory
{
  /// <summary>
  /// A factory for creating mutable methods.
  /// </summary>
  public class MethodFactory
  {
    private struct MethodSignatureItems
    {
      public readonly ICollection<MutableGenericParameter> GenericParameters;
      public readonly Type ReturnType;
      public readonly ICollection<ParameterDeclaration> ParameterDeclarations;

      public MethodSignatureItems (
          ICollection<MutableGenericParameter> genericParameters, Type returnType, ICollection<ParameterDeclaration> parameterDeclarations)
      {
        ReturnType = returnType;
        ParameterDeclarations = parameterDeclarations;
        GenericParameters = genericParameters;
      }
    }

    private static readonly IMethodSignatureStringBuilderHelper s_methodSignatureStringBuilderHelper =
        new GenericParameterCompatibleMethodSignatureStringBuilderHelper();

    private readonly IMemberSelector _memberSelector;
    private readonly IRelatedMethodFinder _relatedMethodFinder;

    public MethodFactory (IMemberSelector memberSelector, IRelatedMethodFinder relatedMethodFinder)
    {
      ArgumentUtility.CheckNotNull ("memberSelector", memberSelector);
      ArgumentUtility.CheckNotNull ("relatedMethodFinder", relatedMethodFinder);

      _memberSelector = memberSelector;
      _relatedMethodFinder = relatedMethodFinder;
    }

    public MutableMethodInfo CreateMethod (
        ProxyType declaringType,
        string name,
        MethodAttributes attributes,
        IEnumerable<GenericParameterDeclaration> genericParameters,
        Func<GenericParameterContext, Type> returnTypeProvider,
        Func<GenericParameterContext, IEnumerable<ParameterDeclaration>> parameterProvider,
        Func<MethodBodyCreationContext, Expression> bodyProvider)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("genericParameters", genericParameters);
      ArgumentUtility.CheckNotNull ("returnTypeProvider", returnTypeProvider);
      ArgumentUtility.CheckNotNull ("parameterProvider", parameterProvider);
      // Body provider may be null (for abstract methods).

      // TODO : virtual and static is an invalid combination

      var isAbstract = attributes.IsSet (MethodAttributes.Abstract);
      if (!isAbstract && bodyProvider == null)
        throw new ArgumentNullException ("bodyProvider", "Non-abstract methods must have a body.");
      if (isAbstract && bodyProvider != null)
        throw new ArgumentException ("Abstract methods cannot have a body.", "bodyProvider");

      MemberAttributesUtility.ValidateAttributes ("methods", MemberAttributesUtility.InvalidMethodAttributes, attributes, "attributes");

      var isVirtual = attributes.IsSet (MethodAttributes.Virtual);
      var isNewSlot = attributes.IsSet (MethodAttributes.NewSlot);
      if (isAbstract && !isVirtual)
        throw new ArgumentException ("Abstract methods must also be virtual.", "attributes");
      if (!isVirtual && isNewSlot)
        throw new ArgumentException ("NewSlot methods must also be virtual.", "attributes");

      var methodItems = GetMethodSignatureItems (declaringType, genericParameters, returnTypeProvider, parameterProvider);

      var signature = new MethodSignature (
          methodItems.ReturnType,
          methodItems.ParameterDeclarations.Select (pd => pd.Type),
          methodItems.GenericParameters.Count,
          s_methodSignatureStringBuilderHelper);
      if (declaringType.AddedMethods.Any (m => m.Name == name && MethodSignature.Create (m).Equals (signature)))
        throw new InvalidOperationException ("Method with equal name and signature already exists.");

      var baseMethod = GetBaseMethod (declaringType, name, signature, isVirtual, isNewSlot);
      // TODO : if it is an implicit baseMethod override, it needs at least the same ore more public visibility

      var body = GetMethodBody (declaringType, attributes, bodyProvider, methodItems, baseMethod);

      return new MutableMethodInfo (
          declaringType, name, attributes, methodItems.GenericParameters, methodItems.ReturnType, methodItems.ParameterDeclarations, baseMethod, body);
    }

    private MethodSignatureItems GetMethodSignatureItems (
        ProxyType declaringType,
        IEnumerable<GenericParameterDeclaration> genericParameters,
        Func<GenericParameterContext, Type> returnTypeProvider,
        Func<GenericParameterContext, IEnumerable<ParameterDeclaration>> parameterProvider)
    {
      var genericParameterDeclarations = genericParameters.ConvertToCollection();
      var memberSelector = new MemberSelector (new BindingFlagsEvaluator());
      var genericParams = genericParameterDeclarations
          .Select ((p, i) => new MutableGenericParameter (memberSelector, i, p.Name, declaringType.Namespace, p.Attributes)).ToList();

      var genericParameterContext = new GenericParameterContext (genericParams.Cast<Type>());
      foreach (var paraAndDecl in genericParams.Zip (genericParameterDeclarations, (p, d) => new { Parameter = p, Declaration = d }))
      {
        paraAndDecl.Parameter.SetBaseTypeConstraint (paraAndDecl.Declaration.BaseConstraintProvider (genericParameterContext));
        paraAndDecl.Parameter.SetInterfaceConstraints (paraAndDecl.Declaration.InterfaceConstraintsProvider (genericParameterContext));
      }

      var returnType = ProviderUtility.GetNonNullValue (returnTypeProvider, genericParameterContext, "returnTypeProvider");
      var parameters = ProviderUtility.GetNonNullValue (parameterProvider, genericParameterContext, "parameterProvider").ConvertToCollection();

      return new MethodSignatureItems (genericParams, returnType, parameters);
    }

    private MethodInfo GetBaseMethod (ProxyType declaringType, string name, MethodSignature signature, bool isVirtual, bool isNewSlot)
    {
      if (!isVirtual || isNewSlot)
        return null;

      var baseMethod = _relatedMethodFinder.GetMostDerivedVirtualMethod (name, signature, declaringType.BaseType);
      if (baseMethod != null && baseMethod.IsFinal)
      {
        Assertion.IsNotNull (baseMethod.DeclaringType);
        var message = string.Format ("Cannot override final method '{0}.{1}'.", baseMethod.DeclaringType.Name, baseMethod.Name);
        throw new NotSupportedException (message);
      }

      return baseMethod;
    }

    private Expression GetMethodBody (
        ProxyType declaringType,
        MethodAttributes attributes,
        Func<MethodBodyCreationContext, Expression> bodyProvider,
        MethodSignatureItems signatureItems,
        MethodInfo baseMethod)
    {
      var parameterExpressions = signatureItems.ParameterDeclarations.Select (pd => pd.Expression);
      var isStatic = attributes.IsSet (MethodAttributes.Static);
      var context = new MethodBodyCreationContext (
          declaringType, isStatic, parameterExpressions, signatureItems.GenericParameters.Cast<Type>(), signatureItems.ReturnType, baseMethod, _memberSelector);
      var body = bodyProvider == null ? null : BodyProviderUtility.GetTypedBody (signatureItems.ReturnType, bodyProvider, context);

      return body;
    }
  }
}