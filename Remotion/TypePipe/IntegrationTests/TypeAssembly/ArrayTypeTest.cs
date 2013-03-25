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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [Ignore ("TODO 5409")]
  [TestFixture]
  public class ArrayTypeTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void CopyVector ()
    {
      var type = AssembleType<DomainType> (
          typeContext =>
          {
            var vectorType = typeContext.ProxyType.MakeArrayType();

            typeContext.ProxyType.AddMethod (
                "Method",
                MethodAttributes.Public | MethodAttributes.Static,
                vectorType,
                new[] { new ParameterDeclaration (vectorType) },
                ctx =>
                {
                  var arrayConstructor = vectorType.GetConstructors().Single();
                  var newArray = Expression.Variable (vectorType, "arr");
                  var index = Expression.Variable (typeof (int), "i");
                  var breakLabel = Expression.Label();

                  return Expression.Block (
                      new[] { newArray, index },
                      Expression.Assign (newArray, Expression.New (arrayConstructor, Expression.Property (ctx.Parameters[0], "Length"))),
                      Expression.Assign (index, Expression.Constant (0)),
                      Expression.Loop (
                          Expression.Block (
                              Expression.IfThen (Expression.Equal (index, Expression.ArrayLength (ctx.Parameters[0])), Expression.Goto (breakLabel)),
                              Expression.Assign (Expression.ArrayAccess (newArray, index), Expression.ArrayAccess (ctx.Parameters[0], index)),
                              Expression.Increment (index)),
                          breakLabel),
                      newArray);
                });
          });

      var method = type.GetMethod ("Method", BindingFlags.Static | BindingFlags.Public);
      var array = Array.CreateInstance (type, 2);
      array.SetValue (Activator.CreateInstance (type), 0);
      array.SetValue (Activator.CreateInstance (type), 1);

      var result = method.Invoke (null, new object[] { array });

      Assert.That (result, Is.Not.SameAs (array));
      Assert.That (result.GetType().GetElementType(), Is.EqualTo (type));
      Assert.That (result, Is.EqualTo (array));
    }

    [Test]
    public void CreateMultiDimensionalArray ()
    {
      // public static ProxyType[,] Method (int l) {
      //  return new ProxyType[l,l];
      // }
      var type = AssembleType<DomainType> (
          typeContext =>
          {
            var multiDimensionalArrayType = typeContext.ProxyType.MakeArrayType (2);

            typeContext.ProxyType.AddMethod (
                "Method",
                MethodAttributes.Public | MethodAttributes.Static,
                multiDimensionalArrayType,
                new[] { new ParameterDeclaration (typeof (int), "l") },
                ctx => Expression.NewArrayBounds (typeContext.ProxyType, ctx.Parameters[0], ctx.Parameters[0]));
          });
      var method = type.GetMethod ("Method", BindingFlags.Static | BindingFlags.Public);

      var result = (Array) method.Invoke (null, new object[] { 7 });

      Assert.That (result.GetType().GetElementType(), Is.EqualTo (type));
      Assert.That (result.Rank, Is.EqualTo (2));
      Assert.That (result.GetLength (0), Is.EqualTo (7));
      Assert.That (result.GetLength (1), Is.EqualTo (7));
      Assert.That (result.GetLowerBound (0), Is.EqualTo (0));
      Assert.That (result.GetLength (1), Is.EqualTo (0));
    }

    [Test]
    public void CreateJaggedArray ()
    {
      var createArray = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => Array.CreateInstance (null, 0));

      // public static ProxyType[][] Method (int l1, int l2) {
      //   return new ProxyType[][] { new ProxyType[l1], new ProxyType[l2] };
      // }
      var type = AssembleType<DomainType> (
          typeContext =>
          {
            var elementType = typeContext.ProxyType;
            var vectorType = elementType.MakeArrayType();
            var jaggedVectorType = vectorType.MakeArrayType();

            typeContext.ProxyType.AddMethod (
                "Method",
                MethodAttributes.Public | MethodAttributes.Static,
                jaggedVectorType,
                new[] { new ParameterDeclaration (typeof (int), "l1"), new ParameterDeclaration (typeof (int), "l2") },
                ctx =>
                Expression.NewArrayInit (
                    vectorType,
                    Expression.Call (createArray, Expression.Constant (elementType), ctx.Parameters[0]),
                    Expression.Call (createArray, Expression.Constant (elementType), ctx.Parameters[1])));
          });

      var method = type.GetMethod ("Method", BindingFlags.Static | BindingFlags.Public);

      var result = (Array) method.Invoke (null, new object[] { 7, 8 });

      Assert.That (result.GetType().GetElementType(), Is.EqualTo (type.MakeArrayType()));
      Assert.That (result.Rank, Is.EqualTo (1));
      Assert.That (result.GetLength (0), Is.EqualTo (7));
      Assert.That (result.GetLength (1), Is.EqualTo (8));
    }

    public class DomainType { }
  }
}