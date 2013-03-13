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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using System.Linq;

namespace Remotion.TypePipe.IntegrationTests.MutableReflection
{
  [TestFixture]
  public class ExpressionFactoriesTest
  {
    [Test]
    public void Convert_ThrowsIfCastIsGuaranteedToFail ()
    {
      var valueType = ReflectionObjectMother.GetSomeValueType();
      var otherValueType = typeof (bool);
      var genericParameter = ReflectionObjectMother.GetSomeGenericParameter();
      var otherGenericParameter = typeof (IList<>).GetGenericArguments().Single();
      Assert.That (otherValueType, Is.Not.SameAs (valueType));
      Assert.That (otherGenericParameter, Is.Not.SameAs (genericParameter));

      CheckConvertThrows (valueType, genericParameter);
      CheckConvertThrows (genericParameter, valueType);
      CheckConvertThrows (valueType, otherValueType);
      CheckConvertThrows (genericParameter, otherGenericParameter);
    }

    [Test]
    public void Assign_ForcesUserToInsertConvertViaObject ()
    {
      var genericParameter = typeof (IDictionary<,>).GetGenericArguments().First();
      var otherGenericParameter = typeof (IList<>).GetGenericArguments().Single();
      Assert.That (otherGenericParameter, Is.Not.SameAs (genericParameter));

      var left = Expression.Variable (genericParameter);
      var right = Expression.Default (otherGenericParameter);

      Assert.That (
          () => Expression.Assign (left, right),
          Throws.ArgumentException.With.Message.EqualTo ("Expression of type 'T' cannot be used for assignment to type 'TKey'"));

      var convertedRight = Expression.Convert (Expression.Convert (right, typeof (object)), left.Type);
      Assert.That (() => Expression.Assign (left, convertedRight), Throws.Nothing);
    }

    private static void CheckConvertThrows (Type fromType, Type toType)
    {
      var message = string.Format ("No coercion operator is defined between types '{0}' and '{1}'.", fromType, toType);
      Assert.That (() => Expression.Convert (Expression.Default (fromType), toType), Throws.InvalidOperationException.With.Message.EqualTo (message));
    }
  }
}