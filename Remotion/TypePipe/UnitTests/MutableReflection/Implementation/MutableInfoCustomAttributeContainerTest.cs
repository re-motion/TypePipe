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
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class MutableInfoCustomAttributeContainerTest
  {
    private Func<ReadOnlyCollection<ICustomAttributeData>> _attributeProvider;
    private Func<bool> _canAddCustomAttributesDecider;

    private MutableInfoCustomAttributeContainer _container;

    private ConstructorInfo _attributeCtor;

    [SetUp]
    public void SetUp ()
    {
      _attributeProvider = () => { throw new Exception ("should be lazy"); };
      _canAddCustomAttributesDecider = () => { throw new Exception ("should be lazy"); };

      _container = new MutableInfoCustomAttributeContainer (() => _attributeProvider(), () => _canAddCustomAttributesDecider());

      _attributeCtor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new ObsoleteAttribute());
    }

    [Test]
    public void AddedCustomAttributeDeclarations ()
    {
      Assert.That (_container.AddedCustomAttributeDeclarations, Is.Empty);
    }

    [Test]
    public void AddCustomAttribute ()
    {
      var declaration = new CustomAttributeDeclaration (_attributeCtor, new object[0]);
      _canAddCustomAttributesDecider = () => true;

      _container.AddCustomAttribute (declaration);

      Assert.That (_container.AddedCustomAttributeDeclarations, Is.EqualTo (new[] { declaration }));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Adding custom attributes to this element is not supported.")]
    public void AddCustomAttribute_CannotAdd ()
    {
      _canAddCustomAttributesDecider = () => false;
      _container.AddCustomAttribute (new CustomAttributeDeclaration (_attributeCtor, new object[0]));
    }

    [Test]
    public void GetCustomAttributeData ()
    {
      var addedData = new CustomAttributeDeclaration (_attributeCtor, new object[0]);
      var existingData = new CustomAttributeDeclaration (_attributeCtor, new object[0]);
      _canAddCustomAttributesDecider = () => true;
      _container.AddCustomAttribute (addedData);

      var callCount = 0;
      _attributeProvider = () =>
      {
        callCount++;
        return new ICustomAttributeData[] { existingData }.ToList().AsReadOnly();
      };

      var result = _container.GetCustomAttributeData();

      Assert.That (result, Is.EqualTo (new[] { addedData, existingData }));

      Assert.That (callCount, Is.EqualTo (1));
      _container.GetCustomAttributeData();
      Assert.That (callCount, Is.EqualTo (1), "should be cached");
    }
  }
}