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
using System.Collections.Generic;
using System.Linq;
using Remotion.Development.UnitTesting;
using Remotion.FunctionalProgramming;
using Rhino.Mocks.Constraints;

// ReSharper disable once CheckNamespace
namespace Remotion.Development.RhinoMocks.UnitTesting
{
  /// <summary>
  /// Extensions methods for the <see cref="ListArg{T}"/> class.
  /// </summary>
  static partial class ListArgExtensions
  {
    /// <summary>
    /// Similiar to <see cref="ListArg{T}.Equal"/> but without considering the order of the elements in the collection.
    /// </summary>
    public static T Equivalent<T> (this ListArg<T> arg, IEnumerable collection) where T : IEnumerable
    {
      var items = collection.Cast<object>().ToArray();
      var type = typeof (ListArg<>).Assembly.GetType ("Rhino.Mocks.ArgManager", true);
      var message = "equivalent to collection [" + string.Join (", ", (IEnumerable<object>) items) + "]";
      var constraint = new PredicateConstraintWithMessage<T> (c => c.Cast<object>().SetEquals (items), message);
      PrivateInvoke.InvokeNonPublicStaticMethod (type, "AddInArgument", constraint);
      return default (T);
    }

    /// <summary>
    /// Similiar to <see cref="ListArg{T}.Equal"/> but without considering the order of the elements in the collection.
    /// </summary>
    public static T Equivalent<T> (this ListArg<T> arg, params object[] items) where T : IEnumerable
    {
      return Equivalent (arg, (IEnumerable) items);
    }

    /// <summary>
    /// Similiar to <see cref="ListArg{T}.Equal"/> but without considering the order of the elements in the collection.
    /// </summary>
    public static T[] Equivalent<T> (this ListArg<IEnumerable<T>> arg, params T[] items)
    {
      var argManagerType = typeof (ListArg<>).Assembly.GetType ("Rhino.Mocks.ArgManager", true);
      var message = "equivalent to collection [" + string.Join (", ", items) + "]";
      var constraint = new PredicateConstraintWithMessage<IEnumerable<T>> (c => c.SetEquals (items), message);
      PrivateInvoke.InvokeNonPublicStaticMethod (argManagerType, "AddInArgument", constraint);

      return new T[0];
    }

    private class PredicateConstraintWithMessage<T> : PredicateConstraint<T>
    {
      private readonly string _message;

      public PredicateConstraintWithMessage (Predicate<T> predicate, string message)
          : base (predicate)
      {
        _message = message;
      }

      public override string Message
      {
        get { return _message; }
      }
    }
  }
}