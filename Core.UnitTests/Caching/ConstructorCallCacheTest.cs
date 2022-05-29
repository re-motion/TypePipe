using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.Caching;
using Moq;

namespace Remotion.TypePipe.UnitTests.Caching
{
  [TestFixture]
  public class ConstructorCallCacheTest
  {   
    private Mock<ITypeCache> _typeCacheMock;
    private Mock<IConstructorDelegateFactory> _constructorDelegateFactoryMock;

    private ConstructorCallCache _constructorCallCache;

    private IDictionary<ConstructionKey, Delegate> _constructorCalls;

    private readonly Type _assembledType = typeof (AssembledType);
    private readonly Delegate _generatedCtorCall = new Func<int> (() => 7);
    private Type _delegateType;
    private bool _allowNonPublic;

    [SetUp]
    public void SetUp ()
    {
      _typeCacheMock = new Mock<ITypeCache> (MockBehavior.Strict);

      _constructorDelegateFactoryMock = new Mock<IConstructorDelegateFactory> (MockBehavior.Strict);

      _constructorCallCache = new ConstructorCallCache (_typeCacheMock.Object, _constructorDelegateFactoryMock.Object);
      _constructorCalls = (ConcurrentDictionary<ConstructionKey, Delegate>) PrivateInvoke.GetNonPublicField (_constructorCallCache, "_constructorCalls");

      _delegateType = ReflectionObjectMother.GetSomeDelegateType();
      _allowNonPublic = BooleanObjectMother.GetRandomBoolean();
    }

    [Test]
    public void GetOrCreateConstructorCall_CacheHit ()
    {
      var typeID = AssembledTypeIDObjectMother.Create();
      _constructorCalls.Add (new ConstructionKey (typeID, _delegateType, _allowNonPublic), _generatedCtorCall);

      var result = _constructorCallCache.GetOrCreateConstructorCall (typeID, _delegateType, _allowNonPublic);

      Assert.That (result, Is.SameAs (_generatedCtorCall));
    }

    [Test]
    public void GetOrCreateConstructorCall_CacheMiss ()
    {
      var typeID = AssembledTypeIDObjectMother.Create();

      _typeCacheMock
          .Setup (
              mock => mock.GetOrCreateType (
                  // Use strongly typed Equals overload.
                  It.Is<AssembledTypeID> (id => id.Equals (typeID))))
          .Returns (_assembledType)
          .Verifiable();

      _constructorDelegateFactoryMock
          .Setup (mock => mock.CreateConstructorCall (typeID.RequestedType, _assembledType, _delegateType, _allowNonPublic))
          .Returns (_generatedCtorCall)
          .Verifiable();

      var result = _constructorCallCache.GetOrCreateConstructorCall (typeID, _delegateType, _allowNonPublic);

      Assert.That (result, Is.SameAs (_generatedCtorCall));

      var key = new ConstructionKey (typeID, _delegateType, _allowNonPublic);
      Assert.That (_constructorCalls[key], Is.SameAs (_generatedCtorCall));
    }

    private class AssembledType {}  
  }
}