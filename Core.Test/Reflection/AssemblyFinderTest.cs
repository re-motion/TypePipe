using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Development.UnitTesting;
using Rubicon.Reflection;
using Rubicon.Utilities;

namespace Rubicon.Core.UnitTests.Reflection
{
  [TestFixture]
  public class AssemblyFinderTest
  {
    [Serializable]
    private class TestFixture
    {
      private string _baseDirectory;

      private string _markedAssemblyName;
      private string _markedExeAssemblyName;
      private string _markedAssemblyWithDerivedAttributeName;
      private string _markedReferencedAssemblyName;

      private string _markedAssemblyInOtherDirectoryName;
      private string _markedExeAssemblyInOtherDirectoryName;
      private string _markedAssemblyWithDerivedAttributeInOtherDirectoryName;

      private string _markerAttributeAssemblyName;
      private Type _markerAttributeType;

      public TestFixture ()
      {
        _markerAttributeAssemblyName = "Reflection.TestAssemblies.MarkerAttributeAssembly";
        AssemblyCompiler assemblyCompiler = new AssemblyCompiler (
            @"Reflection\TestAssemblies\MarkerAttributeAssembly",
            Path.Combine (AppDomain.CurrentDomain.BaseDirectory, _markerAttributeAssemblyName + ".dll"));
        assemblyCompiler.Compile();
        _markerAttributeType =
            assemblyCompiler.CompiledAssembly.GetType ("Rubicon.Core.UnitTests.Reflection.TestAssemblies.MarkerAttributeAssembly.MarkerAttribute");
        Assert.IsNotNull (_markerAttributeType);

        _baseDirectory = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "Reflection.AssemblyFinderTest");

        if (Directory.Exists (_baseDirectory))
          Directory.Delete (_baseDirectory, true);
        Directory.CreateDirectory (_baseDirectory);
        File.Copy (
            _markerAttributeAssemblyName + ".dll",
            Path.Combine (_baseDirectory, _markerAttributeAssemblyName + ".dll"),
            true);

        _markedReferencedAssemblyName = CompileTestAssemblyInSeparateAppDomain (
            AppDomain.CurrentDomain.BaseDirectory, "MarkedReferencedAssembly", "dll");
        //File.Copy (
        //    _markedReferencedAssemblyName + ".dll",
        //    Path.Combine (_baseDirectory, _markedReferencedAssemblyName + ".dll"),
        //    true);
    
        _markedAssemblyName = CompileTestAssemblyInSeparateAppDomain (
            AppDomain.CurrentDomain.BaseDirectory, "MarkedAssembly", "dll", _markedReferencedAssemblyName + ".dll");
        _markedExeAssemblyName = CompileTestAssemblyInSeparateAppDomain (AppDomain.CurrentDomain.BaseDirectory, "MarkedExeAssembly", "exe");
        _markedAssemblyWithDerivedAttributeName = CompileTestAssemblyInSeparateAppDomain (
            AppDomain.CurrentDomain.BaseDirectory, "MarkedAssemblyWithDerivedAttribute", "dll");
        CompileTestAssemblyInSeparateAppDomain (AppDomain.CurrentDomain.BaseDirectory, "UnmarkedAssembly", "dll");

        _markedAssemblyInOtherDirectoryName = CompileTestAssemblyInSeparateAppDomain (
            _baseDirectory, "MarkedAssemblyInOtherDirectory", "dll", _markedReferencedAssemblyName + ".dll");
        _markedExeAssemblyInOtherDirectoryName = CompileTestAssemblyInSeparateAppDomain (_baseDirectory, "MarkedExeAssemblyInOtherDirectory", "exe");
        _markedAssemblyWithDerivedAttributeInOtherDirectoryName = CompileTestAssemblyInSeparateAppDomain (
            _baseDirectory, "MarkedAssemblyWithDerivedAttributeInOtherDirectory", "dll");
        CompileTestAssemblyInSeparateAppDomain (_baseDirectory, "UnmarkedAssemblyInOtherDirectory", "dll");
      }

