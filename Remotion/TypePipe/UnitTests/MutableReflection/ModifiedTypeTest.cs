//// Copyright (c) rubicon IT GmbH, www.rubicon.eu
////
//// See the NOTICE file distributed with this work for additional information
//// regarding copyright ownership.  rubicon licenses this file to you under 
//// the Apache License, Version 2.0 (the "License"); you may not use this 
//// file except in compliance with the License.  You may obtain a copy of the 
//// License at
////
////   http://www.apache.org/licenses/LICENSE-2.0
////
//// Unless required by applicable law or agreed to in writing, software 
//// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
//// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
//// License for the specific language governing permissions and limitations
//// under the License.
//// 
//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using Microsoft.Scripting.Ast;
//using NUnit.Framework;
//using Remotion.TypePipe.MutableReflection;

//namespace Remotion.TypePipe.UnitTests.MutableReflection
//{
//  [TestFixture]
//  public class ModifiedTypeTest
//  {
//    private Type _originalType;
//    private MutableType _modifiedType;

//    [SetUp]
//    public void SetUp ()
//    {
//      _originalType = typeof (object);
//      _modifiedType = new MutableType (_originalType); 
//    }

//    [Test]
//    public void Initialization ()
//    {
//      Assert.That (_modifiedType.OriginalType, Is.SameAs (_originalType));
//    }

//    [Test]
//    public void Initialization_ThrowsIfOriginalTypeCannotBeSubclassed ()
//    {
//      var msg = "Original type must not be sealed, an interface, a value type, an enum, a delegate, contain generic parameters and "
//              + "must have an accessible constructor.\r\nParameter name: originalType";
//      Assert.That (() => new MutableType (typeof (string)), Throws.ArgumentException.With.Message.EqualTo (msg)); // sealed
//      Assert.That (() => new MutableType (typeof (IDisposable)), Throws.ArgumentException.With.Message.EqualTo (msg)); // interface
//      Assert.That (() => new MutableType (typeof (int)), Throws.ArgumentException.With.Message.EqualTo (msg)); // value type
//      Assert.That (() => new MutableType (typeof (ExpressionType)), Throws.ArgumentException.With.Message.EqualTo (msg)); // enum
//      Assert.That (() => new MutableType (typeof (Delegate)), Throws.ArgumentException.With.Message.EqualTo (msg));
//      Assert.That (() => new MutableType (typeof (MulticastDelegate)), Throws.ArgumentException.With.Message.EqualTo (msg));
//      Assert.That (() => new MutableType (typeof (List<>)), Throws.ArgumentException.With.Message.EqualTo (msg)); // open generics
//      Assert.That (() => new MutableType (typeof (List<int>)), Throws.Nothing); // closed generics
//      Assert.That (() => new MutableType (typeof (BlockExpression)), Throws.ArgumentException.With.Message.EqualTo (msg)); // no accessible constructor 
//    }

//    [Test]
//    public void BaseType ()
//    {
//      Assert.That (_modifiedType.BaseType, Is.EqualTo (typeof (object)));
//    }

//    [Test]
//    public void GetConstructorImpl_WithSingleAddedConstructor ()
//    {
//      // Arrange
//      var futureConstructor = FutureConstructorInfoObjectMother.Create (_modifiedType);
//      _modifiedType.AddConstructor (futureConstructor);

//      BindingFlags bindingFlags = (BindingFlags) (-1);
//      Binder binder = null;
//      Type[] parameterTypes = Type.EmptyTypes; // Cannot be null
//      ParameterModifier[] parameterModifiers = null;

//      // Act
//      var constructor = _modifiedType.GetConstructor (bindingFlags, binder, parameterTypes, parameterModifiers);

//      // Assert
//      Assert.That (constructor, Is.SameAs (futureConstructor));
//      Assert.That (constructor.DeclaringType, Is.SameAs (_modifiedType));
//    }

//    [Test]
//    public void GetConstructorImpl_WithoutAddedConstructor ()
//    {
//      // Arrange
//      BindingFlags bindingFlags = (BindingFlags) (-1);
//      Binder binder = null;
//      Type[] parameterTypes = Type.EmptyTypes; // Cannot be null
//      ParameterModifier[] parameterModifiers = null;

//      // Act
//      var constructor = _modifiedType.GetConstructor (bindingFlags, binder, parameterTypes, parameterModifiers);

//      // Assert
//      Assert.That (constructor, Is.Null);
//    }

//    [Test]
//    public void GetAttributeFlagsImpl ()
//    {
//      Assert.That (_modifiedType.Attributes, Is.EqualTo (_originalType.Attributes));
//    }

//    [Test]
//    public void GetInterfaces ()
//    {
//      // TODO: was reintun
//      Assert.That (_modifiedType.GetInterfaces(), Is.EqualTo (_originalType.GetInterfaces()));
//    }

//    [Test]
//    public void GetFields ()
//    {
//      // TODO: was reintun
//      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
//      Assert.That (_modifiedType.GetFields(bindingFlags), Is.EqualTo (_originalType.GetFields (bindingFlags)));
//    }
//  }
//}