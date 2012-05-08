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
using System.Reflection;
using Remotion.Reflection.MemberSignatures;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// This class contains extensions methods for <see cref="IRelatedMethodFinder"/> objects.
  /// </summary>
  public static class RelatedMethodFinderExtensions
  {
    public static MethodInfo GetBaseMethod (this IRelatedMethodFinder relatedMethodFinder, MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("relatedMethodFinder", relatedMethodFinder);
      ArgumentUtility.CheckNotNull ("method", method);

      var rootDefinition = method.GetBaseDefinition ();
      if (method.Equals (rootDefinition))
        return null;

      return relatedMethodFinder.GetBaseMethod (method.Name, MethodSignature.Create (method), method.DeclaringType.BaseType);
    }
  }
}