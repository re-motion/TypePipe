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
using System.Collections;
using System.Linq;

namespace Remotion.TypePipe.Development.UnitTesting.Expressions
{
  internal static class Assert
  {
    public static void AreEqual (object expected, object actual, string message)
    {
      if (expected == actual)
        return;

      if (expected == null)
        throw new InvalidOperationException (message);

      if (expected.Equals (actual))
        return;

      var expectedEnumerable = expected as IEnumerable;
      var actualEnumerable = actual as IEnumerable;
      if (expectedEnumerable != null && actualEnumerable != null && expectedEnumerable.Cast<object>().SequenceEqual (actualEnumerable.Cast<object>()))
        return;

      throw new InvalidOperationException (message);
    }

    public static void IsInstanceOf<T> (object actual, string message)
    {
      if (!(actual is T))
        throw new InvalidOperationException (message);
    }
  }
}