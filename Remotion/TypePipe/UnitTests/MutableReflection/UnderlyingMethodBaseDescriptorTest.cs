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
using NUnit.Framework;
using Remotion.TypePipe.Expressions;
using Remotion.Development.UnitTesting.Enumerables;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class UnderlyingMethodBaseDescriptorTest
  {
    [Test]
    public void CreateOriginalBodyExpression ()
    {
      var methodBase = ReflectionObjectMother.GetSomeMethod();
      var returnType = ReflectionObjectMother.GetSomeType();
      var parameterDescriptors = UnderlyingParameterInfoDescriptorObjectMother.CreateMultiple (2);

      var result = TestableUnderlyingMethodBaseDescriptor<MethodBase>.CreateOriginalBodyExpression (
          methodBase, returnType, parameterDescriptors.AsOneTime());

      Assert.That (result, Is.TypeOf<OriginalBodyExpression>());
      var originalBodyExpression = ((OriginalBodyExpression) result);
      Assert.That (originalBodyExpression.Type, Is.SameAs (returnType));
      Assert.That (originalBodyExpression.MethodBase, Is.SameAs (methodBase));
      Assert.That (originalBodyExpression.Arguments, Is.EqualTo (new[] { parameterDescriptors[0].Expression, parameterDescriptors[1].Expression }));
    }
  }
}