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
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.TypeAssembly;

namespace Remotion.TypePipe.FutureReflection
{
  /// <summary>
  /// Represents an existing <see cref="Type"/> that can be changed. Changes are recorded and later applied to the existing type via an
  /// instance of <see cref="ITypeModifier"/>.
  /// </summary>
  public abstract class ModifiedType : MutableType
  {
    public Type OriginalType { get { throw new NotImplementedException (); } }

    public virtual TypeModificationSpecification GetModificationSpecification ()
    {
      throw new NotImplementedException ();
    }
  }
}