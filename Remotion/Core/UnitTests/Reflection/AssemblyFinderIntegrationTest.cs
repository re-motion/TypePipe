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
using NUnit.Framework.SyntaxHelpers;
using Remotion.Development.UnitTesting;
using Remotion.Reflection;
using Remotion.Utilities;
using Rhino.Mocks;
using Rhino_Is = Rhino.Mocks.Constraints.Is;

namespace Remotion.UnitTests.Reflection
{
  [TestFixture]
  [Serializable]
  public class AssemblyFinderIntegrationTest
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

    public string RelativeSearchPathDirectory
    {
      get { return string.Join (";", new[] {_relativeSearchPathDirectoryForDlls, _relativeSearchPathDirectoryForExes}); }
    }

    public string DynamicDirectory
    {
      get { return _dynamicDirectory; }
    }

    [TestFixtureSetUp]
    public void TestFixtureSetUp ()
    {
      _relativeSearchPathDirectoryForDlls = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "Reflection.AssemblyFinderIntegrationTest.Dlls");
      _relativeSearchPathDirectoryForExes = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "Reflection.AssemblyFinderIntegrationTest.Exes");
      _dynamicDirectory = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "Reflection.AssemblyFinderIntegrationTest.Dynamic");

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
    }

    private FilteringAssemblyLoader CreateLoaderForMarkedAssemblies ()
    {
      var markerAttributeType = typeof (MarkerAttribute);
      var attributeFilter = new AttributeAssemblyLoaderFilter (markerAttributeType);
      return new FilteringAssemblyLoader (attributeFilter);
    }

    [TestFixtureTearDown]
    public void TeastFixtureTearDown ()
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

    [Test]
    public void FindRootAssemblies_ForAppDomain_WithConsiderDynamicDirectoryTrue ()
    {
      ExecuteInSeparateAppDomain (delegate
      {
        Assembly firstInMemoryAssembly = CompileTestAssemblyInMemory ("FirstInMemoryAssembly", _markedReferencedAssemblyName + ".dll");
        Assembly secondInMemoryAssembly = CompileTestAssemblyInMemory ("SecondInMemoryAssembly");
        CompileTestAssemblyInMemory ("UnmarkedInMemoryAssembly");

        InitializeDynamicDirectory ();

        FilteringAssemblyLoader loader = CreateLoaderForMarkedAssemblies ();
        var rootAssemblyFinder = SearchPathRootAssemblyFinder.CreateForCurrentAppDomain (true);
        var rootAssemblies = rootAssemblyFinder.FindRootAssemblies (loader);

        Assert.That (rootAssemblies, List.Not.Contains (firstInMemoryAssembly));
        Assert.That (rootAssemblies, List.Not.Contains (secondInMemoryAssembly));

        Assert.That (
            rootAssemblies,
            Is.EquivalentTo (
                new[]
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
      });
    }

    [Test]
    public void FindRootAssemblies_WithConsiderDynamicDirectoryFalse ()
    {
      ExecuteInSeparateAppDomain (delegate
      {
        Assembly firstInMemoryAssembly = CompileTestAssemblyInMemory ("FirstInMemoryAssembly", _markedReferencedAssemblyName + ".dll");
        Assembly secondInMemoryAssembly = CompileTestAssemblyInMemory ("SecondInMemoryAssembly");
        CompileTestAssemblyInMemory ("UnmarkedInMemoryAssembly");

        InitializeDynamicDirectory ();

        FilteringAssemblyLoader loader = CreateLoaderForMarkedAssemblies ();
        var rootAssemblyFinder = SearchPathRootAssemblyFinder.CreateForCurrentAppDomain (false);
        var rootAssemblies = rootAssemblyFinder.FindRootAssemblies (loader);

        Assert.That (rootAssemblies, List.Not.Contains (firstInMemoryAssembly));
        Assert.That (rootAssemblies, List.Not.Contains (secondInMemoryAssembly));

        Assert.That (
            rootAssemblies,
            Is.EquivalentTo (
                new[]
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
      });
    }

    [Test]
    public void FindAssemblies_References ()
    {
      ExecuteInSeparateAppDomain (delegate
      {
        Assembly markedAssembly = Assembly.Load (_markedAssemblyName);

        var loader = CreateLoaderForMarkedAssemblies ();
        var rootAssemblyFinderStub = MockRepository.GenerateStub<IRootAssemblyFinder> ();
        rootAssemblyFinderStub.Stub (stub => stub.FindRootAssemblies (loader)).Return (new[] { markedAssembly });
        rootAssemblyFinderStub.Replay ();

        var assemblyFinder = new AssemblyFinder (rootAssemblyFinderStub, loader);

        Assembly[] assemblies = assemblyFinder.FindAssemblies ();
        Assert.That (assemblies, Is.EquivalentTo (new[] { markedAssembly, Assembly.Load (_markedReferencedAssemblyName) }));
      });
    }

    [Test]
    public void FindAssemblies_WithSpecificFiler_ConsiderAssemblyFalse ()
    {
      ExecuteInSeparateAppDomain (delegate
      {
        InitializeDynamicDirectory ();

        var filterMock = new MockRepository ().StrictMock<IAssemblyLoaderFilter> ();
        filterMock.Expect (mock => mock.ShouldConsiderAssembly (Arg<AssemblyName>.Is.Anything)).Return (false).Repeat.AtLeastOnce ();
        filterMock.Replay ();

        var loader = new FilteringAssemblyLoader (filterMock);
        var rootAssemblyFinder = SearchPathRootAssemblyFinder.CreateForCurrentAppDomain (true);
        var assemblyFinder = new AssemblyFinder (rootAssemblyFinder, loader);
        
        Assembly[] assemblies = assemblyFinder.FindAssemblies ();

        filterMock.VerifyAllExpectations ();
        Assert.That (assemblies, Is.Empty);
      });
    }

    [Test]
    public void FindAssemblies_WithSpecificFiler_ConsiderAssemblyTrueIncludeAssemblyFalse ()
    {
      ExecuteInSeparateAppDomain (delegate
      {
        InitializeDynamicDirectory ();

        var filterMock = new MockRepository ().StrictMock<IAssemblyLoaderFilter> ();
        filterMock.Expect (mock => mock.ShouldConsiderAssembly (Arg<AssemblyName>.Is.NotNull)).Return (true).Repeat.AtLeastOnce ();
        filterMock.Expect (mock => mock.ShouldIncludeAssembly (Arg<Assembly>.Is.NotNull)).Return (false).Repeat.AtLeastOnce ();
        filterMock.Replay ();

        var loader = new FilteringAssemblyLoader (filterMock);
        var rootAssemblyFinder = SearchPathRootAssemblyFinder.CreateForCurrentAppDomain (true);
        var assemblyFinder = new AssemblyFinder (rootAssemblyFinder, loader);

        Assembly[] assemblies = assemblyFinder.FindAssemblies ();

        filterMock.VerifyAllExpectations();
        Assert.That (assemblies, Is.Empty);
      });
    }

    private void ExecuteInSeparateAppDomain (CrossAppDomainDelegate test)
    {
      AppDomain appDomain = null;

      try
      {
        var appDomainSetup = new AppDomainSetup();
        appDomainSetup.ApplicationName = "Test";
        appDomainSetup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        appDomainSetup.PrivateBinPath = RelativeSearchPathDirectory;
        appDomainSetup.DynamicBase = DynamicDirectory;
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

    private Assembly CompileTestAssemblyInMemory (string assemblyName, params string[] referencedAssemblies)
    {
      AssemblyCompiler assemblyCompiler = AssemblyCompiler.CreateInMemoryAssemblyCompiler (
          string.Format (@"Reflection\TestAssemblies\{0}", assemblyName),
          ArrayUtility.Combine (new[] { typeof (MarkerAttribute).Module.Name }, referencedAssemblies));
      assemblyCompiler.Compile ();
      return assemblyCompiler.CompiledAssembly;
    }

    private string CompileTestAssemblyInSeparateAppDomain (
        string directory,
        string assemblyName,
        string extension,
        params string[] referncedAssemblies)
    {
      string outputAssembly = string.Format (@"Reflection.TestAssemblies.{0}.{1}", assemblyName, extension);
      var assemblyCompiler = new AssemblyCompiler (
          string.Format (@"Reflection\TestAssemblies\{0}", assemblyName),
          Path.Combine (directory, outputAssembly),
          ArrayUtility.Combine (new[] { typeof (MarkerAttribute).Module.Name }, referncedAssemblies));
      assemblyCompiler.CompileInSeparateAppDomain ();
      return Path.GetFileNameWithoutExtension (outputAssembly);
    }

    private void ResetDirectory (string directory)
    {
      if (Directory.Exists (directory))
        Directory.Delete (directory, true);
      Directory.CreateDirectory (directory);
    }
  }
}
