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
using Remotion.TypePipe.FutureInfos;

namespace Remotion.TypePipe.UnitTests.FutureInfos.Integration
{
  [TestFixture]
  public class FutureTypeExpressionTreeIntegration
  {
    private FutureType _futureType;

    [SetUp]
    public void SetUp ()
    {
      _futureType = new FutureType();
    }

    [Test]
    public void NewExpression ()
    {
      // Act
      TestDelegate action = () => Expression.New (_futureType);

      // Assert
      Assert.That (action, Throws.Nothing);
    }

    [Test]
    public void ParameterExpression ()
    {
      // Act
      TestDelegate action = () => Expression.Parameter (_futureType);

      // Assert
      Assert.That (action, Throws.Nothing);
    }

    [Test]
    public void VariableExpression ()
    {
      // Act
      TestDelegate action = () => Expression.Parameter (_futureType);

      // Assert
      Assert.That (action, Throws.Nothing);
    }

    [Test]
    public void ConvertExpression ()
    {
      // Arrange
      var expression = Expression.Constant (new object());

      // Act
      TestDelegate action = () => Expression.Convert (expression, _futureType);

      // Assert
      Assert.That (action, Throws.Nothing);
    }

    [Test]
    public void TypeAs ()
    {
      // Arrange
      var expression = Expression.Constant (new object());

      // Act
      TestDelegate action = () => Expression.TypeAs (expression, _futureType);

      // Assert
      Assert.That (action, Throws.Nothing);
    }

    [Test]
    public void TypeIs ()
    {
      // Arrange
      var expression = Expression.Constant (new object());

      // Act
      TestDelegate action = () => Expression.TypeIs (expression, _futureType);

      // Assert
      Assert.That (action, Throws.Nothing);
    }
  }
}