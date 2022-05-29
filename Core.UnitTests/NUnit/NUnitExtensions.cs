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
using NUnit.Framework.Constraints;
using Remotion.Utilities;

namespace Remotion.TypePipe.UnitTests.NUnit
{
  public static class NUnitExtensions
  {
    public static EqualConstraint ArgumentExceptionMessageEqualTo (this ConstraintExpression constraintExpression, string message, string paramName)
    {
      AssertThatMessageContainsWhitespaces (message: message);
      AssertThatParameterDoesNotContainWhitespaces (paramName: paramName);
      return constraintExpression.With.Message.EqualTo (new ArgumentException (message: message, paramName: paramName).Message);
    }

    public static EqualConstraint ArgumentOutOfRangeExceptionMessageEqualTo (this ConstraintExpression constraintExpression, string message, string paramName, int actualValue)
    {
      AssertThatMessageContainsWhitespaces (message: message);
      AssertThatParameterDoesNotContainWhitespaces (paramName: paramName);
      return constraintExpression.With.Message.EqualTo (new ArgumentOutOfRangeException (message: message, paramName: paramName, actualValue: actualValue).Message);
    }

    private static void AssertThatMessageContainsWhitespaces (string message)
    {
      Assertion.IsTrue (message.Contains (" "), "The exception message must contain at least one whitespace.\r\nmessage: {0}", message);
    }

    private static void AssertThatParameterDoesNotContainWhitespaces (string paramName)
    {
      Assertion.IsFalse (paramName.Contains (" "), "The parameter must not contain any whitespaces.\r\nparamName: ", paramName);
    }
  }
}