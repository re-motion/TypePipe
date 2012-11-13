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
using System.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Decorates <see cref="IModuleBuilder"/> to ensure that types are defined with a unique name.
  /// </summary>
  public class UniqueNamingModuleBuilderDecorator : IModuleBuilder
  {
    private readonly IModuleBuilder _innerModuleBuilder;

    private int _counter;

    [CLSCompliant(false)]
    public IModuleBuilder InnerModuleBuilder
    {
      get { return _innerModuleBuilder; }
    }

    [CLSCompliant (false)]
    public UniqueNamingModuleBuilderDecorator (IModuleBuilder innerModuleBuilder)
    {
      ArgumentUtility.CheckNotNull ("innerModuleBuilder", innerModuleBuilder);

      _innerModuleBuilder = innerModuleBuilder;
    }

    [CLSCompliant (false)]
    public ITypeBuilder DefineType (string suggestedName, TypeAttributes attr, Type parent)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("suggestedName", suggestedName);
      ArgumentUtility.CheckNotNull ("parent", parent);

      _counter++;
      var name = string.Format ("{0}_Proxy{1}", suggestedName, _counter);
      return _innerModuleBuilder.DefineType (name, attr, parent);
    }
  }
}