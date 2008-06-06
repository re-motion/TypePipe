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

namespace Remotion
{
  // TODO: Remove finalizer, check derivations whether they really need a finalizer (unmanaged resources only); document the change
  /// <summary>
  /// This class can be used as a base class for any class that needs to implement <see cref="IDisposable"/>, but does not want to 
  /// expose a public <c>Dispose</c> method.
  /// <see cref="DisposableBase"/>
  /// </summary>
  [Serializable]
  public abstract class DisposableExplicitBase : IDisposable
  {
    private bool _disposed = false;

    protected abstract void Dispose (bool disposing);

    void IDisposable.Dispose()
    {
      Dispose();
    }

    protected void Dispose()
    {
      if (! _disposed)
      {
        Dispose (true);
        GC.SuppressFinalize (this);
        _disposed = true;
      }
    }

    ~DisposableExplicitBase()
    {
      Dispose (false);
    }

    protected bool Disposed
    { 
      get { return _disposed; }
    }

    protected void AssertNotDisposed ()
    {
      if (_disposed)
        throw new InvalidOperationException ("Object disposed.");
    }

    protected void Resurrect ()
    {
      if (_disposed)
      {
        _disposed = false;
        GC.ReRegisterForFinalize (this);
      }
    }
  }

  /// <summary>
  /// This class can be used as a base class for any class that needs to implement <see cref="IDisposable"/>.
  /// <see cref="DisposableExplicitBase"/>
  /// </summary>
  [Serializable]
  public abstract class DisposableBase: DisposableExplicitBase
  {
    public new void Dispose()
    {
      base.Dispose();
    }
  }

}
