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
#if NETFRAMEWORK
using System;
using System.IO;
using Microsoft.Win32;
using Remotion.FunctionalProgramming;

// ReSharper disable once CheckNamespace
namespace Remotion.Development.UnitTesting.PEVerifyPathSources
{
  partial class WindowsSdk6PEVerifyPathSource : PotentialPEVerifyPathSourceBase
  {
    public const string WindowsSdkRegistryKey = @"SOFTWARE\Microsoft\Microsoft SDKs\Windows";
    public const string WindowsSdkRegistryVersionValue = "CurrentVersion";
    public const string WindowsSdkRegistryInstallationFolderValue = "InstallationFolder";

    public override string GetLookupDiagnostics (PEVerifyVersion version)
    {
      if (version != PEVerifyVersion.DotNet2)
        return "Windows SDK 6: n/a";
      else
      {
        return string.Format (
            "Windows SDK 6: Registry: HKEY_LOCAL_MACHINE\\{0}\\[CurrentVersion]\\{1}\\bin\\PEVerify.exe",
            WindowsSdkRegistryKey,
            WindowsSdkRegistryInstallationFolderValue);
      }
    }

    protected override string GetPotentialPEVerifyPath (PEVerifyVersion version)
    {
      if (version != PEVerifyVersion.DotNet2)
        return null;

      return Maybe
          .ForValue (RegistryKey.OpenBaseKey (RegistryHive.LocalMachine, RegistryView.Registry32))
          .Select (key => key.OpenSubKey (WindowsSdkRegistryKey, false))
          .Select (key => key.GetValue (WindowsSdkRegistryVersionValue) as string)
          .Select (windowsSdkVersion => Registry.LocalMachine.OpenSubKey (WindowsSdkRegistryKey + "\\" + windowsSdkVersion, false))
          .Select (key => key.GetValue (WindowsSdkRegistryInstallationFolderValue) as string)
          .Select (path => Path.Combine (path, "bin"))
          .Select (path => Path.Combine (path, "PEVerify.exe"))
          .ValueOrDefault();
    }
  }
}
#endif
