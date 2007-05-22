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
    private string _baseDirectory;

    private Assembly _firstInMemoryAssembly;
    private Assembly _secondInMemoryAssembly;
    private Assembly _unmarkedInMemoryAssembly;
    private string _markedAssemblyName;
    private string _markedExeAssemblyName;
    private string _markedAssemblyWithDerivedAttributeName;
    private string _markedAssemblyInOtherDirectoryName;
    private string _markedExeAssemblyInOtherDirectoryName;
    private string _markedAssemblyWithDerivedAttributeInOtherDirectoryName;
    private string _markerAttributeAssemblyName;
    private Type _markerAttributeType;

    [TestFixtureSetUp]
    public void TestFixtureSetUp ()
    {
      _markerAttributeAssemblyName = "Reflection.TestAssemblies.MarkerAttributeAssembly";
      AssemblyCompiler assemblyCompiler = new AssemblyCompiler (
          @"Reflection\TestAssemblies\MarkerAttributeAssembly",
          Path.Combine (AppDomain.CurrentDomain.BaseDirectory, _markerAttributeAssemblyName + ".dll"));
      assemblyCompiler.Compile();
      _markerAttributeType =
          assemblyCompiler.CompiledAssembly.GetType ("Rubicon.Core.UnitTests.Reflection.TestAssemblies.MarkerAttributeAssembly.MarkerAttribute");

      _baseDirectory = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "Reflection.AssemblyFinderTest");

      if (Directory.Exists (_baseDirectory))
        Directory.Delete (_baseDirectory, true);
      Directory.CreateDirectory (_baseDirectory);
      File.Copy (
          _markerAttributeAssemblyName + ".dll",
          Path.Combine (_baseDirectory, _markerAttributeAssemblyName + ".dll"),
          true);

      _firstInMemoryAssembly = CompileTestAssemblyInMemory ("FirstInMemoryAssembly");
      _secondInMemoryAssembly = CompileTestAssemblyInMemory ("SecondInMemoryAssembly");
      _unmarkedInMemoryAssembly = CompileTestAssemblyInMemory ("UnmarkedInMemoryAssembly");

      _markedAssemblyName = CompileTestAssemblyInSeparateAppDomain (AppDomain.CurrentDomain.BaseDirectory, "MarkedAssembly", "dll");
      _markedExeAssemblyName = CompileTestAssemblyInSeparateAppDomain (AppDomain.CurrentDomain.BaseDirectory, "MarkedExeAssembly", "exe");
      _markedAssemblyWithDerivedAttributeName = 
          CompileTestAssemblyInSeparateAppDomain (AppDomain.CurrentDomain.BaseDirectory, "MarkedAssemblyWithDerivedAttribute", "dll");
      CompileTestAssemblyInSeparateAppDomain (AppDomain.CurrentDomain.BaseDirectory, "UnmarkedAssembly", "dll");

      _markedAssemblyInOtherDirectoryName = CompileTestAssemblyInSeparateAppDomain (_baseDirectory, "MarkedAssemblyInOtherDirectory", "dll");
      _markedExeAssemblyInOtherDirectoryName = CompileTestAssemblyInSeparateAppDomain (_baseDirectory, "MarkedExeAssemblyInOtherDirectory", "exe");
      _markedAssemblyWithDerivedAttributeInOtherDirectoryName =
          CompileTestAssemblyInSeparateAppDomain (_baseDirectory, "MarkedAssemblyWithDerivedAttributeInOtherDirectory", "dll");
      CompileTestAssemblyInSeparateAppDomain (_baseDirectory, "UnmarkedAssemblyInOtherDirectory", "dll");
    }

    [Test]
    public void Initialize_WithRootAssemblies ()
    {
      AssemblyFinder assemblyFinder = 
          new AssemblyFinder (_markerAttributeType, _firstInMemoryAssembly, _secondInMemoryAssembly, _unmarkedInMemoryAssembly);

      Assert.That (assemblyFinder.RootAssemblies.Length, Is.EqualTo (3));
      Assert.That (assemblyFinder.RootAssemblies, List.Contains (_firstInMemoryAssembly));
      Assert.That (assemblyFinder.RootAssemblies, List.Contains (_secondInMemoryAssembly));
      Assert.That (assemblyFinder.RootAssemblies, List.Contains (_unmarkedInMemoryAssembly));
    }

    [Test]
    public void Initialize_WithAppDomainBaseDirectory ()
    {
      AssemblyFinder assemblyFinder = new AssemblyFinder (_markerAttributeType);

      Assert.That (assemblyFinder.RootAssemblies.Length, Is.EqualTo (5));
      Assert.That (assemblyFinder.RootAssemblies, List.Contains (_firstInMemoryAssembly));
      Assert.That (assemblyFinder.RootAssemblies, List.Contains (_secondInMemoryAssembly));
      Assert.That (assemblyFinder.RootAssemblies, List.Contains (Assembly.Load (_markedAssemblyName)));
      Assert.That (assemblyFinder.RootAssemblies, List.Contains (Assembly.Load (_markedExeAssemblyName)));
      Assert.That (assemblyFinder.RootAssemblies, List.Contains (Assembly.Load (_markedAssemblyWithDerivedAttributeName)));
    }

    [Test]
    public void Initialize_WithBaseDirectory ()
    {
      AssemblyFinder assemblyFinder = new AssemblyFinder (_markerAttributeType, _baseDirectory);

      Assert.That (assemblyFinder.RootAssemblies.Length, Is.EqualTo (3));
      Assert.That (
          assemblyFinder.RootAssemblies,
          List.Contains (Assembly.ReflectionOnlyLoadFrom (Path.Combine (_baseDirectory, _markedAssemblyInOtherDirectoryName + ".dll"))));
      Assert.That (
          assemblyFinder.RootAssemblies,
          List.Contains (Assembly.ReflectionOnlyLoadFrom (Path.Combine (_baseDirectory, _markedExeAssemblyInOtherDirectoryName + ".exe"))));
      Assert.That (
         assemblyFinder.RootAssemblies,
         List.Contains (Assembly.ReflectionOnlyLoadFrom (Path.Combine (_baseDirectory, _markedAssemblyWithDerivedAttributeInOtherDirectoryName + ".dll"))));
    }

    private Assembly CompileTestAssemblyInMemory (string assemblyName)
    {
      AssemblyCompiler assemblyCompiler = AssemblyCompiler.CreateInMemoryAssemblyCompiler (
          string.Format (@"Reflection\TestAssemblies\{0}", assemblyName),
          _markerAttributeAssemblyName + ".dll");
      assemblyCompiler.Compile();
      return assemblyCompiler.CompiledAssembly;
    }

    private string CompileTestAssemblyInSeparateAppDomain (string directory, string assemblyName, string extension)
    {
      string outputAssembly = string.Format (@"Reflection.TestAssemblies.{0}.{1}", assemblyName, extension);
      AssemblyCompiler assemblyCompiler = new AssemblyCompiler (
          string.Format (@"Reflection\TestAssemblies\{0}", assemblyName),
          Path.Combine (directory, outputAssembly),
          _markerAttributeAssemblyName + ".dll");
      assemblyCompiler.CompileInSeparateAppDomain();
      return Path.GetFileNameWithoutExtension (outputAssembly);
    }
  }
}