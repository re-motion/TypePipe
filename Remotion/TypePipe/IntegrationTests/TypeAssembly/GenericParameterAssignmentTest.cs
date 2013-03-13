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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using System.Linq;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.FunctionalProgramming;
using Remotion.Development.UnitTesting.Enumerables;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class GenericParameterAssignmentTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void Assign_LocalVariable ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetGenericMethodDefinition ((DomainType o) => o.GenericMethod<C, D> (null, null));
      var type = AssembleType<DomainType> (
          p =>
          p.GetOrAddOverride (method)
           .SetBody (
               ctx =>
               {
                 var tRef = ctx.GenericParameters[0];
                 var pRef = (Expression) ctx.Parameters[0];

                 var genFromRef1 = new { ToType = tRef, FromType = typeof (object), FromOperand = (Expression) Expression.Default (typeof (object)) };
                 var genFromRef2 = new { ToType = tRef, FromType = typeof (A), FromOperand = (Expression) Expression.Default (typeof (A)) };
                 var genFromRef3 = new { ToType = tRef, FromType = typeof (B), FromOperand = (Expression) Expression.Default (typeof (B)) };
                 var genFromRef4 = new { ToType = tRef, FromType = typeof (C) };
                 var genFromRef5 = new { ToType = tRef, FromType = typeof (D) };

                 var genFromGen1 = new { ToType = tRef, FromType = tRef, FromOperand = pRef };
                 var genFromGen2 = new { ToType = tRef, FromType = ctx.GenericParameters[1], FromOperand = (Expression) ctx.Parameters[1] };

                 var refFromGen1 = new { ToType = typeof (object), FromType = tRef, FromOperand = pRef };
                 var refFromGen2 = new { ToType = typeof (A), FromType = tRef, FromOperand = pRef };
                 var refFromGen3 = new { ToType = typeof (B), FromType = tRef, FromOperand = pRef };
                 var refFromGen4 = new { ToType = typeof (C), FromType = tRef };
                 var refFromGen5 = new { ToType = typeof (D), FromType = tRef };

                 var genfromVal = new { ToType = ctx.GenericParameters[0], FromType = typeof (string) };
                 var valFromGen = new { ToType = typeof (int), FromType = ctx.GenericParameters[0] };

                 var invalidCompileTimeCasts = new[] { genFromRef4, genFromRef5, refFromGen4, refFromGen5, genfromVal, valFromGen };
                 invalidCompileTimeCasts.ApplySideEffect (m => CheckExceptionIsThrown (m.ToType, m.FromType)).ForceEnumeration();

                 var validCasts = new[] { genFromRef1, genFromRef2, genFromRef3, genFromGen1, genFromGen2, refFromGen1, refFromGen2, refFromGen3 };
                 var variables = validCasts.Select (m => Expression.Variable (m.ToType)).ToList();
                 var assignments = validCasts.Zip (variables, (m, v) => CreateConvertAssignment (v, m.FromType, m.FromOperand));

                 return Expression.Block (variables, assignments);
               }));

      var instance = (DomainType) Activator.CreateInstance (type);
      Assert.That (() => instance.GenericMethod<C, D> (null, null), Throws.Nothing);
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

    public class DomainType
    {
      public virtual void GenericMethod<T1, T2> (T1 t1, T2 t2)
          where T1 : B where T2 : T1 {}
    }

    public class A {}
    public class B : A {}
    public class C : B {}
    public class D : C {}
  }
}