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
using Remotion.FunctionalProgramming;
using Remotion.TypePipe.MutableReflection;
using Remotion.Development.UnitTesting.Enumerables;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class GenericParameterAssignmentTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void CreateAssignment ()
    {
      SkipSavingAndPeVerification();
      var genericParameter = ReflectionObjectMother.GetSomeGenericParameter();

      // TODO rework this test.. there are a lot more options :/!!!

      // RefType <- x
      Assert.That (() => CreateAssignment (typeof (object), typeof (string)), Throws.Nothing);
      Assert.That (() => CreateAssignment (typeof (IComparable), typeof (string)), Throws.Nothing);

      Assert.That (() => CreateAssignment (typeof (object), typeof (object)), Throws.Nothing);
      Assert.That (() => CreateAssignment (typeof (object), typeof (int)), Throws.ArgumentException);
      Assert.That (() => CreateAssignment (typeof (object), genericParameter), Throws.ArgumentException);

      Assert.That (() => CreateAssignment (typeof (string), typeof (object)), Throws.ArgumentException);
      Assert.That (() => CreateAssignment (typeof (string), typeof (int)), Throws.ArgumentException);
      Assert.That (() => CreateAssignment (typeof (string), genericParameter), Throws.ArgumentException);

      // ValueType <- x
      Assert.That (() => CreateAssignment (typeof (int), typeof (object)), Throws.ArgumentException);
      Assert.That (() => CreateAssignment (typeof (int), typeof (int)), Throws.Nothing);
      Assert.That (() => CreateAssignment (typeof (int), genericParameter), Throws.ArgumentException);

      // Generic <- x
      Assert.That (() => CreateAssignment (genericParameter, typeof (object)), Throws.ArgumentException);
      Assert.That (() => CreateAssignment (genericParameter, typeof (int)), Throws.ArgumentException);
      Assert.That (() => CreateAssignment (genericParameter, genericParameter), Throws.Nothing);
    }

    [Test]
    public void Assign_LocalVariable ()
    {
      SkipDeletion();

      //public void GenericMethod<TRef, TOtherRef, TValue> (TRef tref, TOtherRef tOtherRef, TValue tValue)
      //    where TRef : B
      //    where TOtherRef : TRef
      //    where TValue : int // Not possible in C#.
      //{} 

      var type = AssembleType<DomainType> (
          p =>
          p.AddGenericMethod (
              "GenericMethod",
              MethodAttributes.Public,
              new[]
              {
                  new GenericParameterDeclaration ("TRef", constraintProvider: ctx => new[] { typeof (B) }),
                  new GenericParameterDeclaration ("TOtherRef", constraintProvider: ctx => new[] { ctx.GenericParameters[0] }),
                  new GenericParameterDeclaration ("TValue", constraintProvider: ctx => new[] { typeof (int) })
              },
              returnTypeProvider: ctx => typeof (void),
              parameterProvider: ctx =>
                  new[]
                  {
                      new ParameterDeclaration (ctx.GenericParameters[0], "tRef"),
                      new ParameterDeclaration (ctx.GenericParameters[1], "tOtherRef"),
                      new ParameterDeclaration (ctx.GenericParameters[2], "tValue")
                  },
           
               bodyProvider: ctx =>
               {
                 var tRef = ctx.GenericParameters[0];
                 var pRef = (Expression) ctx.Parameters[0];

                 var genFromRef1 = new { ToType = tRef, FromType = typeof (object), FromOperand = (Expression) Expression.Default (typeof (object)) };
                 var genFromRef2 = new { ToType = tRef, FromType = typeof (A), FromOperand = (Expression) Expression.Default (typeof (A)) };
                 var genFromRef3 = new { ToType = tRef, FromType = typeof (B), FromOperand = (Expression) Expression.Default (typeof (B)) };
                 var genFromRef4 = new { ToType = tRef, FromType = typeof (C) };
                 var genFromRef5 = new { ToType = tRef, FromType = typeof (D) };

                 var genFromVal1 = new { ToType = ctx.GenericParameters[2], FromType = typeof (int), FromOperand = (Expression) Expression.Constant (7) };
                 var genfromVal2 = new { ToType = ctx.GenericParameters[2], FromType = typeof (string) };
                 
                 var genFromGen1 = new { ToType = tRef, FromType = tRef, FromOperand = pRef };
                 var genFromGen2 = new { ToType = tRef, FromType = ctx.GenericParameters[1], FromOperand = (Expression) ctx.Parameters[1] };
                 var genFromGen3 = new { ToType = tRef, FromType = ctx.GenericParameters[2] };

                 var refFromGen1 = new { ToType = typeof (object), FromType = tRef, FromOperand = pRef };
                 var refFromGen2 = new { ToType = typeof (A), FromType = tRef, FromOperand = pRef };
                 var refFromGen3 = new { ToType = typeof (B), FromType = tRef, FromOperand = pRef };
                 var refFromGen4 = new { ToType = typeof (C), FromType = tRef };
                 var refFromGen5 = new { ToType = typeof (D), FromType = tRef };

                 var valFromGen1 = new { ToType = typeof (int), FromType = ctx.GenericParameters[2], FromOperand = (Expression) ctx.Parameters[2] };
                 var valFromGen2 = new { ToType = typeof (string), FromType = ctx.GenericParameters[2] };

                 var invalidCompileTimeCasts = new[] { genFromRef4, genFromRef5, genfromVal2, genFromGen3, refFromGen4, refFromGen5, valFromGen2 };
                 invalidCompileTimeCasts.ApplySideEffect (m => CheckExceptionIsThrown (m.ToType, m.FromType)).ForceEnumeration();

                 var validCasts = new[]
                                  {
                                      genFromRef1, genFromRef2, genFromRef3,
                                      genFromVal1,
                                      genFromGen1, genFromGen2,
                                      refFromGen1, refFromGen2, refFromGen3,
                                      //valFromGen1
                                  };
                 var variables = validCasts.Select (m => Expression.Variable (m.ToType)).ToList();
                 var assignments = validCasts.Zip (variables, (m, v) => CreateConvertAssignment (v, m.FromType, m.FromOperand));

                 return Expression.Block (variables, assignments);
               }));

      var method = type.GetMethod ("GenericMethod").MakeGenericMethod (typeof (C), typeof (D), typeof (int));
      var instance = (DomainType) Activator.CreateInstance (type);
      
      // instance.GenericMethod<C, D, int> (null, null, 7);
      Assert.That (() => method.Invoke (instance, new object[] { null, null, 7 }), Throws.Nothing);
    }

    private Expression CreateAssignment (Type toType, Type fromType)
    {
      return Expression.Assign (Expression.Variable (toType), Expression.Default (fromType));
    }

    private Expression CreateConvertAssignment (ParameterExpression variable, Type fromType, Expression fromOperand)
    {
      return Expression.Assign (variable, Expression.Convert (fromOperand, variable.Type));
    }

    private void CheckExceptionIsThrown (Type toType, Type fromType)
    {
      Assert.That (
          () => Expression.Convert (Expression.Default (fromType), toType),
          Throws.InvalidOperationException.With.Message.StartsWith ("No coercion operator is defined between types"));
    }

    public class DomainType { }

    public class A {}
    public class B : A {}
    public class C : B {}
    public class D : C {}
  }
}