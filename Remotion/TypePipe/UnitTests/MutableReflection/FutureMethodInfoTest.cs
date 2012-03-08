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
using System.Reflection;
using NUnit.Framework;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class FutureMethodInfoTest
  {
    [Test]
    public void FutureMethodInfo_IsAMethodInfo ()
    {
      Assert.That (FutureMethodInfoObjectMother.Create(), Is.InstanceOf<MethodInfo> ());
    }

    [Test]
    public void DeclaringType ()
    {
      var declaringType = FutureTypeObjectMother.Create();
      var futureMethodInfo = FutureMethodInfoObjectMother.Create (declaringType: declaringType);
      Assert.That(futureMethodInfo.DeclaringType, Is.SameAs(declaringType));
    }

    [Test]
    public void Attributes ()
    {
      var futureMethodInfo = FutureMethodInfoObjectMother.Create (methodAttributes: MethodAttributes.Final);
      Assert.That (futureMethodInfo.Attributes, Is.EqualTo (MethodAttributes.Final));
    }

    [Test]
    public void GetParameters ()
    {
      var parameters = new[] { FutureParameterInfoObjectMother.Create(), FutureParameterInfoObjectMother.Create() };
      var futureMethodInfo = FutureMethodInfoObjectMother.Create (parameters: parameters);
      Assert.That (futureMethodInfo.GetParameters(), Is.SameAs(parameters));
    }
  }
}