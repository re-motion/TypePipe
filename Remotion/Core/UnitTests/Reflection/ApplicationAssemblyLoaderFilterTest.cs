// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Reflection;
using Remotion.Utilities;

namespace Remotion.UnitTests.Reflection
{
  [TestFixture]
  public class ApplicationAssemblyLoaderFilterTest
  {
    [SetUp]
    public void SetUp ()
    {
      ApplicationAssemblyLoaderFilter.Instance.Reset ();
    }

    [TearDown]
    public void TearDown ()
    {
      ApplicationAssemblyLoaderFilter.Instance.Reset ();
    }

    [Test]
    public void ApplicationAssemblyMatchExpression ()
    {
      ApplicationAssemblyLoaderFilter filter = ApplicationAssemblyLoaderFilter.Instance;
      Assert.AreEqual (@"^((mscorlib)|(System)|(System\..*)|(Microsoft\..*)|(Remotion\..*\.Generated\..*))$",
          filter.SystemAssemblyMatchExpression);
    }

    [Test]
    public void ApplicationAssemblyConsidering ()
    {
      ApplicationAssemblyLoaderFilter filter = ApplicationAssemblyLoaderFilter.Instance;
      Assert.IsTrue (filter.ShouldConsiderAssembly (typeof (AttributeAssemblyLoaderFilterTest).Assembly.GetName ()));
      Assert.IsTrue (filter.ShouldConsiderAssembly (typeof (TestFixtureAttribute).Assembly.GetName ()));
      Assert.IsTrue (filter.ShouldConsiderAssembly (typeof (AssemblyFinder).Assembly.GetName ()));

      Assert.IsFalse (filter.ShouldConsiderAssembly (typeof (object).Assembly.GetName ()));
      Assert.IsFalse (filter.ShouldConsiderAssembly (new AssemblyName ("System")));
      Assert.IsFalse (filter.ShouldConsiderAssembly (new AssemblyName ("Microsoft.Something.Whatever")));
      Assert.IsFalse (filter.ShouldConsiderAssembly (new AssemblyName ("Remotion.Mixins.Generated.Unsigned")));
      Assert.IsFalse (filter.ShouldConsiderAssembly (new AssemblyName ("Remotion.Mixins.Generated.Signed")));
      Assert.IsFalse (filter.ShouldConsiderAssembly (new AssemblyName ("Remotion.Data.DomainObjects.Generated.Signed")));
      Assert.IsFalse (filter.ShouldConsiderAssembly (new AssemblyName ("Remotion.Data.DomainObjects.Generated.Unsigned")));
    }

    [Test]
    public void AddIgnoredAssembly ()
    {
      ApplicationAssemblyLoaderFilter filter = ApplicationAssemblyLoaderFilter.Instance;
      Assert.IsTrue (filter.ShouldConsiderAssembly (typeof (AssemblyFinder).Assembly.GetName ()));
      filter.AddIgnoredAssembly (typeof (AssemblyFinder).Assembly.GetName ().Name);
      Assert.IsFalse (filter.ShouldConsiderAssembly (typeof (AssemblyFinder).Assembly.GetName ()));
    }

    [Test]
    public void ApplicationAssemblyInclusion_DependsOnAttribute ()
    {
      string compiledAssemblyPath = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "NonApplicationMarkedAssembly.dll");
      try
      {
        AppDomainRunner.Run (
            delegate (object[] args)
            {
              var path = (string) args[0];

              ApplicationAssemblyLoaderFilter filter = ApplicationAssemblyLoaderFilter.Instance;
              Assert.IsTrue (filter.ShouldIncludeAssembly (typeof (AttributeAssemblyLoaderFilterTest).Assembly));
              Assert.IsTrue (filter.ShouldIncludeAssembly (typeof (TestFixtureAttribute).Assembly));
              Assert.IsTrue (filter.ShouldIncludeAssembly (typeof (AssemblyFinder).Assembly));
              Assert.IsTrue (filter.ShouldIncludeAssembly (typeof (object).Assembly));
              Assert.IsTrue (filter.ShouldIncludeAssembly (typeof (Uri).Assembly));

              var assemblyCompiler = new AssemblyCompiler (@"Reflection\TestAssemblies\NonApplicationMarkedAssembly", path,
                  typeof (NonApplicationAssemblyAttribute).Assembly.Location);
              assemblyCompiler.Compile ();
              Assert.IsFalse (filter.ShouldIncludeAssembly (assemblyCompiler.CompiledAssembly));
            }, compiledAssemblyPath);
      }
      finally
      {
        if (File.Exists (compiledAssemblyPath))
          FileUtility.DeleteAndWaitForCompletion (compiledAssemblyPath);
      }
    }

  }
}