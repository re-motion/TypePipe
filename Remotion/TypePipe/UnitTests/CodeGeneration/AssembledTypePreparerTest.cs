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

using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.TypePipe.UnitTesting.Expressions;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.Expressions;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.Dlr.Ast;

namespace Remotion.TypePipe.UnitTests.CodeGeneration
{
  [TestFixture]
  public class AssembledTypePreparerTest
  {
    private AssembledTypePreparer _preparer;

    [SetUp]
    public void SetUp ()
    {
      _preparer = new AssembledTypePreparer();
    }

    [Test]
    public void AddTypeID ()
    {
      var proxyType = MutableTypeObjectMother.Create();
      var typeIDExpression = ExpressionTreeObjectMother.GetSomeExpression();

      _preparer.AddTypeID (proxyType, new[] { typeIDExpression });

      Assert.That (proxyType.AddedFields, Has.Count.EqualTo (1));
      var typeIDField = proxyType.AddedFields.Single();
      Assert.That (typeIDField.Name, Is.EqualTo ("__typeID"));
      Assert.That (typeIDField.Attributes, Is.EqualTo (FieldAttributes.Private | FieldAttributes.Static));
      Assert.That (typeIDField.FieldType, Is.SameAs (typeof (object[])));

      Assert.That (proxyType.MutableTypeInitializer, Is.Not.Null);
      var expectedTypeInitialization = Expression.Block (
          typeof (void),
          Expression.Assign (
              Expression.Field (null, typeIDField),
              Expression.NewArrayInit (typeof (object), typeIDExpression)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedTypeInitialization, proxyType.MutableTypeInitializer.Body);
    }

    [Test]
    public void ExtractTypeID ()
    {
      var result = _preparer.ExtractTypeID (typeof (AssembledType));

      Assert.That (result, Is.EqualTo (new object[] { 1, "2" }));
    }

    private class AssembledType
    {
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
      private static object[] __typeID = new object[] { 1, "2" };
// ReSharper restore UnusedMember.Local
// ReSharper restore InconsistentNaming
    }
  }
}