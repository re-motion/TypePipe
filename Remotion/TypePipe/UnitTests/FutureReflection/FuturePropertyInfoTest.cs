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
using Remotion.FunctionalProgramming;
using Remotion.TypePipe.FutureReflection;

namespace Remotion.TypePipe.UnitTests.FutureReflection
{
  [TestFixture]
  public class FuturePropertyInfoTest
  {
    [Test]
    public void FuturePropertyInfo_IsAPropertyInfo ()
    {
      Assert.That (FuturePropertyInfoObjectMother.Create(), Is.InstanceOf<PropertyInfo>());
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = "At least one of the accessors must be specified.")]
    public void Initialization_ThrowsIfBothAccessorsAreNull()
    {
      var declaringType = typeof (object);
      var propertyType = typeof (object);
      var getMethod = Maybe<MethodInfo>.Nothing;
      var setMethod = Maybe<MethodInfo>.Nothing;

      new FuturePropertyInfo (declaringType, propertyType, getMethod, setMethod);
    }

    [Test]
    public void DeclaringType ()
    {
      var declaringType = FutureTypeObjectMother.Create ();
      var futurePropertyInfo = FuturePropertyInfoObjectMother.Create (declaringType: declaringType);
      Assert.That (futurePropertyInfo.DeclaringType, Is.SameAs(declaringType));
    }

    [Test]
    public void PropertyType ()
    {
      var propertyType = FutureTypeObjectMother.Create ();
      var futurePropertyInfo = FuturePropertyInfoObjectMother.Create (propertyType: propertyType);
      Assert.That (futurePropertyInfo.PropertyType, Is.SameAs (propertyType));
    }

    [Test]
    public void GetGetMethod ()
    {
      var getMethod = FutureMethodInfoObjectMother.Create();
      var futurePropertyInfo = FuturePropertyInfoObjectMother.Create(getMethod: getMethod);
      Assert.That (futurePropertyInfo.GetGetMethod(), Is.SameAs(getMethod));
    }

    [Test]
    public void GetSetMethod ()
    {
      var setMethod = FutureMethodInfoObjectMother.Create ();
      var futurePropertyInfo = FuturePropertyInfoObjectMother.Create (setMethod: setMethod);
      Assert.That (futurePropertyInfo.GetSetMethod (), Is.SameAs (setMethod));
    }

    [Test]
    public void CanWrite ()
    {
      Assert.That (FuturePropertyInfoObjectMother.Create().CanWrite, Is.True);
    }
  }
}