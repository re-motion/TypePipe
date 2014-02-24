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
using System.Linq;
using Remotion.TypePipe.Dlr.Ast.Compiler;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation
{
  /// <summary>
  /// Creates or retrieves delegate <see cref="Type"/>s by delegating to <see cref="DelegateHelpers.MakeDelegateType"/>.
  /// </summary>
  public class DelegateProvider : IDelegateProvider
  {
    public Type GetDelegateType (Type returnType, IEnumerable<Type> parameterTypes)
    {
      ArgumentUtility.CheckNotNull ("returnType", returnType);
      ArgumentUtility.CheckNotNull ("parameterTypes", parameterTypes);

      var types = parameterTypes.Concat (new[] { returnType }).ToArray();
      return DelegateHelpers.MakeDelegateType (types);
    }
  }
}