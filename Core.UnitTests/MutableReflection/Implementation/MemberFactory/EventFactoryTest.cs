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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection.Implementation.MemberFactory;
using Moq;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation.MemberFactory
{
  [TestFixture]
  public class EventFactoryTest
  {
    private Mock<IMethodFactory> _methodFactoryMock;

    private EventFactory _factory;

    private MutableType _mutableType;

    [SetUp]
    public void SetUp ()
    {
      _methodFactoryMock = new Mock<IMethodFactory> (MockBehavior.Strict);

      _factory = new EventFactory (_methodFactoryMock.Object);

      _mutableType = MutableTypeObjectMother.Create();
    }

    [Test]
    public void CreateEvent_Providers ()
    {
      var name = "Event";
      var accessorAttributes = (MethodAttributes) 7;
      var handlerType = typeof (SomeDelegate);
      var handlerReturnType = typeof (string);
      Func<MethodBodyCreationContext, Expression> addBodyProvider = ctx => null;
      Func<MethodBodyCreationContext, Expression> removeBodyProvider = ctx => null;
      Func<MethodBodyCreationContext, Expression> raiseBodyProvider = ctx => null;
      var fakeAddMethod = MutableMethodInfoObjectMother.Create (parameters: new[] { ParameterDeclarationObjectMother.Create (handlerType) });
      var fakeRemoveMethod = MutableMethodInfoObjectMother.Create (parameters: new[] { ParameterDeclarationObjectMother.Create (handlerType) });
      var fakeRaiseMethod = MutableMethodInfoObjectMother.Create (
          returnType: typeof (string),
          parameters: new[]
                      {
                          ParameterDeclarationObjectMother.Create (typeof (object)),
                          ParameterDeclarationObjectMother.Create (typeof (int).MakeByRefType())
                      });

      _methodFactoryMock
          .Setup (
              mock =>
                  mock.CreateMethod (
                      _mutableType,
                      "add_Event",
                      accessorAttributes | MethodAttributes.SpecialName,
                      GenericParameterDeclaration.None,
                      It.IsAny<Func<GenericParameterContext, Type>>(),
                      It.IsAny<Func<GenericParameterContext, IEnumerable<ParameterDeclaration>>>(),
                      addBodyProvider))
          .Callback (
              (
                  MutableType declaringType,
                  string nameArgument,
                  MethodAttributes attributes,
                  IEnumerable<GenericParameterDeclaration> genericParameters,
                  Func<GenericParameterContext, Type> returnTypeProvider,
                  Func<GenericParameterContext, IEnumerable<ParameterDeclaration>> parameterProvider,
                  Func<MethodBodyCreationContext, Expression> bodyProvider) =>
              {
                var returnType = returnTypeProvider (null);
                Assert.That (returnType, Is.SameAs (typeof (void)));

                var parameter = parameterProvider (null).Single();
                Assert.That (parameter.Type, Is.SameAs (handlerType));
                Assert.That (parameter.Name, Is.EqualTo ("handler"));
              })
          .Returns (fakeAddMethod)
          .Verifiable();
      _methodFactoryMock
          .Setup (
              mock =>
                  mock.CreateMethod (
                      _mutableType,
                      "remove_Event",
                      accessorAttributes | MethodAttributes.SpecialName,
                      GenericParameterDeclaration.None,
                      It.IsAny<Func<GenericParameterContext, Type>>(),
                      It.IsAny<Func<GenericParameterContext, IEnumerable<ParameterDeclaration>>>(),
                      removeBodyProvider))
          .Callback (
              (
                  MutableType declaringType,
                  string nameArgument,
                  MethodAttributes attributes,
                  IEnumerable<GenericParameterDeclaration> genericParameters,
                  Func<GenericParameterContext, Type> returnTypeProvider,
                  Func<GenericParameterContext, IEnumerable<ParameterDeclaration>> parameterProvider,
                  Func<MethodBodyCreationContext, Expression> bodyProvider) =>
              {
                var returnType = returnTypeProvider (null);
                Assert.That (returnType, Is.SameAs (typeof (void)));

                var parameter = parameterProvider (null).Single();
                Assert.That (parameter.Type, Is.SameAs (handlerType));
                Assert.That (parameter.Name, Is.EqualTo ("handler"));
              })
          .Returns (fakeRemoveMethod)
          .Verifiable();
      _methodFactoryMock
          .Setup (
              mock =>
                  mock.CreateMethod (
                      _mutableType,
                      "raise_Event",
                      accessorAttributes | MethodAttributes.SpecialName,
                      GenericParameterDeclaration.None,
                      It.IsAny<Func<GenericParameterContext, Type>>(),
                      It.IsAny<Func<GenericParameterContext, IEnumerable<ParameterDeclaration>>>(),
                      raiseBodyProvider))
          .Callback (
              (
                  MutableType declaringType,
                  string nameArgument,
                  MethodAttributes attributes,
                  IEnumerable<GenericParameterDeclaration> genericParameters,
                  Func<GenericParameterContext, Type> returnTypeProvider,
                  Func<GenericParameterContext, IEnumerable<ParameterDeclaration>> parameterProvider,
                  Func<MethodBodyCreationContext, Expression> bodyProvider) =>
              {
                var returnType = returnTypeProvider (null);
                Assert.That (returnType, Is.SameAs (handlerReturnType));

                var parameters = parameterProvider (null).ToList();
                Assert.That (parameters[0].Type, Is.SameAs (typeof (object)));
                Assert.That (parameters[0].Name, Is.EqualTo ("sender"));
                Assert.That (parameters[0].Attributes, Is.EqualTo (ParameterAttributes.None));
                Assert.That (parameters[1].Type, Is.SameAs (typeof (int).MakeByRefType()));
                Assert.That (parameters[1].Name, Is.EqualTo ("outParam"));
                Assert.That (parameters[1].Attributes, Is.EqualTo (ParameterAttributes.Out));
              })
          .Returns (fakeRaiseMethod)
          .Verifiable();

      var result = _factory.CreateEvent (_mutableType, name, handlerType, accessorAttributes, addBodyProvider, removeBodyProvider, raiseBodyProvider);

      _methodFactoryMock.Verify();
      Assert.That (result.DeclaringType, Is.SameAs (_mutableType));
      Assert.That (result.Name, Is.EqualTo (name));
      Assert.That (result.Attributes, Is.EqualTo (EventAttributes.None));
      Assert.That (result.EventHandlerType, Is.SameAs (handlerType));
      Assert.That (result.MutableAddMethod, Is.SameAs (fakeAddMethod));
      Assert.That (result.MutableRemoveMethod, Is.SameAs (fakeRemoveMethod));
      Assert.That (result.MutableRaiseMethod, Is.SameAs (fakeRaiseMethod));
    }

    [Test]
    public void CreateEvent_Providers_WithoutRaiseAccessor ()
    {
      Func<MethodBodyCreationContext, Expression> addBodyProvider = ctx => null;
      Func<MethodBodyCreationContext, Expression> removeBodyProvider = ctx => null;
      var addAndRemoveMethod = MutableMethodInfoObjectMother.Create (parameters: new[] { ParameterDeclarationObjectMother.Create (typeof (Action)) });
      _methodFactoryMock
          .Setup (
              stub => stub.CreateMethod (
                  It.IsAny<MutableType>(),
                  It.Is<string>(param => !param.StartsWith ("raise_")),
                  It.IsAny<MethodAttributes>(),
                  It.IsAny<IEnumerable<GenericParameterDeclaration>>(),
                  It.IsAny<Func<GenericParameterContext, Type>>(),
                  It.IsAny<Func<GenericParameterContext, IEnumerable<ParameterDeclaration>>>(),
                  It.IsAny<Func<MethodBodyCreationContext, Expression>>()))
          .Returns (addAndRemoveMethod);

      var result = _factory.CreateEvent (_mutableType, "Event", typeof (Action), 0, addBodyProvider, removeBodyProvider, null);

      Assert.That (result.MutableRaiseMethod, Is.Null);
    }

    [Test]
    public void CreateEvent_Providers_ThrowsForNonDelegateHandlerType ()
    {
      Assert.That (
          () => _factory.CreateEvent (_mutableType, "Event", typeof (int), 0, null, null, null),
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "Parameter 'handlerType' is a 'System.Int32', which cannot be assigned to type 'System.Delegate'.\r\nParameter name: handlerType"));
    }

    [Test]
    public void CreateEvent_Providers_ThrowsForInvalidAccessorAttributes ()
    {
      var message = "The following MethodAttributes are not supported for event accessor methods: "
                    + "RequireSecObject.\r\nParameter name: accessorAttributes";
      Assert.That (() => CreateEvent (_mutableType, MethodAttributes.RequireSecObject), Throws.ArgumentException.With.Message.EqualTo (message));
    }

    [Test]
    public void CreateEvent_Providers_ThrowsIfAlreadyExists ()
    {
      var factory = new EventFactory (new MethodFactory (new RelatedMethodFinder()));

      Func<MethodBodyCreationContext, Expression> bodyProvider = ctx => Expression.Empty();
      var event_ = _mutableType.AddEvent ("Event", typeof (Action), addBodyProvider: bodyProvider, removeBodyProvider: bodyProvider);

      Assert.That (
          () => factory.CreateEvent (_mutableType, "OtherName", event_.EventHandlerType, 0, bodyProvider, bodyProvider, null),
          Throws.Nothing);

      Assert.That (
          () => factory.CreateEvent (_mutableType, event_.Name, typeof (Action<int>), 0, bodyProvider, bodyProvider, null),
          Throws.Nothing);

      Assert.That (
          () => factory.CreateEvent (_mutableType, event_.Name, event_.EventHandlerType, 0, bodyProvider, bodyProvider, null),
          Throws.InvalidOperationException.With.Message.EqualTo ("Event with equal name and signature already exists."));
    }

    [Test]
    public void CreateEvent_Accessors ()
    {
      var name = "Event";
      var attributes = (EventAttributes) 7;
      var argumentType = ReflectionObjectMother.GetSomeType ();
      var returnType = ReflectionObjectMother.GetSomeOtherType ();
      var addRemoveParameters = new[] { new ParameterDeclaration (typeof (Func<,>).MakeGenericType (argumentType, returnType), "handler") };
      var addMethod = MutableMethodInfoObjectMother.Create (_mutableType, parameters: addRemoveParameters);
      var removeMethod = MutableMethodInfoObjectMother.Create (_mutableType, parameters: addRemoveParameters);
      var raiseMethod = MutableMethodInfoObjectMother.Create (
          _mutableType, returnType: returnType, parameters: new[] { new ParameterDeclaration (argumentType, "arg") });

      var result = _factory.CreateEvent (_mutableType, name, attributes, addMethod, removeMethod, raiseMethod);

      Assert.That (result.DeclaringType, Is.SameAs (_mutableType));
      Assert.That (result.Name, Is.EqualTo (name));
      Assert.That (result.Attributes, Is.EqualTo (attributes));
      Assert.That (result.MutableAddMethod, Is.SameAs (addMethod));
      Assert.That (result.MutableRemoveMethod, Is.SameAs (removeMethod));
      Assert.That (result.MutableRaiseMethod, Is.SameAs (raiseMethod));
    }

    [Test]
    public void CreateEvent_Accessors_ThrowsForInvalidPropertyAttributes ()
    {
      // No invalid EventAttributes.
    }

    [Test]
    public void CreateEvent_Accessors_ThrowsForDifferentStaticness ()
    {
      var staticMethod = MutableMethodInfoObjectMother.Create (attributes: MethodAttributes.Static);
      var instanceMethod = MutableMethodInfoObjectMother.Create (attributes: 0 /*instance*/);

      var message = "Accessor methods must be all either static or non-static.\r\nParameter name: addMethod";
      Assert.That (
          () => _factory.CreateEvent (_mutableType, "Event", 0, staticMethod, instanceMethod, null),
          Throws.ArgumentException.With.Message.EqualTo (message));
      Assert.That (
          () => _factory.CreateEvent (_mutableType, "Event", 0, instanceMethod, staticMethod, null),
          Throws.ArgumentException.With.Message.EqualTo (message));
      Assert.That (
          () => _factory.CreateEvent (_mutableType, "Event", 0, instanceMethod, instanceMethod, staticMethod),
          Throws.ArgumentException.With.Message.EqualTo (message));
    }

    [Test]
    public void CreateEvent_Accessors_ThrowsForDifferentDeclaringType ()
    {
      var method = MutableMethodInfoObjectMother.Create (_mutableType);
      var nonMatchingMethod = MutableMethodInfoObjectMother.Create ();

      var message = "{0} method is not declared on the current type.\r\nParameter name: {1}";
      Assert.That (
          () => _factory.CreateEvent (_mutableType, "Event", 0, nonMatchingMethod, method, null),
          Throws.ArgumentException.With.Message.EqualTo (string.Format (message, "Add", "addMethod")));
      Assert.That (
          () => _factory.CreateEvent (_mutableType, "Event", 0, method, nonMatchingMethod, null),
          Throws.ArgumentException.With.Message.EqualTo (string.Format (message, "Remove", "removeMethod")));
      Assert.That (
          () => _factory.CreateEvent (_mutableType, "Event", 0, method, method, nonMatchingMethod),
          Throws.ArgumentException.With.Message.EqualTo (string.Format (message, "Raise", "raiseMethod")));
    }

    [Test]
    public void CreateEvent_Accessors_ThrowsForNonVoidAddMethodOrRemoveMethod ()
    {
      var nonVoidMethod = MutableMethodInfoObjectMother.Create (declaringType: _mutableType, returnType: typeof (int));
      var voidMethod = MutableMethodInfoObjectMother.Create (declaringType: _mutableType);

      var message = "{0} method must have return type void.\r\nParameter name: {1}";
      Assert.That (
          () => _factory.CreateEvent (_mutableType, "Event", 0, nonVoidMethod, voidMethod, null),
          Throws.ArgumentException.With.Message.EqualTo (string.Format (message, "Add", "addMethod")));
      Assert.That (
          () => _factory.CreateEvent (_mutableType, "Event", 0, voidMethod, nonVoidMethod, null),
          Throws.ArgumentException.With.Message.EqualTo (string.Format (message, "Remove", "removeMethod")));
    }

    [Test]
    public void CreateEvent_Accessors_ThrowsForAddMethodWithNonDelegateParameter ()
    {
      var nonParameterMethod = MutableMethodInfoObjectMother.Create (_mutableType);
      var nonDelegateMethod = MutableMethodInfoObjectMother.Create (_mutableType, parameters: new[] { ParameterDeclarationObjectMother.Create () });
      var method = MutableMethodInfoObjectMother.Create (_mutableType, parameters: new[] { ParameterDeclarationObjectMother.Create (typeof (Action)) });

      var message = "{0} method must have a single parameter that is assignable to 'System.Delegate'.\r\nParameter name: {1}";
      Assert.That (
          () => _factory.CreateEvent (_mutableType, "Event", 0, nonParameterMethod, method, null),
          Throws.ArgumentException.With.Message.EqualTo (string.Format (message, "Add", "addMethod")));
      Assert.That (
          () => _factory.CreateEvent (_mutableType, "Event", 0, method, nonParameterMethod, null),
          Throws.ArgumentException.With.Message.EqualTo (string.Format (message, "Remove", "removeMethod")));
      Assert.That (
          () => _factory.CreateEvent (_mutableType, "Event", 0, nonDelegateMethod, method, null),
          Throws.ArgumentException.With.Message.EqualTo (string.Format (message, "Add", "addMethod")));
      Assert.That (
          () => _factory.CreateEvent (_mutableType, "Event", 0, method, nonDelegateMethod, null),
          Throws.ArgumentException.With.Message.EqualTo (string.Format (message, "Remove", "removeMethod")));
    }

    [Test]
    public void CreateEvent_Accessors_ThrowsForNonMatchingAddRemoveHandlerParameter ()
    {
      var addMethod = MutableMethodInfoObjectMother.Create (_mutableType, parameters: new[] { ParameterDeclarationObjectMother.Create (typeof (Action)) });
      var removeMethod = MutableMethodInfoObjectMother.Create (_mutableType, parameters: new[] { ParameterDeclarationObjectMother.Create (typeof (Func<int>)) });
      Assert.That (
          () => _factory.CreateEvent (_mutableType, "Event", 0, addMethod, removeMethod, null),
          Throws.ArgumentException
              .With.Message.EqualTo ("The type of the handler parameter is different for the add and remove method.\r\nParameter name: removeMethod"));
    }

    [Test]
    public void CreateEvent_Accessors_ThrowsForNonMatchingRaiseMethodSignature ()
    {
      var handlerType1 = typeof (Action<long>);
      var handlerType2 = typeof (Func<int, string>);
      var addOrRemoveMethod1 = MutableMethodInfoObjectMother.Create (_mutableType, parameters: new[] { ParameterDeclarationObjectMother.Create (handlerType1) });
      var addOrRemoveMethod2 = MutableMethodInfoObjectMother.Create (_mutableType, parameters: new[] { ParameterDeclarationObjectMother.Create (handlerType2) });
      var raiseMethod = MutableMethodInfoObjectMother.Create (_mutableType, parameters: new[] { ParameterDeclarationObjectMother.Create (typeof (int)) });

      var message = "The signature of the raise method does not match the handler type.\r\nParameter name: raiseMethod";
      Assert.That (
          () => _factory.CreateEvent (_mutableType, "Event", 0, addOrRemoveMethod1, addOrRemoveMethod1, raiseMethod),
          Throws.ArgumentException.With.Message.EqualTo (message));
      Assert.That (
          () => _factory.CreateEvent (_mutableType, "Event", 0, addOrRemoveMethod2, addOrRemoveMethod2, raiseMethod),
          Throws.ArgumentException.With.Message.EqualTo (message));
    }

    [Test]
    public void CreateEvent_Accessors_ThrowsIfAlreadyExists ()
    {
      var addRemoveMethod = MutableMethodInfoObjectMother.Create (
          _mutableType, parameters: new[] { ParameterDeclarationObjectMother.Create (typeof (Action)) });
      var differentHandlerAddRemoveMethod = MutableMethodInfoObjectMother.Create (
          _mutableType, parameters: new[] { ParameterDeclarationObjectMother.Create (typeof (Func<int>)) });
      var event_ = _mutableType.AddEvent2 ("Event", addMethod: addRemoveMethod, removeMethod: addRemoveMethod);

      Assert.That (() => _factory.CreateEvent (_mutableType, "OtherName", 0, addRemoveMethod, addRemoveMethod, null), Throws.Nothing);
      Assert.That (
          () => _factory.CreateEvent (_mutableType, event_.Name, 0, differentHandlerAddRemoveMethod, differentHandlerAddRemoveMethod, null),
          Throws.Nothing);
      Assert.That (
          () => _factory.CreateEvent (_mutableType, event_.Name, 0, addRemoveMethod, addRemoveMethod, null),
          Throws.InvalidOperationException.With.Message.EqualTo ("Event with equal name and signature already exists."));
    }

    private MutableEventInfo CreateEvent (MutableType mutableType, MethodAttributes accessorAttributes)
    {
      var argumentType = ReflectionObjectMother.GetSomeType ();
      var returnType = ReflectionObjectMother.GetSomeOtherType ();
      var handlerType = typeof (Func<,>).MakeGenericType (argumentType, returnType);

      return _factory.CreateEvent (
          mutableType,
          "dummy",
          handlerType,
          accessorAttributes,
          ctx => Expression.Empty (),
          ctx => Expression.Empty (),
          ctx => Expression.Default (returnType));
    }

    public delegate string SomeDelegate (object sender, out int outParam);
  }
}