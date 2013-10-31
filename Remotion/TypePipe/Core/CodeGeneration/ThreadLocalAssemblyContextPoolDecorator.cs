using System;
using System.Runtime.Remoting.Messaging;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration
{
  /// <summary>
  /// Decorates the <see cref="IAssemblyContextPool"/> with a thread-local cache. This allows the reuse of the same assembly context within a thread.
  /// </summary>
  public class ThreadLocalAssemblyContextPoolDecorator : IAssemblyContextPool
  {
    private class ThreadLocalAssemblyContext
    {
      private readonly AssemblyContext _value;
      private int _count;

      public ThreadLocalAssemblyContext (AssemblyContext value)
      {
        _value = value;
      }

      public AssemblyContext Value
      {
        get { return _value; }
      }

      public int Count
      {
        get { return _count; }
      }

      public void IncrementCount ()
      {
        _count++;
      }

      public void DecrementCount ()
      {
        Assertion.IsTrue (_count > 0, "Count can never be decremented past 0.");
        _count--;
      }
    }

    private readonly string _instanceID = Guid.NewGuid().ToString();
    private readonly IAssemblyContextPool _assemblyContextPool;

    public ThreadLocalAssemblyContextPoolDecorator (IAssemblyContextPool assemblyContextPool)
    {
      _assemblyContextPool = assemblyContextPool;
    }

    public void Enqueue (AssemblyContext assemblyContext)
    {
      ArgumentUtility.CheckNotNull ("assemblyContext", assemblyContext);

      var threadLocalAssemblyContext = (ThreadLocalAssemblyContext) CallContext.GetData (_instanceID);
      if (threadLocalAssemblyContext == null)
      {
        throw new InvalidOperationException (
            "No AssemblyContext has been dequeued on the current thread. "
            + "An AssemblyContext must be enqueued on the same thread it was dequeued from.");
      }

      if (threadLocalAssemblyContext.Value != assemblyContext)
      {
        throw new InvalidOperationException (
            "The specified AssemblyContext has been dequeued on a different thread. "
            + "An AssemblyContext must be enqueued on the same thread it was dequeued from.");
      }

      threadLocalAssemblyContext.DecrementCount();

      if (threadLocalAssemblyContext.Count == 0)
      {
        try
        {
          _assemblyContextPool.Enqueue (assemblyContext);
        }
        catch
        {
          threadLocalAssemblyContext.IncrementCount();
          throw;
        }

        CallContext.FreeNamedDataSlot (_instanceID);
      }
    }

    public AssemblyContext Dequeue ()
    {
      var threadLocalAssemblyContext = (ThreadLocalAssemblyContext) CallContext.GetData (_instanceID);
      if (threadLocalAssemblyContext == null)
      {
        var assemblyContext = _assemblyContextPool.Dequeue();
        threadLocalAssemblyContext = new ThreadLocalAssemblyContext (assemblyContext);
        CallContext.SetData (_instanceID, threadLocalAssemblyContext);
      }

      threadLocalAssemblyContext.IncrementCount();
      return threadLocalAssemblyContext.Value;
    }

    public AssemblyContext[] DequeueAll ()
    {
      return _assemblyContextPool.DequeueAll();
    }
  }
}