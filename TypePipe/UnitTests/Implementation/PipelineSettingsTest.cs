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
using NUnit.Framework;

namespace Remotion.TypePipe.UnitTests.Implementation
{
  [TestFixture]
  public class PipelineSettingsTest
  {
    [Test]
    public void Initialization ()
    {
      var settings = new PipelineSettings (true, "keyFile", true, "assemblyDirectory", "assemblyName{counter}suffix", 2);
      Assert.That (settings.ForceStrongNaming, Is.True);
      Assert.That (settings.KeyFilePath, Is.EqualTo ("keyFile"));
      Assert.That (settings.EnableSerializationWithoutAssemblySaving, Is.True);
      Assert.That (settings.AssemblyDirectory, Is.EqualTo ("assemblyDirectory"));
      Assert.That (settings.AssemblyNamePattern, Is.EqualTo ("assemblyName{counter}suffix"));
      Assert.That (settings.DegreeOfParallelism, Is.EqualTo (2));
    }

    [Test]
    public void Initialization_WithDegreeOfParallelism_LessThanOne_ThrowsArgumentOutOfRangeException ()
    {
      Assert.That (
          () => new PipelineSettings (false, null, false, null, "assemblyName", 0),
          Throws.TypeOf<ArgumentOutOfRangeException>()
              .With.Message.EqualTo (
                  "The degree of parallelism must be greater than 0.\r\nParameter name: degreeOfParallelism\r\nActual value was 0."));
    }

    [Test]
    public void Initialization_WithDegreeOfParallelism_GreaterThanOneAndAssemblyNamePatternDoesNotContainCounter_ThrowsArgumentException ()
    {
      Assert.That (
          () => new PipelineSettings (false, null, false, null, "assemblyName", 2),
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "When a degree of parallelism greater than 1 is specified, the '{counter}' placeholder must be included in the assembly name pattern."
                  + "\r\nParameter name: assemblyNamePattern"));
    }

    [Test]
    public void Build_FromExisting_CopiesValues ()
    {
      var settings = new PipelineSettings (true, "keyFile", true, "assemblyDirectory", "assemblyName_{counter}", 2);
      var newSettings = PipelineSettings.From (settings).Build();
      Assert.That (newSettings.ForceStrongNaming, Is.True);
      Assert.That (newSettings.KeyFilePath, Is.EqualTo ("keyFile"));
      Assert.That (newSettings.EnableSerializationWithoutAssemblySaving, Is.True);
      Assert.That (newSettings.AssemblyDirectory, Is.EqualTo ("assemblyDirectory"));
      Assert.That (newSettings.AssemblyNamePattern, Is.EqualTo ("assemblyName_{counter}"));
      Assert.That (newSettings.DegreeOfParallelism, Is.EqualTo (2));
    }

    [Test]
    public void Build_SetForceStrongNaming ()
    {
      var settings = PipelineSettings.New().SetForceStrongNaming (true).Build();
      Assert.That (settings.ForceStrongNaming, Is.True);
    }

    [Test]
    public void Build_SetKeyFilePath ()
    {
      var settings = PipelineSettings.New().SetKeyFilePath ("set_value").Build();
      Assert.That (settings.KeyFilePath, Is.EqualTo ("set_value"));
    }

    [Test]
    public void Build_SetEnableSerializationWithoutAssemblySaving ()
    {
      var settings = PipelineSettings.New().SetEnableSerializationWithoutAssemblySaving (true).Build();
      Assert.That (settings.EnableSerializationWithoutAssemblySaving, Is.True);
    }

    [Test]
    public void Build_SetAssemblyDirectory ()
    {
      var settings = PipelineSettings.New().SetAssemblyDirectory ("set_value").Build();
      Assert.That (settings.AssemblyDirectory, Is.EqualTo ("set_value"));
    }

    [Test]
    public void AssemblyNamePatternBuild_SetKeyFilePath ()
    {
      var settings = PipelineSettings.New().SetAssemblyNamePattern ("set_value").Build();
      Assert.That (settings.AssemblyNamePattern, Is.EqualTo ("set_value"));
    }

    [Test]
    public void Build_SetDegreeOfParallelism ()
    {
      var settings = PipelineSettings.New().SetDegreeOfParallelism (3).Build();
      Assert.That (settings.DegreeOfParallelism, Is.EqualTo (3));
    }

    [Test]
    public void Build_SetDegreeOfParallelism_LessThanOne_ThrowsArgumentOutOfRangeException ()
    {
      var builder = PipelineSettings.New();
      Assert.That (
          () => builder.SetDegreeOfParallelism (0),
          Throws.TypeOf<ArgumentOutOfRangeException>()
              .With.Message.EqualTo ("The degree of parallelism must be greater than 0.\r\nParameter name: value\r\nActual value was 0."));
    }
  }
}