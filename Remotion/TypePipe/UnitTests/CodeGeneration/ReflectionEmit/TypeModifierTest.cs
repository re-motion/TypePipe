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
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class TypeModifierTest
  {
    [Test]
    public void ApplyModifications ()
    {
      var handlerFactoryMock = MockRepository.GenerateStrictMock<ISubclassProxyBuilderFactory> ();

      var descriptor = UnderlyingTypeDescriptorObjectMother.Create (originalType: typeof (ClassWithMembers));
      var mutableTypePartialMock = MockRepository.GeneratePartialMock<MutableType> (
          descriptor,
          new MemberSignatureEqualityComparer (),
          new BindingFlagsEvaluator ());

      // Fields
      var existingFields = mutableTypePartialMock.ExistingFields.ToArray ();
      Assert.That (existingFields, Has.Length.EqualTo (1  /* 2 TODO 4695 */));
      //var modifiedField = existingFields[0];
      // Modifying existing fields is not supported (TODO 4695)
      //MutableFieldInfoTestHelper.ModifyField (modifiedField);
      var unmodifiedField = existingFields[0 /* 2  TODO 4695 */];

      // Constructors
      var existingConstructors = mutableTypePartialMock.ExistingConstructors.ToArray ();
      Assert.That (existingConstructors, Has.Length.EqualTo (2));
      var modifiedConstructor = existingConstructors[0];
      MutableConstructorInfoTestHelper.ModifyConstructor (modifiedConstructor);
      var unmodifiedConstructor = existingConstructors[1];

      // Methods
      // TODO 4809  Remove the weird Where matching by name
      var existingMethods = mutableTypePartialMock.ExistingMethods.Where(m => m.Name.StartsWith("Method")).ToArray ();
      Assert.That (existingMethods, Has.Length.EqualTo (2));
      var modifiedMethod = existingMethods[0];
      MutableMethodInfoTestHelper.ModifyMethod (modifiedMethod);
      var unmodifiedMethod = existingMethods[1];

      var builderMock = MockRepository.GenerateStrictMock<ISubclassProxyBuilder>();
      handlerFactoryMock.Expect (mock => mock.CreateBuilder (mutableTypePartialMock)).Return (builderMock);

      // TODO 4809 Remove
      foreach (var inheritedMethods in mutableTypePartialMock.ExistingMethods.Except(existingMethods))
      {
        MutableMethodInfo info = inheritedMethods;
        builderMock.Expect (mock => mock.HandleUnmodifiedMethod (info));
      }

      bool buildCalled = false;
// ReSharper disable AccessToModifiedClosure
      builderMock.Expect (mock => mock.HandleUnmodifiedField (unmodifiedField)).WhenCalled (mi => Assert.That (buildCalled, Is.False));
      builderMock.Expect (mock => mock.HandleUnmodifiedConstructor (unmodifiedConstructor)).WhenCalled (mi => Assert.That (buildCalled, Is.False));
      builderMock.Expect (mock => mock.HandleUnmodifiedMethod (unmodifiedMethod)).WhenCalled (mi => Assert.That (buildCalled, Is.False));
      mutableTypePartialMock.Expect (mock => mock.Accept (builderMock)).WhenCalled (mi => Assert.That (buildCalled, Is.False));
// ReSharper restore AccessToModifiedClosure

      var fakeType = ReflectionObjectMother.GetSomeType();
      builderMock.Expect (mock => mock.Build()).Return (fakeType).WhenCalled (mi => buildCalled = true);

      var typeModifier = new TypeModifier (handlerFactoryMock);

      var result = typeModifier.ApplyModifications (mutableTypePartialMock);

      handlerFactoryMock.VerifyAllExpectations();
      builderMock.VerifyAllExpectations();
      mutableTypePartialMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeType));
    }
  }

  public class ClassWithMembers
  {
    public int Field1;
    //public int Field2;  TODO 4809

    public ClassWithMembers () { }
    public ClassWithMembers (int i)
    {
      Dev.Null = i;
    }

    public virtual void Method1 () { }
    public virtual void Method2 () { }
  }
}