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

// ReSharper disable once CheckNamespace
namespace Remotion.Development.UnitTesting.ObjectMothers
{
  /// <summary>
  /// Provides boolean values for unit tests.
  /// </summary>
  static partial class BooleanObjectMother
  {
    private static readonly Random s_random = new Random ();

    /// <summary>
    /// Gets a random <see cref="bool"/> value. This is used by unit tests when they need code to work with arbitrary boolean values. Rather than
    /// duplicating the test, once for <see langword="true" /> and once for <see langword="false" />, the test is written once and is executed 
    /// with both <see langword="true" /> and <see langword="false" /> values chosen at random.
    /// </summary>
    /// <returns>A random <see cref="bool"/> value.</returns>
    public static bool GetRandomBoolean ()
    {
      return s_random.Next (2) == 1;
    }
  }
}