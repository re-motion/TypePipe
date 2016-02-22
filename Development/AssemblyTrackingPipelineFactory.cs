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
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.TypeAssembly.Implementation;

namespace Remotion.TypePipe.Development
{
  /// <summary>
  /// This <see cref="IPipelineFactory"/> enables saving, verification and cleanup of generated assemblies, which is useful for testing.
  /// The capabilities are available via <see cref="AssemblyTrackingCodeManager"/>.
  /// <para>
  /// To use assembly tracking register <see cref="AssemblyTrackingPipelineFactory"/> for <see cref="IPipelineFactory"/> in your IoC container.
  /// </para>
  /// </summary>
  /// <threadsafety static="true" instance="true"/>
  public class AssemblyTrackingPipelineFactory : DefaultPipelineFactory
  {
    public AssemblyTrackingCodeManager AssemblyTrackingCodeManager { get; private set; }

    protected override ICodeManager NewCodeManager (ITypeCache typeCache, ITypeAssembler typeAssembler, IAssemblyContextPool assemblyContextPool)
    {
      var codeManager = base.NewCodeManager (typeCache, typeAssembler, assemblyContextPool);
      AssemblyTrackingCodeManager = new AssemblyTrackingCodeManager (codeManager);

      return AssemblyTrackingCodeManager;
    }
  }
}