using System;
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
      _assemblyContext = new AssemblyContext (
          MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(),
          MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>());

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
    public void Dequeue_Once_ReturnsAssemblyContextFromInnerPool ()
    {
      _assemblyContextPoolMock.Expect (mock => mock.Dequeue()).Return (_assemblyContext);

      var actual = _decorator.Dequeue();

      Assert.That (actual, Is.SameAs (_assemblyContext));
    }

    [Test]
    public void Dequeue_TwiceOnSameThread_ReturnsAssemblyContextFromThreadLocalCacheOnSecondCall ()
    {
      var hasDequeued = false;
      _assemblyContextPoolMock.Expect (mock => mock.Dequeue())
          .WhenCalled (mi => { hasDequeued = true; })
          .Return (_assemblyContext);

      var actual1 = _decorator.Dequeue();
      Assert.That (hasDequeued, Is.True);

      hasDequeued = false;
      var actual2 = _decorator.Dequeue();
      Assert.That (hasDequeued, Is.False);

      Assert.That (actual2, Is.SameAs (actual1));
    }

    [Test]
    public void Enqueue_AfterDequeue_PassesAssemblyContextToInnerPool ()
    {
      _assemblyContextPoolMock.Expect (mock => mock.Dequeue()).Return (_assemblyContext);
      _assemblyContextPoolMock.Expect (mock => mock.Enqueue (_assemblyContext));

      var expected = _decorator.Dequeue();

      _decorator.Enqueue (expected);

      _assemblyContextPoolMock.VerifyAllExpectations();
    }

    [Test]
    public void Enqueue_TwiceAfterTwoDequeues_PassesAssemblyContextToInnerPoolOnSecondCall ()
    {
      var hasEnqueued = false;
      _assemblyContextPoolMock.Expect (mock => mock.Dequeue()).Return (_assemblyContext);
      _assemblyContextPoolMock
          .Expect (mock => mock.Enqueue (_assemblyContext))
          .WhenCalled (mi => { hasEnqueued = true; });

      var expected1 = _decorator.Dequeue();
      var expected2 = _decorator.Dequeue();

      _decorator.Enqueue (expected1);
      Assert.That (hasEnqueued, Is.False);

      _decorator.Enqueue (expected2);
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

      var otherAssemblyContext = new AssemblyContext (
          MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(),
          MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>());

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

      var expected = _decorator.Dequeue();

      Assert.That (() => _decorator.Enqueue (expected), Throws.Exception.SameAs (exception));
      Assert.That (hasEnqueued, Is.False);

      Assert.That (() => _decorator.Enqueue (expected), Throws.Nothing);
      Assert.That (hasEnqueued, Is.True);

      _assemblyContextPoolMock.VerifyAllExpectations();
    }
  }
}