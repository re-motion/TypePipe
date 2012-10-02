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
using Remotion.FunctionalProgramming;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Represents the TypePipe counterpart of <see cref="CustomAttributeData"/>.
  /// Can be used to retrieve attribute data from <see cref="MemberInfo"/>s and <see cref="ParameterInfo"/>s.
  /// </summary>
  public static class TypePipeCustomAttributeData
  {
    private static readonly IRelatedMethodFinder s_relatedMethodFinder = new RelatedMethodFinder();

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (MemberInfo member, bool inherit = false)
    {
      ArgumentUtility.CheckNotNull ("member", member);

      // TODO: inherit can only be true for overridable members (types, methods, properties, events)

      switch(member.MemberType)
      {
        case MemberTypes.TypeInfo:
        case MemberTypes.NestedType:
          return GetCustomAttributes ((Type) member, inherit);
        case MemberTypes.Method:
          return GetCustomAttributes ((MethodInfo) member, inherit);
      }

      return GetCustomAttributes (CustomAttributeData.GetCustomAttributes, member);
    }

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (Type type, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("type", type);

      if (inherit)
      {
        var typeHierarchy = type.CreateSequence (t => t.BaseType);
        return typeHierarchy
            .SelectMany (t => GetCustomAttributes (CustomAttributeData.GetCustomAttributes, t))
            .Where (IsInheritableAttribute);
      }
      else
        return GetCustomAttributes (CustomAttributeData.GetCustomAttributes, type);
    }

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (MethodInfo method, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("method", method);

      if (inherit)
      {
        var methodHierarchy = method.CreateSequence (m => s_relatedMethodFinder.GetBaseMethod (m));
        return methodHierarchy
            .SelectMany (m => GetCustomAttributes (CustomAttributeData.GetCustomAttributes, m))
            .Where (IsInheritableAttribute);
      }
      else
        return GetCustomAttributes (CustomAttributeData.GetCustomAttributes, method);
    }

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (ParameterInfo parameter)
    {
      ArgumentUtility.CheckNotNull ("parameter", parameter);

      return GetCustomAttributes (CustomAttributeData.GetCustomAttributes, parameter);
    }

    private static bool IsInheritableAttribute (ICustomAttributeData customAttributeData)
    {
      var attributeType = customAttributeData.Constructor.DeclaringType;
      Assertion.IsNotNull (attributeType);
      // TODO: implement using attributedata?!
      var attributeUsageAttribute = (AttributeUsageAttribute) attributeType.GetCustomAttributes (typeof (AttributeUsageAttribute), true).Single ();

      return attributeUsageAttribute.Inherited;
    }

    private static IEnumerable<ICustomAttributeData> GetCustomAttributes<T> (Func<T, IEnumerable<CustomAttributeData>> customAttributeUtility, T info)
    {
      var typePipeCustomAttributeProvider = info as ITypePipeCustomAttributeProvider;
      if (typePipeCustomAttributeProvider != null)
        return typePipeCustomAttributeProvider.GetCustomAttributeData();
      else
        return customAttributeUtility (info).Select (a => new CustomAttributeDataAdapter (a)).Cast<ICustomAttributeData>();
    }
  }
}