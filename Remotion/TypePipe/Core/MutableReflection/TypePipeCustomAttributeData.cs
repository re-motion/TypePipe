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
    private static readonly IRelatedPropertyFinder s_relatedPropertyFinder = new RelatedPropertyFinder();
    private static readonly IRelatedEventFinder s_relatedEventFinder = new RelatedEventFinder();

    // TODO 5072: Overloads for FieldInfo, ConstructorInfo
    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (MemberInfo member, bool inherit = false)
    {
      ArgumentUtility.CheckNotNull ("member", member);

      switch (member.MemberType)
      {
        case MemberTypes.TypeInfo:
        case MemberTypes.NestedType:
          return GetCustomAttributes ((Type) member, inherit);
        case MemberTypes.Method:
          return GetCustomAttributes ((MethodInfo) member, inherit);
        case MemberTypes.Property:
          return GetCustomAttributes ((PropertyInfo) member, inherit);
        case MemberTypes.Event:
          return GetCustomAttributes ((EventInfo) member, inherit);
        default:
          return GetCustomAttributes (CustomAttributeData.GetCustomAttributes, member);
      }
    }

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (Type type, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("type", type);

      return GetCustomAttributes (CustomAttributeData.GetCustomAttributes, t => t.BaseType, type, inherit);
    }

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (MethodInfo method, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("method", method);

      return GetCustomAttributes (CustomAttributeData.GetCustomAttributes, s_relatedMethodFinder.GetBaseMethod, method, inherit);
    }

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (PropertyInfo property, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("property", property);

      return GetCustomAttributes (CustomAttributeData.GetCustomAttributes, s_relatedPropertyFinder.GetBaseProperty, property, inherit);
    }

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (EventInfo @event, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("event", @event);

      return GetCustomAttributes (CustomAttributeData.GetCustomAttributes, s_relatedEventFinder.GetBaseEvent, @event, inherit);
    }

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (ParameterInfo parameter)
    {
      ArgumentUtility.CheckNotNull ("parameter", parameter);

      return GetCustomAttributes (CustomAttributeData.GetCustomAttributes, parameter);
    }

    private static IEnumerable<ICustomAttributeData> GetCustomAttributes<T> (
        Func<T, IEnumerable<CustomAttributeData>> customAttributeProvider,
        Func<T, T> baseMemberProvider,
        T member,
        bool inherit)
        where T : MemberInfo
    {
      var attributes = GetCustomAttributes (customAttributeProvider, member);
      if (!inherit)
        return attributes;

      var baseMember = baseMemberProvider (member);
      var inheritedAttributes = baseMember
          .CreateSequence (baseMemberProvider)
          .SelectMany (m => GetCustomAttributes (customAttributeProvider, m))
          .Where (d => AttributeUtility.IsAttributeInherited (d.Constructor.DeclaringType));
      
      var allAttributesWithInheritance = attributes.Concat (inheritedAttributes);
      return EvaluateAllowMultiple (allAttributesWithInheritance);
    }

    private static IEnumerable<ICustomAttributeData> EvaluateAllowMultiple (IEnumerable<ICustomAttributeData> attributesFromDerivedToBase)
    {
      var encounteredAttributeTypes = new HashSet<Type>();
      foreach (var data in attributesFromDerivedToBase)
      {
        if (!encounteredAttributeTypes.Contains (AttributeType (data)) || AttributeUtility.IsAttributeAllowMultiple (AttributeType (data)))
          yield return data;

        encounteredAttributeTypes.Add (AttributeType (data));
      }
    }

    // TODO 5072: Property on ICustomAttributeData
    private static Type AttributeType (ICustomAttributeData d)
    {
      return d.Constructor.DeclaringType;
    }

    private static IEnumerable<ICustomAttributeData> GetCustomAttributes<T> (
        Func<T, IEnumerable<CustomAttributeData>> customAttributeProvider, T info)
    {
      var typePipeCustomAttributeProvider = info as ITypePipeCustomAttributeProvider;
      if (typePipeCustomAttributeProvider != null)
        return typePipeCustomAttributeProvider.GetCustomAttributeData();
      else
        return customAttributeProvider (info).Select (a => new CustomAttributeDataAdapter (a)).Cast<ICustomAttributeData>();
    }
  }
}