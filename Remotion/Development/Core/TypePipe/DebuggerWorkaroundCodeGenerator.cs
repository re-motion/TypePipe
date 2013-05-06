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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Remotion.Diagnostics;
using Remotion.Logging;
using Remotion.TypePipe.CodeGeneration.Implementation.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.Implementation.ReflectionEmit.Abstractions;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.Development.TypePipe
{
  /// <summary>
  /// Derives from <see cref="ReflectionEmitCodeGenerator"/> and adapts <see cref="DefineType"/> to count the defined types and reset the module
  /// context when the number of types exceeds the given threshold. This can be used as a workaround for the Reflection.Emit bug where calls to
  /// <see cref="TypeBuilder.CreateType"/> take a very long time to complete  when the debugger is attached and a large number of types is generated
  /// into the same <see cref="AssemblyBuilder"/>.
  /// <para>
  /// To use this workaround register <see cref="DebuggerWorkaroundPipelineFactory"/> for <see cref="IPipelineFactory"/> in your IoC container.
  /// </para>
  /// </summary>
  public class DebuggerWorkaroundCodeGenerator : ReflectionEmitCodeGenerator
  {
    private static readonly ILog s_log = LogManager.GetLogger (typeof (DebuggerWorkaroundCodeGenerator));

    private readonly IDebuggerInterface _debuggerInterface;
    private readonly int _maximumTypesPerAssembly;

    private int _typeCountForCurrentAssembly;
    private int _totalTypeCount;
    private int _resetCount;

    [CLSCompliant (false)]
    public DebuggerWorkaroundCodeGenerator (
        IModuleBuilderFactory moduleBuilderFactory,
        bool forceStrongNaming,
        string keyFilePath,
        IDebuggerInterface debuggerInterface,
        int maximumTypesPerAssembly)
        : base (moduleBuilderFactory, forceStrongNaming, keyFilePath)
    {
      ArgumentUtility.CheckNotNull ("debuggerInterface", debuggerInterface);
      Debug.Assert (maximumTypesPerAssembly > 0);

      _debuggerInterface = debuggerInterface;
      _maximumTypesPerAssembly = maximumTypesPerAssembly;
    }

    public int MaximumTypesPerAssembly
    {
      get { return _maximumTypesPerAssembly; }
    }

    public int TypeCountForCurrentAssembly
    {
      get { return _typeCountForCurrentAssembly; }
    }

    public int TotalTypeCount
    {
      get { return _totalTypeCount; }
    }

    public int ResetCount
    {
      get { return _resetCount; }
    }

    public override string FlushCodeToDisk (IEnumerable<CustomAttributeDeclaration> assemblyAttributes)
    {
      throw new NotSupportedException ("Method FlushCodeToDisk is not supported by DebuggerWorkaroundCodeGenerator.");
    }

    [CLSCompliant (false)]
    public override ITypeBuilder DefineType (string name, TypeAttributes attributes, IEmittableOperandProvider emittableOperandProvider)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("emittableOperandProvider", emittableOperandProvider);

      _typeCountForCurrentAssembly++;
      _totalTypeCount++;

      if (_debuggerInterface.IsAttached && _typeCountForCurrentAssembly > _maximumTypesPerAssembly)
      {
        _resetCount++;
        s_log.InfoFormat ("Type threshold was exceeded (CurrentTypeCount: {0}, TotalTypeCount: {1}).", _typeCountForCurrentAssembly, _totalTypeCount);
        s_log.InfoFormat ("Started new assembly (ResetCount: {0}).", _resetCount);

        _typeCountForCurrentAssembly = 1;
        ResetModuleContext();
      }

      return base.DefineType (name, attributes, emittableOperandProvider);
    }
  }
}