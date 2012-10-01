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
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Holds the member and value for a named attribute argument declaration.
  /// </summary>
  public class NamedAttributeArgumentDeclaration : ICustomAttributeNamedArgument
  {
    private readonly MemberInfo _memberInfo;
    private readonly Type _memberType;
    private readonly object _value;

    public NamedAttributeArgumentDeclaration (PropertyInfo propertyInfo, object value)
    {
      ArgumentUtility.CheckNotNull ("propertyInfo", propertyInfo);
      ArgumentUtility.CheckType ("value", value, propertyInfo.PropertyType);

      var setMethod = propertyInfo.GetSetMethod ();
      if (setMethod == null)
      {
        var message = string.Format ("Property '{0}' has no public setter.", propertyInfo.Name);
        throw new ArgumentException (message, "propertyInfo");
      }

      if (setMethod.IsStatic)
      {
        var message = string.Format ("Property '{0}' is not an instance property.", propertyInfo.Name);
        throw new ArgumentException (message, "propertyInfo");
      }

      _memberInfo = propertyInfo;
      _memberType = propertyInfo.PropertyType;
      _value = value;
    }

    public NamedAttributeArgumentDeclaration (FieldInfo fieldInfo, object value)
    {
      ArgumentUtility.CheckNotNull ("fieldInfo", fieldInfo);
      ArgumentUtility.CheckType ("value", value, fieldInfo.FieldType);

      if (fieldInfo.IsLiteral || fieldInfo.IsInitOnly)
      {
        var message = string.Format ("Field '{0}' is not writable.", fieldInfo.Name);
        throw new ArgumentException (message, "fieldInfo");
      }

      if (!fieldInfo.IsPublic)
      {
        var message = string.Format ("Field '{0}' is not public.", fieldInfo.Name);
        throw new ArgumentException (message, "fieldInfo");
      }

      if (fieldInfo.IsStatic)
      {
        var message = string.Format ("Field '{0}' is not an instance field.", fieldInfo.Name);
        throw new ArgumentException (message, "fieldInfo");
      }

      _memberInfo = fieldInfo;
      _memberType = fieldInfo.FieldType;
      _value = value;
    }

    public MemberInfo MemberInfo
    {
      get { return _memberInfo; }
    }

    public Type MemberType
    {
      get { return _memberType; }
    }

    public object Value
    {
      get { return _value; }
    }
  }
}