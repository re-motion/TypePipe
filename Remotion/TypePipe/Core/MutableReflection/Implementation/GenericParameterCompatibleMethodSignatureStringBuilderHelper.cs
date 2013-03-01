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
using System.Text;
using Remotion.Reflection.MemberSignatures;
using Remotion.TypePipe.MutableReflection.Generics;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// This <see cref="MethodSignatureStringBuilderHelper"/> executes custom logic in case the specified <see cref="Type"/> is a generic parameter
  /// but has neither a <see cref="Type.DeclaringMethod"/> nor a <see cref="Type.DeclaringType"/>, which happens when we construct
  /// <see cref="MutableGenericParameter"/> instances before the declaring method is available.
  /// </summary>
  public class GenericParameterCompatibleMethodSignatureStringBuilderHelper : MethodSignatureStringBuilderHelper
  {
    // TODO Review: Is this ambigious when we support generic proxy type definitions?
    public override void AppendTypeString (StringBuilder sb, Type type)
    {
      // ReSharper disable ConditionIsAlwaysTrueOrFalse
      // ReSharper disable HeuristicUnreachableCode
      if (type.IsGenericParameter && type.DeclaringMethod == null && type.DeclaringType == null)
        sb.Append ("[").Append (type.GenericParameterPosition).Append ("]");
      // ReSharper restore HeuristicUnreachableCode
      // ReSharper restore ConditionIsAlwaysTrueOrFalse
      else
        base.AppendTypeString (sb, type);
    }
  }
}