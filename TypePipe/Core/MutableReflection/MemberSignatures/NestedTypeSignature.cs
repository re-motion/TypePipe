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

namespace Remotion.TypePipe.MutableReflection.MemberSignatures
{
  /// <summary>
  /// Represents a nested type signature and allows signatures to be compared to each other.
  /// </summary>
  public class NestedTypeSignature : IMemberSignature, IEquatable<NestedTypeSignature>
  {
    public static NestedTypeSignature Create (Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);
      Assertion.IsNotNull (type.DeclaringType);

      return new NestedTypeSignature (type.GetGenericArguments().Length);
    }

    private readonly int _genericParameterCount;

    public NestedTypeSignature (int genericParameterCount)
    {
      _genericParameterCount = genericParameterCount;
    }

    public int GenericParameterCount
    {
      get { return _genericParameterCount; }
    }

    public override string ToString ()
    {
      return "`" + GenericParameterCount;
    }

    public bool Equals (NestedTypeSignature other)
    {
      return !ReferenceEquals (other, null)
             && GenericParameterCount == other.GenericParameterCount;
    }

    public sealed override bool Equals (object obj)
    {
      if (obj == null || obj.GetType () != GetType ())
        return false;

      var other = (NestedTypeSignature) obj;
      return Equals (other);
    }

    bool IEquatable<IMemberSignature>.Equals (IMemberSignature other)
    {
      return Equals (other);
    }


    public override int GetHashCode ()
    {
      return GenericParameterCount.GetHashCode();
    }
  }
}