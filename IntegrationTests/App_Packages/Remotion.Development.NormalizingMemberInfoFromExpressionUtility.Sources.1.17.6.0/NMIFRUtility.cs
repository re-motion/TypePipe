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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.FunctionalProgramming;
using Remotion.Utilities;

// ReSharper disable once CheckNamespace
namespace Remotion.Development.UnitTesting.Reflection
{
  /// <summary>
  /// Provides typed access to the reflection objects for members referenced in <see cref="Expression"/> instances.
  /// Note that the returned <see cref="MemberInfo"/>s represents exactly the member specified by the user.
  /// That means that the <see cref="MemberInfo.ReflectedType"/> equals the generic parameter <c>TSourceObject</c>.
  /// See also <see cref="MemberInfoFromExpressionUtility"/>.
  /// </summary>
  /// <remarks>
  /// This class has no support for <i>normalizing</i> methods and properties defined in interfaces and explicit interface implementations.
  /// </remarks>
  static partial class NormalizingMemberInfoFromExpressionUtility
  {
    private const BindingFlags AllBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    private static readonly MemberInfoEqualityComparer<MethodInfo> s_methodComparer = MemberInfoEqualityComparer<MethodInfo>.Instance;

    public static MemberInfo GetMember (Expression<Action> expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return GetMemberInfoFromExpression (null, expression.Body);
    }

    public static MemberInfo GetMember<TMemberType> (Expression<Func<TMemberType>> expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return GetMemberInfoFromExpression (null, expression.Body);
    }

    public static MemberInfo GetMember<TSourceObject> (Expression<Action<TSourceObject>> expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return GetMemberInfoFromExpression (typeof (TSourceObject), expression.Body);
    }

    public static MemberInfo GetMember<TSourceObject, TMemberType> (Expression<Func<TSourceObject, TMemberType>> expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return GetMemberInfoFromExpression (typeof (TSourceObject), expression.Body);
    }

    public static FieldInfo GetField<TFieldType> (Expression<Func<TFieldType>> expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return GetFieldInfoFromMemberExpression (expression.Body);
    }

    public static FieldInfo GetField<TSourceObject, TFieldType> (Expression<Func<TSourceObject, TFieldType>> expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return GetFieldInfoFromMemberExpression (expression.Body);
    }

    public static ConstructorInfo GetConstructor<TType> (Expression<Func<TType>> expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return GetConstructorInfoFromNewExpression (expression.Body);
    }

    public static MethodInfo GetMethod (Expression<Action> expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return GetMethodInfoFromMethodCallExpression (null, expression.Body);
    }

    public static MethodInfo GetMethod<TReturnType> (Expression<Func<TReturnType>> expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return GetMethodInfoFromMethodCallExpression (null, expression.Body);
    }

    public static MethodInfo GetMethod<TSourceObject> (Expression<Action<TSourceObject>> expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return GetMethodInfoFromMethodCallExpression (typeof (TSourceObject), expression.Body);
    }

    public static MethodInfo GetMethod<TSourceObject, TReturnType> (Expression<Func<TSourceObject, TReturnType>> expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return GetMethodInfoFromMethodCallExpression (typeof (TSourceObject), expression.Body);
    }

    public static MethodInfo GetGenericMethodDefinition (Expression<Action> expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return GetGenericMethodDefinition (null, expression.Body);
    }

    public static MethodInfo GetGenericMethodDefinition<TReturnType> (Expression<Func<TReturnType>> expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return GetGenericMethodDefinition (null, expression.Body);
    }

    public static MethodInfo GetGenericMethodDefinition<TSourceObject> (Expression<Action<TSourceObject>> expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return GetGenericMethodDefinition (typeof (TSourceObject), expression.Body);
    }

    public static MethodInfo GetGenericMethodDefinition<TSourceObject, TReturnType> (Expression<Func<TSourceObject, TReturnType>> expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return GetGenericMethodDefinition (typeof (TSourceObject), expression.Body);
    }

    public static PropertyInfo GetProperty<TPropertyType> (Expression<Func<TPropertyType>> expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return GetPropertyInfoFromMemberExpression (null, expression.Body);
    }

    public static PropertyInfo GetProperty<TSourceObject, TPropertyType> (Expression<Func<TSourceObject, TPropertyType>> expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return GetPropertyInfoFromMemberExpression (typeof (TSourceObject), expression.Body);
    }

