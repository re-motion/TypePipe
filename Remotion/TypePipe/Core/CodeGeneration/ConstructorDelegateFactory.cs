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
using Remotion.Reflection;
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
    private readonly IDelegateFactory _delegateFactory;

    public ConstructorDelegateFactory (IConstructorFinder constructorFinder, IDelegateFactory delegateFactory)
    {
      ArgumentUtility.CheckNotNull ("constructorFinder", constructorFinder);
      ArgumentUtility.CheckNotNull ("delegateFactory", delegateFactory);
      
      _constructorFinder = constructorFinder;
      _delegateFactory = delegateFactory;
    }

    public Delegate CreateConstructorCall (Type requestedType, Type assembledType, Type delegateType, bool allowNonPublic)
    {
      ArgumentUtility.CheckNotNull ("requestedType", requestedType);
      ArgumentUtility.CheckNotNull ("assembledType", assembledType);
      ArgumentUtility.CheckNotNull ("delegateType", delegateType);

      var ctorSignature = _delegateFactory.GetSignature (delegateType);
      var constructor = _constructorFinder.GetConstructor (requestedType, ctorSignature.Item1, allowNonPublic, assembledType);

      return _delegateFactory.CreateConstructorCall (constructor, delegateType);
    }
  }
}