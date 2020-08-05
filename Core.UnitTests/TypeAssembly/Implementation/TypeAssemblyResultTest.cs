using System;
using System.Collections.Generic;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Moq;

namespace Remotion.TypePipe.UnitTests.TypeAssembly.Implementation
{
  [TestFixture]
  public class TypeAssemblyResultTest
  {
    [Test]
    public void Initialize_WithTypeAndAdditionalTypes ()
    {
      var type = ReflectionObjectMother.GetSomeClassType();
      var additionalTypes = new Mock<IReadOnlyDictionary<object, Type>> (MockBehavior.Strict);

      var result = new TypeAssemblyResult(type, additionalTypes.Object);

      Assert.That (result.Type, Is.SameAs (type));
      Assert.That (result.AdditionalTypes, Is.SameAs (additionalTypes.Object));
    }

    [Test]
    public void Initialize_WithTypeOnly ()
    {
      var type = ReflectionObjectMother.GetSomeClassType();

      var result = new TypeAssemblyResult(type);

      Assert.That (result.Type, Is.SameAs (type));
      Assert.That (result.AdditionalTypes, Is.Empty);
    }
  }
}