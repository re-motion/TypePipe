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
  public class ModifiedTypeTest
  {
    private Type _originalType;
    private ModifiedType _modifiedType;

    [SetUp]
    public void SetUp ()
    {
      _originalType = typeof (string);
      _modifiedType = new ModifiedType (_originalType); 
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_modifiedType.OriginalType, Is.SameAs (_originalType));
    }

     [Test]
    public void BaseType ()
    {
      Assert.That (_modifiedType.BaseType, Is.EqualTo (typeof (object)));
    }

    [Test]
    public void HasElementTypeImpl ()
    {
      Assert.That (_modifiedType.HasElementType, Is.False);
    }

    [Test]
    public void Assembly ()
    {
      Assert.That (_modifiedType.Assembly, Is.Null);
    }

    [Test]
    public void GetConstructorImpl_WithSingleAddedConstructor ()
    {
      // Arrange
      var futureConstructor = FutureConstructorInfoObjectMother.Create (_modifiedType);
      _modifiedType.AddConstructor (futureConstructor);

      BindingFlags bindingFlags = (BindingFlags) (-1);
      Binder binder = null;
      Type[] parameterTypes = Type.EmptyTypes; // Cannot be null
      ParameterModifier[] parameterModifiers = null;

      // Act
      var constructor = _modifiedType.GetConstructor (bindingFlags, binder, parameterTypes, parameterModifiers);

      // Assert
      Assert.That (constructor, Is.SameAs (futureConstructor));
      Assert.That (constructor.DeclaringType, Is.SameAs (_modifiedType));
    }

    [Test]
    public void GetConstructorImpl_WithoutAddedConstructor ()
    {
      // Arrange
      BindingFlags bindingFlags = (BindingFlags) (-1);
      Binder binder = null;
      Type[] parameterTypes = Type.EmptyTypes; // Cannot be null
      ParameterModifier[] parameterModifiers = null;

      // Act
      var constructor = _modifiedType.GetConstructor (bindingFlags, binder, parameterTypes, parameterModifiers);

      // Assert
      Assert.That (constructor, Is.Null);
    }

    [Test]
    public void IsByRefImpl ()
    {
      Assert.That (_modifiedType.IsByRef, Is.False);
    }

    [Test]
    public void UnderlyingSystemType ()
    {
      Assert.That (_modifiedType.UnderlyingSystemType, Is.SameAs (_modifiedType));
    }

    [Test]
    public void GetAttributeFlagsImpl ()
    {
      Assert.That (_modifiedType.Attributes, Is.EqualTo (_originalType.Attributes));
    }
  }
}