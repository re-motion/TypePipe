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
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.CodeGeneration
{
  /// <summary>
  /// Defines an interfaces for classes building subclass proxies from a <see cref="MutableType"/>.
  /// The staged process is necessary to allow the generation of types and method bodies which reference each other.
  /// </summary>
  /// <remarks>
  /// Note that the methods must be called in the following order.
  /// <list type="number">
  ///   <item><see cref="DeclareType"/></item>
  ///   <item><see cref="DefineTypeFacet"/></item>
  ///   <item><see cref="CreateType"/></item>
  /// </list>
  /// </remarks>
  public interface IMutableTypeCodeGenerator
  {
    void DeclareType ();
    void DefineTypeFacet ();
    Type CreateType ();
  }
}