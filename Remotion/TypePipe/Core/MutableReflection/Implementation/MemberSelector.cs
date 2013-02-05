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
using Remotion.FunctionalProgramming;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// Selects members based on <see cref="BindingFlags"/> and other criteria. This is used to implement member access operations in 
  /// <see cref="ProxyType"/>.
  /// </summary>
  public class MemberSelector : IMemberSelector
  {
    private readonly IBindingFlagsEvaluator _bindingFlagsEvaluator;

    public MemberSelector (IBindingFlagsEvaluator bindingFlagsEvaluator)
    {
      ArgumentUtility.CheckNotNull ("bindingFlagsEvaluator", bindingFlagsEvaluator);

      _bindingFlagsEvaluator = bindingFlagsEvaluator;
    }

    public IEnumerable<FieldInfo> SelectFields (IEnumerable<FieldInfo> fields, BindingFlags bindingAttr, Type declaringType)
    {
      ArgumentUtility.CheckNotNull ("fields", fields);
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);

      return FilterByFlags (fields, bindingAttr, declaringType, f => _bindingFlagsEvaluator.HasRightAttributes (f.Attributes, bindingAttr));
    }

    public IEnumerable<T> SelectMethods<T> (IEnumerable<T> methods, BindingFlags bindingAttr, Type declaringType)
        where T : MethodBase
    {
      ArgumentUtility.CheckNotNull ("methods", methods);
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);

      return FilterByFlags (methods, bindingAttr, declaringType, m => _bindingFlagsEvaluator.HasRightAttributes (m.Attributes, bindingAttr));
    }

    public IEnumerable<PropertyInfo> SelectProperties (IEnumerable<PropertyInfo> properties, BindingFlags bindingAttr, Type declaringType)
    {
      ArgumentUtility.CheckNotNull ("properties", properties);
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);

      Func<PropertyInfo, bool> predicate = p => p.GetAccessors (true).Any (a => _bindingFlagsEvaluator.HasRightAttributes (a.Attributes, bindingAttr));
      return FilterByFlags (properties, bindingAttr, declaringType, predicate);
    }

    public IEnumerable<EventInfo> SelectEvents (IEnumerable<EventInfo> events, BindingFlags bindingAttr, Type declaringType)
    {
      ArgumentUtility.CheckNotNull ("events", events);
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);

      Func<EventInfo, bool> predicate = e => _bindingFlagsEvaluator.HasRightAttributes (e.GetAddMethod (true).Attributes, bindingAttr);
      return FilterByFlags (events, bindingAttr, declaringType, predicate);
    }

    public FieldInfo SelectSingleField (IEnumerable<FieldInfo> fields, BindingFlags bindingAttr, string name, Type declaringType)
    {
      ArgumentUtility.CheckNotNull ("fields", fields);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);

      return SelectSingle (fields, name, bindingAttr, declaringType, SelectFields, "field");
    }

    public T SelectSingleMethod<T> (
        IEnumerable<T> methods,
        Binder binder,
        BindingFlags bindingAttr,
        string nameOrNull,
        Type declaringType,
        Type[] parameterTypesOrNull,
        ParameterModifier[] modifiersOrNull)
        where T : MethodBase
    {
      ArgumentUtility.CheckNotNull ("methods", methods);
      ArgumentUtility.CheckNotNull ("binder", binder);
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      CheckModifiers (parameterTypesOrNull, modifiersOrNull);

      if (nameOrNull != null)
        methods = methods.Where (m => m.Name == nameOrNull);

      var candidates = SelectMethods (methods, bindingAttr, declaringType).ToArray();
      if (candidates.Length == 0)
        return null;

      if (parameterTypesOrNull == null)
      {
        if (candidates.Length > 1)
        {
          var message = string.Format ("Ambiguous method name '{0}'.", nameOrNull);
          throw new AmbiguousMatchException (message);
        }

        return candidates.Single();
      }

      // DefaultBinder does *not* support null 'parameterTypes'.
      return (T) binder.SelectMethod (bindingAttr, candidates, parameterTypesOrNull, modifiersOrNull);
    }

    public PropertyInfo SelectSingleProperty (
        IEnumerable<PropertyInfo> properties,
        Binder binder,
        BindingFlags bindingAttr,
        string name,
        Type declaringType,
        Type propertyTypeOrNull,
        Type[] indexerTypesOrNull,
        ParameterModifier[] modifiersOrNull)
    {
      ArgumentUtility.CheckNotNull ("properties", properties);
      ArgumentUtility.CheckNotNull ("binder", binder);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);

      CheckModifiers (indexerTypesOrNull, modifiersOrNull);

      var byName = properties.Where (p => p.Name == name);
      var candidates = SelectProperties (byName, bindingAttr, declaringType).ToArray();
      if (candidates.Length == 0)
        return null;

      // DefaultBinder does support null 'propertyType' and/or 'indexerTypes'.
      return binder.SelectProperty (bindingAttr, candidates, propertyTypeOrNull, indexerTypesOrNull, modifiersOrNull);
    }

    public EventInfo SelectSingleEvent (IEnumerable<EventInfo> events, BindingFlags bindingAttr, string name, Type declaringType)
    {
      ArgumentUtility.CheckNotNull ("events", events);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);

      return null;
    }

    private IEnumerable<T> FilterByFlags<T> (IEnumerable<T> candidates, BindingFlags bindingAttr, Type declaringType, Func<T, bool> predicate)
        where T : MemberInfo
    {
      if ((bindingAttr & BindingFlags.DeclaredOnly) == BindingFlags.DeclaredOnly)
        candidates = candidates.Where (m => m.DeclaringType == declaringType);

      return candidates.Where (predicate);
    }

    private T SelectSingle<T> (
        IEnumerable<T> members,
        string name,
        BindingFlags bindingAttr,
        Type declaringType,
        Func<IEnumerable<T>, BindingFlags, Type, IEnumerable<T>> selectMembers,
        string memberKind)
        where T : MemberInfo
    {
      var byName = members.Where (m => m.Name == name);
      var byFlags = selectMembers (byName, bindingAttr, declaringType);

      return byFlags.SingleOrDefault (() => new AmbiguousMatchException (string.Format ("Ambiguous {0} name '{1}'.", memberKind, name)));
    }

    private void CheckModifiers (Type[] parameterTypes, ParameterModifier[] modifiers)
    {
      if (parameterTypes == null && modifiers != null)
        throw new ArgumentException ("Modifiers must not be specified if parameter types are null.", "modifiers");
    }
  }
}