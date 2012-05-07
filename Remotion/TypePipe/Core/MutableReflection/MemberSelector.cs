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

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Selects members based on <see cref="BindingFlags"/> and other criteria. This is used to implement member access operations in 
  /// <see cref="MutableType"/>.
  /// </summary>
  public class MemberSelector : IMemberSelector
  {
    private readonly IBindingFlagsEvaluator _bindingFlagsEvaluator;

    public MemberSelector (IBindingFlagsEvaluator bindingFlagsEvaluator)
    {
      ArgumentUtility.CheckNotNull ("bindingFlagsEvaluator", bindingFlagsEvaluator);

      _bindingFlagsEvaluator = bindingFlagsEvaluator;
    }

    public IEnumerable<T> SelectMethods<T> (IEnumerable<T> candidates, BindingFlags bindingAttr, MutableType declaringType)
        where T : MethodBase
    {
      ArgumentUtility.CheckNotNull ("candidates", candidates);
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);

      var methods = candidates.Where (method => _bindingFlagsEvaluator.HasRightAttributes (method.Attributes, bindingAttr));
      if ((bindingAttr & BindingFlags.DeclaredOnly) == BindingFlags.DeclaredOnly)
        methods = methods.Where (method => declaringType.IsEquivalentTo(method.DeclaringType));

      return methods;
    }

    public IEnumerable<FieldInfo> SelectFields (IEnumerable<FieldInfo> candidates, BindingFlags bindingAttr)
    {
      ArgumentUtility.CheckNotNull ("candidates", candidates);

      return candidates.Where (field => _bindingFlagsEvaluator.HasRightAttributes (field.Attributes, bindingAttr));
    }

    public T SelectSingleMethod<T> (
        IEnumerable<T> methods,
        Binder binder,
        BindingFlags bindingAttr,
        string name,
        MutableType declaringType,
        Type[] typesOrNull,
        ParameterModifier[] modifiersOrNull)
        where T: MethodBase
    {
      ArgumentUtility.CheckNotNull ("binder", binder);
      ArgumentUtility.CheckNotNull ("methods", methods);
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);

      if (typesOrNull == null && modifiersOrNull != null)
        throw new ArgumentException ("Modifiers must not be specified if types are null.", "modifiersOrNull");

      var methodsFilteredByName = methods.Where (mi => mi.Name == name);
      var candidates = SelectMethods (methodsFilteredByName, bindingAttr, declaringType).ToArray ();
      if (candidates.Length == 0)
        return null;

      if (typesOrNull == null)
      {
        if (candidates.Length > 1)
        {
          var message = string.Format ("Ambiguous method name '{0}'.", name);
          throw new AmbiguousMatchException (message);
        }

        return candidates.Single();
      }

      return (T) binder.SelectMethod (bindingAttr, candidates, typesOrNull, modifiersOrNull);
    }

    public FieldInfo SelectSingleField (IEnumerable<FieldInfo> fields, BindingFlags bindingAttr, string name)
    {
      ArgumentUtility.CheckNotNull ("fields", fields);

      var candidates = fields.Where (fi => fi.Name == name);

      var message = string.Format ("Ambiguous field name '{0}'.", name);
      return SelectFields (candidates, bindingAttr).SingleOrDefault (() => new AmbiguousMatchException (message));
    }
  }
}