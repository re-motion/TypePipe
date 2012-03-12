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
using Remotion.TypePipe.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableTypeTest
  {
    private ITypeTemplate _typeTemplate;
    private MutableType _mutableType;

    [SetUp]
    public void SetUp ()
    {
      _typeTemplate = MockRepository.GenerateStub<ITypeTemplate>();
      _mutableType = new MutableType (typeof (string), _typeTemplate);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_mutableType.RequestedType, Is.EqualTo (typeof (string)));
      Assert.That (_mutableType.TypeTemplate, Is.SameAs (_typeTemplate));
      Assert.That (_mutableType.AddedInterfaces, Is.Empty);
      Assert.That (_mutableType.AddedFields, Is.Empty);
    }

    [Test]
    public void AddInterface ()
    {
      _typeTemplate.Stub (stub => stub.GetInterfaces ()).Return (Type.EmptyTypes);

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
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Interface 'System.IDisposable' is already implemented.")]
    public void AddInterface_ThrowsIfAlreadyImplemented ()
    {
      _typeTemplate.Stub (stub => stub.GetInterfaces ()).Return (new[] { typeof (IDisposable) });

      _mutableType.AddInterface (typeof (IDisposable));
    }

    [Test]
    public void GetInterfaces ()
    {
      _typeTemplate.Stub (stub => stub.GetInterfaces()).Return (new[] { typeof (IDisposable) });
      _mutableType.AddInterface (typeof (IComparable));

      Assert.That (_mutableType.GetInterfaces(), Is.EqualTo (new[] { typeof (IDisposable), typeof (IComparable) }));
    }

    [Test]
    public void AddField ()
    {
      _typeTemplate.Stub (stub => stub.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
          .Return (new FieldInfo[0]);

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
    [ExpectedException (typeof(InvalidOperationException), ExpectedMessage = "Field with name '_bla' already exists.")]
    public void AddField_ThrowsIfAlreadyExist ()
    {
      var fieldInfo = MockRepository.GenerateStub<FieldInfo>();
      fieldInfo.Stub (stub => stub.Name).Return ("_bla");
      _typeTemplate
          .Stub (stub => stub.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
          .Return (new[] { fieldInfo });

      _mutableType.AddField ("_bla", typeof (string), FieldAttributes.Private);
    }

    [Test]
    public void GetFields ()
    {
      var fieldInfo1 = FutureFieldInfoObjectMother.Create();
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
      _typeTemplate.Stub (stub => stub.GetFields (bindingFlags)).Return (new[] { fieldInfo1 });
      var fieldInfo2 = _mutableType.AddField ("field2", typeof (UnspecifiedType), 0);

      Assert.That (_mutableType.GetFields (bindingFlags), Is.EqualTo (new[] { fieldInfo1, fieldInfo2 }));
    }

    //[Test]
    //public void AddConstructor ()
    //{
    //  var futureConstructor = FutureConstructorInfoObjectMother.Create (_mutableType);
    //  _mutableType.AddConstructor (futureConstructor);

    //  Assert.That (_mutableType.AddedConstructors, Is.EqualTo (new[] { futureConstructor }));
    //}

    [Test]
    public void HasElementTypeImpl ()
    {
      Assert.That (_mutableType.HasElementType, Is.False);
    }

    [Test]
    public void Assembly ()
    {
      Assert.That (_mutableType.Assembly, Is.Null);
    }

    [Test]
    public void IsByRefImpl ()
    {
      Assert.That (_mutableType.IsByRef, Is.False);
    }

    [Test]
    public void UnderlyingSystemType ()
    {
      Assert.That (_mutableType.UnderlyingSystemType, Is.SameAs (_mutableType));
    }

    [Test]
    public void GetAttributeFlagsImpl ()
    {
      _typeTemplate.Stub (stub => stub.GetAttributeFlags()).Return (TypeAttributes.Sealed);

      Assert.That (_mutableType.Attributes, Is.EqualTo (TypeAttributes.Sealed));
    }

    [Test]
    public void BaseType ()
    {
      var baseType = typeof (IDisposable);
      _typeTemplate.Stub (stub => stub.GetBaseType()).Return(baseType);

      Assert.That (_mutableType.BaseType, Is.SameAs(baseType));
    }
  }
}