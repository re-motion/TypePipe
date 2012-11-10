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
using Remotion.Utilities;

namespace Remotion.TypePipe
{
  /// <summary>
  /// Uses <see cref="LambdaExpression"/> to create delegates enabling the efficient invocation of constructors.
  /// </summary>
  public class DelegateFactory : IDelegateFactory
  {
    public Delegate CreateConstructorCall (Type declaringType, Type[] parameterTypes, bool allowNonPublic, Type delegateType)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("parameterTypes", parameterTypes);
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("delegateType", delegateType, typeof (Delegate));

      var constructor = GetConstructor (declaringType, parameterTypes, allowNonPublic);
      var parameters = constructor.GetParameters().Select (p => Expression.Parameter (p.ParameterType, p.Name)).ToArray();
      // ReSharper disable CoVariantArrayConversion
      var constructorCall = Expression.New (constructor, parameters);
      // ReSharper restore CoVariantArrayConversion
      var lambda = Expression.Lambda (delegateType, constructorCall, parameters);

      return lambda.Compile();
    }

    private ConstructorInfo GetConstructor (Type declaringType, Type[] parameterTypes, bool allowNonPublic)
    {
      var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
      if (allowNonPublic)
        bindingFlags |= BindingFlags.NonPublic;

      return declaringType.GetConstructor (bindingFlags, null, parameterTypes, null);

      // TODO 5172: Constructor not found!
    }
  }
}