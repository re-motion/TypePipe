using System;
using NUnit.Framework;
using Remotion.ServiceLocation;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.Implementation.Remotion;

namespace Remotion.TypePipe.UnitTests.Implementation
{
  [TestFixture]
  public class IPipelineFactoryTest
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
      var factory = _serviceLocator.GetInstance<IPipelineFactory>();

      Assert.That (factory, Is.Not.Null);
      Assert.That (factory, Is.TypeOf (typeof (RemotionPipelineFactory)));
    }

    [Test]
    public void GetInstance_Twice ()
    {
      var factory1 = _serviceLocator.GetInstance<IPipelineFactory>();
      var factory2 = _serviceLocator.GetInstance<IPipelineFactory>();

      Assert.That (factory1, Is.Not.SameAs (factory2));
    }
  }
}