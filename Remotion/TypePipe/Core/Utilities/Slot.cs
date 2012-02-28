// This file is part of the re-motion TypePipe project (typepipe.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-motion TypePipe is free software; you can redistribute it 
// and/or modify it under the terms of the Apache License, Version 2.0
// as published by the Apache Software Foundation.
// 
// re-motion TypePipe is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// Apache License, Version 2.0 for more details.
// 
// You should have received a copy of the Apache License, Version 2.0
// along with re-motion; if not, see http://www.apache.org/licenses.
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