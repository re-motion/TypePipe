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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.Caching
{
  /// <summary>
  /// Retrieves the generated type or its constructors for the requested type from the cache or delegates to the contained
  /// <see cref="ITypeAssembler"/> instance.
  /// </summary>
  public class TypeCache : ITypeCache
  {
    private readonly ConcurrentDictionary<AssembledTypeID, Lazy<Type>> _types = new ConcurrentDictionary<AssembledTypeID, Lazy<Type>>();
    private readonly ConcurrentDictionary<ConstructionKey, Delegate> _constructorCalls = new ConcurrentDictionary<ConstructionKey, Delegate>();

    private readonly ITypeAssembler _typeAssembler;
    private readonly IConstructorDelegateFactory _constructorDelegateFactory;
    private readonly IAssemblyContextPool _assemblyContextPool;

    private readonly Func<AssembledTypeID, Lazy<Type>> _createTypeFunc;
    private readonly Func<ConstructionKey, Delegate> _createConstructorCallFunc;

    public TypeCache (
        ITypeAssembler typeAssembler,
        IConstructorDelegateFactory constructorDelegateFactory,
        IAssemblyContextPool assemblyContextPool)
    {
      ArgumentUtility.CheckNotNull ("typeAssembler", typeAssembler);
      ArgumentUtility.CheckNotNull ("constructorDelegateFactory", constructorDelegateFactory);
      ArgumentUtility.CheckNotNull ("assemblyContextPool", assemblyContextPool);

      _typeAssembler = typeAssembler;
      _constructorDelegateFactory = constructorDelegateFactory;
      _assemblyContextPool = assemblyContextPool;

      _createTypeFunc = CreateType;
      _createConstructorCallFunc = CreateConstructorCall;
    }

    public string ParticipantConfigurationID
    {
      get { return _typeAssembler.ParticipantConfigurationID; }
    }

    public ReadOnlyCollection<IParticipant> Participants
    {
      get { return _typeAssembler.Participants; }
    }

    public Type GetOrCreateType (Type requestedType)
    {
      // Using Assertion.DebugAssert because it will be compiled away.
      Assertion.DebugAssert(requestedType != null);

      var typeID = _typeAssembler.ComputeTypeID (requestedType);

      return GetOrCreateType (typeID);
    }

    public Type GetOrCreateType (AssembledTypeID typeID)
    {
      var lazyType = _types.GetOrAdd (typeID, _createTypeFunc);

      try
      {
        return lazyType.Value;
      }
      catch
      {
        //TODO 5840: Either the exception is caught in the Lazy object or when removing the Lazy object, there could be a race-condition?
        //Lazy<Type> value;
        //_types.TryRemove (typeID, out value);
        throw;
      }
    }

    private Lazy<Type> CreateType (AssembledTypeID typeID)
    {
      return new Lazy<Type> (
          () =>
          {
            //TODO 5840: Add timeout to Dequeue, log warning, return to Dequeuing without timeout.
            var assemblyContext = _assemblyContextPool.Dequeue();
            try
            {
              return _typeAssembler.AssembleType (typeID, assemblyContext.ParticipantState, assemblyContext.MutableTypeBatchCodeGenerator);
            }
            finally
            {
              _assemblyContextPool.Enqueue (assemblyContext);
            }
          },
          LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public Delegate GetOrCreateConstructorCall (Type requestedType, Type delegateType, bool allowNonPublic)
    {
      // Using Assertion.DebugAssert because it will be compiled away.
      Assertion.DebugAssert (requestedType != null);
      Assertion.DebugAssert (delegateType != null && typeof(Delegate).IsAssignableFrom(delegateType));

      var typeID = _typeAssembler.ComputeTypeID (requestedType);

      return GetOrCreateConstructorCall (typeID, delegateType, allowNonPublic);
    }

    public Delegate GetOrCreateConstructorCall (AssembledTypeID typeID, Type delegateType, bool allowNonPublic)
    {
      // Using Assertion.DebugAssert because it will be compiled away.
      Assertion.DebugAssert (delegateType != null && typeof(Delegate).IsAssignableFrom(delegateType));

      var constructionKey = new ConstructionKey (typeID, delegateType, allowNonPublic);
      return _constructorCalls.GetOrAdd (constructionKey, _createConstructorCallFunc);
    }

    private Delegate CreateConstructorCall (ConstructionKey key)
    {
      var assembledType = GetOrCreateType (key.TypeID);
      return _constructorDelegateFactory.CreateConstructorCall (key.TypeID.RequestedType, assembledType, key.DelegateType, key.AllowNonPublic);
    }

    public Type GetOrCreateAdditionalType (object additionalTypeID)
    {
      ArgumentUtility.CheckNotNull ("additionalTypeID", additionalTypeID);

      var assemblyContext = _assemblyContextPool.Dequeue();
      try
      {
        return _typeAssembler.GetOrAssembleAdditionalType (
            additionalTypeID,
            assemblyContext.ParticipantState,
            assemblyContext.MutableTypeBatchCodeGenerator);
      }
      finally
      {
        _assemblyContextPool.Enqueue (assemblyContext);
      }
    }

    public void LoadTypes (IEnumerable<Type> generatedTypes)
    {
      ArgumentUtility.CheckNotNull ("generatedTypes", generatedTypes);

      var assembledTypes = new List<Type>();
      var additionalTypes = new List<Type>();

      foreach (var type in generatedTypes)
      {
        if (_typeAssembler.IsAssembledType (type))
          assembledTypes.Add (type);
        else
          additionalTypes.Add (type);
      }

      // ReSharper disable LoopCanBeConvertedToQuery
      var loadedAssembledTypes = new List<Type>();
      foreach (var assembledType in assembledTypes)
      {
        var typeID = _typeAssembler.ExtractTypeID (assembledType);
        Type assembledTypeForClosure = assembledType;
        if (_types.TryAdd (typeID, new Lazy<Type> (() => assembledTypeForClosure, LazyThreadSafetyMode.None)))
          loadedAssembledTypes.Add (assembledType);
      }
      // ReSharper restore LoopCanBeConvertedToQuery

      //TODO 5840: Reenable or completly remove RebuildParticipantState
      //var assemblyContexts = _assemblyContextPool.DequeueAll();
      //try
      //{
      //  _typeAssembler.RebuildParticipantState (loadedAssembledTypes, additionalTypes, assemblyContext.ParticipantState);
      //}
      //finally
      //{
      //  foreach (var assemblyContext in assemblyContexts)
      //    _assemblyContextPool.Enqueue (assemblyContext);
      //}
    }
  }
}