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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Represents the TypePipe counterpart of <see cref="CustomAttributeData"/>.
  /// Can be used to retrieve attribute data from <see cref="MemberInfo"/>s and <see cref="ParameterInfo"/>s.
  /// Note that the results returned by this class are cached.
  /// </summary>
  public static class TypePipeCustomAttributeData
  {
    private static readonly ICustomAttributeDataRetriever s_customAttributeDataRetriever = new CustomAttributeDataRetriever();

    private static readonly ConcurrentDictionary<CustomAttributeDataCacheKey, IReadOnlyCollection<ICustomAttributeData>> s_cache =
        new ConcurrentDictionary<CustomAttributeDataCacheKey, IReadOnlyCollection<ICustomAttributeData>>();

    private static readonly Func<Type, Type> s_baseTypeProvider = type => type.BaseType;
    private static readonly Func<MethodInfo, MethodInfo> s_baseMethodProvider = new RelatedMethodFinder().GetBaseMethod;
    private static readonly Func<PropertyInfo, PropertyInfo> s_basePropertyProvider = new RelatedPropertyFinder().GetBaseProperty;
    private static readonly Func<EventInfo, EventInfo> s_baseEventProvider = new RelatedEventFinder().GetBaseEvent;

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (MemberInfo member, bool inherit = false)
    {
      ArgumentUtility.CheckNotNull ("member", member);

      switch (member.MemberType)
      {
        case MemberTypes.TypeInfo:
        case MemberTypes.NestedType:
          return GetCustomAttributes ((Type) member, inherit);
        case MemberTypes.Field:
          return GetCustomAttributes ((FieldInfo) member);
        case MemberTypes.Constructor:
          return GetCustomAttributes ((ConstructorInfo) member);
        case MemberTypes.Method:
          return GetCustomAttributes ((MethodInfo) member, inherit);
        case MemberTypes.Property:
          return GetCustomAttributes ((PropertyInfo) member, inherit);
        case MemberTypes.Event:
          return GetCustomAttributes ((EventInfo) member, inherit);
        case MemberTypes.Custom:
          throw new NotSupportedException ("Member type 'Custom' is not supported.");
        default:
          throw new Exception ("unreachable");
      }
    }

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (Type type, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("type", type);

      return GetCachedAttributes (type, inherit, s_baseTypeProvider);
    }

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (FieldInfo field)
    {
      ArgumentUtility.CheckNotNull ("field", field);

      return GetCachedAttributes (field, inherit: false, baseMemberProvider: null);
    }

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (ConstructorInfo constructor)
    {
      ArgumentUtility.CheckNotNull ("constructor", constructor);

      return GetCachedAttributes (constructor, inherit: false, baseMemberProvider: null);
    }

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (MethodInfo method, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("method", method);

      return GetCachedAttributes (method, inherit, s_baseMethodProvider);
    }

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (PropertyInfo property, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("property", property);

      return GetCachedAttributes (property, inherit, s_basePropertyProvider);
    }

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (EventInfo @event, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("event", @event);

      return GetCachedAttributes (@event, inherit, s_baseEventProvider);
    }

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (ParameterInfo parameter)
    {
      ArgumentUtility.CheckNotNull ("parameter", parameter);

      return GetCachedAttributes (parameter, s_customAttributeDataRetriever.GetCustomAttributeData);
    }

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (Assembly assembly)
    {
      ArgumentUtility.CheckNotNull ("assembly", assembly);

      return GetCachedAttributes (assembly, s_customAttributeDataRetriever.GetCustomAttributeData);
    }

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (Module module)
    {
      ArgumentUtility.CheckNotNull ("module", module);

      return GetCachedAttributes (module, s_customAttributeDataRetriever.GetCustomAttributeData);
    }

    private static IReadOnlyCollection<ICustomAttributeData> GetCachedAttributes<T> (
        T target, Func<T, IEnumerable<ICustomAttributeData>> attributeDataRetriever)
        where T : ICustomAttributeProvider
    {
      var key = new CustomAttributeDataCacheKey (target, inherit: false);

      // Optimization: Do not extract code duplication into method with Func parameters. This would require creating a closure that captures
      // variables from an outer scope, which is expensive.
      IReadOnlyCollection<ICustomAttributeData> attributes;
      if (s_cache.TryGetValue (key, out attributes))
        return attributes;

      return s_cache.GetOrAdd (key, k => attributeDataRetriever ((T) k.AttributeTarget).ToList().AsReadOnly());
    }

    private static IReadOnlyCollection<ICustomAttributeData> GetCachedAttributes<T> (T member, bool inherit, Func<T, T> baseMemberProvider)
        where T : MemberInfo
    {
      if (member is IMutableMember)
        return GetAttributes (member, inherit, baseMemberProvider);

      var key = new CustomAttributeDataCacheKey (member, inherit);

      // Optimization: Do not extract code duplication into method with Func parameters. This would require creating a closure that captures
      // variables from an outer scope, which is expensive.
      IReadOnlyCollection<ICustomAttributeData> attributes;
      if (s_cache.TryGetValue (key, out attributes))
        return attributes;

      return s_cache.GetOrAdd (key, k => GetAttributes ((T) k.AttributeTarget, k.Inherit, baseMemberProvider));
    }

    private static IReadOnlyCollection<ICustomAttributeData> GetAttributes<T> (T member, bool inherit, Func<T, T> baseMemberProvider)
        where T : MemberInfo
    {
      var attributes = s_customAttributeDataRetriever.GetCustomAttributeData (member);
      if (!inherit)
        return attributes.ToList().AsReadOnly();

      var baseMember = baseMemberProvider (member);
      if (baseMember == null)
        return attributes.ToList().AsReadOnly();

      var inheritedAttributes = GetCachedAttributes (baseMember, true, baseMemberProvider)
          .Where (a => AttributeUtility.IsAttributeInherited (a.Type));

      var allAttributes = attributes.Concat (inheritedAttributes);
      return EvaluateAllowMultiple (allAttributes).ToList().AsReadOnly();
    }

    private static IEnumerable<ICustomAttributeData> EvaluateAllowMultiple (IEnumerable<ICustomAttributeData> attributesFromDerivedToBase)
    {
      var encounteredAttributeTypes = new HashSet<Type>();
      foreach (var data in attributesFromDerivedToBase)
      {
        if (!encounteredAttributeTypes.Contains (data.Type) || AttributeUtility.IsAttributeAllowMultiple (data.Type))
          yield return data;

        encounteredAttributeTypes.Add (data.Type);
      }
    }
  }
}