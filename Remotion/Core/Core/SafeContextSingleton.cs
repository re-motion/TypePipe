// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Runtime.Remoting.Messaging;
using Remotion.Context;
using Remotion.Utilities;

namespace Remotion
{
  /// <summary>
  /// Provides automatic "Singleton" behavior based on the <see cref="SafeContext"/> class.
  /// </summary>
  /// <typeparam name="T">The type for which a single instance should be held in the <see cref="SafeContext"/>.</typeparam>
  /// <remarks>
  /// This class stores a single instance of <typeparamref name="T"/> in the <see cref="SafeContext"/>. Use it to ensure that exactly one 
  /// instance of <typeparamref name="T"/> exists per thread, web context, or the respective current <see cref="SafeContext"/> policy.
  /// </remarks>
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
