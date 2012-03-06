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
  public class FutureTypeTest
  {
    private FutureType _futureType;

    [SetUp]
    public void SetUp ()
    {
      _futureType = new FutureType (TypeAttributes.Public | TypeAttributes.BeforeFieldInit);
    }

    [Test]
    public void FutureType_IsAType ()
    {
      Assert.That (_futureType, Is.InstanceOf<Type>());
    }

    [Test]
    public void AddConstructor ()
    {
      var futureConstructor = new FutureConstructorInfo (_futureType, FutureParameterInfo.EmptyParameters);
      _futureType.AddConstructor (futureConstructor);

      Assert.That (_futureType.Constructors, Is.EqualTo (new[] { futureConstructor }));
    }

    [Test]
    public void BaseType ()
    {
      Assert.That (_futureType.BaseType, Is.EqualTo (typeof (object)));
    }

    [Test]
    public void Name ()
    {
      Assert.That (_futureType.Name, Is.Null);
    }

    [Test]
    public void HasElementTypeImpl ()
    {
      Assert.That (_futureType.HasElementType, Is.False);
    }

    [Test]
    public void Assembly ()
    {
      Assert.That (_futureType.Assembly, Is.Null);
    }

    [Test]
    public void GetConstructorImpl_WithSingleAddedConstructor ()
    {
      // Arrange
      var futureConstructor = new FutureConstructorInfo (_futureType, FutureParameterInfo.EmptyParameters);
      _futureType.AddConstructor (futureConstructor);

      BindingFlags bindingFlags = (BindingFlags) (-1);
      Binder binder = null;
      Type[] parameterTypes = Type.EmptyTypes; // Cannot be null
      ParameterModifier[] parameterModifiers = null;

      // Act
      var constructor = _futureType.GetConstructor (bindingFlags, binder, parameterTypes, parameterModifiers);

      // Assert
      Assert.That (constructor, Is.SameAs (futureConstructor));
      Assert.That (constructor.DeclaringType, Is.SameAs (_futureType));
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
      var constructor = _futureType.GetConstructor (bindingFlags, binder, parameterTypes, parameterModifiers);

      // Assert
      Assert.That (constructor, Is.Null);
    }

    [Test]
    public void IsByRefImpl ()
    {
      Assert.That (_futureType.IsByRef, Is.False);
    }

    [Test]
    public void UnderlyingSystemType ()
    {
      Assert.That (_futureType.UnderlyingSystemType, Is.SameAs (_futureType));
    }

    [Test]
    public void GetAttributeFlagsImpl ()
    {
      Assert.That (_futureType.Attributes, Is.EqualTo (TypeAttributes.Public | TypeAttributes.BeforeFieldInit));
    }
  }
}