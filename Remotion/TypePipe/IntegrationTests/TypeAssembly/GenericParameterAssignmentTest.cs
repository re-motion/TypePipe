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

    [Ignore ("5440")]
    [Test]
    public void Assign_LocalVariable ()
    {
      var method = typeof (DomainType).GetMethod ("GenericMethod");
      var type = AssembleType<DomainType> (
          p =>
          p.GetOrAddOverride (method)
           .SetBody (
               ctx =>
               {
                 var genFromRef = new { ToType = ctx.GenericParameters[0], FromType = typeof (B) };
                 var genFromVal = new { ToType = ctx.GenericParameters[2], FromType = typeof (int) };
                 var genFromGen = new { ToType = ctx.GenericParameters[0], FromType = ctx.GenericParameters[1] };
                 var refFromGen = new { ToType = typeof (A), FromType = ctx.GenericParameters[0] };
                 var valFromGen = new { ToType = typeof (int), FromType = ctx.GenericParameters[1] };

                 var mapping = new[] { genFromRef, genFromVal, genFromGen, refFromGen, valFromGen };
                 var variables = mapping.Select (m => Expression.Variable (m.ToType)).ToList();
                 var assignments = mapping.Zip (variables, (m, v) => CreateConvertAssignment (v, m.FromType));

                 return Expression.Block (variables, assignments);
               }));

      var instance = (DomainType) Activator.CreateInstance (type);
      instance.GenericMethod<A, B, int>();
    }

    private Expression CreateAssignment (Type toType, Type fromType)
    {
      return Expression.Assign (Expression.Variable (toType), Expression.Default (fromType));
    }

    private Expression CreateConvertAssignment (ParameterExpression variable, Type fromType)
    {
      return Expression.Assign (variable, Expression.Convert (Expression.Default (fromType), variable.Type));
    }

    public class DomainType
    {
      public virtual void GenericMethod<TRef, TOtherRef, TValue> () where TOtherRef : TRef {}
    }

    public class A {}
    public class B : A{}
  }
}