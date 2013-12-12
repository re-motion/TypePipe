using System;
using NUnit.Framework;
using Remotion.ServiceLocation;
using Remotion.TypePipe.Implementation.Remotion;

namespace Remotion.TypePipe.UnitTests
{
  [TestFixture]
  public class IPipelineRegistryTest
  {
    private DefaultServiceLocator _serviceLocator;

    [SetUp]
    public void SetUp ()
    {
      _serviceLocator = DefaultServiceLocator.Create();
    }

    [Test]
    public void GetInstance_Once ()
    {
      var factory = _serviceLocator.GetInstance<IPipelineRegistry>();

      Assert.That (factory, Is.Not.Null);
      Assert.That (factory, Is.TypeOf (typeof (RemotionPipelineRegistry)));
    }

    [Test]
    public void GetInstance_Twice ()
    {
      var factory1 = _serviceLocator.GetInstance<IPipelineRegistry>();
      var factory2 = _serviceLocator.GetInstance<IPipelineRegistry>();

      Assert.That (factory1, Is.SameAs (factory2));
    }
  }
}