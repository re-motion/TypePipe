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
using System.Configuration;

namespace Remotion.TypePipe.Configuration
{
  // TODO 5545: Move this to Remotion.Core.
  /// <summary>
  /// An <c>App.config</c>-based configuration provider.<br/>
  /// <example>
  /// Example of an <c>App.config</c> configuration file.
  /// <code>
  /// &lt;configuration&gt;
  ///   &lt;configSections&gt;
  ///     &lt;section name="typePipe" type="Remotion.TypePipe.Configuration.TypePipeConfigurationSection, Remotion.TypePipe"/&gt;
  ///     &lt;!-- ... --&gt;
  ///   &lt;/configSections&gt;
  ///   
  ///   &lt;typePipe xmlns="http://typepipe.codeplex.com/configuration"&gt;
  ///     &lt;forceStrongNaming keyFilePath="keyFile.snk" /&gt;
  ///   &lt;/typePipe&gt;
  ///   &lt;!-- ... --&gt;
  /// &lt;/configuration&gt;
  /// </code>
  /// </example>
  /// </summary>
  public class AppConfigBasedSettingsProvider
  {
    private readonly TypePipeConfigurationSection _section;

    public AppConfigBasedSettingsProvider ()
    {
      _section = (TypePipeConfigurationSection) ConfigurationManager.GetSection ("typePipe") ?? new TypePipeConfigurationSection();
    }

    public bool ForceStrongNaming
    {
      get { return _section.ForceStrongNaming.ElementInformation.IsPresent; }
    }

    public string KeyFilePath
    {
      get { return _section.ForceStrongNaming.KeyFilePath; }
    }

    // TODO 5370
    public bool EnableComplexSerialization { get; set; }

    // TODO 5370
    public PipelineSettings GetSettings ()
    {
      return new PipelineSettings ("remotion-default-pipeline")
             {
                 ForceStrongNaming = ForceStrongNaming,
                 KeyFilePath = KeyFilePath,
                 EnableSerializationWithoutAssemblySaving = EnableComplexSerialization
             };
    }
  }
}