/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Remotion.SecurityManager.UnitTests.AclTools.Expansion
{
  /// <summary>
  /// Wrapper around the property of a class, which allows the property to exist indepently of any
  /// concrete instance of the class.
  /// </summary>
  /// <remarks>For an application example see <see cref="Properties{T}"/>.</remarks>
  /// <typeparam name="TClass">The class for which we want to create the <see cref="Property{TClass,TProperty}"/> object.</typeparam>
  /// <typeparam name="TProperty">The return value of the property.</typeparam>
  /// <example>
  /// <code>
  /// <![CDATA[
  /// var userNameProperty = new Property<User, string> (x => x.UserName);
  /// var userName = userNameProperty.Get(user);
  /// ]]>
  /// </code>
  /// </example>
  public class Property<TClass, TProperty>
  {
    private readonly PropertyInfo _propertyInfo;

    public Property (Expression<Func<TClass, TProperty>> propertyLambda)
    {
      _propertyInfo = (PropertyInfo) ((MemberExpression) propertyLambda.Body).Member;
    }

    public TProperty Get (TClass obj)
    {
      return (TProperty) _propertyInfo.GetValue (obj, null);
    }

    public void Set (TClass obj, TProperty value)
    {
      _propertyInfo.SetValue (obj, value, null);
    }
  }
}