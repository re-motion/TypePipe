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
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// Implements <see cref="IUnderlyingTypeFactory"/> by creating new runtime types from <see cref="TypeBuilder"/> objects.
  /// </summary>
  public class UnderlyingTypeFactory : IUnderlyingTypeFactory
  {
    public UnderlyingTypeFactory ()
    {
    }

    // Generated lazily, on demand.
    private ModuleBuilder _moduleBuilder;
    private int _counter;

    // TODO Review: What about structs and generic base types, delegate types?
    public Type CreateUnderlyingSystemType (Type baseTypeOrNull, IEnumerable<Type> newInterfaces)
    {
      // Base type may be null (for interfaces).
      ArgumentUtility.CheckNotNull ("newInterfaces", newInterfaces);

      _moduleBuilder = _moduleBuilder ?? CreateModuleBuilder();
      _counter++;

      var isClass = baseTypeOrNull != null;
      var name = "UnderlyingSystemType" + _counter;
      var attributes = (isClass ? TypeAttributes.Class : TypeAttributes.Interface) | TypeAttributes.Abstract;

      var typeBuilder = _moduleBuilder.DefineType (name, attributes, baseTypeOrNull, newInterfaces.ToArray());
      if (isClass)
        AddDummyConstructor (typeBuilder);

      return typeBuilder.CreateType();
    }

    private ModuleBuilder CreateModuleBuilder ()
    {
      // TODO 5057: Ensure that generated assembly can be garbage collected when switching to .NET 4.x.
      var assemblyName = new AssemblyName ("TypePipe.UnderlyingSystemTypes");
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