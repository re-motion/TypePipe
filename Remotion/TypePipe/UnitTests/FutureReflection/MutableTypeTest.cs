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
using Remotion.TypePipe.FutureReflection;

namespace Remotion.TypePipe.UnitTests.FutureReflection
{
  [TestFixture]
  public class MutableTypeTest
  {
    private MutableType _mutableType;

    [SetUp]
    public void SetUp ()
    {
      _mutableType = new TestableMutableType();
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_mutableType.AddedInterfaces, Is.Empty);
      Assert.That (_mutableType.AddedFields, Is.Empty);
    }

    [Test]
    public void AddInterface ()
    {
      _mutableType.AddInterface (typeof (IDisposable));
      _mutableType.AddInterface (typeof (IComparable));

      Assert.That (_mutableType.AddedInterfaces, Is.EqualTo (new[] { typeof(IDisposable), typeof(IComparable) }));
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Type must be an interface.\r\nParameter name: interfaceType")]
    public void AddInterface_ThrowsIfNotAnInterface ()
    {
      _mutableType.AddInterface (typeof (string));
    }

    [Test]
    public void AddField ()
    {
      var newField = _mutableType.AddField ("_newField", typeof (string), FieldAttributes.Private);

      // Correct field info instance
      Assert.That (newField, Is.TypeOf<FutureFieldInfo>());
      Assert.That (newField.Name, Is.EqualTo ("_newField"));
      Assert.That (newField.FieldType, Is.EqualTo (typeof (string)));
      Assert.That (newField.Attributes, Is.EqualTo (FieldAttributes.Private));
      // Field info is stored
      Assert.That (_mutableType.AddedFields, Is.EqualTo (new[] { newField }));
    }

    [Test]
    public void AddConstructor ()
    {
      var futureConstructor = FutureConstructorInfoObjectMother.Create (_mutableType);
      _mutableType.AddConstructor (futureConstructor);

      Assert.That (_mutableType.AddedConstructors, Is.EqualTo (new[] { futureConstructor }));
    }
  }
}