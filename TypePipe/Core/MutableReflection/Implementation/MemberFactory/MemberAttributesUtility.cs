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
using System.Linq;
using System.Reflection;

namespace Remotion.TypePipe.MutableReflection.Implementation.MemberFactory
{
  /// <summary>
  /// A utility class for checking the user-specified attributes when creating a mutable member.
  /// </summary>
  public class MemberAttributesUtility
  {
    public static readonly FieldAttributes[] InvalidFieldAttributes =
        new[]
        {
            FieldAttributes.Literal, FieldAttributes.HasFieldMarshal, FieldAttributes.HasDefault, FieldAttributes.HasFieldRVA
        };

    public static readonly MethodAttributes[] InvalidConstructorAttributes =
        new[]
        {
            MethodAttributes.Final, MethodAttributes.Virtual, MethodAttributes.CheckAccessOnOverride, MethodAttributes.Abstract,
            MethodAttributes.PinvokeImpl, MethodAttributes.UnmanagedExport, MethodAttributes.RequireSecObject
        };

    public static readonly MethodAttributes[] InvalidMethodAttributes = new[] { MethodAttributes.RequireSecObject };

    public static readonly PropertyAttributes[] InvalidPropertyAttributes =
        new[]
        {
            PropertyAttributes.HasDefault, PropertyAttributes.Reserved2, PropertyAttributes.Reserved3, PropertyAttributes.Reserved4
        };

    public static readonly EventAttributes[] InvalidEventAttributes = new EventAttributes[0];

    public static void ValidateAttributes<T> (string memberKind, T[] invalidAttributes, T attributes, string parameterName)
    {
      var hasInvalidAttributes = invalidAttributes.Any (a => IsSet (attributes, a));
      if (hasInvalidAttributes)
      {
        var invalidAttributeList = string.Join (", ", invalidAttributes.Select (x => Enum.GetName (typeof (T), x)));
        var message = string.Format ("The following {0} are not supported for {1}: {2}.", typeof (T).Name, memberKind, invalidAttributeList);
        throw new ArgumentException (message, parameterName);
      }
    }

    private static bool IsSet<T> (T actual, T expected)
    {
      var f1 = (int) (object) actual;
      var f2 = (int) (object) expected;

      return (f1 & f2) == f2;
    }
  }
}