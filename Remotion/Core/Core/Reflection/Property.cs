// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Reflection
{
  /// <summary>
  /// Wrapper around the property of a class, which allows the property to exist indepently of any
  /// concrete instance of the class.
  /// </summary>
  /// <remarks>For an application example see <see cref="Property{TClass,TProperty}"/>.</remarks>
  /// <typeparam name="TClass">The class for which we want to create the <see cref="Properties{T}"/> object.</typeparam>
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
      ArgumentUtility.CheckNotNull ("propertyLambda", propertyLambda);
      //_propertyInfo = (PropertyInfo) ((MemberExpression) propertyLambda.Body).Member;

      var memberExpression = propertyLambda.Body as MemberExpression;
      if (memberExpression == null)
      {
        throw new ArgumentException ("The body of the passed expression is not a MemberExpression, the passed expression does therefore not represent a property.");
      }

      var propertyInfo = memberExpression.Member as PropertyInfo;
      if (propertyInfo == null)
      {
        throw new ArgumentException ("The passed expression does not represent a property.");
      }

      _propertyInfo = propertyInfo;
    }

    public PropertyInfo PropertyInfo
    {
      get { return _propertyInfo; }
    }

    public TProperty Get (TClass obj)
    {
      ArgumentUtility.CheckNotNull ("obj", obj);
      return (TProperty) _propertyInfo.GetValue (obj, null);
    }

    public void Set (TClass obj, TProperty value)
    {
      ArgumentUtility.CheckNotNull ("obj", obj);
      _propertyInfo.SetValue (obj, value, null);
    }
  }
}
