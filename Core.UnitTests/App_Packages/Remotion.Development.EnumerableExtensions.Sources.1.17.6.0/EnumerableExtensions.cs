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
using System.Linq;
using Remotion.Utilities;

// ReSharper disable once CheckNamespace
namespace Remotion.Development.UnitTesting.Enumerables
{
  /// <summary>
  /// Provides extensions methods for <see cref="IEnumerable{T}"/>.
  /// </summary>
  public static partial class EnumerableExtensions
  {
    /// <summary>
    /// Wraps an <see cref="IEnumerable{T}"/> to ensure that it is iterated only once.
    /// </summary>
    /// <typeparam name="T">The element type of the <see cref="IEnumerable{T}"/>.</typeparam>
    /// <param name="source">The source <see cref="IEnumerable{T}"/> to be wrapped.</param>
    /// <returns>An instance of <see cref="OneTimeEnumerable{T}"/> decorating the <paramref name="source"/>.</returns>
    public static OneTimeEnumerable<T> AsOneTime<T> (this IEnumerable<T> source)
    {
      ArgumentUtility.CheckNotNull ("source", source);

      return new OneTimeEnumerable<T> (source);
    }

    /// <summary>
    /// Forces the enumeration of the <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The element type of the <see cref="IEnumerable{T}"/>.</typeparam>
    /// <param name="source">The source <see cref="IEnumerable{T}"/>.</param>
    /// <returns>An array containing all values computed by <paramref name="source"/>.</returns>
    public static T[] ForceEnumeration<T> (this IEnumerable<T> source)
    {
      ArgumentUtility.CheckNotNull ("source", source);

      return source.ToArray();
    }
  }
}