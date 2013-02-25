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

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// Provides extensions methods for working with <see cref="Enum"/> attributed with the <see cref="FlagsAttribute"/>.
  /// </summary>
  public static class EnumExtensions
  {
    public static bool IsSet<T> (this Enum actual, T expected)
    {
      var f1 = (int) (object) actual;
      var f2 = (int) (object) expected;
      return (f1 & f2) == f2;
    }

    public static T Set<T> (this Enum flags1, T flags2)
    {
      return (T) (object) ((int) (object) flags1 | (int) (object) flags2);
    }

    public static T Unset<T> (this Enum flags1, T flags2)
    {
      return (T) (object) ((int) (object) flags1 & ~(int) (object) flags2);
    }
  }
}