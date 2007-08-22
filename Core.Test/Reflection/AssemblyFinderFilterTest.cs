using System;
using NUnit.Framework;
using Rubicon.Reflection;
using System.Reflection;

[assembly: Rubicon.Core.UnitTests.Reflection.TestMarker]

namespace Rubicon.Core.UnitTests.Reflection
{
  public class TestMarkerAttribute : Attribute { }

  [TestFixture]
  public class AssemblyFinderFilterTest
  {
    [Test]
    public void NonSystemAssemblyMatchExpression ()
    {
      NonSystemAssemblyFinderFilter filter = new NonSystemAssemblyFinderFilter();
      Assert.AreEqual (@"^(mscorlib)|(System)|(System\..*)|(Microsoft\..*)$", filter.SystemAssemblyMatchExpression);
    }

    [Test]
    public void NonSystemAssemblyConsidering ()
    {
      NonSystemAssemblyFinderFilter filter = new NonSystemAssemblyFinderFilter ();
      Assert.IsTrue (filter.ShouldConsiderAssembly (typeof (AssemblyFinderFilterTest).Assembly.GetName ()));
      Assert.IsTrue (filter.ShouldConsiderAssembly (typeof (TestFixtureAttribute).Assembly.GetName ()));
      Assert.IsFalse (filter.ShouldConsiderAssembly (typeof (object).Assembly.GetName ()));
      Assert.IsFalse (filter.ShouldConsiderAssembly (new AssemblyName ("System")));
      Assert.IsFalse (filter.ShouldConsiderAssembly (new AssemblyName ("Microsoft.Something.Whatever")));
    }

    [Test]
    public void NonSystemAssemblyInclusion ()
    {
      NonSystemAssemblyFinderFilter filter = new NonSystemAssemblyFinderFilter ();
      Assert.IsTrue (filter.ShouldIncludeAssembly (typeof (AssemblyFinderFilterTest).Assembly));
      Assert.IsTrue (filter.ShouldIncludeAssembly (typeof (TestFixtureAttribute).Assembly));
      Assert.IsTrue (filter.ShouldIncludeAssembly (typeof (object).Assembly));
      Assert.IsTrue (filter.ShouldIncludeAssembly (typeof (Uri).Assembly));
    }

    [Test]
    public void AttributeConsidering ()
    {
      AttributeAssemblyFinderFilter filter = new AttributeAssemblyFinderFilter (typeof (SerializableAttribute)); // attribute type doesn't matter here
      Assert.IsTrue (filter.ShouldConsiderAssembly (typeof (AssemblyFinderFilterTest).Assembly.GetName()));
      Assert.IsTrue (filter.ShouldConsiderAssembly (typeof (TestFixtureAttribute).Assembly.GetName()));
      Assert.IsTrue (filter.ShouldConsiderAssembly (typeof (object).Assembly.GetName()));
      Assert.IsTrue (filter.ShouldConsiderAssembly (new AssemblyName ("name does not matter")));
    }

    [Test]
    public void AttributeInclusion ()
    {
      AttributeAssemblyFinderFilter filter = new AttributeAssemblyFinderFilter (typeof (TestMarkerAttribute));
      Assert.IsTrue (filter.ShouldIncludeAssembly (typeof (AssemblyFinderFilterTest).Assembly));
      Assert.IsFalse (filter.ShouldIncludeAssembly (typeof (TestFixtureAttribute).Assembly));
      Assert.IsFalse (filter.ShouldIncludeAssembly (typeof (object).Assembly));
      Assert.IsFalse (filter.ShouldIncludeAssembly (typeof (Uri).Assembly));

      filter = new AttributeAssemblyFinderFilter (typeof (CLSCompliantAttribute));
      Assert.IsTrue (filter.ShouldIncludeAssembly (typeof (AssemblyFinder).Assembly));
      Assert.IsFalse (filter.ShouldIncludeAssembly (typeof (AssemblyFinderFilterTest).Assembly));
      Assert.IsTrue (filter.ShouldIncludeAssembly (typeof (TestFixtureAttribute).Assembly));
      Assert.IsTrue (filter.ShouldIncludeAssembly (typeof (object).Assembly));
      Assert.IsTrue (filter.ShouldIncludeAssembly (typeof (Uri).Assembly));

      filter = new AttributeAssemblyFinderFilter (typeof (SerializableAttribute));
      Assert.IsFalse (filter.ShouldIncludeAssembly (typeof (AssemblyFinderFilterTest).Assembly));
      Assert.IsFalse (filter.ShouldIncludeAssembly (typeof (TestFixtureAttribute).Assembly));
      Assert.IsFalse (filter.ShouldIncludeAssembly (typeof (object).Assembly));
      Assert.IsFalse (filter.ShouldIncludeAssembly (typeof (Uri).Assembly));
    }
  }
}