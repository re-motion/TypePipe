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
using System.IO;
using System.Reflection;
using Remotion.Utilities;

// ReSharper disable once CheckNamespace
namespace Remotion.Development.UnitTesting.IO
{
  /// <summary>
  /// Provides functionality for loading an <see cref="Assembly"/> from a file path without locking the file.
  /// </summary>
  static partial class AssemblyLoader
  {
    public static Assembly LoadWithoutLocking (string assemblyFilenameOrPath)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("assemblyFilenameOrPath", assemblyFilenameOrPath);

      var bytes = File.ReadAllBytes (assemblyFilenameOrPath);
      return Assembly.Load (bytes);
    }
  }
}