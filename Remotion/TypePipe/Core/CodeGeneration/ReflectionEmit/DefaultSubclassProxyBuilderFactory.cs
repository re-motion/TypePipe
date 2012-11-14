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
using System.Runtime.CompilerServices;
using System.Threading;
using Remotion.ServiceLocation;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// A class that solely exists for the default service locator configuration, i.e., we need a class that the 
  /// <see cref="ConcreteImplementationAttribute"/> on <see cref="ISubclassProxyBuilderFactory"/> can point to.
  /// This is needed because the <see cref="SubclassProxyBuilder"/> constructor contains arguments that cannot be configured using the
  /// <see cref="ConcreteImplementationAttribute"/>.
  /// </summary>
  public class DefaultSubclassProxyBuilderFactory : SubclassProxyBuilderFactory
  {
    private static int s_counter;

    private static IModuleBuilder CreateModuleBuilder ()
    {
      var uniqueCounterValue = Interlocked.Increment (ref s_counter);
      var assemblyName = "TypePipe_GeneratedAssembly_" + uniqueCounterValue;

      return ReflectionEmitBackendFactory.CreateModuleBuilder (assemblyName).Item1;
    }

    public DefaultSubclassProxyBuilderFactory ()
        : base (CreateModuleBuilder(), DebugInfoGenerator.CreatePdbGenerator())
    {
    }
  }
}