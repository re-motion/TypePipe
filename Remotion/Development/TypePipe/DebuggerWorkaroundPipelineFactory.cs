// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 

using System;
using System.Reflection.Emit;
using Remotion.Diagnostics;
using Remotion.Reflection.CodeGeneration.TypePipe;
using Remotion.Reflection.TypeDiscovery;
using Remotion.TypePipe.Implementation;

namespace Remotion.Development.TypePipe
{
  /// <summary>
  /// Use this <see cref="IPipelineFactory"/> as a workaround for the Reflection.Emit bug that causes calls to <see cref="TypeBuilder.CreateType"/>
  /// take a very long time to complete when the debugger is attached and a large number of types is generated into the same
  /// <see cref="AssemblyBuilder"/>.
  /// In addition, this class creates pipelines that automatically add the <see cref="NonApplicationAssemblyAttribute"/> to an in-memory assembly
  /// immediately after it has been created.
  /// <para>
  /// To use this workaround register <see cref="DebuggerWorkaroundPipelineFactory"/> for <see cref="IPipelineFactory"/> in your IoC container.
  /// </para>
  /// </summary>
  public class DebuggerWorkaroundPipelineFactory : RemotionPipelineFactory
  {
    public DebuggerWorkaroundPipelineFactory ()
    {
      DebuggerInterface = new DebuggerInterface();
      MaximumTypesPerAssembly = 11;
    }

    public IDebuggerInterface DebuggerInterface { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of types that will be generated into a single <see cref="AssemblyBuilder"/> while the debugger is attached.
    /// </summary>
    /// <value>The maximum number of types per assembly; the default is 11.</value>
    public int MaximumTypesPerAssembly { get; set; }

    [CLSCompliant (false)]
    protected override IReflectionEmitCodeGeneratorAndGeneratedCodeFlusher NewReflectionEmitCodeGenerator (
        string participantConfigurationID,
        bool forceStrongNaming,
        string keyFilePath,
        string assemblyDirectory,
        string assemblyNamePattern)
    {
      var moduleBuilderFactory = NewModuleBuilderFactory (participantConfigurationID);

      return new ReflectionEmitCodeGeneratorDecoratorWithGeneratedCodeFlusherSemantics (
          new DebuggerWorkaroundCodeGenerator (
              moduleBuilderFactory,
              forceStrongNaming,
              keyFilePath,
              DebuggerInterface,
              MaximumTypesPerAssembly,
              assemblyDirectory,
              assemblyNamePattern));
    }
  }
}