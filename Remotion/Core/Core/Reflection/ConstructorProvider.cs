// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Text;
using Remotion.Utilities;

namespace Remotion.Reflection
{
  /// <summary>
  /// Retrieves constructors and creates uses <see cref="LambdaExpression"/> to create delegates enabling their efficient invocation.
  /// </summary>
  public class ConstructorProvider : IConstructorProvider
  {
    public ConstructorInfo GetConstructor (
        Type generatedType, Type[] generatedParamterTypes, bool allowNonPublic, Type originalType, Type[] originalParameterTypes)
    {
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
      var constructor = generatedType.GetConstructor (bindingFlags, null, generatedParamterTypes, null);

      if (constructor == null)
      {
        var message = string.Format (
            "Type {0} does not contain a constructor with the following signature: ({1}).",
            originalType.FullName,
            SeparatedStringBuilder.Build (", ", originalParameterTypes, pt => pt.Name));
        throw new MissingMethodException (message);
      }

      if (!constructor.IsPublic && !allowNonPublic)
      {
        var message = string.Format (
            "Type {0} contains a constructor with the required signature, but it is not public (and the allowNonPublic flag is not set).",
            originalType.FullName);
        throw new MissingMethodException (message);
      }

      return constructor;
    }

    public Delegate CreateConstructorCall (ConstructorInfo constructor, Type delegateType)
    {
      ArgumentUtility.CheckNotNull ("constructor", constructor);
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("delegateType", delegateType, typeof (Delegate));

      var parameters = constructor.GetParameters().Select (p => Expression.Parameter (p.ParameterType, p.Name)).ToArray();
      // ReSharper disable CoVariantArrayConversion
      var constructorCall = Expression.New (constructor, parameters);
      // ReSharper restore CoVariantArrayConversion
      var lambda = Expression.Lambda (delegateType, constructorCall, parameters);

      return lambda.Compile();
    }
  }
}