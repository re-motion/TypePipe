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
using System.Collections.ObjectModel;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.FutureReflection
{
  /// <summary>
  /// Represents a <see cref="Type"/> that can be changed. Changes are recorded and, depending on the concrete <see cref="MutableType"/>, applied
  /// to an existing type or to a newly created type.
  /// </summary>
  public abstract class MutableType : Type
  {
    private readonly List<Type> _addedInterfaces = new List<Type>();
    private readonly List<FieldInfo> _addedFields = new List<FieldInfo>();

    private readonly List<FutureConstructorInfo> _addedConstructors = new List<FutureConstructorInfo> ();

    public ReadOnlyCollection<Type> AddedInterfaces
    {
      get { return _addedInterfaces.AsReadOnly(); }
    }

    public ReadOnlyCollection<FieldInfo> AddedFields
    {
      get { return _addedFields.AsReadOnly(); }
    }

    public ReadOnlyCollection<FutureConstructorInfo> AddedConstructors
    {
      get { return _addedConstructors.AsReadOnly (); }
    }

    public void AddInterface (Type interfaceType)
    {
      ArgumentUtility.CheckNotNull ("interfaceType", interfaceType);

      if (!interfaceType.IsInterface)
        throw new ArgumentException ("Type must be an interface.", "interfaceType");

      // TODO: Ensure that interfaces + addedInterfaces does no already contain 'interfaceType'

      _addedInterfaces.Add (interfaceType);
    }

    public FieldInfo AddField (string name, Type type, FieldAttributes attributes)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("type", type);

      // TODO: Ensure that fields + addedFields so not already contain this field

      var fieldInfo = new FutureFieldInfo (this, name, type, attributes);
      _addedFields.Add (fieldInfo);

      return fieldInfo;
    }

    // TODO: Don't take a futureconstructorinfo but a field type
    public void AddConstructor (FutureConstructorInfo futureConstructorInfo)
    {
      ArgumentUtility.CheckNotNull ("futureConstructorInfo", futureConstructorInfo);

      // TODO: Ensure that constructors + addedConstructors does not contain a constructor with same signature

      _addedConstructors.Add (futureConstructorInfo);
    }
  }
}