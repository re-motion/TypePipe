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
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// A helper class that is used to implement custom <see cref="Attribute"/>-related members of <see cref="IMutableInfo"/> on mutable reflection
  /// objects.
  /// </summary>
  public class MutableInfoCustomAttributeHelper
  {
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

    public object[] GetCustomAttributes (bool inherit)
    {
      return GetCustomAttributes (GetCustomAttributeDataForMutableInfo (inherit), typeof (object));
    }

    public object[] GetCustomAttributes (Type attributeType, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("attributeType", attributeType);

      return GetCustomAttributes (GetCustomAttributeDataForMutableInfo (inherit), attributeType);
    }

    public bool IsDefined (Type attributeType, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("attributeType", attributeType);

      return IsDefined (GetCustomAttributeDataForMutableInfo (inherit), attributeType);
    }

    private IEnumerable<ICustomAttributeData> GetCustomAttributeDataForMutableInfo (bool inherit)
    {
      Assertion.IsTrue (_mutableInfo is MemberInfo || (_mutableInfo is ParameterInfo && !inherit));

      return _mutableInfo is MemberInfo
                 ? TypePipeCustomAttributeData.GetCustomAttributes ((MemberInfo) _mutableInfo, inherit)
                 : TypePipeCustomAttributeData.GetCustomAttributes ((ParameterInfo) _mutableInfo);
    }

    private static object[] GetCustomAttributes (IEnumerable<ICustomAttributeData> customAttributeDatas, Type attributeType)
    {
      var attributeArray = customAttributeDatas
          .Where (a => attributeType.IsAssignableFrom (a.Type))
          .Select (a => a.CreateInstance())
          .ToArray();

      if (attributeArray.GetType().GetElementType() != attributeType)
      {
        var typedAttributeArray = Array.CreateInstance (attributeType, attributeArray.Length);
        Array.Copy (attributeArray, typedAttributeArray, attributeArray.Length);
        attributeArray = (object[]) typedAttributeArray;
      }

      return attributeArray;
    }

    private static bool IsDefined (IEnumerable<ICustomAttributeData> customAttributeDatas, Type attributeType)
    {
      return customAttributeDatas.Any (a => attributeType.IsAssignableFrom (a.Type));
    }
  }
}