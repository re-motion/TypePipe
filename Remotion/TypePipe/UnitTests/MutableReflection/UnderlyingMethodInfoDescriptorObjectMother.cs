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
using System.Reflection;
using Microsoft.Scripting.Ast;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.Expressions;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class UnderlyingMethodInfoDescriptorObjectMother
  {
    private class UnspecifiedType
    {
      public void UnspecifiedMethod() { }
    }

    public static UnderlyingMethodInfoDescriptor CreateForNew (
        string name = "UnspecifiedMethod",
        MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
        Type returnType = null,
        IEnumerable<UnderlyingParameterInfoDescriptor> parameterDescriptors = null,
        MethodInfo baseMethod = null,
        bool isGenericMethod = false,
        bool isGenericMethodDefinition = false,
        bool containsGenericParameters = false,
        Expression body = null)
    {
      var actualReturnType = returnType ?? typeof (UnspecifiedType);
      return CreateForNewWithNullBody (
          name,
          attributes,
          returnType,
          parameterDescriptors,
          baseMethod,
          isGenericMethod,
          isGenericMethodDefinition,
          containsGenericParameters,
          body ?? ExpressionTreeObjectMother.GetSomeExpression (actualReturnType));
    }

    public static UnderlyingMethodInfoDescriptor CreateForNewWithNullBody (
        string name = "UnspecifiedMethod",
        MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
        Type returnType = null,
        IEnumerable<UnderlyingParameterInfoDescriptor> parameterDescriptors = null,
        MethodInfo baseMethod = null,
        bool isGenericMethod = false,
        bool isGenericMethodDefinition = false,
        bool containsGenericParameters = false,
        Expression body = null)
    {
      var actualReturnType = returnType ?? typeof (UnspecifiedType);
      return UnderlyingMethodInfoDescriptor.Create (
          name,
          attributes,
          actualReturnType,
          parameterDescriptors ?? new UnderlyingParameterInfoDescriptor[0],
          baseMethod,
          isGenericMethod,
          isGenericMethodDefinition,
          containsGenericParameters,
          body);
    }

    public static UnderlyingMethodInfoDescriptor CreateForExisting (
        MethodInfo originalMethodInfo = null,
        RelatedMethodFinder relatedMethodFinder = null)
    {
      return UnderlyingMethodInfoDescriptor.Create (
          originalMethodInfo ?? NormalizingMemberInfoFromExpressionUtility.GetMethod ((UnspecifiedType obj) => obj.UnspecifiedMethod()),
          relatedMethodFinder ?? new RelatedMethodFinder());
    }
  }
}