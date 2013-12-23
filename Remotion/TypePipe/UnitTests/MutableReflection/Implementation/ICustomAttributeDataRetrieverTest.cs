using System;
using NUnit.Framework;
using Remotion.ServiceLocation;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class ICustomAttributeDataRetrieverTest
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
      var factory = _serviceLocator.GetInstance<ICustomAttributeDataRetriever>();

      Assert.That (factory, Is.Not.Null);
      Assert.That (factory, Is.TypeOf (typeof (CustomAttributeDataRetriever)));
    }

    [Test]
    public void GetInstance_Twice ()
    {
      var factory1 = _serviceLocator.GetInstance<ICustomAttributeDataRetriever>();
      var factory2 = _serviceLocator.GetInstance<ICustomAttributeDataRetriever>();

      Assert.That (factory1, Is.Not.SameAs (factory2));
    }
  }
}