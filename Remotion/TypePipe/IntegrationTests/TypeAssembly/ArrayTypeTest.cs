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
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using System.Linq;
using Remotion.TypePipe.MutableReflection.BodyBuilding;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class ArrayTypeTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void CopyVector ()
    {
      var type = AssembleType<DomainType> (
          typeContext =>
          {
            var elementType = typeContext.ProxyType;
            var vectorType = elementType.MakeArrayType();

            typeContext.ProxyType.AddMethod (
                "Method",
                MethodAttributes.Public | MethodAttributes.Static,
                vectorType,
                new[] { new ParameterDeclaration (vectorType) },
                ctx =>
                {
                  var newVector = Expression.Variable (vectorType, "newVector");
                  var index = Expression.Variable (typeof (int), "i");
                  var breakLabel = Expression.Label();

                  return Expression.Block (
                      new[] { newVector, index },
                      Expression.Assign (newVector, Expression.NewArrayBounds (elementType, Expression.Property (ctx.Parameters[0], "Length"))),
                      Expression.Assign (index, Expression.Constant (0)),
                      Expression.Loop (
                          Expression.Block (
                              Expression.IfThen (Expression.Equal (index, Expression.ArrayLength (ctx.Parameters[0])), Expression.Goto (breakLabel)),
                              Expression.Assign (Expression.ArrayAccess (newVector, index), Expression.ArrayAccess (ctx.Parameters[0], index)),
                              Expression.PostIncrementAssign (index)),
                          breakLabel),
                      newVector);
                });
          });

      var method = type.GetMethod ("Method", BindingFlags.Static | BindingFlags.Public);
      var array = Array.CreateInstance (type, 2);
      array.SetValue (Activator.CreateInstance (type), 0);
      array.SetValue (Activator.CreateInstance (type), 1);

      var result = method.Invoke (null, new object[] { array });

      Assert.That (result.GetType().GetElementType(), Is.EqualTo (type));
      Assert.That (result, Is.Not.SameAs (array));
      Assert.That (result, Is.EqualTo (array));
    }

    [Test]
    public void CreateMultiDimensionalArray ()
    {
      // public static ProxyType[,] Method (int l1, int l2) {
      //  return new ProxyType[l1,l2];
      // }
      var createArray = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => Array.CreateInstance (null, 0, 0));
      var type = AssembleType<DomainType> (
          typeContext =>
          {
            var elementType = typeContext.ProxyType;
            var multiDimensionalArrayType = elementType.MakeArrayType (2);

            typeContext.ProxyType.AddMethod (
                "Method",
                MethodAttributes.Public | MethodAttributes.Static,
                multiDimensionalArrayType,
                new[] { new ParameterDeclaration (typeof (int), "l1"), new ParameterDeclaration (typeof (int), "l1") },
                ctx =>
                {
                  var array = Expression.Call (createArray, Expression.Constant (elementType, typeof (Type)), ctx.Parameters[0], ctx.Parameters[1]);
                  return Expression.Convert (array, multiDimensionalArrayType);
                });
          });
      var method = type.GetMethod ("Method", BindingFlags.Static | BindingFlags.Public);

      var result = (Array) method.Invoke (null, new object[] { 7, 8 });

      Assert.That (result.GetType().GetElementType(), Is.EqualTo (type));
      Assert.That (result.Rank, Is.EqualTo (2));
      Assert.That (result.GetLength (0), Is.EqualTo (7));
      Assert.That (result.GetLength (1), Is.EqualTo (8));
      Assert.That (result.GetLowerBound (0), Is.EqualTo (0));
      Assert.That (result.GetLowerBound (1), Is.EqualTo (0));
    }

    [Test]
    public void CreateJaggedVector ()
    {
      // public static ProxyType[][] Method (int l1, int l2) {
      //   return new ProxyType[][] { new ProxyType[l1], new ProxyType[l2] };
      // }
      var type = AssembleType<DomainType> (
          typeContext =>
          {
            var elementType = typeContext.ProxyType;
            var vectorType = elementType.MakeArrayType();
            var vectorVectorType = vectorType.MakeArrayType();

            typeContext.ProxyType.AddMethod (
                "Method",
                MethodAttributes.Public | MethodAttributes.Static,
                vectorVectorType,
                new[] { new ParameterDeclaration (typeof (int), "l1"), new ParameterDeclaration (typeof (int), "l2") },
                ctx =>
                Expression.NewArrayInit (
                    vectorType,
                    Expression.NewArrayBounds (elementType, ctx.Parameters[0]),
                    Expression.NewArrayBounds (elementType, ctx.Parameters[1])));
          });

      var method = type.GetMethod ("Method", BindingFlags.Static | BindingFlags.Public);

      var result = (Array) method.Invoke (null, new object[] { 7, 8 });

      Assert.That (result.GetType().GetElementType(), Is.EqualTo (type.MakeArrayType()));
      Assert.That (result.Rank, Is.EqualTo (1));
      Assert.That (result.Length, Is.EqualTo (2));
      Assert.That (result.GetValue (0).As<Array>().Length, Is.EqualTo (7));
      Assert.That (result.GetValue (1).As<Array>().Length, Is.EqualTo (8));
    }

    [Test]
    public void UnsupportedArrayMembers ()
    {
      SkipSavingAndPeVerification();

      CheckThrowsNotSupportedException (
          ctx =>
          {
            var arrayConstructor = ctx.DeclaringType.MakeArrayType().GetConstructors().Single();
            return Expression.New (arrayConstructor, Expression.Constant (7));
          },
          "Array constructors of array types containing a custom element type cannot be used directly in expression trees. For one-dimensional "
          + "arrays use the NewArrayBounds or NewArrayInit expression factories. For multi-dimensional arrays call the static method "
          + "Array.CreateInstance and cast the result to the specific array type.");

      CheckThrowsNotSupportedException (
          ctx =>
          {
            var arrayType = ctx.DeclaringType.MakeArrayType();
            var arrayMethod = arrayType.GetMethod ("Get");
            return Expression.Call (Expression.Default (arrayType), arrayMethod, Expression.Constant (7));
          },
          "Methods on array types containing a custom element type cannot be used in expression trees. For one-dimensional arrays use the "
          + "specialized expression factories ArrayAccess and ArrayLength.For multi-dimensional arrays call Array.GetValue, Array.SetValue, "
          + "Array.Length and related base members.");

      CheckThrowsNotSupportedException (
          ctx => Expression.NewArrayBounds (ctx.DeclaringType, Expression.Constant (7), Expression.Constant (7)),
          "The expression factory NewArrayBounds is not supported for multi-dimensional arrays. To create a multi-dimensional array call the "
          + "static method Array.CreateInstance and cast the result to the specific array type.");
    }

    private void CheckThrowsNotSupportedException (Func<MethodBodyCreationContext, Expression> bodyProvider, string messagePart)
    {
      Assert.That (
          () => AssembleType<DomainType> (p => p.AddMethod ("M", bodyProvider: bodyProvider)),
          Throws.TypeOf<NotSupportedException>().With.Message.Contains (messagePart));
    }

    public class DomainType {}
  }
}