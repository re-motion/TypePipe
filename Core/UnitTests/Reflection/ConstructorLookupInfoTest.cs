using System;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Reflection;
using Remotion.UnitTests.Reflection.TestDomain;

namespace Remotion.UnitTests.Reflection
{
  [TestFixture]
  public class ConstructorLookupInfoTest
  {
    [Test]
    public void GetDelegate_WithExactMatchFromBase ()
    {
      ConstructorLookupInfo lookupInfo = new ConstructorLookupInfo (typeof (TestClass));
      var actual = (Func<Base, TestClass>) lookupInfo.GetDelegate (typeof (Func<Base, TestClass>));

      TestClass instance = actual (null);
      Assert.That (instance.InvocationType, Is.SameAs (typeof (Base)));
    }

    [Test]
    public void GetDelegate_WithExactMatchFromDerived ()
    {
      ConstructorLookupInfo lookupInfo = new ConstructorLookupInfo (typeof (TestClass));
      var actual = (Func<Derived, TestClass>) lookupInfo.GetDelegate (typeof (Func<Derived, TestClass>));

      TestClass instance = actual (null);
      Assert.That (instance.InvocationType, Is.SameAs (typeof (Derived)));
    }

    [Test]
    public void GetDelegate_WithExactMatchFromDerivedDerived ()
    {
      ConstructorLookupInfo lookupInfo = new ConstructorLookupInfo (typeof (TestClass));
      var actual = (Func<DerivedDerived, TestClass>) lookupInfo.GetDelegate (typeof (Func<DerivedDerived, TestClass>));

      TestClass instance = actual (null);
      Assert.That (instance.InvocationType, Is.SameAs (typeof (Derived)));
    }
  }
}