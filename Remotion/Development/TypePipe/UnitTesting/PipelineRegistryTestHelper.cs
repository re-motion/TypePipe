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

using System.Linq;
using Remotion.ServiceLocation;
using Remotion.TypePipe;
using Remotion.TypePipe.Configuration;

namespace Remotion.Development.TypePipe.UnitTesting
{
  /// <summary>
  /// Provides supports for unit tests working with the <see cref="IPipelineRegistry"/>.
  /// </summary>
  public static class PipelineRegistryTestHelper
  {
    /// <summary>
    /// Returns the global <see cref="IPipelineRegistry"/> from the <see cref="SafeServiceLocator"/>.
    /// </summary>
    public static IPipelineRegistry GloablRegistry
    {
      get { return SafeServiceLocator.Current.GetInstance<IPipelineRegistry>(); }
    }

    /// <summary>
    /// Replaces the default pipeline (in the <see cref="GloablRegistry"/>) with a new, equivalent <see cref="IPipeline"/> instance. This can be
    /// used to avoid code generation tests influencing each other.
    /// </summary>
    public static void ResetDefaultPipeline ()
    {
      var pipelineRegistry = SafeServiceLocator.Current.GetInstance<IPipelineRegistry>();
      // TODO 5370: This should use the same settings as the default pipeline, add a way to retrieve these settings.
      var defaulPipeline = PipelineFactory.Create (
          "use same identifier here",
          new AppConfigBasedSettingsProvider().GetSettings(),
          pipelineRegistry.DefaultPipeline.Participants.ToArray());
      pipelineRegistry.SetDefaultPipeline (defaulPipeline);
    }
  }
}