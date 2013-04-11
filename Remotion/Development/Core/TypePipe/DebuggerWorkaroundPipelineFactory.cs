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
using Remotion.TypePipe;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.Configuration;
using Remotion.TypePipe.Implementation;

namespace Remotion.Development.TypePipe
{
  /// <summary>
  /// Use this <see cref="PipelineFactory"/> as a workaround for the Reflection.Emit bug that causes calls to <see cref="TypeBuilder.CreateType"/>
  /// take a very long time to complete when the debugger is attached and a large number of types is generated into the same
  /// <see cref="AssemblyBuilder"/>.
  /// <para>
  /// To use this workaround register <see cref="DebuggerWorkaroundPipelineFactory"/> for <see cref="IPipelineFactory"/> in your IoC container.
  /// </para>
  /// </summary>
  public class DebuggerWorkaroundPipelineFactory : PipelineFactory
  {
    private readonly int _maximumTypesPerAssembly;

    public DebuggerWorkaroundPipelineFactory (int maximumTypesPerAssembly)
    {
      _maximumTypesPerAssembly = maximumTypesPerAssembly;
    }

    [CLSCompliant (false)]
    protected override IReflectionEmitCodeGenerator NewReflectionEmitCodeGenerator (IConfigurationProvider configurationProvider)
    {
      var moduleBuilderFactory = NewModuleBuilderFactory();
      var debuggerInterface = new DebuggerInterface();

      return new DebuggerWorkaroundCodeGenerator (moduleBuilderFactory, configurationProvider, debuggerInterface, _maximumTypesPerAssembly);
    }
  }
}