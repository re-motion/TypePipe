// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
  /// <threadsafety>
  /// The data managed by this class is held in the <see cref="SafeContext"/> and therefore thread-local. The class is safe to be used from multiple
  /// threads at the same time, but each thread will have its own copy of the data.
  /// </threadsafety>
  public class SafeContextSingleton<T> where T : class
  {
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
        return GetCurrentInternal() != null;
      }
    }

    public T Current
    {
      get
      {
        // Performancetuning: SafeContext.Instance.GetData is quite expensive, so only called once
        T current = GetCurrentInternal();
          
        if (current == null)
        {
          current = _creator();
          SetCurrent(current);
        }

        return current;
      }
    }

    public void SetCurrent (T value)
    {
      SafeContext.Instance.SetData (_currentKey, value);
    }

    private T GetCurrentInternal ()
    {
      return (T) SafeContext.Instance.GetData (_currentKey);
    }
  }
}
