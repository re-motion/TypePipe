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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Rhino.Mocks;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class MutableInfoCustomAttributeHelperTest
  {
    private IMutableInfo _mutableInfoMock;
    private Func<ReadOnlyCollection<ICustomAttributeData>> _attributeProvider;
    private Func<bool> _canAddCustomAttributesDecider;

    private MutableInfoCustomAttributeHelper _helper;

    private ConstructorInfo _abcAttributeConstructor;

    [SetUp]
    public void SetUp ()
    {
      _mutableInfoMock = MockRepository.GenerateStrictMock<IMutableInfo>();
      _attributeProvider = () => { throw new Exception ("setup before usage"); };
      _canAddCustomAttributesDecider = () => { throw new Exception ("setup before usage"); };

      _helper = new MutableInfoCustomAttributeHelper (_mutableInfoMock, () => _attributeProvider(), () => _canAddCustomAttributesDecider());

      _abcAttributeConstructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new AbcAttribute());
    }

    [Test]
    public void AddedCustomAttributeDeclarations ()
    {
      Assert.That (_helper.AddedCustomAttributeDeclarations, Is.Empty);
    }

    [Test]
    public void AddCustomAttribute ()
    {
      var declaration = new CustomAttributeDeclaration (_abcAttributeConstructor, new object[0]);
      _canAddCustomAttributesDecider = () => true;

      _helper.AddCustomAttribute (declaration);

      Assert.That (_helper.AddedCustomAttributeDeclarations, Is.EqualTo (new[] { declaration }));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Adding custom attributes to this element is not supported.")]
    public void AddCustomAttribute_CannotAdd ()
    {
      var declaration = new CustomAttributeDeclaration (_abcAttributeConstructor, new object[0]);
      _canAddCustomAttributesDecider = () => false;

      _helper.AddCustomAttribute (declaration);
    }

    [Test]
    public void GetCustomAttributeData ()
    {
      var addedData = new CustomAttributeDeclaration (_abcAttributeConstructor, new object[0]);
      var existingData = new CustomAttributeDeclaration (_abcAttributeConstructor, new object[0]);
      _attributeProvider = () => new ICustomAttributeData[] { existingData }.ToList().AsReadOnly();
      _canAddCustomAttributesDecider = () => true;
      _helper.AddCustomAttribute (addedData);

      var result = _helper.GetCustomAttributeData();

      Assert.That (result, Is.EqualTo (new[] { addedData, existingData }));
    }

    public class AbcAttribute : Attribute { }
  }
}