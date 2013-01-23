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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Remotion.Collections;
using Remotion.TypePipe.Caching;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// Implements <see cref="IUnderlyingSystemTypeFactory"/> by creating new runtime types from <see cref="TypeBuilder"/> objects.
  /// </summary>
  /// <remarks>
  /// This class is used behind the <see cref="TypeCache"/>, therefore the incrementation of the <see cref="_counter"/> field does not need to be
  /// guarded.
  /// </remarks>
  public class UnderlyingSystemTypeFactory : IUnderlyingSystemTypeFactory
  {
    private class Comparer : IEqualityComparer<Tuple<string, Type, HashSet<Type>>>
    {
      public bool Equals (Tuple<string, Type, HashSet<Type>> x, Tuple<string, Type, HashSet<Type>> y)
      {
        return EqualityUtility.Equals (x.Item1, y.Item1)
               && EqualityUtility.Equals (x.Item2, y.Item2)
               && x.Item3.SetEquals (y.Item3);
      }

      public int GetHashCode (Tuple<string, Type, HashSet<Type>> obj)
      {
        // TODO
        return 7;
      }
    }

    private readonly ICache<Tuple<string, Type, HashSet<Type>>, Type> _cache =
        CacheFactory.Create<Tuple<string, Type, HashSet<Type>>, Type> (new Comparer());

    private ModuleBuilder _moduleBuilder;
    private int _counter;

    public Type CreateUnderlyingSystemType (CustomType customType)
    {
      ArgumentUtility.CheckNotNull ("customType", customType);
      Assertion.IsNotNull (customType.BaseType);

      // tODO: think about: re-implemented interfaces, maybe skip them.
      var addedInterfaces = customType.GetInterfaces().Except (customType.BaseType.GetInterfaces());
      var key = Tuple.Create (customType.FullName, customType.BaseType, new HashSet<Type> (addedInterfaces));

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
      AddDummyConstructor (typeBuilder);

      return typeBuilder.CreateType();
    }

    private ModuleBuilder CreateModuleBuilder ()
    {
      var assemblyName = new AssemblyName ("UnderlyingSystemTypeFactory");
      var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly (assemblyName, AssemblyBuilderAccess.Run);
      var moduleBuilder = assemblyBuilder.DefineDynamicModule (assemblyName + ".dll");

      return moduleBuilder;
    }

    private void AddDummyConstructor (TypeBuilder typeBuilder)
    {
      var ctorBuilder = typeBuilder.DefineConstructor (MethodAttributes.Public, CallingConventions.HasThis, Type.EmptyTypes);
      ctorBuilder.GetILGenerator().Emit (OpCodes.Ret);
    }
  }
}