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
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.Implementation
{
  /// <inheritdoc />
  /// <threadsafety static="true" instance="true"/>
  public class ConstructorFinder : IConstructorFinder
  {
    public ConstructorFinder ()
    {
    }

    public ConstructorInfo GetConstructor (Type requestedType, Type[] parameterTypes, bool allowNonPublic, Type assembledType)
    {
      ArgumentUtility.CheckNotNull ("assembledType", assembledType);
      ArgumentUtility.CheckNotNull ("parameterTypes", parameterTypes);
      ArgumentUtility.CheckNotNull ("requestedType", requestedType);

      CheckConstructorOnRequestedType (requestedType, parameterTypes, allowNonPublic);
      CheckNotAbstract (assembledType, requestedType);

      // Constructors that where copied from the requested type to the assembled type are always public. However, this does not hold when the
      // "don't create an assembled type if no modifications are made" optimization kicks in.
      return assembledType.GetConstructor (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, parameterTypes, null);
    }

    private void CheckNotAbstract (Type assembledType, Type requestedType)
    {
      if (assembledType.IsAbstract)
      {
        var message = string.Format ("The type '{0}' cannot be constructed because the assembled type is abstract.", requestedType);
        throw new InvalidOperationException (message);
      }
    }

    private void CheckConstructorOnRequestedType (Type requestedType, Type[] parameterTypes, bool allowNonPublic)
    {
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
      var constructor = requestedType.GetConstructor (bindingFlags, null, parameterTypes, null);

      if (constructor == null)
      {
        var message = string.Format (
            "Type '{0}' does not contain a constructor with the following signature: ({1}).",
            requestedType.FullName,
            String.Join ((string) ", ", (IEnumerable<string>) parameterTypes.Select (pt => pt.Name)));
        throw new MissingMethodException (message);
      }

      if (!constructor.IsPublic && !allowNonPublic)
      {
        var message = string.Format (
            "Type '{0}' contains a constructor with the required signature, but it is not public (and the allowNonPublic flag is not set).",
            requestedType.FullName);
        throw new MissingMethodException (message);
      }
    }
  }
}