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

    public IEnumerable<FieldInfo> SelectFields (IEnumerable<FieldInfo> fields, BindingFlags bindingAttr)
    {
      ArgumentUtility.CheckNotNull ("fields", fields);

      return fields.Where (field => _bindingFlagsEvaluator.HasRightAttributes (field.Attributes, bindingAttr));
    }

    public IEnumerable<T> SelectMethods<T> (IEnumerable<T> methods, BindingFlags bindingAttr, Type declaringType)
        where T : MethodBase
    {
      ArgumentUtility.CheckNotNull ("methods", methods);
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);

      var candidates = methods.Where (m => _bindingFlagsEvaluator.HasRightAttributes (m.Attributes, bindingAttr));
      if ((bindingAttr & BindingFlags.DeclaredOnly) == BindingFlags.DeclaredOnly)
        candidates = candidates.Where (m => declaringType == m.DeclaringType);

      return candidates;
    }

    public IEnumerable<PropertyInfo> SelectProperties (IEnumerable<PropertyInfo> properties, BindingFlags bindingAttr)
    {
      ArgumentUtility.CheckNotNull ("properties", properties);

      return properties.Where (p => p.GetAccessors (true).Any (a => _bindingFlagsEvaluator.HasRightAttributes (a.Attributes, bindingAttr)));
    }

    public FieldInfo SelectSingleField (IEnumerable<FieldInfo> fields, BindingFlags bindingAttr, string name)
    {
      ArgumentUtility.CheckNotNull ("fields", fields);

      var candidates = fields.Where (fi => fi.Name == name);

      var message = string.Format ("Ambiguous field name '{0}'.", name);
      return SelectFields (candidates, bindingAttr).SingleOrDefault (() => new AmbiguousMatchException (message));
    }

    public T SelectSingleMethod<T> (
        IEnumerable<T> methods,
        Binder binder,
        BindingFlags bindingAttr,
        string nameOrNull,
        Type declaringType,
        Type[] typesOrNull,
        ParameterModifier[] modifiersOrNull)
        where T : MethodBase
    {
      ArgumentUtility.CheckNotNull ("methods", methods);
      ArgumentUtility.CheckNotNull ("binder", binder);
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);

      if (typesOrNull == null && modifiersOrNull != null)
        throw new ArgumentException ("Modifiers must not be specified if types are null.", "modifiersOrNull");

      if (nameOrNull != null)
        methods = methods.Where (m => m.Name == nameOrNull);

      var candidates = SelectMethods (methods, bindingAttr, declaringType).ToArray();
      if (candidates.Length == 0)
        return null;

      if (typesOrNull == null)
      {
        if (candidates.Length > 1)
        {
          var message = string.Format ("Ambiguous method name '{0}'.", nameOrNull);
          throw new AmbiguousMatchException (message);
        }

        return candidates.Single();
      }

      return (T) binder.SelectMethod (bindingAttr, candidates, typesOrNull, modifiersOrNull);
    }
  }
}