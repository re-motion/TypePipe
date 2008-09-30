using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Remotion.SecurityManager.UnitTests.AclTools.Expansion
{
  /// <summary>
  /// Wrapper around the property of a class, which allows the property to exist indepently of any
  /// concrete instance of the class.
  /// </summary>
  /// <remarks>For an usage example see <see cref="Properties{T}"/>.</remarks>
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