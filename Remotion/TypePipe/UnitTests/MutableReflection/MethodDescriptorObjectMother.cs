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
  public static class MethodDescriptorObjectMother
  {
    private class UnspecifiedType
    {
      public void UnspecifiedMethod() { }
    }

    public static MethodDescriptor CreateForNew (
        string name = "UnspecifiedMethod",
        MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
        Type returnType = null,
        IEnumerable<ParameterDeclaration> parameterDeclarations = null,
        MethodInfo baseMethod = null,
        bool isGenericMethod = false,
        bool isGenericMethodDefinition = false,
        bool containsGenericParameters = false,
        Expression body = null)
    {
      parameterDeclarations = parameterDeclarations ?? ParameterDeclaration.EmptyParameters;

      returnType = returnType ?? (body != null ? body.Type : typeof (UnspecifiedType));

      if (body == null && !attributes.IsSet (MethodAttributes.Abstract))
        body = ExpressionTreeObjectMother.GetSomeExpression (returnType);


      return MethodDescriptor.Create (
          name,
          attributes,
          returnType,
          ParameterDescriptor.CreateFromDeclarations(parameterDeclarations),
          baseMethod,
          isGenericMethod,
          isGenericMethodDefinition,
          containsGenericParameters,
          body);
    }

    public static MethodDescriptor CreateForExisting (
        MethodInfo originalMethodInfo = null,
        RelatedMethodFinder relatedMethodFinder = null)
    {
      return MethodDescriptor.Create (
          originalMethodInfo ?? NormalizingMemberInfoFromExpressionUtility.GetMethod ((UnspecifiedType obj) => obj.UnspecifiedMethod()),
          relatedMethodFinder ?? new RelatedMethodFinder());
    }
  }
}