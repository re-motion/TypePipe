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
using Remotion.TypePipe.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration
{
  /// <summary>
  /// Creates delegates for constructing instances of assembled types.
  /// </summary>
  /// <threadsafety static="true" instance="true"/>
  public class ConstructorDelegateFactory : IConstructorDelegateFactory
  {
    private readonly IConstructorFinder _constructorFinder;

    public ConstructorDelegateFactory (IConstructorFinder constructorFinder)
    {
      ArgumentUtility.CheckNotNull ("constructorFinder", constructorFinder);
      
      _constructorFinder = constructorFinder;
    }

    public Delegate CreateConstructorCall (Type requestedType, Type assembledType, Type delegateType, bool allowNonPublic)
    {
      ArgumentUtility.CheckNotNull ("requestedType", requestedType);
      ArgumentUtility.CheckNotNull ("assembledType", assembledType);
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("delegateType", delegateType, typeof (Delegate));

      var ctorSignature = GetSignature (delegateType);
      var parameterTypes = ctorSignature.Item1;
      var returnType = ctorSignature.Item2;

      var constructor = _constructorFinder.GetConstructor (requestedType, parameterTypes, allowNonPublic, assembledType);

      var parameters = constructor.GetParameters().Select (p => Expression.Parameter (p.ParameterType, p.Name)).ToArray();
      var constructorCall = Expression.New (constructor, parameters.Cast<Expression>());
      var boxedConstructorCall = Expression.Convert (constructorCall, returnType);
      var lambda = Expression.Lambda (delegateType, boxedConstructorCall, parameters);

      return lambda.Compile();
    }

    private Tuple<Type[], Type> GetSignature (Type delegateType)
    {
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("delegateType", delegateType, typeof (Delegate));

      var invokeMethod = delegateType.GetMethod ("Invoke");
      Assertion.IsNotNull (invokeMethod, "Delegate has no Invoke() method.");

      var parameterTypes = invokeMethod.GetParameters().Select (p => p.ParameterType).ToArray();
      var returnType = invokeMethod.ReturnType;

      return Tuple.Create (parameterTypes, returnType);
    }
  }
}