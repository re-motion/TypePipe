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
using System.Linq;
using System.Reflection;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Descriptors;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Descriptors
{
  public static class ParameterDescriptorObjectMother
  {
    public static readonly ParameterDescriptor[] Empty = new ParameterDescriptor[0];

    public static ParameterDescriptor Create (
        Type parameterType = null, string name = "parameter", ParameterAttributes attributes = ParameterAttributes.In)
    {
      return CreateForNew (parameterType, name, attributes);
    }

    public static ParameterDescriptor CreateForNew (
        Type parameterType = null, string name = "parameter", ParameterAttributes attributes = ParameterAttributes.In)
    {
      var parameterDeclartion = new ParameterDeclaration (parameterType ?? typeof (UnspecifiedType), name, attributes);
      return ParameterDescriptor.CreateFromDeclarations (new[] { parameterDeclartion }).Single();
    }

    public static ParameterDescriptor CreateForExisting (ParameterInfo underlyingParameter = null)
    {
      string s;
      underlyingParameter = underlyingParameter
        ?? NormalizingMemberInfoFromExpressionUtility.GetMethod ((UnspecifiedType obj) => obj.Method (out s)).GetParameters().Single();

      return ParameterDescriptor.CreateFromMethodBase ((MethodBase) underlyingParameter.Member)
                                .Single (p => p.UnderlyingSystemInfo == underlyingParameter);
    }

    public static ParameterDescriptor[] CreateMultiple (int count)
    {
      return ParameterDescriptor.CreateFromDeclarations (ParameterDeclarationObjectMother.CreateMultiple (count)).ToArray();
    }

    private class UnspecifiedType
    {
      public void Method (out string parameterName)
      {
        parameterName = "";
      }
    }
  }
}