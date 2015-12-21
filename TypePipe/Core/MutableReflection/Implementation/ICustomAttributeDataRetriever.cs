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

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// Retrieves <see cref="ICustomAttributeData"/> objects from implementations of <see cref="ICustomAttributeProvider"/>, that is
  /// <see cref="MemberInfo"/>, <see cref="ParameterInfo"/>, <see cref="Assembly"/> and <see cref="Module"/>.
  /// </summary>
  /// <remarks>Extension point is no longer required since bug in assembly binding redirection has been fixed in .NET 4.0.</remarks>
  internal interface ICustomAttributeDataRetriever
  {
    IEnumerable<ICustomAttributeData> GetCustomAttributeData (MemberInfo member);
    IEnumerable<ICustomAttributeData> GetCustomAttributeData (ParameterInfo parameter);
    IEnumerable<ICustomAttributeData> GetCustomAttributeData (Assembly assembly);
    IEnumerable<ICustomAttributeData> GetCustomAttributeData (Module module);
  }
}