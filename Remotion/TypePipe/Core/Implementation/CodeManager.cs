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

using System.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.Utilities;

namespace Remotion.TypePipe.Implementation
{
  // TODO 5503: docs
  public class CodeManager : ICodeManager
  {
    private readonly ICodeGenerator _codeGenerator;
    private readonly ITypeCache _typeCache;

    public CodeManager (ICodeGenerator codeGenerator, ITypeCache typeCache)
    {
      ArgumentUtility.CheckNotNull ("codeGenerator", codeGenerator);
      ArgumentUtility.CheckNotNull ("typeCache", typeCache);

      _codeGenerator = codeGenerator;
      _typeCache = typeCache;
    }

    public string AssemblyDirectory
    {
      get { return _codeGenerator.AssemblyDirectory; }
    }

    public string AssemblyName
    {
      get { return _codeGenerator.AssemblyName; }
    }

    public void SetAssemblyDirectory (string assemblyDirectory)
    {
      _codeGenerator.SetAssemblyDirectory (assemblyDirectory);
    }

    public void SetAssemblyName (string assemblyName)
    {
      _codeGenerator.SetAssemblyName (assemblyName);
    }

    public string FlushCodeToDisk ()
    {
      return _typeCache.FlushCodeToDisk();
    }

    public void LoadFlushedCode (Assembly assembly)
    {
      _typeCache.LoadFlushedCode (assembly);
    }
  }
}