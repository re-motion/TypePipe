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
using Remotion.Utilities;

namespace Remotion
{
  public class CallContextSingleton<T>
  {
    private readonly object _callContextLock = new object ();
    private readonly string _callContextKey;
    private readonly Func<T> _creator;

    public CallContextSingleton(string callContextKey, Func<T> creator)
    {
      ArgumentUtility.CheckNotNull ("callContextKey", callContextKey);
      ArgumentUtility.CheckNotNull ("creator", creator);

      _callContextKey = callContextKey;
      _creator = creator;
    }

    public bool HasCurrent
    {
      get
      {
        lock (_callContextLock)
        {
          return GetCurrentInternal() != null;
        }
      }
    }

    public T Current
    {
      get
      {
        lock (_callContextLock)
        {
          if (!HasCurrent)
            SetCurrent (_creator());

          return GetCurrentInternal();
        }
      }
    }

    public void SetCurrent (T value)
    {
      lock (_callContextLock)
      {
        CallContext.SetData (_callContextKey, value);
      }
    }

    private T GetCurrentInternal ()
    {
      return (T) CallContext.GetData (_callContextKey);
    }
  }
}
