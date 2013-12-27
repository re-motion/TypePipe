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
using System.Linq;
using NUnit.Framework;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class CustomAttributeFinderTest
  {
    private ICustomAttributeDataProvider _providerMock;

    private bool _randomInherit;

    [SetUp]
    public void SetUp ()
    {
      _providerMock = MockRepository.GenerateStrictMock<ICustomAttributeDataProvider>();

      _randomInherit = BooleanObjectMother.GetRandomBoolean();
    }

    [Test]
    public void GetCustomAttributes ()
    {
      var datas = new[] { CustomAttributeDeclarationObjectMother.Create (typeof (ObsoleteAttribute)) };
      _providerMock.Expect (mock => mock.GetCustomAttributeData (_randomInherit)).Return (datas);

      var result = CustomAttributeFinder.GetCustomAttributes (_providerMock, _randomInherit);

      Assert.That (result.Select (a => a.GetType()), Is.EqualTo (new[] { typeof (ObsoleteAttribute) }));
    }

    [Test]
    public void GetCustomAttributes_NewInstance ()
    {
      var datas = new ICustomAttributeData[] { CustomAttributeDeclarationObjectMother.Create() };
      _providerMock.Expect (mock => mock.GetCustomAttributeData (_randomInherit)).Return (datas).Repeat.Twice();

      var attribute1 = CustomAttributeFinder.GetCustomAttributes (_providerMock, _randomInherit).Single();
      var attribute2 = CustomAttributeFinder.GetCustomAttributes (_providerMock, _randomInherit).Single ();

      Assert.That (attribute1, Is.Not.SameAs (attribute2));
    }

    [Test]
    public void GetCustomAttributes_Filter ()
    {
      var datas = new[] { CustomAttributeDeclarationObjectMother.Create (typeof (DerivedAttribute)) };
      _providerMock.Expect (mock => mock.GetCustomAttributeData (_randomInherit)).Return (datas).Repeat.Times (4);

      Assert.That (CustomAttributeFinder.GetCustomAttributes (_providerMock, typeof (UnrelatedAttribute), _randomInherit), Is.Empty);
      Assert.That (CustomAttributeFinder.GetCustomAttributes (_providerMock, typeof (DerivedAttribute), _randomInherit), Has.Length.EqualTo (1));
      Assert.That (CustomAttributeFinder.GetCustomAttributes (_providerMock, typeof (BaseAttribute), _randomInherit), Has.Length.EqualTo (1));
      Assert.That (CustomAttributeFinder.GetCustomAttributes (_providerMock, typeof (IBaseAttributeInterface), _randomInherit), Has.Length.EqualTo (1));
    }

    [Test]
    public void GetCustomAttributes_ArrayType ()
    {
      // Standard reflection. Use as reference behavior.
      Assert.That (typeof (int).GetCustomAttributes (_randomInherit), Is.TypeOf (typeof (object[])));
      Assert.That (typeof (int).GetCustomAttributes (typeof (BaseAttribute), _randomInherit), Is.TypeOf (typeof (BaseAttribute[])));

      _providerMock.Expect (mock => mock.GetCustomAttributeData (_randomInherit)).Return (new ICustomAttributeData[0]).Repeat.Times (2);

      Assert.That (CustomAttributeFinder.GetCustomAttributes (_providerMock, _randomInherit), Is.TypeOf (typeof (object[])));
      Assert.That (CustomAttributeFinder.GetCustomAttributes (_providerMock, typeof (BaseAttribute), _randomInherit), Is.TypeOf (typeof (BaseAttribute[])));
    }

    [Test]
    public void IsDefined ()
    {
      var datas = new[] { CustomAttributeDeclarationObjectMother.Create (typeof (DerivedAttribute)) };
      _providerMock.Expect (mock => mock.GetCustomAttributeData (_randomInherit)).Return (datas).Repeat.Times (4);

      Assert.That (CustomAttributeFinder.IsDefined (_providerMock, typeof (UnrelatedAttribute), _randomInherit), Is.False);
      Assert.That (CustomAttributeFinder.IsDefined (_providerMock, typeof (DerivedAttribute), _randomInherit), Is.True);
      Assert.That (CustomAttributeFinder.IsDefined (_providerMock, typeof (BaseAttribute), _randomInherit), Is.True);
      Assert.That (CustomAttributeFinder.IsDefined (_providerMock, typeof (IBaseAttributeInterface), _randomInherit), Is.True);
    }

    public interface IBaseAttributeInterface { }
    public class BaseAttribute : Attribute, IBaseAttributeInterface { }
    public class DerivedAttribute : BaseAttribute { }
    public class UnrelatedAttribute : Attribute { }
  }
}