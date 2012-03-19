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
using System.Reflection.Emit;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.BuilderAbstractions;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class TypeModificationHandlerTest
  {
    private ITypeBuilder _subclassProxyBuilderMock;
    private TypeModificationHandler _handler;

    [SetUp]
    public void SetUp ()
    {
      _subclassProxyBuilderMock = MockRepository.GenerateMock<ITypeBuilder>();
      _handler = new TypeModificationHandler (_subclassProxyBuilderMock);
    }

    [Test]
    public void HandleAddedInterface ()
    {
      var addedInterface = ReflectionObjectMother.GetSomeInterfaceType();

      _handler.HandleAddedInterface (addedInterface);

      _subclassProxyBuilderMock.AssertWasCalled (mock => mock.AddInterfaceImplementation (addedInterface));
    }

    [Test]
    public void HandleAddedField ()
    {
      var addedField = MutableFieldInfoObjectMother.Create();

      _handler.HandleAddedField (addedField);

      _subclassProxyBuilderMock.AssertWasCalled (mock => mock.DefineField (addedField.Name, addedField.FieldType, addedField.Attributes));
    }

    [Test]
    public void HandleAddedField_WithCustomAttribute ()
    {
      var constructor = ReflectionObjectMother.GetConstructor (() => new CustomAttribute(""));
      var property = ReflectionObjectMother.GetProperty ((CustomAttribute attr) => attr.Property);
      var field = ReflectionObjectMother.GetField ((CustomAttribute attr) => attr.Field);
      var constructorArguments = new object[] { "ctorArgs" };
      var declaration = new CustomAttributeDeclaration (
          constructor,
          constructorArguments,
          new NamedAttributeArgumentDeclaration (property, 7),
          new NamedAttributeArgumentDeclaration (field, "test"));
      var addedField = MutableFieldInfoObjectMother.Create ();
      addedField.AddCustomAttribute (declaration);

      var fieldBuilderMock = MockRepository.GenerateMock<IFieldBuilder>();
      _subclassProxyBuilderMock
          .Stub (stub => stub.DefineField (addedField.Name, addedField.FieldType, addedField.Attributes))
          .Return (fieldBuilderMock);
      fieldBuilderMock
          .Expect (mock => mock.SetCustomAttribute (Arg<CustomAttributeBuilder>.Is.Anything))
          .WhenCalled (mi => CheckCustomAttributeBuilder (
              (CustomAttributeBuilder) mi.Arguments[0], 
              constructor,
              constructorArguments,
              new[] { property },
              new object[]  { 7 },
              new[] { field },
              new[] { "test" }));
      
      _handler.HandleAddedField (addedField);

      fieldBuilderMock.VerifyAllExpectations();
    }

    private void CheckCustomAttributeBuilder (
        CustomAttributeBuilder builder,
        ConstructorInfo expectedCtor,
        object[] expectedCtorArgs,
        PropertyInfo[] expectedPropertyInfos,
        object[] expectedPropertyValues,
        FieldInfo[] expectedFieldInfos,
        object[] expectedFieldValues)
    {
      var actualConstructor = (ConstructorInfo) PrivateInvoke.GetNonPublicField (builder, "m_con");
      var actualConstructorArgs = (object[]) PrivateInvoke.GetNonPublicField (builder, "m_constructorArgs");
      var actualBlob = (byte[]) PrivateInvoke.GetNonPublicField (builder, "m_blob");

      Assert.That (actualConstructor, Is.SameAs (expectedCtor));
      Assert.That (actualConstructorArgs, Is.EqualTo (expectedCtorArgs));

      var testBuilder = new CustomAttributeBuilder (
          expectedCtor, expectedCtorArgs, expectedPropertyInfos, expectedPropertyValues, expectedFieldInfos, expectedFieldValues);
      var expectedBlob = (byte[]) PrivateInvoke.GetNonPublicField (testBuilder, "m_blob");
      Assert.That (actualBlob, Is.EqualTo (expectedBlob));
    }

    public class CustomAttribute : Attribute
    {
      public string Field;

      public CustomAttribute (string ctorArgument)
      {
        CtorArgument = ctorArgument;
      }

      public string CtorArgument { get; private set; }
      public int Property { get; set; }
    }
  }
}