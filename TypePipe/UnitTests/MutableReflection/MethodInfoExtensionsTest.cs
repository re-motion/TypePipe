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
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection.Implementation;
using Remotion.TypePipe.UnitTests.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MethodInfoExtensionsTest
  {
    [Test]
    public void IsRunetimeMethodInfo ()
    {
      var runtimeMethod = ReflectionObjectMother.GetSomeMethod();
      var customMethod = CustomMethodInfoObjectMother.Create();

      Assert.That (runtimeMethod.IsRuntimeMethodInfo(), Is.True);
      Assert.That (customMethod.IsRuntimeMethodInfo(), Is.False);
    }

    [Test]
    public void IsGenericMethodInstantiation ()
    {
      var nonGenericMethod = ReflectionObjectMother.GetSomeNonGenericMethod();
      var genericMethodDefinition = NormalizingMemberInfoFromExpressionUtility.GetGenericMethodDefinition (() => Method<Dev.T, Dev.T>());
      var methodInstantiation = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => Method<int, string>());

      Assert.That (nonGenericMethod.IsGenericMethodInstantiation(), Is.False);
      Assert.That (genericMethodDefinition.IsGenericMethodInstantiation(), Is.False);
      Assert.That (methodInstantiation.IsGenericMethodInstantiation(), Is.True);
    }

    [Test]
    public void MakeTypePipeGenericMethod_MakesGenericMethodWithCustomTypeArgument ()
    {
      var genericMethodDefinition = NormalizingMemberInfoFromExpressionUtility.GetGenericMethodDefinition (() => Method<Dev.T, Dev.T>());
      var runtimeType = ReflectionObjectMother.GetSomeType();
      var customType = CustomTypeObjectMother.Create();

      var result = genericMethodDefinition.MakeTypePipeGenericMethod (runtimeType, customType);

      Assert.That (result.IsGenericMethod, Is.True);
      Assert.That (result.IsGenericMethodDefinition, Is.False);
      Assert.That (result.GetGenericArguments(), Is.EqualTo (new[] { runtimeType, customType }));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "'ToString' is not a generic method definition. MakeTypePipeGenericMethod may only be called on a method for which"
        + " MethodInfo.IsGenericMethodDefinition is true.")]
    public void MakeTypePipeGenericMethod_NoGenericTypeDefinition ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => ToString());
      method.MakeTypePipeGenericMethod (typeof (int));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException),
        ExpectedMessage = "The generic definition 'Method' has 2 generic parameter(s), but 1 generic argument(s) were provided. "
                          + "A generic argument must be provided for each generic parameter.\r\nParameter name: typeArguments")]
    public void MakeTypePipeGenericType_UsesGenericArgumentUtilityToValidateGenericArguments ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetGenericMethodDefinition (() => Method<Dev.T, Dev.T>());
      method.MakeTypePipeGenericMethod (typeof (int));
    }

    private void Method<T1, T2> () {}
  }
}