      public void Initialize_WithRootAssemblies ()
      {
        Assembly firstInMemoryAssembly = CompileTestAssemblyInMemory ("FirstInMemoryAssembly", _markedReferencedAssemblyName + ".dll");
        Assembly secondInMemoryAssembly = CompileTestAssemblyInMemory ("SecondInMemoryAssembly");
        Assembly unmarkedInMemoryAssembly = CompileTestAssemblyInMemory ("UnmarkedInMemoryAssembly");

        AssemblyFinder assemblyFinder =
            new AssemblyFinder (_markerAttributeType, firstInMemoryAssembly, secondInMemoryAssembly, unmarkedInMemoryAssembly);

        Assert.That (assemblyFinder.RootAssemblies.Length, Is.EqualTo (3));
        Assert.That (assemblyFinder.RootAssemblies, List.Contains (firstInMemoryAssembly));
        Assert.That (assemblyFinder.RootAssemblies, List.Contains (secondInMemoryAssembly));
        Assert.That (assemblyFinder.RootAssemblies, List.Contains (unmarkedInMemoryAssembly));
      }

      public void Initialize_WithAppDomainBaseDirectory ()
      {
        Assembly firstInMemoryAssembly = CompileTestAssemblyInMemory ("FirstInMemoryAssembly", _markedReferencedAssemblyName + ".dll");
        Assembly secondInMemoryAssembly = CompileTestAssemblyInMemory ("SecondInMemoryAssembly");
        CompileTestAssemblyInMemory ("UnmarkedInMemoryAssembly");

        AssemblyFinder assemblyFinder = new AssemblyFinder (_markerAttributeType);

        Assert.That (assemblyFinder.RootAssemblies.Length, Is.EqualTo (6));
        Assert.That (assemblyFinder.RootAssemblies, List.Contains (firstInMemoryAssembly));
        Assert.That (assemblyFinder.RootAssemblies, List.Contains (secondInMemoryAssembly));
        Assert.That (assemblyFinder.RootAssemblies, List.Contains (Assembly.Load (_markedAssemblyName)));
        Assert.That (assemblyFinder.RootAssemblies, List.Contains (Assembly.Load (_markedExeAssemblyName)));
        Assert.That (assemblyFinder.RootAssemblies, List.Contains (Assembly.Load (_markedAssemblyWithDerivedAttributeName)));
        Assert.That (assemblyFinder.RootAssemblies, List.Contains (Assembly.Load (_markedReferencedAssemblyName)));
      }

      public void Initialize_WithBaseDirectory_ReflectionOnly ()
      {
        AssemblyFinder assemblyFinder = new AssemblyFinder (_markerAttributeType, _baseDirectory);

        Assert.That (assemblyFinder.RootAssemblies.Length, Is.EqualTo (3));
        Assert.That (assemblyFinder.RootAssemblies, List.Contains (LoadAssemblyReflectionOnly (_markedAssemblyInOtherDirectoryName + ".dll")));
        Assert.That (assemblyFinder.RootAssemblies, List.Contains (LoadAssemblyReflectionOnly (_markedExeAssemblyInOtherDirectoryName + ".exe")));
        Assert.That (
            assemblyFinder.RootAssemblies,
            List.Contains (LoadAssemblyReflectionOnly (_markedAssemblyWithDerivedAttributeInOtherDirectoryName + ".dll")));
      }

      public void FindAssemblies_WithRootAssemblies ()
      {
        Assembly markedAssembly = Assembly.Load (_markedAssemblyName);
        AssemblyFinder assemblyFinder = new AssemblyFinder (_markerAttributeType, markedAssembly);

        Assembly[] assemblies = assemblyFinder.FindAssemblies();

        Assert.That (assemblyFinder.RootAssemblies.Length, Is.EqualTo (1));
        Assert.That (assemblyFinder.RootAssemblies, List.Contains (markedAssembly));

        Assert.That (assemblies.Length, Is.EqualTo (2));
        Assert.That (assemblies, List.Contains (markedAssembly));
        Assert.That (assemblies, List.Contains (Assembly.Load (_markedReferencedAssemblyName)));
      }

