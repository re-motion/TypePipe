using System;
using System.Threading;
using NUnit.Framework;
using Remotion.TypePipe.CodeGeneration;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration
{
  [TestFixture]
  public class ThreadLocalAssemblyContextPoolDecoratorTest
  {
    private IAssemblyContextPool _assemblyContextPoolMock;
    private ThreadLocalAssemblyContextPoolDecorator _decorator;
    private AssemblyContext _assemblyContext;

    [SetUp]
    public void SetUp ()
    {
      _assemblyContextPoolMock = MockRepository.GenerateStrictMock<IAssemblyContextPool>();
      _assemblyContext = CreateAssemblyContext();

      _decorator = new ThreadLocalAssemblyContextPoolDecorator (_assemblyContextPoolMock);
    }

    [Test]
    public void DequeueAll_ReturnsAssemblyContextsFromInnerPool ()
    {
      AssemblyContext[] expected = { _assemblyContext };
      _assemblyContextPoolMock.Expect (mock => mock.DequeueAll()).Return (expected);

      var actual = _decorator.DequeueAll();

      Assert.That (actual, Is.SameAs (expected));
    }

    [Test]
    public void DequeueAll_AfterDequeueAllNoEnqueue_ThrowsInvalidOperationException ()
    {
      var assemblyContexts = new[] { CreateAssemblyContext(), CreateAssemblyContext() };
      _assemblyContextPoolMock.Expect (mock => mock.DequeueAll()).Return (assemblyContexts);

      _decorator.DequeueAll();

      Assert.That (
          () => _decorator.DequeueAll(),
          Throws.InvalidOperationException.And.Message.EqualTo (
              "DequeueAll() cannot be invoked from the same thread as a previous call to DequeueAll() until all dequeued AssemblyContext have been returned to the pool."));
    }

    [Test]
    public void DequeueAll_AfterDequeueAndNoEnqueue_ThrowsInvalidOperationException ()
    {
      _assemblyContextPoolMock.Expect (mock => mock.Dequeue()).Return (_assemblyContext);

      _decorator.Dequeue();

      Assert.That (
          () => _decorator.DequeueAll(),
          Throws.InvalidOperationException.And.Message.EqualTo (
              "DequeueAll() cannot be invoked from the same thread as Dequeue() until all dequeued AssemblyContext have been returned to the pool."));
    }

    [Test]
    public void DequeueAll_AfterMultipleDequeueAndEnqueueNotComplete_ThrowsInvalidOperationException ()
    {
      _assemblyContextPoolMock.Expect (mock => mock.Dequeue()).Return (_assemblyContext);

      _decorator.Dequeue();
      _decorator.Dequeue();
      _decorator.Enqueue (_assemblyContext);

      Assert.That (
          () => _decorator.DequeueAll(),
          Throws.InvalidOperationException.And.Message.EqualTo (
              "DequeueAll() cannot be invoked from the same thread as Dequeue() until all dequeued AssemblyContext have been returned to the pool."));
    }

    [Test]
    public void Dequeue_Once_ReturnsAssemblyContextFromInnerPool ()
    {
      _assemblyContextPoolMock.Expect (mock => mock.Dequeue()).Return (_assemblyContext);

      var assemblyContext = _decorator.Dequeue();

      Assert.That (assemblyContext, Is.SameAs (_assemblyContext));
    }

    [Test]
    public void Dequeue_TwiceOnSameThread_ReturnsAssemblyContextFromThreadLocalCacheOnSecondCall ()
    {
      var hasDequeued = false;
      _assemblyContextPoolMock.Expect (mock => mock.Dequeue())
          .WhenCalled (
              mi =>
              {
                Assert.That (hasDequeued, Is.False);
                hasDequeued = true;
              })
          .Return (_assemblyContext);

      var assemblyContext1 = _decorator.Dequeue();
      Assert.That (hasDequeued, Is.True);

      hasDequeued = false;
      var assemblyContext2 = _decorator.Dequeue();
      Assert.That (hasDequeued, Is.False);

      Assert.That (assemblyContext2, Is.SameAs (assemblyContext1));
    }

    [Test]
    public void Dequeue_WithDifferentThreads_ReturnsDifferentAssemblyContexts ()
    {
      var expectedAssemblyContextFromOtherThread = CreateAssemblyContext();
      _assemblyContextPoolMock.Expect (mock => mock.Dequeue()).Return (expectedAssemblyContextFromOtherThread);
      _assemblyContextPoolMock.Expect (mock => mock.Dequeue()).Return (_assemblyContext);

      AssemblyContext actualAssemblyContextFromOtherThread = null;
      var otherThread = new Thread (() => { actualAssemblyContextFromOtherThread = _decorator.Dequeue(); });
      otherThread.Start();
      otherThread.Join (TimeSpan.FromSeconds (1));

      var actualAssemblyContextFromLocalThread = _decorator.Dequeue();

      Assert.That (actualAssemblyContextFromOtherThread, Is.SameAs (expectedAssemblyContextFromOtherThread));
      Assert.That (actualAssemblyContextFromLocalThread, Is.Not.SameAs (actualAssemblyContextFromOtherThread));
    }

    [Test]
    public void Dequeue_AfterDequeueAllAndEnqueueNotComplete_ThrowsInvalidOperationException ()
    {
      var assemblyContexts = new[] { CreateAssemblyContext(), CreateAssemblyContext() };

      _assemblyContextPoolMock.Expect (mock => mock.DequeueAll()).Return (assemblyContexts);
      _assemblyContextPoolMock.Expect (mock => mock.Enqueue (assemblyContexts[0]));

      _decorator.DequeueAll();
      _decorator.Enqueue (assemblyContexts[0]);

      Assert.That (
          () => _decorator.Dequeue(),
          Throws.InvalidOperationException.And.Message.EqualTo (
              "Dequeue() cannot be invoked from the same thread as DequeueAll() until all dequeued AssemblyContext have been returned to the pool."));
    }

    [Test]
    public void Dequeue_AfterDequeueAllAndNoEnqueue_ThrowsInvalidOperationException ()
    {
      var assemblyContexts = new[] { CreateAssemblyContext(), CreateAssemblyContext() };

      _assemblyContextPoolMock.Expect (mock => mock.DequeueAll()).Return (assemblyContexts);

      _decorator.DequeueAll();

      Assert.That (
          () => _decorator.Dequeue(),
          Throws.InvalidOperationException.And.Message.EqualTo (
              "Dequeue() cannot be invoked from the same thread as DequeueAll() until all dequeued AssemblyContext have been returned to the pool."));
    }

    [Test]
    public void Dequeue_AfterDequeueAllAndEnqueue ()
    {
      var assemblyContexts = new[] { CreateAssemblyContext(), CreateAssemblyContext() };

      _assemblyContextPoolMock.Expect (mock => mock.DequeueAll()).Return (assemblyContexts);
      _assemblyContextPoolMock.Expect (mock => mock.Enqueue (assemblyContexts[0]));
      _assemblyContextPoolMock.Expect (mock => mock.Enqueue (assemblyContexts[1]));
      _assemblyContextPoolMock.Expect (mock => mock.Dequeue()).Return (assemblyContexts[0]);

      _decorator.DequeueAll();

      _decorator.Enqueue (assemblyContexts[0]);
      _decorator.Enqueue (assemblyContexts[1]);

      var assemblyContext = _decorator.Dequeue();

      Assert.That (assemblyContext, Is.SameAs (assemblyContexts[0]));

      _assemblyContextPoolMock.VerifyAllExpectations();
    }

    [Test]
    public void Enqueue_AfterDequeue_PassesAssemblyContextToInnerPool ()
    {
      _assemblyContextPoolMock.Expect (mock => mock.Dequeue()).Return (_assemblyContext);
      _assemblyContextPoolMock.Expect (mock => mock.Enqueue (_assemblyContext));

      var assemblyContext = _decorator.Dequeue();

      _decorator.Enqueue (assemblyContext);

      _assemblyContextPoolMock.VerifyAllExpectations();
    }

    [Test]
    public void Enqueue_TwiceAfterTwoDequeues_PassesAssemblyContextToInnerPoolOnSecondCall ()
    {
      var hasEnqueued = false;
      _assemblyContextPoolMock.Expect (mock => mock.Dequeue()).Return (_assemblyContext);
      _assemblyContextPoolMock
          .Expect (mock => mock.Enqueue (_assemblyContext))
          .WhenCalled (
              mi =>
              {
                Assert.That (hasEnqueued, Is.False);
                hasEnqueued = true;
              });

      var assemblyContext1 = _decorator.Dequeue();
      var assemblyContext2 = _decorator.Dequeue();

      _decorator.Enqueue (assemblyContext1);
      Assert.That (hasEnqueued, Is.False);

      _decorator.Enqueue (assemblyContext2);
      Assert.That (hasEnqueued, Is.True);

      _assemblyContextPoolMock.VerifyAllExpectations();
    }

    [Test]
    public void Enqueue_WithoutDequeue_ThrowsInvalidOperationException ()
    {
      Assert.That (
          () => _decorator.Enqueue (_assemblyContext),
          Throws.InvalidOperationException.And.Message.EqualTo (
              "No AssemblyContext has been dequeued on the current thread. "
              + "An AssemblyContext must be enqueued on the same thread it was dequeued from."));
    }

    [Test]
    public void Enqueue_WithAssemblyContextFromOtherThread_ThrowsInvalidOperationException ()
    {
      _assemblyContextPoolMock.Expect (mock => mock.Dequeue()).Return (_assemblyContext);

      _decorator.Dequeue();

      var otherAssemblyContext = CreateAssemblyContext();

      Assert.That (
          () => _decorator.Enqueue (otherAssemblyContext),
          Throws.InvalidOperationException.And.Message.EqualTo (
              "The specified AssemblyContext has been dequeued on a different thread. "
              + "An AssemblyContext must be enqueued on the same thread it was dequeued from."));
    }

    [Test]
    public void Enqueue_IsStillSupportedAfterInnerPoolEnqueueFails ()
    {
      var exception = new Exception();
      var hasEnqueued = false;

      _assemblyContextPoolMock.Expect (mock => mock.Dequeue()).Return (_assemblyContext);
      _assemblyContextPoolMock
          .Expect (mock => mock.Enqueue (_assemblyContext))
          .Throw (exception);
      _assemblyContextPoolMock
          .Expect (mock => mock.Enqueue (_assemblyContext))
          .WhenCalled (mi => { hasEnqueued = true; });

      var assemblyContext = _decorator.Dequeue();

      Assert.That (() => _decorator.Enqueue (assemblyContext), Throws.Exception.SameAs (exception));
      Assert.That (hasEnqueued, Is.False);

      Assert.That (() => _decorator.Enqueue (assemblyContext), Throws.Nothing);
      Assert.That (hasEnqueued, Is.True);

      _assemblyContextPoolMock.VerifyAllExpectations();
    }

    [Test]
    public void Enqueue_AfterDequeueAll ()
    {
      var hasEnqueued = false;
      var assemblyContexts = new[] { CreateAssemblyContext(), CreateAssemblyContext() };

      _assemblyContextPoolMock.Expect (mock => mock.DequeueAll()).Return (assemblyContexts);
      _assemblyContextPoolMock
          .Expect (mock => mock.Enqueue (assemblyContexts[0]))
          .WhenCalled (mi => { hasEnqueued = true; });
      _assemblyContextPoolMock
          .Expect (mock => mock.Enqueue (assemblyContexts[1]))
          .WhenCalled (mi => { hasEnqueued = true; });

      _decorator.DequeueAll();

      _decorator.Enqueue (assemblyContexts[0]);
      Assert.That (hasEnqueued, Is.True);

      hasEnqueued = false;
      _decorator.Enqueue (assemblyContexts[1]);
      Assert.That (hasEnqueued, Is.True);

      _assemblyContextPoolMock.VerifyAllExpectations();
    }

    private AssemblyContext CreateAssemblyContext ()
    {
      return new AssemblyContext (
          MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(),
          MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>());
    }
  }
}