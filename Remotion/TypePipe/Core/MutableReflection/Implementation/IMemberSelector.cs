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
  /// Defines an interface for objects selecting members based on <see cref="BindingFlags"/> and other criteria.
  /// </summary>
  public interface IMemberSelector
  {
    IEnumerable<FieldInfo> SelectFields (IEnumerable<FieldInfo> fields, BindingFlags bindingAttr, Type declaringType);
    IEnumerable<T> SelectMethods<T> (IEnumerable<T> methods, BindingFlags bindingAttr, Type declaringType)
        where T : MethodBase;
    IEnumerable<PropertyInfo> SelectProperties (IEnumerable<PropertyInfo> properties, BindingFlags bindingAttr, Type declaringType);

    FieldInfo SelectSingleField (IEnumerable<FieldInfo> fields, BindingFlags bindingAttr, string name, Type declaringType);

    T SelectSingleMethod<T> (
        IEnumerable<T> methods,
        Binder binder,
        BindingFlags bindingAttr,
        string nameOrNull,
        Type declaringType,
        Type[] typesOrNull,
        ParameterModifier[] modifiersOrNull)
        where T: MethodBase;

    PropertyInfo SelectSingleProperty (IEnumerable<PropertyInfo> properties, BindingFlags bindingAttr, string name, Type declaringType);
  }
}