    private static MemberInfo GetMemberInfoFromExpression (Type sourceObjectType, Expression expression)
    {
      if (expression is MemberExpression)
        if (((MemberExpression) expression).Member is PropertyInfo)
          return GetPropertyInfoFromMemberExpression (sourceObjectType, expression);
        else
          return GetFieldInfoFromMemberExpression (expression);

      if (expression is MethodCallExpression)
        return GetMethodInfoFromMethodCallExpression (sourceObjectType, expression);
      if (expression is NewExpression)
        return GetConstructorInfoFromNewExpression (expression);

      throw new ArgumentException ("Must be a MemberExpression, MethodCallExpression or NewExpression.", "expression");
    }

    private static T GetTypedMemberInfoFromMemberExpression<T> (Expression expression, string memberType)
        where T: MemberInfo
    {
      var memberExpression = expression as MemberExpression;
      if (memberExpression == null)
        throw new ArgumentException ("Must be a MemberExpression.", "expression");

      var member = memberExpression.Member as T;
      if (member == null)
      {
        var message = string.Format ("Must hold a {0} access expression.", memberType);
        throw new ArgumentException (message, "expression");
      }

      return member;
    }

    private static FieldInfo GetFieldInfoFromMemberExpression (Expression expression)
    {
      return GetTypedMemberInfoFromMemberExpression<FieldInfo> (expression, "field");
    }

    private static PropertyInfo GetPropertyInfoFromMemberExpression (Type sourceObjectType, Expression expression)
    {
      // For redeclared properties (overridden in C#) the MemberExpression contains the root definition.
      var property = GetTypedMemberInfoFromMemberExpression<PropertyInfo> (expression, "property");

      if (sourceObjectType == null)
        return property;

      var baseTypeSequence = sourceObjectType.CreateSequence (t => t.BaseType);
      return baseTypeSequence.SelectMany (type => type.GetProperties (AllBindingFlags)).First (p => IsPropertyOverride (p, property));
    }

    private static bool IsPropertyOverride (PropertyInfo property, PropertyInfo baseDefinitionProperty)
    {
      var getter = property.GetGetMethod (true);
      var setter = property.GetSetMethod (true);
      var getterBaseDefinition = baseDefinitionProperty.GetGetMethod (true);
      var setterBaseDefinition = baseDefinitionProperty.GetSetMethod (true);

      return SafeIsMethodOverride (getter, getterBaseDefinition) || SafeIsMethodOverride (setter, setterBaseDefinition);
    }

    private static bool SafeIsMethodOverride (MethodInfo accessorOrNull, MethodInfo accessorBaseDefinitionOrNull)
    {
      return accessorOrNull != null && s_methodComparer.Equals (accessorOrNull.GetBaseDefinition(), accessorBaseDefinitionOrNull);
    }

    private static ConstructorInfo GetConstructorInfoFromNewExpression (Expression expression)
    {
      var newExpression = expression as NewExpression;
      if (newExpression == null)
        throw new ArgumentException ("Must be a NewExpression.", "expression");

      return newExpression.Constructor;
    }

    private static MethodInfo GetMethodInfoFromMethodCallExpression (Type sourceObjectType, Expression expression)
    {
      var methodCallExpression = expression as MethodCallExpression;
      if (methodCallExpression == null)
        throw new ArgumentException ("Must be a MethodCallExpression.", "expression");

      // For virtual methods the MethodCallExpression containts the root definition.
      var method = methodCallExpression.Method;

      if (sourceObjectType == null)
        return method;

      Type[] genericMethodArguments = null;
      if (method.IsGenericMethod && !method.IsGenericMethodDefinition)
      {
        genericMethodArguments = method.GetGenericArguments();
        method = method.GetGenericMethodDefinition();
      }

      var baseDefinition = method.GetBaseDefinition();
      var methodOnSourceType = sourceObjectType.GetMethods (AllBindingFlags)
          .Single (m => s_methodComparer.Equals (m.GetBaseDefinition(), baseDefinition));

      if (genericMethodArguments != null)
        return methodOnSourceType.MakeGenericMethod (genericMethodArguments);

      return methodOnSourceType;
    }

    private static MethodInfo GetGenericMethodDefinition (Type sourceObjectType, Expression expression)
    {
      var methodInfo = GetMethodInfoFromMethodCallExpression (sourceObjectType, expression);
      if (!methodInfo.IsGenericMethod)
        throw new ArgumentException ("Must hold a generic method access expression.", "expression");

      return methodInfo.GetGenericMethodDefinition();
    }
  }
}