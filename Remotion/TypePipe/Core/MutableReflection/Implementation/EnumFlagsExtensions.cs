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
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// Provides extensions methods for working with <see cref="Enum"/> attributed with the <see cref="FlagsAttribute"/>.
  /// </summary>
  public static class EnumFlagsExtensions
  {
    public static bool IsSet<T> (this Enum attributes, T flags)
    {
      Assertion.IsTrue (attributes.GetType() == typeof (T));
      Assertion.IsTrue (AttributeUtility.IsDefined<FlagsAttribute> (typeof (T), false));

      var attributesAsInt = (int) (object) attributes;
      var flagsAsInt = (int) (object) flags;
      Assertion.IsTrue (flagsAsInt != 0);

      return (attributesAsInt & flagsAsInt) == flagsAsInt;
    }

    public static bool IsUnset<T> (this Enum attributes, T flags)
    {
      return !IsSet (attributes, flags);
    }

    public static T Set<T> (this Enum attributes, T flags)
    {
      Assertion.IsTrue (attributes.GetType() == typeof (T));
      Assertion.IsTrue (AttributeUtility.IsDefined<FlagsAttribute> (typeof (T), false));

      return (T) (object) ((int) (object) attributes | (int) (object) flags);
    }

    public static T Unset<T> (this Enum attributes, T flags)
    {
      Assertion.IsTrue (attributes.GetType() == typeof (T));
      Assertion.IsTrue (AttributeUtility.IsDefined<FlagsAttribute> (typeof (T), false));

      return (T) (object) ((int) (object) attributes & ~(int) (object) flags);
    }
  }
}