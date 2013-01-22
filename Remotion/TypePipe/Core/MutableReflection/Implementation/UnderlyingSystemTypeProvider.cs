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
using System.Reflection;
using System.Reflection.Emit;
using Remotion.Collections;
using Remotion.TypePipe.Caching;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// Implements <see cref="IUnderlyingSystemTypeProvider"/> by creating new runtime types from <see cref="TypeBuilder"/> objects.
  /// </summary>
  /// <remarks>
  /// This class is used behind the <see cref="TypeCache"/>, therefore the incrementation of the <see cref="_counter"/> field does not need to be
  /// guarded.
  /// </remarks>
  public class UnderlyingSystemTypeProvider : IUnderlyingSystemTypeProvider
  {
    private readonly ICache<Tuple<string, Type, HashSet<Type>>, Type> _cache = CacheFactory.Create<Tuple<string, Type, HashSet<Type>>, Type>();

    private ModuleBuilder _moduleBuilder;
    private int _counter;

    public Type GetUnderlyingSystemType (ProxyType proxyType)
    {
      ArgumentUtility.CheckNotNull ("proxyType", proxyType);

      // tODO: think about: re-implemented interfaces, maybe skip them.
      var addedInterfaces = new HashSet<Type> (proxyType.AddedInterfaces);
      var key = Tuple.Create (proxyType.Name, proxyType.BaseType, addedInterfaces);

      return _cache.GetOrCreateValue (key, CreateUnderlyingSystemType);
    }

    private Type CreateUnderlyingSystemType (Tuple<string, Type, HashSet<Type>> key)
    {
      _moduleBuilder = _moduleBuilder ?? CreateModuleBuilder();

      _counter++;
      var name = string.Format ("{0}_UnderlyingSystemType_{1}", key.Item1, _counter);
      var attributes = TypeAttributes.Abstract;
      var baseType = key.Item2;
      var interfaces = key.Item3.ToArray();

      var typeBuilder = _moduleBuilder.DefineType (name, attributes, baseType, interfaces);
      ImplementInterfaces (typeBuilder, interfaces);

      return typeBuilder.CreateType();
    }

    private ModuleBuilder CreateModuleBuilder ()
    {
      var assemblyName = new AssemblyName ("UnderlyingSystemTypeProvider");
      var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly (assemblyName, AssemblyBuilderAccess.Run);
      var moduleBuilder = assemblyBuilder.DefineDynamicModule (assemblyName + ".dll");

      return moduleBuilder;
    }

    private void ImplementInterfaces (TypeBuilder typeBuilder, IEnumerable<Type> interfaces)
    {
      // todo: two interfaces defining equal members
      foreach (var ifc in interfaces)
      {
        // tODO methods that belong to interfaces and events?
        foreach (var method in ifc.GetMethods())
          DefineMethod (typeBuilder, method);

        foreach (var property in ifc.GetProperties())
          DefineProperty (typeBuilder, property);

        foreach (var @event in ifc.GetEvents())
          DefineEvent (typeBuilder, @event);
      }
    }

    private void DefineMethod (TypeBuilder typeBuilder, MethodInfo method)
    {
      var parameterTypes = method.GetParameters().Select (p => p.ParameterType).ToArray();
      typeBuilder.DefineMethod (method.Name, method.Attributes, method.ReturnType, parameterTypes);
    }

    private void DefineProperty (TypeBuilder typeBuilder, PropertyInfo property)
    {
      throw new NotImplementedException();
    }

    private void DefineEvent (TypeBuilder typeBuilder, EventInfo @event)
    {
    }
  }
}