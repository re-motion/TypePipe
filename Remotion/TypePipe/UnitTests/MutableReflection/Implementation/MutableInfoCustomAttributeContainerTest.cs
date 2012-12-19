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
using System.Collections.ObjectModel;
using NUnit.Framework;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class MutableInfoCustomAttributeContainerTest
  {
    private Func<ReadOnlyCollection<ICustomAttributeData>> _attributeProvider;
    private bool _canAddCustomAttributes;

    private MutableInfoCustomAttributeContainer _container;

    [SetUp]
    public void SetUp ()
    {
      _attributeProvider = () => { throw new Exception ("should be lazy"); };
      _canAddCustomAttributes = true;

      _container = new MutableInfoCustomAttributeContainer (() => _attributeProvider(), () => _canAddCustomAttributes);
    }

    [Test]
    public void AddedCustomAttributeDeclarations ()
    {
      Assert.That (_container.AddedCustomAttributeDeclarations, Is.Empty);
    }

    [Test]
    public void AddCustomAttribute ()
    {
      SetupAttributeProvider();
      var declaration = CustomAttributeDeclarationObjectMother.Create ();

      _container.AddCustomAttribute (declaration);

      Assert.That (_container.AddedCustomAttributeDeclarations, Is.EqualTo (new[] { declaration }));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Adding custom attributes to this element is not supported.")]
    public void AddCustomAttribute_CannotAdd ()
    {
      _canAddCustomAttributes = false;
      _container.AddCustomAttribute (CustomAttributeDeclarationObjectMother.Create());
    }

    [Test]
    public void AddCustomAttribute_AlreadyPresent_Existing ()
    {
      SetupAttributeProvider (
          CustomAttributeDeclarationObjectMother.Create (typeof (SingleAttribute)),
          CustomAttributeDeclarationObjectMother.Create (typeof (MultipleAttribute)));

      Assert.That (
          () => _container.AddCustomAttribute (CustomAttributeDeclarationObjectMother.Create (typeof (SingleAttribute))),
          Throws.InvalidOperationException.With.Message.EqualTo (
              "Attribute of type 'SingleAttribute' (with AllowMultiple = false) is already present."));
      Assert.That (() => _container.AddCustomAttribute (CustomAttributeDeclarationObjectMother.Create (typeof (MultipleAttribute))), Throws.Nothing);
    }

    [Test]
    public void AddCustomAttribute_AlreadyPresent_Added ()
    {
      SetupAttributeProvider();
      _container.AddCustomAttribute (CustomAttributeDeclarationObjectMother.Create (typeof (SingleAttribute)));
      _container.AddCustomAttribute (CustomAttributeDeclarationObjectMother.Create (typeof (MultipleAttribute)));

      Assert.That (
          () => _container.AddCustomAttribute (CustomAttributeDeclarationObjectMother.Create (typeof (SingleAttribute))),
          Throws.InvalidOperationException.With.Message.EqualTo (
              "Attribute of type 'SingleAttribute' (with AllowMultiple = false) is already present."));
      Assert.That (() => _container.AddCustomAttribute (CustomAttributeDeclarationObjectMother.Create (typeof (MultipleAttribute))), Throws.Nothing);
    }

    [Test]
    public void GetCustomAttributeData ()
    {
      var existingData = CustomAttributeDeclarationObjectMother.Create (typeof (ObsoleteAttribute));
      var addedData = CustomAttributeDeclarationObjectMother.Create (typeof (SerializableAttribute));

      var callCount = 0;
      _attributeProvider = () =>
      {
        callCount++;
        return new ICustomAttributeData[] { existingData }.ToList().AsReadOnly();
      };
      _container.AddCustomAttribute (addedData);

      var result = _container.GetCustomAttributeData();

      Assert.That (result, Is.EqualTo (new[] { addedData, existingData }));

      Assert.That (callCount, Is.EqualTo (1));
      _container.GetCustomAttributeData();
      Assert.That (callCount, Is.EqualTo (1), "should be cached");
    }

    private void SetupAttributeProvider (params ICustomAttributeData[] customAttributeDatas)
    {
      _attributeProvider = () => new ReadOnlyCollection<ICustomAttributeData> (customAttributeDatas);
    }

    [AttributeUsage (AttributeTargets.All, AllowMultiple = false)]
    public class SingleAttribute : Attribute { }

    [AttributeUsage (AttributeTargets.All, AllowMultiple = true)]
    public class MultipleAttribute : Attribute { }
  }
}