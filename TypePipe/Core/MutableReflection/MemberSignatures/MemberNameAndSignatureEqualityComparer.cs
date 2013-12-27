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

namespace Remotion.TypePipe.MutableReflection.MemberSignatures
{
  /// <summary>
  /// Compares two members for equality by considering their name and signature.
  /// This comparer does not support comparing <see langword="null" /> values.
  /// </summary>
  public class MemberNameAndSignatureEqualityComparer : IEqualityComparer<MemberInfo>
  {
    private readonly MemberSignatureEqualityComparer _signatureComparer = new MemberSignatureEqualityComparer();

    public bool Equals (MemberInfo x, MemberInfo y)
    {
      ArgumentUtility.CheckNotNull ("x", x);
      ArgumentUtility.CheckNotNull ("y", y);

      return x.Name == y.Name && _signatureComparer.Equals (x, y);
    }

    public int GetHashCode (MemberInfo obj)
    {
      ArgumentUtility.CheckNotNull ("obj", obj);

      return EqualityUtility.GetRotatedHashCode (obj.Name, _signatureComparer.GetHashCode (obj));
    }
  }
}