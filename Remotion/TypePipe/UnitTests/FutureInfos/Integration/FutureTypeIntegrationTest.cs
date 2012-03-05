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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.TypePipe.FutureInfos;

namespace Remotion.TypePipe.UnitTests.FutureInfos.Integration
{
  [TestFixture]
  public class FutureTypeIntegration
  {
    [Test]
    public void NewExpression ()
    {
      // Arrange
      var futureType = new FutureType ();

      // Act
      TestDelegate action = () => Expression.New (futureType);

      // Assert
      Assert.That (action, Throws.Nothing);
    }

    [Test]
    public void ParameterAndVariableExpression ()
    {
      // Arrange
      var futureType = new FutureType ();

      // Act
      TestDelegate parameterAction = () => Expression.Parameter (futureType);
      TestDelegate variableAction = () => Expression.Parameter (futureType);

      // Assert
      Assert.That (parameterAction, Throws.Nothing);
      Assert.That (variableAction, Throws.Nothing);
    }

    [Test]
    public void ConvertExpression ()
    {
      // Arrange
      var futureType = new FutureType ();
      var input = Expression.Constant (new object());

      // Act
      TestDelegate action = () => Expression.Convert (input, futureType);

      // Assert
      Assert.That (action, Throws.Nothing);
    }
  }
}