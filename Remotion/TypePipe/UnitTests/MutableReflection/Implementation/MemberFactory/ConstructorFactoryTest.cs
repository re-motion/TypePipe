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
using Remotion.TypePipe.Dlr.Ast;
using NUnit.Framework;
using Remotion.Development.TypePipe.UnitTesting.Expressions;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.Expressions;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Implementation.MemberFactory;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation.MemberFactory
{
  [TestFixture]
  public class ConstructorFactoryTest
  {
    private ConstructorFactory _factory;

    private MutableType _mutableType;

    [SetUp]
    public void SetUp ()
    {
      _factory = new ConstructorFactory();

      _mutableType = MutableTypeObjectMother.Create ();
    }

    [Test]
    public void CreateConstructor ()
    {
      var attributes = MethodAttributes.Public;
      var parameters =
          new[]
          {
              ParameterDeclarationObjectMother.Create (typeof (string), "param1"),
              ParameterDeclarationObjectMother.Create (typeof (int), "param2")
          };
      var fakeBody = ExpressionTreeObjectMother.GetSomeExpression (typeof (object));
      Func<ConstructorBodyCreationContext, Expression> bodyProvider = ctx =>
      {
        Assert.That (ctx.This.Type, Is.SameAs (_mutableType));
        Assert.That (ctx.IsStatic, Is.False);
        Assert.That (ctx.Parameters.Select (p => p.Type), Is.EqualTo (new[] { typeof (string), typeof (int) }));
        Assert.That (ctx.Parameters.Select (p => p.Name), Is.EqualTo (new[] { "param1", "param2" }));

        return fakeBody;
      };

      var ctor = _factory.CreateConstructor (_mutableType, attributes, parameters.AsOneTime (), bodyProvider);

      Assert.That (ctor.DeclaringType, Is.SameAs (_mutableType));
      Assert.That (ctor.Name, Is.EqualTo (".ctor"));
      Assert.That (ctor.Attributes, Is.EqualTo (attributes | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName));
      var expectedParameterInfos =
          new[]
          {
              new { ParameterType = parameters[0].Type },
              new { ParameterType = parameters[1].Type }
          };
      var actualParameterInfos = ctor.GetParameters ().Select (pi => new { pi.ParameterType });
      Assert.That (actualParameterInfos, Is.EqualTo (expectedParameterInfos));
      var expectedBody = Expression.Block (typeof (void), fakeBody);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, ctor.Body);
    }

    [Test]
    public void CreateConstructor_Static ()
    {
      var attributes = MethodAttributes.Static;
      Func<ConstructorBodyCreationContext, Expression> bodyProvider = ctx =>
      {
        Assert.That (ctx.IsStatic, Is.True);
        return Expression.Empty ();
      };

      var ctor = _factory.CreateConstructor (_mutableType, attributes, ParameterDeclaration.None, bodyProvider);

      Assert.That (ctor.IsStatic, Is.True);
    }

    [Test]
    public void CreateConstructor_ThrowsForInvalidMethodAttributes ()
    {
      const string message =
          "The following MethodAttributes are not supported for constructors: " +
          "Final, Virtual, CheckAccessOnOverride, Abstract, PinvokeImpl, UnmanagedExport, RequireSecObject.\r\nParameter name: attributes";
      Assert.That (() => CreateConstructor (_mutableType, MethodAttributes.Final), Throws.ArgumentException.With.Message.EqualTo (message));
      Assert.That (() => CreateConstructor (_mutableType, MethodAttributes.Virtual), Throws.ArgumentException.With.Message.EqualTo (message));
      Assert.That (() => CreateConstructor (_mutableType, MethodAttributes.CheckAccessOnOverride), Throws.ArgumentException.With.Message.EqualTo (message));
      Assert.That (() => CreateConstructor (_mutableType, MethodAttributes.Abstract), Throws.ArgumentException.With.Message.EqualTo (message));
      Assert.That (() => CreateConstructor (_mutableType, MethodAttributes.PinvokeImpl), Throws.ArgumentException.With.Message.EqualTo (message));
      Assert.That (() => CreateConstructor (_mutableType, MethodAttributes.UnmanagedExport), Throws.ArgumentException.With.Message.EqualTo (message));
      Assert.That (() => CreateConstructor (_mutableType, MethodAttributes.RequireSecObject), Throws.ArgumentException.With.Message.EqualTo (message));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "A type initializer (static constructor) cannot have parameters.\r\nParameter name: parameters")]
    public void CreateConstructor_ThrowsIfStaticAndNonEmptyParameters ()
    {
      _factory.CreateConstructor (_mutableType, MethodAttributes.Static, ParameterDeclarationObjectMother.CreateMultiple (1), ctx => null);
    }

    [Test]
    public void CreateConstructor_ThrowsIfAlreadyExists ()
    {
      _mutableType.AddConstructor (parameters: ParameterDeclaration.None);

      Func<ConstructorBodyCreationContext, Expression> bodyProvider = ctx => Expression.Empty ();
      Assert.That (
          () => _factory.CreateConstructor (_mutableType, 0, ParameterDeclarationObjectMother.CreateMultiple (2), bodyProvider),
          Throws.Nothing);

      Assert.That (
          () =>
          _factory.CreateConstructor (_mutableType, MethodAttributes.Static, ParameterDeclaration.None, bodyProvider),
          Throws.Nothing);

      Assert.That (
          () => _factory.CreateConstructor (_mutableType, 0, ParameterDeclaration.None, bodyProvider),
          Throws.InvalidOperationException.With.Message.EqualTo ("Constructor with equal signature already exists."));
    }

    private MutableConstructorInfo CreateConstructor (MutableType mutableType, MethodAttributes attributes)
    {
      return _factory.CreateConstructor (mutableType, attributes, ParameterDeclaration.None, ctx => Expression.Empty ());
    }
  }
}