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
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutablePropertyInfoTest
  {
    [Test]
    public void FuturePropertyInfo_IsAPropertyInfo ()
    {
      Assert.That (MutablePropertyInfoObjectMother.Create(), Is.InstanceOf<PropertyInfo>());
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = "At least one of the accessors must be specified.")]
    public void Initialization_ThrowsIfBothAccessorsAreNull()
    {
      var declaringType = ReflectionObjectMother.GetSomeType();
      var propertyType = ReflectionObjectMother.GetSomeType ();
      MethodInfo getMethod = null;
      MethodInfo setMethod = null;

      new MutablePropertyInfo (declaringType, propertyType, getMethod, setMethod);
    }

    [Test]
    public void DeclaringType ()
    {
      var declaringType = MutableTypeObjectMother.Create();
      var futurePropertyInfo = MutablePropertyInfoObjectMother.Create (declaringType: declaringType);
      Assert.That (futurePropertyInfo.DeclaringType, Is.SameAs(declaringType));
    }

    [Test]
    public void PropertyType ()
    {
      var propertyType = MutableTypeObjectMother.Create();
      var futurePropertyInfo = MutablePropertyInfoObjectMother.Create (propertyType: propertyType);
      Assert.That (futurePropertyInfo.PropertyType, Is.SameAs (propertyType));
    }

    [Test]
    public void GetGetMethod ()
    {
      var getMethod = MutableMethodInfoObjectMother.Create();
      var futurePropertyInfo = MutablePropertyInfoObjectMother.Create(getMethod: getMethod);
      Assert.That (futurePropertyInfo.GetGetMethod(), Is.SameAs(getMethod));
    }

    [Test]
    public void GetSetMethod ()
    {
      var setMethod = MutableMethodInfoObjectMother.Create ();
      var futurePropertyInfo = MutablePropertyInfoObjectMother.Create (setMethod: setMethod);
      Assert.That (futurePropertyInfo.GetSetMethod (), Is.SameAs (setMethod));
    }

    [Test]
    public void CanWrite ()
    {
      Assert.That (MutablePropertyInfoObjectMother.Create().CanWrite, Is.True);
    }
  }
}