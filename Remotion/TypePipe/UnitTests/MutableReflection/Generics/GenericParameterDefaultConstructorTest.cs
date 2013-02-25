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
using Remotion.TypePipe.MutableReflection.Generics;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  [TestFixture]
  public class GenericParameterDefaultConstructorTest
  {
    private GenericParameter _declaringType;

    private GenericParameterDefaultConstructor _constructor;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = GenericParameterObjectMother.Create ();

      _constructor = new GenericParameterDefaultConstructor (_declaringType);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_constructor.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (_constructor.Attributes, Is.EqualTo (MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName));
      Assert.That (_constructor.IsPublic, Is.True);
      Assert.That (_constructor.IsStatic, Is.False);
    }

    [Test]
    public void GetCustomAttributeData ()
    {
      Assert.That (_constructor.GetCustomAttributeData(), Is.Empty);
    }

    [Test]
    public void GetParameters ()
    {
      Assert.That (_constructor.GetParameters(), Is.Empty);
    }
  }
}