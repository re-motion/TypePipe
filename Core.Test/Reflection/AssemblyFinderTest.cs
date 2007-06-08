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
      private string _relativeSearchPathDirectoryForDlls;
      private string _relativeSearchPathDirectoryForExes;
      private string _dynamicDirectory;

      private string _markedAssemblyName;
      private string _markedExeAssemblyName;
      private string _markedAssemblyWithDerivedAttributeName;
      private string _markedReferencedAssemblyName;

      private string _markedAssemblyInRelativeSearchPathName;
      private string _markedExeAssemblyInRelativeSearchPathName;

      private string _markedAssemblyInDynamicDirectoryName;
      private string _markedExeAssemblyInDynamicDirectoryName;

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

        _relativeSearchPathDirectoryForDlls = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "Reflection.AssemblyFinderTest.Dlls");
        _relativeSearchPathDirectoryForExes = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "Reflection.AssemblyFinderTest.Exes");
        _dynamicDirectory = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "Reflection.AssemblyFinderTest.Dynamic");

        ResetDirectory(_relativeSearchPathDirectoryForDlls);
        ResetDirectory (_relativeSearchPathDirectoryForExes);
        ResetDirectory (_dynamicDirectory);

        _markedReferencedAssemblyName = CompileTestAssemblyInSeparateAppDomain (
            AppDomain.CurrentDomain.BaseDirectory, "MarkedReferencedAssembly", "dll");
    
        _markedAssemblyName = CompileTestAssemblyInSeparateAppDomain (
            AppDomain.CurrentDomain.BaseDirectory, "MarkedAssembly", "dll", _markedReferencedAssemblyName + ".dll");
        _markedExeAssemblyName = CompileTestAssemblyInSeparateAppDomain (AppDomain.CurrentDomain.BaseDirectory, "MarkedExeAssembly", "exe");
        _markedAssemblyWithDerivedAttributeName = CompileTestAssemblyInSeparateAppDomain (
            AppDomain.CurrentDomain.BaseDirectory, "MarkedAssemblyWithDerivedAttribute", "dll");
        CompileTestAssemblyInSeparateAppDomain (AppDomain.CurrentDomain.BaseDirectory, "UnmarkedAssembly", "dll");

        _markedAssemblyInRelativeSearchPathName = CompileTestAssemblyInSeparateAppDomain (
            _relativeSearchPathDirectoryForDlls, "MarkedAssemblyInRelativeSearchPath", "dll");
        _markedExeAssemblyInRelativeSearchPathName = CompileTestAssemblyInSeparateAppDomain (
           _relativeSearchPathDirectoryForExes, "MarkedExeAssemblyInRelativeSearchPath", "exe");

        _markedAssemblyInDynamicDirectoryName = CompileTestAssemblyInSeparateAppDomain (
            _dynamicDirectory, "MarkedAssemblyInDynamicDirectory", "dll");
        _markedExeAssemblyInDynamicDirectoryName = CompileTestAssemblyInSeparateAppDomain (
         _dynamicDirectory, "MarkedExeAssemblyInDynamicDirectory", "exe");
      }

      public string RelativeSearchPathDirectory
      {
        get { return string.Join (";", new string[] { _relativeSearchPathDirectoryForDlls, _relativeSearchPathDirectoryForExes }); }
      }

      public string DynamicDirectory
      {
        get { return _dynamicDirectory; }
      }

      public void Initialize_WithRootAssemblies ()
      {
        Assembly firstInMemoryAssembly = CompileTestAssemblyInMemory ("FirstInMemoryAssembly", _markedReferencedAssemblyName + ".dll");
        Assembly secondInMemoryAssembly = CompileTestAssemblyInMemory ("SecondInMemoryAssembly");

        AssemblyFinder assemblyFinder =
            new AssemblyFinder (_markerAttributeType, firstInMemoryAssembly, secondInMemoryAssembly);

        Assert.That (_markerAttributeType, Is.SameAs (assemblyFinder.AssemblyMarkerAttribute));
        Assert.That (assemblyFinder.RootAssemblies.Length, Is.EqualTo (2));
        Assert.That (assemblyFinder.RootAssemblies, List.Contains (firstInMemoryAssembly));
        Assert.That (assemblyFinder.RootAssemblies, List.Contains (secondInMemoryAssembly));
      }

      public void Throws_WhenRootAssemblyWithoutmarkerAttribute ()
      {
        Assembly unmarkedInMemoryAssembly = CompileTestAssemblyInMemory ("UnmarkedInMemoryAssembly");
        new AssemblyFinder (_markerAttributeType, unmarkedInMemoryAssembly);
      }

      public void Initialize_WithDefaultConstructor ()
      {
        Assembly firstInMemoryAssembly = CompileTestAssemblyInMemory ("FirstInMemoryAssembly", _markedReferencedAssemblyName + ".dll");
        Assembly secondInMemoryAssembly = CompileTestAssemblyInMemory ("SecondInMemoryAssembly");
        CompileTestAssemblyInMemory ("UnmarkedInMemoryAssembly");

        Directory.CreateDirectory (AppDomain.CurrentDomain.DynamicDirectory);
        File.Copy (
            Path.Combine (_dynamicDirectory, _markedAssemblyInDynamicDirectoryName + ".dll"),
            Path.Combine (AppDomain.CurrentDomain.DynamicDirectory, _markedAssemblyInDynamicDirectoryName + ".dll"));
        File.Copy (
          Path.Combine (_dynamicDirectory, _markedExeAssemblyInDynamicDirectoryName + ".exe"),
          Path.Combine (AppDomain.CurrentDomain.DynamicDirectory, _markedExeAssemblyInDynamicDirectoryName + ".exe"));
      
        AssemblyFinder assemblyFinder = new AssemblyFinder (_markerAttributeType);

        Assert.That (assemblyFinder.RootAssemblies.Length, Is.EqualTo (10));
        Assert.That (assemblyFinder.RootAssemblies, List.Contains (firstInMemoryAssembly));
        Assert.That (assemblyFinder.RootAssemblies, List.Contains (secondInMemoryAssembly));
        Assert.That (assemblyFinder.RootAssemblies, List.Contains (Assembly.Load (_markedAssemblyName)));
        Assert.That (assemblyFinder.RootAssemblies, List.Contains (Assembly.Load (_markedExeAssemblyName)));
        Assert.That (assemblyFinder.RootAssemblies, List.Contains (Assembly.Load (_markedAssemblyWithDerivedAttributeName)));
        Assert.That (assemblyFinder.RootAssemblies, List.Contains (Assembly.Load (_markedReferencedAssemblyName)));
        Assert.That (assemblyFinder.RootAssemblies, List.Contains (Assembly.Load (_markedAssemblyInRelativeSearchPathName)));
        Assert.That (assemblyFinder.RootAssemblies, List.Contains (Assembly.Load (_markedExeAssemblyInRelativeSearchPathName)));
        Assert.That (assemblyFinder.RootAssemblies, List.Contains (Assembly.Load (_markedAssemblyInDynamicDirectoryName)));
        Assert.That (assemblyFinder.RootAssemblies, List.Contains (Assembly.Load (_markedExeAssemblyInDynamicDirectoryName)));
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

      private void ResetDirectory (string directory)
      {
        if (Directory.Exists (directory))
          Directory.Delete (directory, true);
        Directory.CreateDirectory (directory);
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
    [ExpectedException (typeof (ArgumentException),
        ExpectedMessage = "The root assembly '.*' is not tagged with the marker attribute '.*MarkerAttribute'",
        MatchType = MessageMatch.Regex)]
    public void Throws_WhenRootAssemblyWithoutmarkerAttribute ()
    {
      ExecuteInSeparateAppDomain (_testFixture.Throws_WhenRootAssemblyWithoutmarkerAttribute);
    }

    [Test]
    public void Initialize_WithDefaultConstructor ()
    {
      ExecuteInSeparateAppDomain (_testFixture.Initialize_WithDefaultConstructor);
    }

    [Test]
    public void FindAssemblies_WithRootAssemblies ()
    {
      ExecuteInSeparateAppDomain (_testFixture.FindAssemblies_WithRootAssemblies);
    }

    private void ExecuteInSeparateAppDomain (CrossAppDomainDelegate test)
    {
      AppDomain appDomain = null;

      try
      {
        AppDomainSetup appDomainSetup = new AppDomainSetup();
        appDomainSetup.ApplicationName = "Test";
        appDomainSetup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        appDomainSetup.PrivateBinPath = _testFixture.RelativeSearchPathDirectory;
        appDomainSetup.DynamicBase = _testFixture.DynamicDirectory;
        appDomainSetup.ShadowCopyFiles = AppDomain.CurrentDomain.SetupInformation.ShadowCopyFiles;

        appDomain = AppDomain.CreateDomain ("Test", null, appDomainSetup);
        
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