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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Represents the TypePipe counterpart of <see cref="CustomAttributeData"/>.
  /// </summary>
  public class TypePipeCustomAttributeData
  {
    private readonly ConstructorInfo _constructor;
    private readonly ReadOnlyCollection<TypePipeCustomAttributeTypedArgument> _constructorArguments;
    private readonly ReadOnlyCollection<TypePipeCustomAttributeNamedArgument> _namedArguments;

    public TypePipeCustomAttributeData (
        ConstructorInfo constructor,
        IEnumerable<TypePipeCustomAttributeTypedArgument> constructorArguments,
        IEnumerable<TypePipeCustomAttributeNamedArgument> namedArguments)
    {
      ArgumentUtility.CheckNotNull ("constructor", constructor);
      ArgumentUtility.CheckNotNull ("constructorArguments", constructorArguments);
      ArgumentUtility.CheckNotNull ("namedArguments", namedArguments);

      _constructor = constructor;
      _constructorArguments = constructorArguments.ToList().AsReadOnly();
      _namedArguments = namedArguments.ToList().AsReadOnly();
    }

    public ConstructorInfo Constructor
    {
      get { return _constructor; }
    }

    public ReadOnlyCollection<TypePipeCustomAttributeTypedArgument> ConstructorArguments
    {
      get { return _constructorArguments; }
    }
    
    public ReadOnlyCollection<TypePipeCustomAttributeNamedArgument> NamedArguments
    {
      get { return _namedArguments; }
    }
  }
}