/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Runtime.Remoting.Messaging;
using Remotion.Context;
using Remotion.Utilities;

namespace Remotion
{
  public class SafeContextSingleton<T>
  {
    private readonly object _currentLock = new object ();
    private readonly string _currentKey;
    private readonly Func<T> _creator;

    public SafeContextSingleton(string currentKey, Func<T> creator)
    {
      ArgumentUtility.CheckNotNull ("currentKey", currentKey);
      ArgumentUtility.CheckNotNull ("creator", creator);

      _currentKey = currentKey;
      _creator = creator;
    }

    public bool HasCurrent
    {
      get
      {
        lock (_currentLock)
        {
          return GetCurrentInternal() != null;
        }
      }
    }

    public T Current
    {
      get
      {
        lock (_currentLock)
        {
          if (!HasCurrent)
            SetCurrent (_creator());

          return GetCurrentInternal();
        }
      }
    }

    public void SetCurrent (T value)
    {
      lock (_currentLock)
      {
        SafeContext.Instance.SetData (_currentKey, value);
      }
    }

    private T GetCurrentInternal ()
    {
      return (T) SafeContext.Instance.GetData (_currentKey);
    }
  }
}
