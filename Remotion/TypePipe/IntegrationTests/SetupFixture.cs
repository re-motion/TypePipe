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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using System.Linq;
using Remotion.Utilities;

namespace TypePipe.IntegrationTests
{
  [SetUpFixture]
  public class SetupFixture
  {
    private static string s_generatedFileDirectory;

    public static string GeneratedFileDirectory
    {
      get
      {
        Assertion.IsNotNull (s_generatedFileDirectory, "GeneratedFileDirectory can only be called after SetUp has run.");
        return s_generatedFileDirectory; 
      }
    }

    private HashSet<string > _copiedFileNames;

    [SetUp]
    public void SetUp ()
    {
      s_generatedFileDirectory = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "GeneratedAssemblies");
      _copiedFileNames = new HashSet<string>();
      
      PrepareOutputDirectory ();
    }

    [TearDown]
    public void TearDown ()
    {
      CleanupOutputDirectory();
    }

    private void PrepareOutputDirectory ()
    {
      Directory.CreateDirectory (s_generatedFileDirectory);

      CopyModuleToOutputDirectory (GetType().Assembly.ManifestModule);
      CopyModuleToOutputDirectory (typeof (Closure).Assembly.ManifestModule);
    }

    private void CopyModuleToOutputDirectory (Module copiedModule)
    {
      var sourcePath = copiedModule.FullyQualifiedName;
      var destPath = Path.Combine (s_generatedFileDirectory, copiedModule.Name);
      File.Copy (sourcePath, destPath, true);
      _copiedFileNames.Add (copiedModule.Name);
    }

    private void CleanupOutputDirectory ()
    {
      // Only delete directory if no generated files left
      var fileNamesInGeeratedDirectory = Directory.GetFiles (s_generatedFileDirectory).Select (Path.GetFileName);
      if (fileNamesInGeeratedDirectory.Any (f => !_copiedFileNames.Contains (f)))
        return;

      Directory.Delete (s_generatedFileDirectory, true);
    }
  }
}