using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NUnit.Framework;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.Caching;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Caching
{
  [TestFixture]
  public class ConstructorCallCacheTest
  {   
    private ITypeCache _typeCacheMock;
    private IConstructorDelegateFactory _constructorDelegateFactoryMock;

    private ConstructorCallCache _constructorCallCache;

    private IDictionary<ConstructionKey, Delegate> _constructorCalls;

    private readonly Type _assembledType = typeof (AssembledType);
    private readonly Delegate _generatedCtorCall = new Func<int> (() => 7);
    private Type _delegateType;
    private bool _allowNonPublic;

    [SetUp]
    public void SetUp ()
    {
      _typeCacheMock = MockRepository.GenerateStrictMock<ITypeCache>();

      _constructorDelegateFactoryMock = MockRepository.GenerateStrictMock<IConstructorDelegateFactory>();

      _constructorCallCache = new ConstructorCallCache (_typeCacheMock, _constructorDelegateFactoryMock);
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
          .Expect (
              mock => mock.GetOrCreateType (
                  // Use strongly typed Equals overload.
                  Arg<AssembledTypeID>.Matches (id => id.Equals (typeID))))
          .Return (_assembledType);

      _constructorDelegateFactoryMock
          .Expect (mock => mock.CreateConstructorCall (typeID.RequestedType, _assembledType, _delegateType, _allowNonPublic))
          .Return (_generatedCtorCall);

      var result = _constructorCallCache.GetOrCreateConstructorCall (typeID, _delegateType, _allowNonPublic);

      Assert.That (result, Is.SameAs (_generatedCtorCall));

      var key = new ConstructionKey (typeID, _delegateType, _allowNonPublic);
      Assert.That (_constructorCalls[key], Is.SameAs (_generatedCtorCall));
    }

    private class AssembledType {}  
  }
}