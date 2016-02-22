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
namespace Remotion.Development.UnitTesting
{
  /// <summary>
  /// Provides a <see cref="Null"/> property that can be assigned arbitrary values, and a type <see cref="T"/> to be used as a dummy generic argument.
  /// </summary>
  public static partial class Dev
  {
    /// <summary>
    /// Defines a dummy type that can be used as a generic argument.
    /// </summary>
    public class T
    {
    }

    /// <summary>
    /// Use this in unit tests where you need to assign a value to
    /// something (e.g., for syntactic reasons, or to remove unused variable warnings), but don't care about the result of the assignment.
    /// </summary>
    public static object Null
    {
      get { return null; }
      // ReSharper disable ValueParameterNotUsed
      set { }
      // ReSharper restore ValueParameterNotUsed
    }
  }

  /// <summary>
  /// Provides a <see cref="Dummy"/> field that can be used as a ref or out parameter, and a typed <see cref="Null"/> property that can be assigned 
  /// arbitrary values and always returns the default value for <typeparamref name="T"/>.
  /// </summary>
  public static class Dev<T>
  {
    /// <summary>
    /// Use this in unit tests where you need a ref or out parameter but but don't care about the result of the assignment.
    /// Never rely on the value of the <see cref="Dummy"/> field, it will be changed by other tests.
    /// </summary>
    public static T Dummy;

    /// <summary>
    /// Use this in unit tests where you need to assign a value to
    /// something (e.g., for syntactic reasons, or to remove unused variable warnings), but don't care about the result of the assignment.
    /// </summary>
    public static T Null
    {
      get { return default (T); }
      set { Dev.Null = value; }
    }
  }
}
