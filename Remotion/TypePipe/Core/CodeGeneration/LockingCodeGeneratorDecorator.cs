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

using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration
{
  /// <summary>
  /// Provides a synchronization wrapper around an implementation of <see cref="ICodeGenerator"/>.
  /// </summary>
  public class LockingCodeGeneratorDecorator : ICodeGenerator
  {
    private readonly ICodeGenerator _innerCodeGenerator;
    private readonly object _lock;

    public LockingCodeGeneratorDecorator (ICodeGenerator innerCodeGenerator, object lockObject)
    {
      ArgumentUtility.CheckNotNull ("innerCodeGenerator", innerCodeGenerator);
      ArgumentUtility.CheckNotNull ("lockObject", lockObject);

      _innerCodeGenerator = innerCodeGenerator;
      // TODO Review: Rename _lock to _lockObject
      _lock = lockObject;
    }

    public string AssemblyDirectory
    {
      get
      {
        lock (_lock)
          return _innerCodeGenerator.AssemblyDirectory;
      }
    }

    public string AssemblyName
    {
      get
      {
        lock (_lock)
          return _innerCodeGenerator.AssemblyName;
      }
    }

    public void SetAssemblyDirectory (string assemblyDirectory)
    {
      lock (_lock)
        _innerCodeGenerator.SetAssemblyDirectory (assemblyDirectory);
    }

    public void SetAssemblyName (string assemblyName)
    {
      lock (_lock)
        _innerCodeGenerator.SetAssemblyName (assemblyName);
    }

    public string FlushCodeToDisk ()
    {
      lock (_lock)
        return _innerCodeGenerator.FlushCodeToDisk();
    }
  }
}