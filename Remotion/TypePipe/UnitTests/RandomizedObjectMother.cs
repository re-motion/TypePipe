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
using Remotion.Utilities;

namespace Remotion.TypePipe.UnitTests
{
  /// <summary>
  /// Provides randomized values for unit tests.
  /// </summary>
  public static class RandomizedObjectMother
  {
    private static readonly Random s_random = new Random ();

    /// <summary>
    /// Gets a random object from the specified candidate array. This is used by unit tests when they need code to work with similar but unequal
    /// values. Rather than replicating the test for every possible value, the test is written once and is executed with one of the candidate objects
    /// chosen at random.
    /// </summary>
    /// <typeparam name="T">The type of the candidate objects.</typeparam>
    /// <param name="candidates">The candidate objects.</param>
    /// <returns>A random candidate object.</returns>
    public static T OneOf<T> (params T[] candidates)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("candidates", candidates);

      var index = s_random.Next (candidates.Length);
      return candidates[index];
    }
  }
}