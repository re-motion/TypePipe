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
using Microsoft.Scripting.Ast;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.BodyBuilding
{
  /// <summary>
  /// Provides useful functionality for working with method body provider delegates.
  /// </summary>
  public static class BodyProviderUtility
  {
    public static Expression GetVoidBody<T> (Func<T, Expression> bodyProvider, T context)
        where T : MethodBodyContextBase
    {
      ArgumentUtility.CheckNotNull ("bodyProvider", bodyProvider);
      ArgumentUtility.CheckNotNull ("context", context);

      var body = bodyProvider (context);
      if (body == null)
        throw new InvalidOperationException ("Body provider must return non-null body.");

      return ExpressionTypeUtility.EnsureCorrectType (body, typeof (void));
    }
  }
}