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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Holds all information needed to declara a custom attribute.
  /// </summary>
  public class CustomAttributeDeclaration : ICustomAttributeData
  {
    private readonly ConstructorInfo _constructor;
    private readonly ReadOnlyCollection<object> _constructorArguments;
    private readonly ReadOnlyCollection<ICustomAttributeNamedArgument> _namedArguments;

    public CustomAttributeDeclaration (
        ConstructorInfo constructor,
        object[] constructorArguments,
        params NamedAttributeArgumentDeclaration[] namedArguments)
    {
      ArgumentUtility.CheckNotNull ("constructor", constructor);
      ArgumentUtility.CheckNotNull ("constructorArguments", constructorArguments);
      ArgumentUtility.CheckNotNull ("namedArguments", namedArguments);

      CheckConstructor (constructor);
      CheckConstructorArguments(constructor, constructorArguments);
      CheckDeclaringTypes (constructor, namedArguments);

      _constructor = constructor;
      _constructorArguments = constructorArguments.ToList().AsReadOnly();
      _namedArguments = namedArguments.Cast<ICustomAttributeNamedArgument>().ToList().AsReadOnly();
    }

    public ConstructorInfo Constructor
    {
      get { return _constructor; }
    }

    public ReadOnlyCollection<object> ConstructorArguments
    {
      get { return _constructorArguments; }
    }

    public IEnumerable<ICustomAttributeNamedArgument> NamedArguments
    {
      get { return _namedArguments; }
    }

    private void CheckConstructor (ConstructorInfo constructor)
    {
      if (!typeof (Attribute).IsAssignableFrom (constructor.DeclaringType))
      {
        var message = string.Format ("Type '{0}' does not derive from '{1}'.", constructor.DeclaringType.FullName, typeof(Attribute).FullName);
        throw new ArgumentException (message, "constructor");
      }

      if (!constructor.IsPublic)
      {
        var message = string.Format ("The attribute constructor '{0}' is not a public instance constructor.", constructor);
        throw new ArgumentException (message, "constructor");
      }

      if (!constructor.DeclaringType.IsVisible)
      {
        var message = string.Format ("The attribute type '{0}' is not publicly visible.", constructor.DeclaringType.FullName);
        throw new ArgumentException (message, "constructor");
      }
    }

    private void CheckConstructorArguments (ConstructorInfo constructor, object[] constructorArguments)
    {
      var parameters = constructor.GetParameters ();
      if (parameters.Length != constructorArguments.Length)
      {
        var message = string.Format ("Expected {0} constructor argument(s), but was {1}.", parameters.Length, constructorArguments.Length);
        throw new ArgumentException (message, "constructorArguments");
      }

      for (int i = 0; i < parameters.Length; i++)
      {
        var parameterType = parameters[i].ParameterType;
        var argument = constructorArguments[i];

        if (argument == null)
        {
          if (!NullableTypeUtility.IsNullableType (parameterType))
          {
            var message = string.Format ("Constructor parameter at {0} of type '{1}' cannot be null.", i, parameterType);
            throw new ArgumentItemNullException ("constructorArguments", message);
          }
        }
        else if (!parameterType.IsInstanceOfType (argument))
        {
          throw new ArgumentItemTypeException ("constructorArguments", i, parameterType, argument.GetType ());
        }
      }
    }

    private void CheckDeclaringTypes (ConstructorInfo constructor, NamedAttributeArgumentDeclaration[] namedArguments)
    {
      var attributeType = constructor.DeclaringType;
      foreach (var namedArgument in namedArguments)
      {
        var memberDeclaringType = namedArgument.MemberInfo.DeclaringType;
        if (!memberDeclaringType.IsAssignableFrom (attributeType))
        {
          var message = string.Format (
            "Named argument '{0}' cannot be used with custom attribute type '{1}'.", namedArgument.MemberInfo.Name, attributeType);
          throw new ArgumentException (message, "namedArguments");
        }
      }
    }
  }
}