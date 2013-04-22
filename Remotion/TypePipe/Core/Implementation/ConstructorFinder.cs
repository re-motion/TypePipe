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
using Remotion.Text;
using Remotion.Utilities;

namespace Remotion.TypePipe.Implementation
{
  /// <inheritdoc />
  public class ConstructorFinder : IConstructorFinder
  {
    public ConstructorInfo GetConstructor (Type requestedType, Type[] parameterTypes, bool allowNonPublic, Type assembledType)
    {
      ArgumentUtility.CheckNotNull ("assembledType", assembledType);
      ArgumentUtility.CheckNotNull ("parameterTypes", parameterTypes);
      ArgumentUtility.CheckNotNull ("requestedType", requestedType);

      CheckConstructorOnRequestedType (requestedType, parameterTypes, allowNonPublic);

      // Constructors that where copied from the requested type to the assembled type are always public.
      return assembledType.GetConstructor (parameterTypes);
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
            SeparatedStringBuilder.Build (", ", parameterTypes, pt => pt.Name));
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