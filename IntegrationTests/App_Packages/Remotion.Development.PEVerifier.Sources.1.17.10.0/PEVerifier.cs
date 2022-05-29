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
#if FEATURE_ASSEMBLYBUILDER_SAVE
using System;
using System.Diagnostics;
using System.Reflection;
using Remotion.Development.UnitTesting.PEVerifyPathSources;
using Remotion.Utilities;

// ReSharper disable once CheckNamespace
namespace Remotion.Development.UnitTesting
{
  partial class PEVerifier
  {
    public static PEVerifier CreateDefault ()
    {
      return
          new PEVerifier (
              new CompoundPEVerifyPathSource (
                  new WindowsSdk81aPEVerifyPathSource(),
                  new WindowsSdk80aPEVerifyPathSource(),
                  new WindowsSdk71PEVerifyPathSource(),
                  new WindowsSdk70aPEVerifyPathSource(),
                  new WindowsSdk6PEVerifyPathSource(),
                  new DotNetSdk20PEVerifyPathSource()));
    }

    private readonly IPEVerifyPathSource _pathSource;

    public PEVerifier (IPEVerifyPathSource pathSource)
    {
      _pathSource = pathSource;
    }

    public string GetVerifierPath (PEVerifyVersion version)
    {
      string verifierPath = _pathSource.GetPEVerifyPath (version);
      if (verifierPath == null)
      {
        var message = string.Format (
            "PEVerify for version '{0}' could not be found. Locations searched:\r\n{1}",
            version,
            _pathSource.GetLookupDiagnostics (version));
        throw new PEVerifyException (message);
      }
      return verifierPath;
    }

    public PEVerifyVersion GetDefaultVerifierVersion ()
    {
      return Environment.Version.Major == 4 ? PEVerifyVersion.DotNet4 : PEVerifyVersion.DotNet2;
    }


    public void VerifyPEFile (Assembly assembly)
    {
      ArgumentUtility.CheckNotNull ("assembly", assembly);

      VerifyPEFile (assembly.ManifestModule.FullyQualifiedName);
    }

    public void VerifyPEFile (Assembly assembly, PEVerifyVersion version)
    {
      ArgumentUtility.CheckNotNull ("assembly", assembly);

      VerifyPEFile (assembly.ManifestModule.FullyQualifiedName, version);
    }

    public void VerifyPEFile (string modulePath)
    {
      ArgumentUtility.CheckNotNull ("modulePath", modulePath);

      var version = GetDefaultVerifierVersion();
      VerifyPEFile (modulePath, version);
    }

    public void VerifyPEFile (string modulePath, PEVerifyVersion version)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("modulePath", modulePath);

      var process = StartPEVerifyProcess (modulePath, version);

      string output = process.StandardOutput.ReadToEnd();
      process.WaitForExit();

      if (process.ExitCode != 0)
      {
        throw new PEVerifyException (process.ExitCode, output);
      }
    }

    private Process StartPEVerifyProcess (string modulePath, PEVerifyVersion version)
    {
      string verifierPath = GetVerifierPath(version);

      var process = new Process();
      process.StartInfo.CreateNoWindow = true;
      process.StartInfo.FileName = verifierPath;
      process.StartInfo.RedirectStandardOutput = true;
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
      process.StartInfo.Arguments = string.Format ("/verbose \"{0}\"", modulePath);
      process.Start();
      return process;
    }
  }
}
#endif
