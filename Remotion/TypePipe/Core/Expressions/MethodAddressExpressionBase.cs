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
using Remotion.Utilities;

namespace Remotion.TypePipe.Expressions
{
  /// <summary>
  ///  Represents the address of a method.
  /// </summary>
  public abstract class MethodAddressExpressionBase : TypePipeExpressionBase
  {
    private readonly MethodInfo _method;

    protected MethodAddressExpressionBase (MethodInfo method)
        : base (typeof (IntPtr))
    {
      ArgumentUtility.CheckNotNull ("method", method);

      _method = method;
    }

    public MethodInfo Method
    {
      get { return _method; }
    }
  }
}