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
using System.Reflection;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  public static class CustomParameterInfoObjectMother
  {
    public static CustomParameterInfo Create (
        MemberInfo member = null, int position = 7, string name = "param", Type type = null, ParameterAttributes attributes = (ParameterAttributes) 7)
    {
      member = member ?? NormalizingMemberInfoFromExpressionUtility.GetMethod (() => UnspecifiedMember());
      type = type ?? ReflectionObjectMother.GetSomeType();

      return new TestableCustomParameterInfo (member, position, name, type, attributes);
    }

    private static void UnspecifiedMember () { }
  }
}