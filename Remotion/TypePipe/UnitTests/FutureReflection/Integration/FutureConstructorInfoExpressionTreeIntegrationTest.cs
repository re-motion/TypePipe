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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.TypePipe.FutureReflection;

namespace Remotion.TypePipe.UnitTests.FutureReflection.Integration
{
  [TestFixture]
  public class FutureConstructorInfoExpressionTreeIntegrationTest
  {
    [Test]
    public void Test ()
    {
      var arguments = new[] { "string", 3, new object() };
      var argumentExpressions = arguments.Select (Expression.Constant).Cast<Expression>();
      var parameters = arguments.Select (a => New.FutureParameterInfo (a.GetType())).ToArray();
      var constructor = new FutureConstructorInfo (typeof (object), parameters);

      var expression = Expression.New (constructor, argumentExpressions);

      Assert.That (expression.Constructor, Is.SameAs (constructor));
    }
  }
}