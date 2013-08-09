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
using Remotion.ServiceLocation;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;
using Remotion.FunctionalProgramming;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Represents the TypePipe counterpart of <see cref="CustomAttributeData"/>.
  /// Can be used to retrieve attribute data from <see cref="MemberInfo"/>s and <see cref="ParameterInfo"/>s.
  /// </summary>
  /// <remarks>
  /// The implementation is based on an instance of <see cref="ICustomAttributeDataRetriever"/> which is requested via 
  /// the <see cref="SafeServiceLocator"/>.
  /// </remarks>
  public static class TypePipeCustomAttributeData
  {
    private static readonly ICustomAttributeDataRetriever s_customAttributeDataRetriever =
        SafeServiceLocator.Current.GetInstance<ICustomAttributeDataRetriever>();

    private static readonly IRelatedMethodFinder s_relatedMethodFinder = new RelatedMethodFinder();
    private static readonly IRelatedPropertyFinder s_relatedPropertyFinder = new RelatedPropertyFinder();
    private static readonly IRelatedEventFinder s_relatedEventFinder = new RelatedEventFinder();

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
          return s_customAttributeDataRetriever.GetCustomAttributeData (member);
      }
    }

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (Type type, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("type", type);

      return GetCustomAttributes (type, inherit, t => t.BaseType);
    }

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (FieldInfo field)
    {
      ArgumentUtility.CheckNotNull ("field", field);

      return s_customAttributeDataRetriever.GetCustomAttributeData (field);
    }

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (ConstructorInfo constructor)
    {
      ArgumentUtility.CheckNotNull ("constructor", constructor);

      return s_customAttributeDataRetriever.GetCustomAttributeData (constructor);
    }

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (MethodInfo method, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("method", method);

      return GetCustomAttributes (method, inherit, s_relatedMethodFinder.GetBaseMethod);
    }

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (PropertyInfo property, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("property", property);

      return GetCustomAttributes (property, inherit, s_relatedPropertyFinder.GetBaseProperty);
    }

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (EventInfo @event, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("event", @event);

      return GetCustomAttributes (@event, inherit, s_relatedEventFinder.GetBaseEvent);
    }

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (ParameterInfo parameter)
    {
      ArgumentUtility.CheckNotNull ("parameter", parameter);

      return s_customAttributeDataRetriever.GetCustomAttributeData (parameter);
    }

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (Assembly assembly)
    {
      ArgumentUtility.CheckNotNull ("assembly", assembly);

      return s_customAttributeDataRetriever.GetCustomAttributeData (assembly);
    }

    public static IEnumerable<ICustomAttributeData> GetCustomAttributes (Module module)
    {
      ArgumentUtility.CheckNotNull ("module", module);

      return s_customAttributeDataRetriever.GetCustomAttributeData (module);
    }

    private static IEnumerable<ICustomAttributeData> GetCustomAttributes<T> (T member, bool inherit, Func<T, T> baseMemberProvider)
        where T : MemberInfo
    {
      // TODO 5794
      //ConcurrentDictionary<MemberInfo, IEnumerable<ICustomAttributeData>> d;
      //if (!d.TryGetValue (member, out result))
      //{
      //  result = d.GetOrAdd (member, key => (T) key)
      //}

      var attributes = s_customAttributeDataRetriever.GetCustomAttributeData (member);
      if (!inherit)
        return attributes;

      var baseMember = baseMemberProvider (member); // Base member may be null, which is ok.
      var inheritedAttributes = baseMember
          .CreateSequence (baseMemberProvider)
          .SelectMany (s_customAttributeDataRetriever.GetCustomAttributeData)
          .Where (d => AttributeUtility.IsAttributeInherited (d.Type));

      var allAttributesWithInheritance = attributes.Concat (inheritedAttributes);
      return EvaluateAllowMultiple (allAttributesWithInheritance);
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