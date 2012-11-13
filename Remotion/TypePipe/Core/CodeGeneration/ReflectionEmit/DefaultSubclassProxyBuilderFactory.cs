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
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  public class DefaultSubclassProxyBuilderFactory : SubclassProxyBuilderFactory
  {
    private static IModuleBuilder CreateModuleBuidler ()
    {
      var assemblyName = new AssemblyName ("TypePipe_GeneratedAssembly");
      var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly (assemblyName, AssemblyBuilderAccess.Run);
      var moduleBuilder = assembly.DefineDynamicModule (assemblyName.Name + ".dll", emitSymbolInfo: true);
      var moduleBuilderAdapter = new ModuleBuilderAdapter (moduleBuilder);
      var uniqueNameModuleBuilderDecorator = new UniqueNamingModuleBuilderDecorator (moduleBuilderAdapter);

      return uniqueNameModuleBuilderDecorator;
    }

    public DefaultSubclassProxyBuilderFactory ()
      : base(CreateModuleBuidler(), DebugInfoGenerator.CreatePdbGenerator())
    {
    }
  }
}