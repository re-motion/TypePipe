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
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class CustomAttributeContainerTest
  {
    private CustomAttributeContainer _container;

    [SetUp]
    public void SetUp ()
    {
      _container = new CustomAttributeContainer();
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_container.AddedCustomAttributes, Is.Empty);
    }

    [Test]
    public void AddCustomAttribute ()
    {
      var declaration = CustomAttributeDeclarationObjectMother.Create();

      _container.AddCustomAttribute (declaration);

      Assert.That (_container.AddedCustomAttributes, Is.EqualTo (new[] { declaration }));
    }

    [Test]
    public void AddCustomAttribute_AlreadyPresent ()
    {
      _container.AddCustomAttribute (CustomAttributeDeclarationObjectMother.Create (typeof (SingleAttribute)));
      _container.AddCustomAttribute (CustomAttributeDeclarationObjectMother.Create (typeof (MultipleAttribute)));

      Assert.That (
          () => _container.AddCustomAttribute (CustomAttributeDeclarationObjectMother.Create (typeof (SingleAttribute))),
          Throws.InvalidOperationException.With.Message.EqualTo (
              "Attribute of type 'SingleAttribute' (with AllowMultiple = false) is already present."));
      Assert.That (() => _container.AddCustomAttribute (CustomAttributeDeclarationObjectMother.Create (typeof (MultipleAttribute))), Throws.Nothing);
    }

    [AttributeUsage (AttributeTargets.All, AllowMultiple = false)]
    public class SingleAttribute : Attribute { }

    [AttributeUsage (AttributeTargets.All, AllowMultiple = true)]
    public class MultipleAttribute : Attribute { }
  }
}