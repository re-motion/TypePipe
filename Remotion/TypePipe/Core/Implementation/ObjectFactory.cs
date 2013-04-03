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
using Remotion.TypePipe.Caching;
using Remotion.Utilities;

namespace Remotion.TypePipe.Implementation
{
  /// <summary>
  /// Implements <see cref="IObjectFactory"/> to act as a main entry point into the pipeline for generating types and instantiating them.
  /// </summary>
  public class ObjectFactory : IObjectFactory
  {
    private readonly ITypeCache _typeCache;
    private readonly ICodeManager _codeManager;

    public ObjectFactory (ITypeCache typeCache, ICodeManager codeManager)
    {
      ArgumentUtility.CheckNotNull ("typeCache", typeCache);
      ArgumentUtility.CheckNotNull ("codeManager", codeManager);

      _typeCache = typeCache;
      _codeManager = codeManager;
    }

    public string ParticipantConfigurationID
    {
      get { return _typeCache.ParticipantConfigurationID; }
    }

    public ICodeManager CodeManager
    {
      get { return _codeManager; }
    }

    public T CreateObject<T> (ParamList constructorArguments = null, bool allowNonPublicConstructor = false)
        where T : class
    {
      return (T) CreateObject (typeof (T), constructorArguments, allowNonPublicConstructor);
    }

    public object CreateObject (Type requestedType, ParamList constructorArguments = null, bool allowNonPublicConstructor = false)
    {
      ArgumentUtility.CheckNotNull ("requestedType", requestedType);

      constructorArguments = constructorArguments ?? ParamList.Empty;
      var constructorCall = _typeCache.GetOrCreateConstructorCall (requestedType, constructorArguments.FuncType, allowNonPublicConstructor);
      var instance = constructorArguments.InvokeFunc (constructorCall);

      return instance;
    }

    public Type GetAssembledType (Type requestedType)
    {
      ArgumentUtility.CheckNotNull ("requestedType", requestedType);

      return _typeCache.GetOrCreateType (requestedType);
    }

    public void PrepareExternalUninitializedObject (object instance)
    {
      ArgumentUtility.CheckNotNull ("instance", instance);

      var initializableInstance = instance as IInitializableObject;
      if (initializableInstance != null)
        initializableInstance.Initialize();
    }
  }
}