      public void FindAssemblies_WithRootAssemblies_ReflectionOnly ()
      {
        AssemblyFinder assemblyFinder = new AssemblyFinder (_markerAttributeType, _baseDirectory);

        Assembly[] assemblies = assemblyFinder.FindAssemblies();

        Assert.That (assemblyFinder.RootAssemblies.Length, Is.EqualTo (3));

        Assert.That (assemblies.Length, Is.EqualTo (4));
        Assert.That (assemblies, List.Contains (LoadAssemblyReflectionOnly (_markedAssemblyInOtherDirectoryName + ".dll")));
        Assert.That (assemblies, List.Contains (LoadAssemblyReflectionOnly (_markedExeAssemblyInOtherDirectoryName + ".exe")));
        Assert.That (
            assemblies,
            List.Contains (LoadAssemblyReflectionOnly (_markedAssemblyWithDerivedAttributeInOtherDirectoryName + ".dll")));
        Assert.That (
            assemblies,
            List.Contains (Assembly.ReflectionOnlyLoadFrom (_markedReferencedAssemblyName + ".dll")));
      }

      private Assembly CompileTestAssemblyInMemory (string assemblyName, params string[] referncedAssemblies)
      {
        AssemblyCompiler assemblyCompiler = AssemblyCompiler.CreateInMemoryAssemblyCompiler (
            string.Format (@"Reflection\TestAssemblies\{0}", assemblyName),
            ArrayUtility.Combine (new string[] {_markerAttributeAssemblyName + ".dll"}, referncedAssemblies));
        assemblyCompiler.Compile();
        return assemblyCompiler.CompiledAssembly;
      }

      private string CompileTestAssemblyInSeparateAppDomain (
          string directory,
          string assemblyName,
          string extension,
          params string[] referncedAssemblies)
      {
        string outputAssembly = string.Format (@"Reflection.TestAssemblies.{0}.{1}", assemblyName, extension);
        AssemblyCompiler assemblyCompiler = new AssemblyCompiler (
            string.Format (@"Reflection\TestAssemblies\{0}", assemblyName),
            Path.Combine (directory, outputAssembly),
            ArrayUtility.Combine (new string[] {_markerAttributeAssemblyName + ".dll"}, referncedAssemblies));
        assemblyCompiler.CompileInSeparateAppDomain();
        return Path.GetFileNameWithoutExtension (outputAssembly);
      }

      private Assembly LoadAssemblyReflectionOnly (string assemblyName)
      {
        return Assembly.ReflectionOnlyLoadFrom (Path.Combine (_baseDirectory, assemblyName));
      }
    }

    private TestFixture _testFixture;

    [TestFixtureSetUp]
    public void TestFixtureSetUp ()
    {
      _testFixture = new TestFixture();
    }

    [Test]
    public void Initialize_WithRootAssemblies ()
    {
      ExecuteInSeparateAppDomain (_testFixture.Initialize_WithRootAssemblies);
    }

    [Test]
    public void Initialize_WithAppDomainBaseDirectory ()
    {
      ExecuteInSeparateAppDomain (_testFixture.Initialize_WithAppDomainBaseDirectory);
    }

    [Test]
    public void Initialize_WithBaseDirectory_ReflectionOnly ()
    {
      ExecuteInSeparateAppDomain (_testFixture.Initialize_WithBaseDirectory_ReflectionOnly);
    }

    [Test]
    public void FindAssemblies_WithRootAssemblies ()
    {
      ExecuteInSeparateAppDomain (_testFixture.FindAssemblies_WithRootAssemblies);
    }

    [Test]
    public void FindAssemblies_WithRootAssemblies_ReflectionOnly ()
    {
      ExecuteInSeparateAppDomain (_testFixture.FindAssemblies_WithRootAssemblies_ReflectionOnly);
    }

    private void ExecuteInSeparateAppDomain (CrossAppDomainDelegate test)
    {
      AppDomain appDomain = null;

      try
      {
        appDomain = AppDomain.CreateDomain (
            "Test",
            null,
            AppDomain.CurrentDomain.BaseDirectory,
            AppDomain.CurrentDomain.RelativeSearchPath,
            AppDomain.CurrentDomain.ShadowCopyFiles);

        appDomain.DoCallBack (test);
      }
      finally
      {
        if (appDomain != null)
          AppDomain.Unload (appDomain);
      }
    }
  }
}