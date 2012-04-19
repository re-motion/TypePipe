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
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  /// <summary>
  /// Provides typed access to the reflection objects for members referenced in <see cref="Expression"/> instances.
  /// </summary>
  public static class MemberInfoFromExpressionUtility
  {
    public static ConstructorInfo GetConstructor<T> (Expression<Func<T>> newExpression)
    {
      Assertion.IsTrue (newExpression.Body is NewExpression, "Parameter newExpression must be a NewExpression.");
      var constructor = ((NewExpression) newExpression.Body).Constructor;
      return constructor;
    }

    public static PropertyInfo GetProperty<T> (Expression<Func<T>> memberExpression)
    {
      Assertion.IsTrue (memberExpression.Body is MemberExpression, "Parameter memberExpression must be a MemberExpression.");
      var member = ((MemberExpression) memberExpression.Body).Member;
      Assertion.IsTrue (member is PropertyInfo, "Parameter memberExpression must hold a property access expression.");
      return (PropertyInfo) member;
    }

    public static PropertyInfo GetProperty<TSourceObject, TMemberType> (Expression<Func<TSourceObject, TMemberType>> memberExpression)
    {
      Assertion.IsTrue (memberExpression.Body is MemberExpression, "Parameter memberExpression must be a MemberExpression.");
      var member = ((MemberExpression) memberExpression.Body).Member;
      Assertion.IsTrue (member is PropertyInfo, "Parameter memberExpression must hold a property access expression.");
      return (PropertyInfo) member;
    }

    public static FieldInfo GetField<T> (Expression<Func<T>> memberExpression)
    {
      Assertion.IsTrue (memberExpression.Body is MemberExpression, "Parameter memberExpression must be a MemberExpression.");
      var member = ((MemberExpression) memberExpression.Body).Member;
      Assertion.IsTrue (member is FieldInfo, "Parameter memberExpression must hold a field access expression.");
      return (FieldInfo) member;
    }

    public static FieldInfo GetField<TSourceObject, TMemberType> (Expression<Func<TSourceObject, TMemberType>> memberExpression)
    {
      Assertion.IsTrue (memberExpression.Body is MemberExpression, "Parameter memberExpression must be a MemberExpression.");
      var member = ((MemberExpression) memberExpression.Body).Member;
      Assertion.IsTrue (member is FieldInfo, "Parameter memberExpression must hold a field access expression.");
      return (FieldInfo) member;
    }

    public static MethodInfo GetMethod<T> (Expression<Func<T>> methodCallExpression)
    {
      Assertion.IsTrue (methodCallExpression.Body is MethodCallExpression, "Parameter methodCallExpression must be a MethodCallExpression.");
      return ((MethodCallExpression) methodCallExpression.Body).Method;
    }

    public static MethodInfo GetMethod<TSourceObject, TMemberType> (Expression<Func<TSourceObject, TMemberType>> methodCallExpression)
    {
      Assertion.IsTrue (methodCallExpression.Body is MethodCallExpression, "Parameter methodCallExpression must be a MethodCallExpression.");
      return ((MethodCallExpression) methodCallExpression.Body).Method;
    }

    public static MethodInfo GetMethod<T> (Expression<Action<T>> methodCallExpression)
    {
      Assertion.IsTrue (methodCallExpression.Body is MethodCallExpression, "Parameter methodCallExpression must be a MethodCallExpression.");
      return ((MethodCallExpression) methodCallExpression.Body).Method;
    }

    public static MethodInfo GetMethod<TSourceObject, TMemberType> (Expression<Action<TSourceObject, TMemberType>> methodCallExpression)
    {
      Assertion.IsTrue (methodCallExpression.Body is MethodCallExpression, "Parameter methodCallExpression must be a MethodCallExpression.");
      return ((MethodCallExpression) methodCallExpression.Body).Method;
    }
  }
}