using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Development.UnitTesting;
using Remotion.Reflection;
using Remotion.Utilities;
using Rhino.Mocks;
using Rhino_Is = Rhino.Mocks.Constraints.Is;
using System.Diagnostics;

namespace Remotion.UnitTests.Reflection
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
      private string _markedAssemblyWithOtherFilenameInRelativeSearchPathName;

      private string _markedAssemblyInRelativeSearchPathName;
      private string _markedExeAssemblyInRelativeSearchPathName;

      private string _markedAssemblyInDynamicDirectoryName;
      private string _markedExeAssemblyInDynamicDirectoryName;

      private string _markerAttributeAssemblyName;
      private Type _markerAttributeType;
      private AttributeAssemblyFinderFilter _attributeFilter;

      public TestFixture ()
      {
        _markerAttributeAssemblyName = typeof (MarkerAttribute).Assembly.GetName().Name;
        _markerAttributeType = typeof (MarkerAttribute);
        Assert.IsNotNull (_markerAttributeType);

        _relativeSearchPathDirectoryForDlls = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "Reflection.AssemblyFinderTest.Dlls");
        _relativeSearchPathDirectoryForExes = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "Reflection.AssemblyFinderTest.Exes");
        _dynamicDirectory = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "Reflection.AssemblyFinderTest.Dynamic");

        ResetDirectory (_relativeSearchPathDirectoryForDlls);
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

        _markedAssemblyWithOtherFilenameInRelativeSearchPathName = CompileTestAssemblyInSeparateAppDomain (
            _relativeSearchPathDirectoryForDlls, "MarkedAssemblyWithOtherFilenameInRelativeSearchPath", "dll");

        File.Move (
            Path.Combine (_relativeSearchPathDirectoryForDlls, _markedAssemblyWithOtherFilenameInRelativeSearchPathName + ".dll"),
            Path.Combine (_relativeSearchPathDirectoryForDlls, "_" + _markedAssemblyWithOtherFilenameInRelativeSearchPathName + ".dll"));

        _attributeFilter = new AttributeAssemblyFinderFilter (_markerAttributeType);
      }

      public void Cleanup ()
      {
        ResetDirectory (_relativeSearchPathDirectoryForDlls);
        ResetDirectory (_relativeSearchPathDirectoryForExes);
        ResetDirectory (_dynamicDirectory);

        FileUtility.DeleteAndWaitForCompletion (Path.Combine (AppDomain.CurrentDomain.BaseDirectory, _markedReferencedAssemblyName + ".dll"));
        FileUtility.DeleteAndWaitForCompletion (Path.Combine (AppDomain.CurrentDomain.BaseDirectory, _markedAssemblyName + ".dll"));
        FileUtility.DeleteAndWaitForCompletion (Path.Combine (AppDomain.CurrentDomain.BaseDirectory, _markedExeAssemblyName + ".exe"));
        FileUtility.DeleteAndWaitForCompletion (Path.Combine (AppDomain.CurrentDomain.BaseDirectory, _markedAssemblyWithDerivedAttributeName + ".dll"));
        FileUtility.DeleteAndWaitForCompletion (Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "Reflection.TestAssemblies.UnmarkedAssembly.dll"));
      }

      public string RelativeSearchPathDirectory
      {
        get { return string.Join (";", new string[] {_relativeSearchPathDirectoryForDlls, _relativeSearchPathDirectoryForExes}); }
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
            new AssemblyFinder (_attributeFilter, firstInMemoryAssembly, secondInMemoryAssembly);

        Assert.That (_attributeFilter, Is.SameAs (assemblyFinder.Filter));
        Assert.That (assemblyFinder.RootAssemblies, Is.EquivalentTo (new Assembly[] {firstInMemoryAssembly, secondInMemoryAssembly}));
      }

      public void RootAssemblyNeedntMatchFilter ()
      {
        Assembly unmarkedInMemoryAssembly = CompileTestAssemblyInMemory ("UnmarkedInMemoryAssembly");
        AssemblyFinder finder = new AssemblyFinder (_attributeFilter, unmarkedInMemoryAssembly);
        Assert.That (finder.RootAssemblies, List.Contains (unmarkedInMemoryAssembly));
        Assert.That (finder.FindAssemblies(), List.Contains (unmarkedInMemoryAssembly));
      }

      public void Initialize_WithConsiderDynamicDirectoryTrue ()
      {
        Assembly firstInMemoryAssembly = CompileTestAssemblyInMemory ("FirstInMemoryAssembly", _markedReferencedAssemblyName + ".dll");
        Assembly secondInMemoryAssembly = CompileTestAssemblyInMemory ("SecondInMemoryAssembly");
        CompileTestAssemblyInMemory ("UnmarkedInMemoryAssembly");

        InitializeDynamicDirectory();

        AssemblyFinder assemblyFinder = new AssemblyFinder (_attributeFilter, true);

        Assert.That (assemblyFinder.RootAssemblies, List.Not.Contains (firstInMemoryAssembly));
        Assert.That (assemblyFinder.RootAssemblies, List.Not.Contains (secondInMemoryAssembly));

        Assert.That (
            assemblyFinder.RootAssemblies,
            Is.EquivalentTo (
                new Assembly[]
                    {
                        Assembly.Load (_markedAssemblyName),
                        Assembly.Load (_markedExeAssemblyName),
                        Assembly.Load (_markedAssemblyWithDerivedAttributeName),
                        Assembly.Load (_markedReferencedAssemblyName),
                        Assembly.Load (_markedAssemblyInRelativeSearchPathName),
                        Assembly.Load (_markedExeAssemblyInRelativeSearchPathName),
                        Assembly.Load (_markedAssemblyInDynamicDirectoryName),
                        Assembly.Load (_markedExeAssemblyInDynamicDirectoryName),
                        Assembly.LoadFile (
                            Path.Combine (
                                _relativeSearchPathDirectoryForDlls, "_" + _markedAssemblyWithOtherFilenameInRelativeSearchPathName + ".dll"))
                    }));
      }

      public void Initialize_WithConsiderDynamicDirectoryFalse ()
      {
        Assembly firstInMemoryAssembly = CompileTestAssemblyInMemory ("FirstInMemoryAssembly", _markedReferencedAssemblyName + ".dll");
        Assembly secondInMemoryAssembly = CompileTestAssemblyInMemory ("SecondInMemoryAssembly");
        CompileTestAssemblyInMemory ("UnmarkedInMemoryAssembly");

        InitializeDynamicDirectory ();

        AssemblyFinder assemblyFinder = new AssemblyFinder (_attributeFilter, false);

        Assert.That (assemblyFinder.RootAssemblies, List.Not.Contains (firstInMemoryAssembly));
        Assert.That (assemblyFinder.RootAssemblies, List.Not.Contains (secondInMemoryAssembly));

        Assert.That (
            assemblyFinder.RootAssemblies,
            Is.EquivalentTo (
                new Assembly[]
                    {
                        Assembly.Load (_markedAssemblyName),
                        Assembly.Load (_markedExeAssemblyName),
                        Assembly.Load (_markedAssemblyWithDerivedAttributeName),
                        Assembly.Load (_markedReferencedAssemblyName),
                        Assembly.Load (_markedAssemblyInRelativeSearchPathName),
                        Assembly.Load (_markedExeAssemblyInRelativeSearchPathName),
                        Assembly.LoadFile (
                            Path.Combine (
                                _relativeSearchPathDirectoryForDlls, "_" + _markedAssemblyWithOtherFilenameInRelativeSearchPathName + ".dll"))
                    }));
      }

      private void InitializeDynamicDirectory ()
      {
        if (Directory.Exists (AppDomain.CurrentDomain.DynamicDirectory))
          Directory.Delete (AppDomain.CurrentDomain.DynamicDirectory, true);
        Directory.CreateDirectory (AppDomain.CurrentDomain.DynamicDirectory);
        File.Copy (
            Path.Combine (_dynamicDirectory, _markedAssemblyInDynamicDirectoryName + ".dll"),
            Path.Combine (AppDomain.CurrentDomain.DynamicDirectory, _markedAssemblyInDynamicDirectoryName + ".dll"));
        File.Copy (
            Path.Combine (_dynamicDirectory, _markedExeAssemblyInDynamicDirectoryName + ".exe"),
            Path.Combine (AppDomain.CurrentDomain.DynamicDirectory, _markedExeAssemblyInDynamicDirectoryName + ".exe"));
      }

      public void FindAssemblies_WithRootAssemblies ()
      {
        Assembly markedAssembly = Assembly.Load (_markedAssemblyName);
        AssemblyFinder assemblyFinder = new AssemblyFinder (_attributeFilter, markedAssembly);

        Assembly[] assemblies = assemblyFinder.FindAssemblies();

        Assert.That (assemblyFinder.RootAssemblies, Is.EquivalentTo (new Assembly[] {markedAssembly}));
        Assert.That (assemblies, Is.EquivalentTo (new Assembly[] {markedAssembly, Assembly.Load (_markedReferencedAssemblyName)}));
      }

      public void FindAssemblies_WithSpecificFilter_ConsiderAssemblyFalse ()
      {
        InitializeDynamicDirectory ();

        MockRepository mockRepository = new MockRepository ();
        IAssemblyFinderFilter filter = mockRepository.CreateMock<IAssemblyFinderFilter> ();

        Expect.Call (filter.ShouldConsiderAssembly (null)).IgnoreArguments ().Return (false).Repeat.AtLeastOnce();

        mockRepository.ReplayAll ();

        AssemblyFinder assemblyFinder = new AssemblyFinder (filter, true);
        Assembly[] assemblies = assemblyFinder.FindAssemblies ();

        mockRepository.VerifyAll ();

        Assert.That (assemblyFinder.RootAssemblies, Is.Empty);
        Assert.That (assemblies, Is.Empty);
      }

      public void FindAssemblies_WithSpecificFilter_ConsiderAssemblyTrueIncludeAssemblyFalse ()
      {
        InitializeDynamicDirectory ();

        MockRepository mockRepository = new MockRepository ();
        IAssemblyFinderFilter filter = mockRepository.CreateMock<IAssemblyFinderFilter> ();

        using (mockRepository.Ordered ())
        {
          Expect.Call (filter.ShouldConsiderAssembly (null)).Constraints (Rhino_Is.NotNull ()).Return (true).Repeat.Times (20, int.MaxValue);
          Expect.Call (filter.ShouldIncludeAssembly (null)).Constraints (Rhino_Is.NotNull ()).Return (false).Repeat.Times (20, int.MaxValue);
        }

        mockRepository.ReplayAll ();

        AssemblyFinder assemblyFinder = new AssemblyFinder (filter, true);
        Assembly[] assemblies = assemblyFinder.FindAssemblies ();

        mockRepository.VerifyAll ();

        Assert.That (assemblyFinder.RootAssemblies, Is.Empty);
        Assert.That (assemblies, Is.Empty);
      }

      public void FindAssemblies_WithSpecificFilter_IncludeOnlyRoot ()
      {
        InitializeDynamicDirectory ();

        MockRepository mockRepository = new MockRepository ();
        IAssemblyFinderFilter filter = mockRepository.CreateMock<IAssemblyFinderFilter> ();

        using (mockRepository.Ordered ())
        {
          Expect.Call (filter.ShouldConsiderAssembly (null)).Constraints (Rhino_Is.NotNull()).Return (true).Repeat.Times (20, int.MaxValue); // root assemblies
          Expect.Call (filter.ShouldIncludeAssembly (null)).Constraints (Rhino_Is.NotNull ()).Return (true).Repeat.Times (20, int.MaxValue);
          Expect.Call (filter.ShouldConsiderAssembly (null)).Constraints (Rhino_Is.NotNull()).Return (false).Repeat.AtLeastOnce(); // dependencies
        }
        mockRepository.ReplayAll ();

        AssemblyFinder assemblyFinder = new AssemblyFinder (filter, true);
        Assembly[] assemblies = assemblyFinder.FindAssemblies ();

        mockRepository.VerifyAll ();

        Assert.That (assemblyFinder.RootAssemblies.Length, Is.GreaterThanOrEqualTo (20));
        Assert.That (assemblies, Is.EqualTo (assemblyFinder.RootAssemblies));
      }

      public void FindAssemblies_WithSpecificFilter_IncludeAll ()
      {
        InitializeDynamicDirectory ();

        MockRepository mockRepository = new MockRepository ();
        IAssemblyFinderFilter filter = mockRepository.CreateMock<IAssemblyFinderFilter> ();

        using (mockRepository.Unordered ())
        {
          Expect.Call (filter.ShouldConsiderAssembly (null)).Constraints (Rhino_Is.NotNull ()).Return (true).Repeat.Any ();
          Expect.Call (filter.ShouldIncludeAssembly (null)).Constraints (Rhino_Is.NotNull ()).Return (true).Repeat.Any ();
        }

        mockRepository.ReplayAll ();

        AssemblyFinder assemblyFinder = new AssemblyFinder (filter, true);
        Assembly[] assemblies = assemblyFinder.FindAssemblies ();

        mockRepository.VerifyAll ();

        Assert.That (assemblyFinder.RootAssemblies.Length, Is.GreaterThanOrEqualTo (20));
        Assert.That (assemblies.Length, Is.GreaterThan (assemblyFinder.RootAssemblies.Length));
        Assert.That (assemblyFinder.RootAssemblies, Is.SubsetOf (assemblies));
      }

      private Assembly CompileTestAssemblyInMemory (string assemblyName, params string[] referencedAssemblies)
      {
        AssemblyCompiler assemblyCompiler = AssemblyCompiler.CreateInMemoryAssemblyCompiler (
            string.Format (@"Reflection\TestAssemblies\{0}", assemblyName),
            ArrayUtility.Combine (new string[] {_markerAttributeAssemblyName + ".dll"}, referencedAssemblies));
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

    [TestFixtureTearDown]
    public void TeastFixtureTearDown ()
    {
      _testFixture.Cleanup();
    }

    [Test]
    public void Initialize_WithRootAssemblies ()
    {
      ExecuteInSeparateAppDomain (_testFixture.Initialize_WithRootAssemblies);
    }

    [Test]
    public void RootAssemblyNeedntMatchFilter ()
    {
      ExecuteInSeparateAppDomain (_testFixture.RootAssemblyNeedntMatchFilter);
    }

    [Test]
    public void Initialize_WithConsiderDynamicDirectoryTrue ()
    {
      ExecuteInSeparateAppDomain (_testFixture.Initialize_WithConsiderDynamicDirectoryTrue);
    }

    [Test]
    public void Initialize_WithConsiderDynamicDirectoryFalse ()
    {
      ExecuteInSeparateAppDomain (_testFixture.Initialize_WithConsiderDynamicDirectoryFalse);
    }

    [Test]
    public void FindAssemblies_WithRootAssemblies ()
    {
      Debugger.Break ();
      ExecuteInSeparateAppDomain (_testFixture.FindAssemblies_WithRootAssemblies);
    }

    [Test]
    public void FindAssemblies_WithSpecificFiler_ConsiderAssemblyFalse ()
    {
      ExecuteInSeparateAppDomain (_testFixture.FindAssemblies_WithSpecificFilter_ConsiderAssemblyFalse);
    }

    [Test]
    public void FindAssemblies_WithSpecificFiler_ConsiderAssemblyTrueIncludeAssemblyFalse ()
    {
      ExecuteInSeparateAppDomain (_testFixture.FindAssemblies_WithSpecificFilter_ConsiderAssemblyTrueIncludeAssemblyFalse);
    }

    [Test]
    public void FindAssemblies_WithSpecificFilter_IncludeOnlyRoot ()
    {
      ExecuteInSeparateAppDomain (_testFixture.FindAssemblies_WithSpecificFilter_IncludeOnlyRoot);
    }

    [Test]
    public void FindAssemblies_WithSpecificFilter_IncludeAll ()
    {
      ExecuteInSeparateAppDomain (_testFixture.FindAssemblies_WithSpecificFilter_IncludeAll);
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