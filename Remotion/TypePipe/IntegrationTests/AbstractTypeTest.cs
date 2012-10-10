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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using System.Linq;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace TypePipe.IntegrationTests
{
  [TestFixture]
  public class AbstractTypeTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void NoChanges_RemainsAbstract ()
    {
      var type = AssembleType<AbstractTypeWithoutMethods> (
          mutableType =>
          {
            Assert.That (mutableType.UnderlyingSystemType.IsAbstract, Is.True);
            Assert.That (mutableType.IsAbstract, Is.True);
            Assert.That (mutableType.IsFullyImplemented, Is.True);
          });

      Assert.That (type.IsAbstract, Is.False);
      // The generated default constructor of abstract class has family visibility (protected in C#). 
      Assert.That (() => Activator.CreateInstance (type, nonPublic: true), Throws.Nothing);
    }
    
    [Test]
    public void ImplementPartially_RemainsAbstract ()
    {
      var type = AssembleType<AbstractTypeWithTwoMethods> (
          mutableType =>
          {
            var mutableMethod = mutableType.AllMutableMethods.Single (x => x.Name == "Method1");
            mutableMethod.SetBody (ctx => Expression.Empty());

            Assert.That (mutableType.IsAbstract, Is.True);
            Assert.That (mutableType.IsFullyImplemented, Is.False);
          });

      Assert.That (type.IsAbstract, Is.True);
    }

    [Ignore("TODO 5099")]
    [Test]
    public void AddAbstractMethod_BecomesAbstract ()
    {
      var type = AssembleType<ConcreteType> (
          mutableType =>
          {
            Assert.That (mutableType.IsAbstract, Is.False);
            Assert.That (mutableType.IsFullyImplemented, Is.True);

            var mutableMethod = mutableType.AddAbstractMethod ("Dummy", MethodAttributes.Public, typeof (int), ParameterDeclaration.EmptyParameters);
            Assert.That (mutableMethod.IsAbstract, Is.True);

            Assert.That (mutableType.IsAbstract, Is.True);
            Assert.That (mutableType.IsFullyImplemented, Is.False);
          });

      Assert.That (type.IsAbstract, Is.True);
    }

    [Ignore ("TODO 5099")]
    [Test]
    public void ImplementFully_BecomesConcrete ()
    {
      var type = AssembleType<AbstractTypeWithOneMethod> (
          mutableType =>
          {
            Assert.That (mutableType.IsAbstract, Is.True);
            Assert.That (mutableType.IsFullyImplemented, Is.False);

            var mutableMethod = mutableType.AllMutableMethods.Single (x => x.Name == "Method");
            mutableMethod.SetBody (ctx => Expression.Empty());

            Assert.That (mutableType.IsAbstract, Is.False);
            Assert.That (mutableType.IsFullyImplemented, Is.True);
          });

      Assert.That (type.IsAbstract, Is.False);
    }

    [Ignore ("TODO 5099")]
    [Test]
    public void ImplementFully_AbstractBaseType_AddMethod_BecomesConcrete ()
    {
      var type = AssembleType<AbstractDerivedTypeWithOneMethod> (
          mutableType =>
          {
            Assert.That (mutableType.IsAbstract, Is.True);
            Assert.That (mutableType.IsFullyImplemented, Is.False);

            mutableType.AddMethod (
                "Method",
                MethodAttributes.Public | MethodAttributes.Virtual,
                typeof (void),
                ParameterDeclaration.EmptyParameters,
                ctx => Expression.Empty());

            Assert.That (mutableType.IsAbstract, Is.False);
            Assert.That (mutableType.IsFullyImplemented, Is.True);
          });

      Assert.That (type.IsAbstract, Is.False);
    }

    [Ignore ("TODO 5099")]
    [Test]
    public void ImplementFully_AbstractBaseType_GetOrAddMutableMethod_BecomesConcrete ()
    {
      var type = AssembleType<AbstractDerivedTypeWithOneMethod> (
          mutableType =>
          {
            var abstractBaseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((AbstractTypeWithOneMethod obj) => obj.Method ());
            var mutableMethod = mutableType.GetOrAddMutableMethod (abstractBaseMethod);

            Assert.That (mutableMethod.IsAbstract, Is.True);
            mutableMethod.SetBody (ctx => Expression.Empty ());
            Assert.That (mutableMethod.IsAbstract, Is.False);

            Assert.That (mutableType.IsAbstract, Is.False);
            Assert.That (mutableType.IsFullyImplemented, Is.True);
          });

      Assert.That (type.IsAbstract, Is.False);
    }

    [Test]
    public void AccessingBodyOfAbstractMethod_Throws ()
    {
      var message = "An abstract method has no body.";
      AssembleType<AbstractTypeWithOneMethod> (
          mutableType =>
          {
            var mutableMethod = mutableType.AllMutableMethods.Single (x => x.Name == "Method");

            Assert.That (() => mutableMethod.Body, Throws.InvalidOperationException.With.Message.EqualTo (message));
            mutableMethod.SetBody (
                ctx =>
                {
                  Assert.That (() => ctx.PreviousBody, Throws.InvalidOperationException.With.Message.EqualTo (message));
                  Assert.That (() => ctx.GetPreviousBodyWithArguments (), Throws.InvalidOperationException.With.Message.EqualTo (message));
                  return Expression.Empty ();
                });
          });
    }

    [Test]
    public void BaseCallForAbstractMethod_Throws ()
    {
      AssembleType<AbstractDerivedTypeWithOneMethod> (
          mutableType =>
          {
            var mutableMethod = mutableType.AllMutableMethods.Single();
            mutableMethod.SetBody (
                ctx =>
                {
                  Assert.That (
                      () => ctx.GetBaseCall ("Method"),
                      Throws.ArgumentException.With.Message.EqualTo ("Cannot perform base call on abstract method.\r\nParameter name: baseMethod"));
                  return Expression.Empty();
                });
          });
    }

    public class ConcreteType { }

    public abstract class AbstractTypeWithoutMethods { }

    public abstract class AbstractTypeWithOneMethod
    {
      public abstract void Method ();
    }

    public abstract class AbstractTypeWithTwoMethods
    {
      public abstract void Method1 ();
      public abstract void Method2 ();
    }

    public abstract class AbstractDerivedTypeWithOneMethod : AbstractTypeWithOneMethod
    {
      public virtual void Dummy () { }
    }
  }
}