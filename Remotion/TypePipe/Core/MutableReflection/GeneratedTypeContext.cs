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
using System.Reflection;
using Remotion.Utilities;
using Remotion.FunctionalProgramming;
using System.Linq;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// A context that holds the completed types after code generation. This class can be used to retrieve a generated <see cref="MemberInfo"/>
  /// with the corresponding <see cref="IMutableMember"/>.
  /// </summary>
  public class GeneratedTypeContext
  {
    private readonly Dictionary<MutableType, Type> _typeDictionary;

    public GeneratedTypeContext (IEnumerable<MutableType> mutableTypes, IEnumerable<Type> generatedTypes)
    {
      _typeDictionary = mutableTypes.Zip (generatedTypes).ToDictionary (i => i.Item1, i => i.Item2);
    }

    public MemberInfo GetGeneratedMember (IMutableMember mutableMember)
    {
      ArgumentUtility.CheckNotNull ("mutableMember", mutableMember);

      return _typeDictionary[(MutableType) mutableMember];
    }
  }
}