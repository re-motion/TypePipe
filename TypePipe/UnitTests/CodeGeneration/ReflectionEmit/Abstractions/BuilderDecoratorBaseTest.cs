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
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.Abstractions
{
  [TestFixture]
  public class BuilderDecoratorBaseTest
  {
    private ICustomAttributeTargetBuilder _innerMock;
    private IEmittableOperandProvider _operandProvider;

    private BuilderDecoratorBase _decorator;

    [SetUp]
    public void SetUp ()
    {
      _innerMock = MockRepository.GenerateStrictMock<ICustomAttributeTargetBuilder>();
      _operandProvider = MockRepository.GenerateStrictMock<IEmittableOperandProvider>();

      _decorator = MockRepository.GeneratePartialMock<BuilderDecoratorBase> (_innerMock, _operandProvider);
    }

    [Test]
    public void SetCustomAttribute ()
    {
      var attributeCtor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new AbcAttribute (null));
      var field = NormalizingMemberInfoFromExpressionUtility.GetField ((AbcAttribute obj) => obj.TypeField);
      var type = ReflectionObjectMother.GetSomeType();
      var ctorArg = new object[] { 7, new object[] { "7", type } };
      var declaration = new CustomAttributeDeclaration (attributeCtor, new object[] { ctorArg }, new NamedArgumentDeclaration (field, type));

      var emittableType = ReflectionObjectMother.GetSomeOtherType();
      _operandProvider.Expect (mock => mock.GetEmittableType (type)).Return (emittableType).Repeat.Twice();
      _innerMock
          .Expect (mock => mock.SetCustomAttribute (Arg<CustomAttributeDeclaration>.Is.Anything))
          .WhenCalled (
              mi =>
              {
                var emittableDeclaration = (ICustomAttributeData) mi.Arguments[0];
                Assert.That (emittableDeclaration, Is.Not.SameAs (declaration));
                Assert.That (emittableDeclaration.Constructor, Is.SameAs (attributeCtor));

                Assert.That (emittableDeclaration.ConstructorArguments, Has.Count.EqualTo (1));
                Assert.That (emittableDeclaration.ConstructorArguments.Single(), Is.EqualTo (new object[] { 7, new object[] { "7", emittableType } }));

                Assert.That (emittableDeclaration.NamedArguments, Has.Count.EqualTo (1));
                var namedArgument = emittableDeclaration.NamedArguments.Single();
                Assert.That (namedArgument.MemberInfo, Is.EqualTo (field));
                Assert.That (namedArgument.Value, Is.SameAs (emittableType));
              });

      _decorator.SetCustomAttribute (declaration);

      _operandProvider.VerifyAllExpectations();
      _innerMock.VerifyAllExpectations();
    }

    public class AbcAttribute : Attribute
    {
      public AbcAttribute (object ctorArg) { Dev.Null = ctorArg; }
      public Type TypeField;
    }
  }
}