using System;
using System.Collections.Generic;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.TypeAssembly.Implementation
{
  [TestFixture]
  public class TypeAssemblyResultTest
  {
    [Test]
    public void Initialize_WithTypeAndAdditionalTypes ()
    {
      var type = ReflectionObjectMother.GetSomeClassType();
      var additionalTypes = MockRepository.GenerateStrictMock<IReadOnlyDictionary<object, Type>>();

      var result = new TypeAssemblyResult(type, additionalTypes);

      Assert.That (result.Type, Is.SameAs (type));
      Assert.That (result.AdditionalTypes, Is.SameAs (additionalTypes));
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