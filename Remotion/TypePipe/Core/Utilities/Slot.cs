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
using System.Diagnostics;

namespace Remotion.TypePipe.Utilities
{
  [DebuggerStepThrough]
  internal static class Slot
  {
    public static Slot<T> New<T> (string itemName = "Item", bool allowsNull = false)
    {
      return new Slot<T> (itemName, allowsNull);
    }

    // Different method name to prevent confusion in case T == typeof(string)
    public static Slot<T> WithDefault<T> (T defaultIfItemNotSet, string itemName = "Item", bool allowsNull = false)
    {
      return new Slot<T> (itemName, allowsNull, defaultIfItemNotSet);
    }
  }

  [DebuggerStepThrough]
  internal class Slot<T>
  {
    private readonly string _itemName;
    private T _item;

    internal Slot (string itemName, bool allowsNull)
    {
      _itemName = itemName;
      AllowsNull = allowsNull;
    }

    internal Slot (string itemName, bool allowsNull, T defaultIfItemNotSet)
        : this (itemName, allowsNull)
    {
      _item = defaultIfItemNotSet;
      HasDefault = true;
    }

    public bool AllowsNull { get; private set; }
    public bool HasItem { get; private set; }
    public bool HasDefault { get; private set; }

    public bool CanGet
    {
      get { return HasItem || HasDefault; }
    }

    public T Get ()
    {
      if (!CanGet)
        throw new InvalidOperationException (_itemName + " not set");

      return _item;
    }

    public void Set (T item)
    {
      // ReSharper disable CompareNonConstrainedGenericWithNull
      var isItemNull = item == null;
      // ReSharper restore CompareNonConstrainedGenericWithNull

      if (isItemNull && !AllowsNull)
        throw new ArgumentNullException (_itemName);

      if (HasItem)
        throw new InvalidOperationException (_itemName + " already set");

      _item = item;
      HasItem = true;
    }
  }
}