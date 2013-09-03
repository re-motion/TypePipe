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
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.Implementation.Synchronization;

namespace Remotion.Development.TypePipe
{
  /// <summary>
  /// This <see cref="IPipelineFactory"/> enables saving, verification and cleanup of generated assemblies, which is useful for testing.
  /// The capabilities are available via <see cref="AssemblyTrackingCodeManager"/>.
  /// <para>
  /// To use assembly tracking register <see cref="AssemblyTrackingPipelineFactory"/> for <see cref="IPipelineFactory"/> in your IoC container.
  /// </para>
  /// </summary>
  public class AssemblyTrackingPipelineFactory : DefaultPipelineFactory
  {
    public AssemblyTrackingCodeManager AssemblyTrackingCodeManager { get; private set; }

    protected override ICodeManager NewCodeManager (ICodeManagerSynchronizationPoint codeManagerSynchronizationPoint, ITypeCache typeCache)
    {
      var codeManager = base.NewCodeManager (codeManagerSynchronizationPoint, typeCache);
      AssemblyTrackingCodeManager = new AssemblyTrackingCodeManager (codeManager);

      return AssemblyTrackingCodeManager;
    }
  }
}