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
using System.Reflection;
using NUnit.Framework;
using Remotion.TypePipe.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableTypeTest
  {
    private ITypeInfo _originalTypeInfoStub;
    private MutableType _mutableType;

    [SetUp]
    public void SetUp ()
    {
      _originalTypeInfoStub = MockRepository.GenerateStub<ITypeInfo>();
      _mutableType = new MutableType (typeof (string), _originalTypeInfoStub);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_mutableType.RequestedType, Is.EqualTo (typeof (string)));
      Assert.That (_mutableType.OriginalTypeInfo, Is.SameAs (_originalTypeInfoStub));
      Assert.That (_mutableType.AddedInterfaces, Is.Empty);
      Assert.That (_mutableType.AddedFields, Is.Empty);
    }

    [Test]
    public void AddInterface ()
    {
      _originalTypeInfoStub.Stub (stub => stub.GetInterfaces ()).Return (Type.EmptyTypes);

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
      _originalTypeInfoStub.Stub (stub => stub.GetInterfaces ()).Return (new[] { typeof (IDisposable) });

      _mutableType.AddInterface (typeof (IDisposable));
    }

    [Test]
    public void GetInterfaces ()
    {
      _originalTypeInfoStub.Stub (stub => stub.GetInterfaces()).Return (new[] { typeof (IDisposable) });
      _mutableType.AddInterface (typeof (IComparable));

      Assert.That (_mutableType.GetInterfaces(), Is.EqualTo (new[] { typeof (IDisposable), typeof (IComparable) }));
    }

    [Test]
    public void AddField ()
    {
      _originalTypeInfoStub.Stub (stub => stub.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
          .Return (new FieldInfo[0]);

      var newField = _mutableType.AddField ("_newField", typeof (string), FieldAttributes.Private);

      // Correct field info instance
      Assert.That (newField, Is.TypeOf<FutureFieldInfo>());
      Assert.That (newField.DeclaringType, Is.SameAs (_mutableType));
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
      var fieldInfo = FutureFieldInfoObjectMother.Create(name: "_bla");
      _originalTypeInfoStub
          .Stub (stub => stub.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
          .Return (new[] { fieldInfo });

      _mutableType.AddField ("_bla", typeof (string), FieldAttributes.Private);
    }

    [Test]
    public void GetFields ()
    {
      var fieldInfo1 = FutureFieldInfoObjectMother.Create();
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
      _originalTypeInfoStub.Stub (stub => stub.GetFields (bindingFlags)).Return (new[] { fieldInfo1 });
      var fieldInfo2 = _mutableType.AddField ("field2", typeof (UnspecifiedType), 0);

      Assert.That (_mutableType.GetFields (bindingFlags), Is.EqualTo (new[] { fieldInfo1, fieldInfo2 }));
    }

    [Test]
    public void AddConstructor ()
    {
      _originalTypeInfoStub.Stub (stub => stub.GetConstructors (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        .Return (new ConstructorInfo[0]);
      var parameterTypes = new[] { typeof(string), typeof(int) };
      var newConstructor = _mutableType.AddConstructor (parameterTypes);

      // Correct constroctur info instance
      Assert.That (newConstructor, Is.TypeOf<FutureConstructorInfo>());
      Assert.That (newConstructor.DeclaringType, Is.SameAs (_mutableType));
      // Correct parameters
      var parameters = newConstructor.GetParameters().ToArray();
      Assert.That (parameters, Has.Length.EqualTo (2));
      Assert.That (parameters.Select (p => p.ParameterType), Is.EqualTo (parameterTypes));
      Assert.That (parameters[0], Is.TypeOf<FutureParameterInfo>());
      Assert.That (parameters[1], Is.TypeOf<FutureParameterInfo>());
      Assert.That (parameters[0].ParameterType, Is.EqualTo (typeof (string)));
      Assert.That (parameters[1].ParameterType, Is.EqualTo (typeof (int)));
      // Constructor info is stored
      Assert.That (_mutableType.AddedConstructors, Is.EqualTo (new[] { newConstructor }));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Constructor with same signature already exists.")]
    public void AddConstructor_ThrowsIfAlreadyExists ()
    {
      var constructorInfo = FutureConstructorInfoObjectMother.Create (parameters: new ParameterInfo[0]);
      _originalTypeInfoStub
          .Stub (stub => stub.GetConstructors (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
          .Return (new[] { constructorInfo });

      _mutableType.AddConstructor (Type.EmptyTypes);
    }

    [Test]
    public void GetConstructors ()
    {
      var constructor1 = FutureConstructorInfoObjectMother.Create();
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance; // Don't return static constructors by default
      _originalTypeInfoStub.Stub (stub => stub.GetConstructors (bindingFlags)).Return (new[] { constructor1 });
      var parameterTypes = new[] { typeof (int) }; // Need different signature
      var constructor2 = _mutableType.AddConstructor (parameterTypes);

      Assert.That (_mutableType.GetConstructors (bindingFlags), Is.EqualTo (new[] { constructor1, constructor2 }));
    }

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
      _originalTypeInfoStub.Stub (stub => stub.GetAttributeFlags()).Return (TypeAttributes.Sealed);

      Assert.That (_mutableType.Attributes, Is.EqualTo (TypeAttributes.Sealed));
    }

    [Test]
    public void BaseType ()
    {
      var baseType = typeof (IDisposable);
      _originalTypeInfoStub.Stub (stub => stub.GetBaseType()).Return(baseType);

      Assert.That (_mutableType.BaseType, Is.SameAs(baseType));
    }
  }
}