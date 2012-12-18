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
  // TODO Review: Rename to MutableInfoCustomAttributeContainer
  /// <summary>
  /// A helper class that is used to implement custom <see cref="Attribute"/>-related members of <see cref="IMutableInfo"/> on mutable reflection
  /// objects.
  /// </summary>
  public class MutableInfoCustomAttributeHelper
  {
    // TODO Review: Remove
    private readonly IMutableInfo _mutableInfo;
    // TODO 5057: Use Lazy<T>
    private readonly DoubleCheckedLockingContainer<ReadOnlyCollection<ICustomAttributeData>> _existingCustomAttributeDatas;
    private readonly Func<bool> _canAddCustomAttributesDecider;

    private readonly List<CustomAttributeDeclaration> _addedCustomAttributeDeclarations = new List<CustomAttributeDeclaration>();

    public MutableInfoCustomAttributeHelper (
        IMutableInfo mutableInfo, Func<ReadOnlyCollection<ICustomAttributeData>> customAttributeDataProvider, Func<bool> canAddCustomAttributesDecider)
    {
      ArgumentUtility.CheckNotNull ("mutableInfo", mutableInfo);
      ArgumentUtility.CheckNotNull ("customAttributeDataProvider", customAttributeDataProvider);
      ArgumentUtility.CheckNotNull ("canAddCustomAttributesDecider", canAddCustomAttributesDecider);

      _mutableInfo = mutableInfo;
      _existingCustomAttributeDatas = new DoubleCheckedLockingContainer<ReadOnlyCollection<ICustomAttributeData>> (customAttributeDataProvider);
      _canAddCustomAttributesDecider = canAddCustomAttributesDecider;
    }

    public ReadOnlyCollection<CustomAttributeDeclaration> AddedCustomAttributeDeclarations
    {
      get { return _addedCustomAttributeDeclarations.AsReadOnly(); }
    }

    public void AddCustomAttribute (CustomAttributeDeclaration customAttributeDeclaration)
    {
      ArgumentUtility.CheckNotNull ("customAttributeDeclaration", customAttributeDeclaration);

      if (!_canAddCustomAttributesDecider())
        throw new NotSupportedException ("Adding custom attributes to this element is not supported.");

      _addedCustomAttributeDeclarations.Add (customAttributeDeclaration);
    }

    public IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      return _addedCustomAttributeDeclarations.Cast<ICustomAttributeData>().Concat (_existingCustomAttributeDatas.Value);
    }

    // TODO Review: Remove, inline at call site
    public object[] GetCustomAttributes (bool inherit)
    {
      return TypePipeCustomAttributeImplementationUtility.GetCustomAttributes (_mutableInfo, inherit);
    }

    public object[] GetCustomAttributes (Type attributeType, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("attributeType", attributeType);

      return TypePipeCustomAttributeImplementationUtility.GetCustomAttributes (_mutableInfo, attributeType, inherit);
    }

    public bool IsDefined (Type attributeType, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("attributeType", attributeType);

      return TypePipeCustomAttributeImplementationUtility.IsDefined (_mutableInfo, attributeType, inherit);
    }
  }
}