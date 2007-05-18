using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Development.UnitTesting;
using Rubicon.Reflection;

namespace Rubicon.Core.UnitTests.Reflection
{
  [TestFixture]
  public class AssemblyFinderTest
  {
    private Assembly _firstInMemoryAssembly;
    private Assembly _secondInMemoryAssembly;

    [TestFixtureSetUp]
    public void TestFixtureSetUp ()
    {
      _firstInMemoryAssembly = CompileTestAssembly ("FirstInMemoryAssembly");
      _secondInMemoryAssembly = CompileTestAssembly ("SecondInMemoryAssembly");
      CompileTestAssembly ("UnmarkedInMemoryAssembly");
    }

    [Test]
    public void Initialize_WithRootAssemblies ()
    {
      AssemblyFinder assemblyFinder = new AssemblyFinder (_firstInMemoryAssembly, _secondInMemoryAssembly);

      Assert.That (assemblyFinder.RootAssemblies.Length, Is.EqualTo (2));
      Assert.That (assemblyFinder.RootAssemblies, List.Contains (_firstInMemoryAssembly));
      Assert.That (assemblyFinder.RootAssemblies, List.Contains (_secondInMemoryAssembly));
    }

    [Test]
    public void Initialize_WithDefaultConstructor ()
    {
      string markedAssemblyName = CompileTestAssemblyInSeparateAppDomain ("MarkedAssembly", "dll");
      string markedExeAssemblyName = CompileTestAssemblyInSeparateAppDomain ("MarkedExeAssembly", "exe");
      CompileTestAssemblyInSeparateAppDomain ("UnmarkedAssembly", "dll");
      AssemblyFinder assemblyFinder = new AssemblyFinder (typeof (TestAssemblyMarkerAttribute));

      Assert.That (assemblyFinder.RootAssemblies.Length, Is.EqualTo (4));
      Assert.That (assemblyFinder.RootAssemblies, List.Contains (_firstInMemoryAssembly));
      Assert.That (assemblyFinder.RootAssemblies, List.Contains (_secondInMemoryAssembly));
      Assert.That (assemblyFinder.RootAssemblies, List.Contains (LoadAssembly (markedAssemblyName)));
      Assert.That (assemblyFinder.RootAssemblies, List.Contains (LoadAssembly (markedExeAssemblyName)));
    }

    //[Test]
    //public void Initialize_WithBaseDirectory ()
    //{
    //  string baseDirectory = "Rubicon.Core.UnitTests.Reflection.TestAssemblies";
    //  string markedAssemblyName = CompileTestAssemblyInSeparateAppDomain ("MarkedAssembly", "dll");
    //  CompileTestAssemblyInSeparateAppDomain ("UnmarkedAssembly", "dll");
    //  AssemblyFinder assemblyFinder = new AssemblyFinder (typeof (TestAssemblyMarkerAttribute), baseDirectory);

    //  Assert.That (assemblyFinder.RootAssemblies.Length, Is.EqualTo (1));
    //  Assert.That (assemblyFinder.RootAssemblies, List.Contains (LoadAssembly (markedAssemblyName)));
    //}

    private Assembly CompileTestAssembly (string assemblyName)
    {
      AssemblyCompiler assemblyCompiler = AssemblyCompiler.CreateInMemoryAssemblyCompiler (
          string.Format (@"Reflection\TestAssemblies\{0}", assemblyName),          
          "Rubicon.Core.UnitTests.dll");
      assemblyCompiler.Compile ();
      return assemblyCompiler.CompiledAssembly;
    }

    private string CompileTestAssemblyInSeparateAppDomain (string assemblyName, string extension)
    {
      string outputAssembly = string.Format (@"Rubicon.Core.UnitTests.Reflection.TestAssemblies.{0}.{1}", assemblyName, extension);
      AssemblyCompiler assemblyCompiler = new AssemblyCompiler (
          string.Format (@"Reflection\TestAssemblies\{0}", assemblyName),
          outputAssembly,
          "Rubicon.Core.UnitTests.dll");
      assemblyCompiler.CompileInSeparateAppDomain ();
      return outputAssembly;
    }

    private Assembly LoadAssembly (string assemblyName)
    {
      return Assembly.LoadFile (Path.Combine (AppDomain.CurrentDomain.BaseDirectory, assemblyName));
    }
  }
}