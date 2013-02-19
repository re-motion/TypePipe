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
using System.Linq;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// A helper class that is used to implement custom <see cref="Attribute"/>-related members of <see cref="IMutableMember"/> on mutable reflection
  /// objects.
  /// </summary>
  public class CustomAttributeContainer
  {
    private readonly List<CustomAttributeDeclaration> _addedCustomAttributes = new List<CustomAttributeDeclaration>();

    public ReadOnlyCollection<CustomAttributeDeclaration> AddedCustomAttributes
    {
      get { return _addedCustomAttributes.AsReadOnly(); }
    }

    public void AddCustomAttribute (CustomAttributeDeclaration customAttribute)
    {
      ArgumentUtility.CheckNotNull ("customAttribute", customAttribute);

      if (_addedCustomAttributes.Any (a => a.Type == customAttribute.Type && !AttributeUtility.IsAttributeAllowMultiple (a.Type)))
      {
        var message = string.Format ("Attribute of type '{0}' (with AllowMultiple = false) is already present.", customAttribute.Type.Name);
        throw new InvalidOperationException (message);
      }

      _addedCustomAttributes.Add (customAttribute);
    }
  }
}