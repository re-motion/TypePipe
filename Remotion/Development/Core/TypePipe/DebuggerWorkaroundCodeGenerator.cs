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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.Configuration;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.Development.TypePipe
{
  /// <summary>
  /// Derives from <see cref="ReflectionEmitCodeGenerator"/> and adapts <see cref="DefineType"/> to count the defined types and reset the
  /// <see cref="IModuleBuilder"/> when the number of types exceeds the given threshold. This can be used as a workaround for the Reflection.Emit bug
  /// where calls to  <see cref="TypeBuilder.CreateType"/> take a very long time to complete  when the debugger is attached and a large number of
  /// types is generated into the same <see cref="AssemblyBuilder"/>.
  /// </summary>
  public class DebuggerWorkaroundCodeGenerator : ReflectionEmitCodeGenerator
  {
    private readonly IDebuggerInterface _debuggerInterface;
    private readonly int _maximumTypesPerAssembly;

    private int _typeCountForCurrentAssembly;
    private int _totalTypeCount;
    private int _resetCount;

    [CLSCompliant (false)]
    public DebuggerWorkaroundCodeGenerator (
        IModuleBuilderFactory moduleBuilderFactory,
        IConfigurationProvider configurationProvider,
        IDebuggerInterface debuggerInterface,
        int maximumTypesPerAssembly)
        : base (moduleBuilderFactory, configurationProvider)
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
        ResetModuleContext();
        _typeCountForCurrentAssembly = 1;
        _resetCount++;
      }

      return base.DefineType (name, attributes, emittableOperandProvider);
    }
  }
}