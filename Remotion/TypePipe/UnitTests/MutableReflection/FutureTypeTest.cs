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
//using System.Reflection;
//using NUnit.Framework;
//using Remotion.TypePipe.MutableReflection;

//namespace Remotion.TypePipe.UnitTests.MutableReflection
//{
//  [TestFixture]
//  public class FutureTypeTest
//  {
//    private FutureType _futureType;

//    [SetUp]
//    public void SetUp ()
//    {
//      _futureType = FutureTypeObjectMother.Create();
//    }

//    [Test]
//    public void GetConstructorImpl_WithSingleAddedConstructor ()
//    {
//      // Arrange
//      var futureConstructor = FutureConstructorInfoObjectMother.Create (_futureType);
//      _futureType.AddConstructor (futureConstructor);

//      BindingFlags bindingFlags = (BindingFlags) (-1);
//      Binder binder = null;
//      Type[] parameterTypes = Type.EmptyTypes; // Cannot be null
//      ParameterModifier[] parameterModifiers = null;

//      // Act
//      var constructor = _futureType.GetConstructor (bindingFlags, binder, parameterTypes, parameterModifiers);

//      // Assert
//      Assert.That (constructor, Is.SameAs (futureConstructor));
//      Assert.That (constructor.DeclaringType, Is.SameAs (_futureType));
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
//      var constructor = _futureType.GetConstructor (bindingFlags, binder, parameterTypes, parameterModifiers);

//      // Assert
//      Assert.That (constructor, Is.Null);
//    }

//  }
//}