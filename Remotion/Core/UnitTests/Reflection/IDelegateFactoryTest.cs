using System;
using NUnit.Framework;
using Remotion.Reflection;
using Remotion.ServiceLocation;

namespace Remotion.UnitTests.Reflection
{
  public class IDelegateFactoryTest
  {
    private DefaultServiceLocator _serviceLocator;

    [SetUp]
    public void SetUp ()
    {
      _serviceLocator = new DefaultServiceLocator();
    }

    [Test]
    public void GetInstance_Once ()
    {
      var factory = _serviceLocator.GetInstance<IDelegateFactory>();

      Assert.That (factory, Is.Not.Null);
      Assert.That (factory, Is.TypeOf (typeof (DelegateFactory)));
    }

    [Test]
    public void GetInstance_Twice ()
    {
      var factory1 = _serviceLocator.GetInstance<IDelegateFactory>();
      var factory2 = _serviceLocator.GetInstance<IDelegateFactory>();

      Assert.That (factory1, Is.Not.SameAs (factory2));
    }
  }
}