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
using System.Xml;

namespace Remotion.TypePipe.Configuration
{
  public class TypePipeConfigurationSection : ConfigurationSection
  {
    private const string c_xmlNamespace = "http://typepipe.codeplex.com/configuration";

    public static readonly string ExampleConfiguration =
        "<typePipe xmlns=\"" + c_xmlNamespace + "\">" + Environment.NewLine +
        "  <forceStrongNaming keyFilePath=\"key.snk\" />" + Environment.NewLine +
        "</typePipe>";

    [ConfigurationProperty ("xmlns")]
    public string XmlNamespace
    {
      get { return c_xmlNamespace; }
    }

    [ConfigurationProperty ("forceStrongNaming")]
    public ForceStrongNamingConfigurationElement ForceStrongNaming
    {
      get { return (ForceStrongNamingConfigurationElement) this["forceStrongNaming"]; }
    }

    protected override bool OnDeserializeUnrecognizedElement (string elementName, XmlReader reader)
    {
      var message = string.Format ("Unknown element name: {0}{2}Example configuration:{2}{1}", elementName, ExampleConfiguration, Environment.NewLine);
      throw new ConfigurationErrorsException (message);
    }
  }
}