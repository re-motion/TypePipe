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

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class MutableParameterInfoObjectMother
  {
    private abstract class UnspecifiedType
    {
      public void M () { }
// ReSharper disable UnusedParameter.Local
      public void M2 (string unspecifiedParameter) { }
// ReSharper restore UnusedParameter.Local
    }

    public static MutableParameterInfo Create (
        MemberInfo member = null,
        int position = 7,
        Type parameterType = null,
        string name = "param7",
        ParameterAttributes attributes = ParameterAttributes.In)
    {
      return CreateForNew (member, position, parameterType, name, attributes);
    }

    public static MutableParameterInfo CreateForNew (
        MemberInfo member = null,
        int position = 7,
        Type parameterType = null,
        string name = "param7",
        ParameterAttributes attributes = ParameterAttributes.In)
    {
      member = member ?? NormalizingMemberInfoFromExpressionUtility.GetMethod ((UnspecifiedType obj) => obj.M());
      parameterType = parameterType ?? typeof (UnspecifiedType);

      var descriptor = UnderlyingParameterInfoDescriptorObjectMother.CreateForNew (parameterType, name, attributes);

      return new MutableParameterInfo (member, position, descriptor);
    }

    public static MutableParameterInfo CreateForExisting (MemberInfo member = null, int position = 0, ParameterInfo originalParameter = null)
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((UnspecifiedType obj) => obj.M2 (""));
      originalParameter = originalParameter ?? method.GetParameters().Single();
      var descriptor = UnderlyingParameterInfoDescriptorObjectMother.CreateForExisting (originalParameter);
      member = member ?? method;

      return new MutableParameterInfo (member, position, descriptor);
    }
  }
}