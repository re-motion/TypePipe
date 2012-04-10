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

namespace Remotion.TypePipe.UnitTests.MutableReflection.ExpressionTreeIntegration
{
  [TestFixture]
  public class MutableFieldInfoTest
  {
    [Test]
    public void Field_Read_Static ()
    {
      var field = MutableFieldInfoObjectMother.Create (attributes: FieldAttributes.Static);

      var expression = Expression.Field (null, field);

      Assert.That (expression.Member, Is.SameAs (field));
    }

    [Test]
    public void Field_Read_Instance ()
    {
      var declaringType = MutableTypeObjectMother.Create();
      var instance = Expression.Variable (declaringType);
      var field = MutableFieldInfoObjectMother.Create (declaringType: declaringType);

      var expression = Expression.Field (instance, field);

      Assert.That (expression.Member, Is.SameAs (field));
    }

    [Test]
    public void Field_Write_Static ()
    {
      var fieldType = MutableTypeObjectMother.Create();
      var field = MutableFieldInfoObjectMother.Create (attributes: FieldAttributes.Static, fieldType: fieldType);
      var value = Expression.Variable (fieldType);

      var fieldExpression = Expression.Field (null, field);
      var expression = Expression.Assign (fieldExpression, value);

      Assert.That (fieldExpression.Member, Is.SameAs (field));
      Assert.That (expression.Left, Is.SameAs (fieldExpression));
    }

    [Test]
    public void Field_Write_Instance ()
    {
      var declaringType = MutableTypeObjectMother.Create();
      var instance = Expression.Variable (declaringType);
      var fieldType = MutableTypeObjectMother.Create();
      var field = MutableFieldInfoObjectMother.Create (declaringType: declaringType, fieldType: fieldType);
      var value = Expression.Variable (fieldType);

      var fieldExpression = Expression.Field (instance, field);
      var expression = Expression.Assign (fieldExpression, value);

      Assert.That (fieldExpression.Member, Is.SameAs (field));
      Assert.That (expression.Left, Is.SameAs (fieldExpression));
    }
  }
}