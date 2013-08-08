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
using Remotion.TypePipe.Dlr.Ast;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Event arguments for the <see cref="MutableMethodInfo.BodyChanged"/> event.
  /// </summary>
  public class BodyChangedEventArgs : EventArgs
  {
    private readonly Expression _oldBody;
    private readonly Expression _newBody;

    public BodyChangedEventArgs (Expression oldBody, Expression newBody)
    {
      // Old body may be null.
      // New body may be null.

      _oldBody = oldBody;
      _newBody = newBody;
    }

    public Expression OldBody
    {
      get { return _oldBody; }
    }

    public Expression NewBody
    {
      get { return _newBody; }
    }
  }
}