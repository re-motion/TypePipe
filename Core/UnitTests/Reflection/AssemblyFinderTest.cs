/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Web;
using System.Xml;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Mixins;
using Remotion.Reflection;
using Remotion.Utilities;
using Rhino.Mocks;
using Remotion.Development.UnitTesting;
using Mocks_Property = Rhino.Mocks.Constraints.Property;
using Mocks_Is = Rhino.Mocks.Constraints.Is;

namespace Remotion.UnitTests.Reflection
{
  [TestFixture]
  public class AssemblyFinderTest
  {
    private MockRepository _mockRepository;
    private IAssemblyFinderFilter _filterMock;
    private Assembly _coreUnitTestsAssembly;
    private Assembly _coreAssembly;
    private Assembly _mscorlibAssembly;
    private Assembly _developmentAssembly;

    [SetUp]
    public void SetUp ()
    {
      _mockRepository = new MockRepository ();
      _filterMock = _mockRepository.CreateMock<IAssemblyFinderFilter> ();

      _coreUnitTestsAssembly = typeof (AssemblyFinderTest).Assembly; // Core.UnitTests
      _coreAssembly = typeof (AssemblyFinder).Assembly; // Core
      _mscorlibAssembly = typeof (object).Assembly; // mscorlib
      _developmentAssembly = typeof (Dev).Assembly; // Development
    }

    [Test]
    public void Initialization_WithRootAssemblies ()
    {
      AssemblyFinder finder = new AssemblyFinder (_filterMock, _coreUnitTestsAssembly, _coreAssembly);
      Assert.That (finder.Filter, Is.SameAs (_filterMock));
      Assert.That (finder.GetRootAssemblies(), Is.EquivalentTo (new object[] { _coreUnitTestsAssembly, _coreAssembly }));
      Assert.That (finder.Loader, Is.Not.Null);
    }

    [Test]
    public void Initialization_WithoutRootAssemblies ()
    {
      AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;
      setup.PrivateBinPath = "a;b;c";
      setup.DynamicBase = Path.GetTempPath();

      AppDomainRunner runner = new AppDomainRunner (setup, delegate
      {
        IAssemblyFinderFilter filter = ApplicationAssemblyFinderFilter.Instance;
        AssemblyFinder finder = new AssemblyFinder (filter, true);
        Assert.That (finder.Filter, Is.SameAs (filter));
        Assert.That (finder.Loader, Is.Not.Null);
        Assert.That (finder.BaseDirectory, Is.EqualTo (AppDomain.CurrentDomain.BaseDirectory));
        Assert.That (finder.RelativeSearchPath, Is.EqualTo (AppDomain.CurrentDomain.RelativeSearchPath));
        Assert.That (finder.DynamicDirectory, Is.EqualTo (AppDomain.CurrentDomain.DynamicDirectory));
      }, new object[0]);
      
      runner.Run ();
    }

    [Test]
    public void FindRootAssemblies_LoadsAssembliesFromDifferentDirectories ()
    {
      TestAssemblyFinder finder = _mockRepository.PartialMock<TestAssemblyFinder> (_filterMock, true, "base", "relative1;relative2", "xy");

      Expect.Call (PrivateInvoke.InvokeNonPublicMethod (finder, "FindAssembliesInPath", "base")).Return (new Assembly[] { _coreUnitTestsAssembly });
      Expect.Call (PrivateInvoke.InvokeNonPublicMethod (finder, "FindAssembliesInPath", "relative1")).Return (new Assembly[] { _coreAssembly });
      Expect.Call (PrivateInvoke.InvokeNonPublicMethod (finder, "FindAssembliesInPath", "relative2")).Return (new Assembly[] { _mscorlibAssembly });
      Expect.Call (PrivateInvoke.InvokeNonPublicMethod (finder, "FindAssembliesInPath", "xy")).Return (new Assembly[] { _developmentAssembly });

      _mockRepository.ReplayAll ();
      Assembly[] result = finder.GetRootAssemblies ();
      Assert.That (result, Is.EquivalentTo (new[] { _coreUnitTestsAssembly, _coreAssembly, _mscorlibAssembly, _developmentAssembly }));
      _mockRepository.VerifyAll ();
    }

    [Test]
    public void FindRootAssemblies_LoadsAssembliesFromDifferentDirectories_ConsiderDynamicFalse ()
    {
      TestAssemblyFinder finder = _mockRepository.PartialMock<TestAssemblyFinder> (_filterMock, false, "base", "relative1;relative2", "xy");

      Expect.Call (PrivateInvoke.InvokeNonPublicMethod (finder, "FindAssembliesInPath", "base")).Return (new Assembly[] { _coreUnitTestsAssembly });
      Expect.Call (PrivateInvoke.InvokeNonPublicMethod (finder, "FindAssembliesInPath", "relative1")).Return (new Assembly[] { _coreAssembly });
      Expect.Call (PrivateInvoke.InvokeNonPublicMethod (finder, "FindAssembliesInPath", "relative2")).Return (new Assembly[] { _mscorlibAssembly });

      _mockRepository.ReplayAll ();
      Assembly[] result = finder.GetRootAssemblies ();
      Assert.That (result, Is.EquivalentTo (new[] { _coreUnitTestsAssembly, _coreAssembly, _mscorlibAssembly }));
      _mockRepository.VerifyAll ();
    }

    [Test]
    public void FindRootAssemblies_LoadsAssembliesFromDifferentDirectories_NullDirectories ()
    {
      TestAssemblyFinder finder = _mockRepository.PartialMock<TestAssemblyFinder> (_filterMock, true, "base", null, null);

      Expect.Call (PrivateInvoke.InvokeNonPublicMethod (finder, "FindAssembliesInPath", "base")).Return (new Assembly[] { _coreUnitTestsAssembly });

      _mockRepository.ReplayAll ();
      Assembly[] result = finder.GetRootAssemblies ();
      Assert.That (result, Is.EquivalentTo (new[] { _coreUnitTestsAssembly }));
      _mockRepository.VerifyAll ();
    }

    [Test]
    public void FindReferencedAssemblies ()
    {
      AssemblyLoader loaderMock = _mockRepository.CreateMock<AssemblyLoader> (_filterMock);
      AssemblyFinder finder = new AssemblyFinder (_filterMock, _coreAssembly);
      finder.Loader = loaderMock;

      // references of Remotion.dll
      Expect.Call (loaderMock.TryLoadAssembly (null, null)).Constraints ( // Castle.DynamicProxy.dll
          Mocks_Property.Value ("FullName", typeof (Castle.DynamicProxy.Generators.Emitters.ClassEmitter).Assembly.FullName),
          Mocks_Is.Equal (_coreAssembly.FullName)).Return (typeof (Castle.DynamicProxy.Generators.Emitters.ClassEmitter).Assembly);
      Expect.Call (loaderMock.TryLoadAssembly (null, null)).Constraints ( // Remotion.Interfaces.dll
          Mocks_Property.Value ("FullName", typeof (ObjectFactory).Assembly.FullName),
          Mocks_Is.Equal (_coreAssembly.FullName)).Return (null);
      Expect.Call (loaderMock.TryLoadAssembly (null, null)).Constraints ( // log4net.dll
          Mocks_Property.Value ("FullName", typeof (log4net.ILog).Assembly.FullName),
          Mocks_Is.Equal (_coreAssembly.FullName)).Return (null);
      Expect.Call (loaderMock.TryLoadAssembly (null, null)).Constraints ( // mscorlib.dll
          Mocks_Property.Value ("FullName", typeof (object).Assembly.FullName),
          Mocks_Is.Equal (_coreAssembly.FullName)).Return (null);
      Expect.Call (loaderMock.TryLoadAssembly (null, null)).Constraints ( // System.dll
          Mocks_Property.Value ("FullName", typeof (Uri).Assembly.FullName),
          Mocks_Is.Equal (_coreAssembly.FullName)).Return (null);
      Expect.Call (loaderMock.TryLoadAssembly (null, null)).Constraints ( // System.Configuration.dll
          Mocks_Property.Value ("FullName", typeof (ConfigurationSection).Assembly.FullName),
          Mocks_Is.Equal (_coreAssembly.FullName)).Return (null);
      Expect.Call (loaderMock.TryLoadAssembly (null, null)).Constraints ( // System.Xml.dll
          Mocks_Property.Value ("FullName", typeof (XmlElement).Assembly.FullName),
          Mocks_Is.Equal (_coreAssembly.FullName)).Return (null);

      // references of Castle.DynamicProxy.dll not yet processed
      Expect.Call (loaderMock.TryLoadAssembly (null, null)).Constraints ( // Castle.Core.dll
          Mocks_Property.Value ("FullName", typeof (Castle.Core.Interceptor.IInterceptor).Assembly.FullName),
          Mocks_Is.Equal (typeof (Castle.DynamicProxy.Generators.Emitters.ClassEmitter).Assembly.FullName))
          .Return (typeof (Castle.Core.Interceptor.IInterceptor).Assembly);

      // references of Castle.Core.dll not yet processed
      Expect.Call (loaderMock.TryLoadAssembly (null, null)).Constraints ( // Castle.Core.dll
          Mocks_Property.Value ("FullName", typeof (HttpContext).Assembly.FullName),
          Mocks_Is.Equal (typeof (Castle.Core.Interceptor.IInterceptor).Assembly.FullName))
          .Return (null);

      _mockRepository.ReplayAll ();
      
      Assembly[] assemblies = finder.FindAssemblies ();
      Assert.That (assemblies, Is.EquivalentTo (new object[] { 
          _coreAssembly, 
          typeof (Castle.DynamicProxy.Generators.Emitters.ClassEmitter).Assembly, 
          typeof (Castle.Core.Interceptor.IInterceptor).Assembly }
      ));
      _mockRepository.VerifyAll ();
    }

    [Test]
    public void FindAssembliesInPath ()
    {
      AssemblyLoader loaderMock = _mockRepository.CreateMock<AssemblyLoader> (_filterMock);
      TestAssemblyFinder finder = new TestAssemblyFinder (_filterMock, _coreAssembly);
      finder.Loader = loaderMock;

      const string path = "AssemblyFinderTest.ExesAndDlls";
      if (Directory.Exists (path))
        Directory.Delete (path, true);
      
      Directory.CreateDirectory (path);
      try
      {
        string exe1 = CreateFile (path, "1.exe");
        string exe2 = CreateFile (path, "2.exe");
        string dll1 = CreateFile (path, "1.dll");
        string dll2 = CreateFile (path, "2.dll");
        string dll3 = CreateFile (path, "3.dll");

        Expect.Call (loaderMock.LoadAssemblies (exe1, exe2)).Return (new Assembly[] { _coreUnitTestsAssembly });
        Expect.Call (loaderMock.LoadAssemblies (dll1, dll2, dll3)).Return (new Assembly[] { _coreAssembly });

        _mockRepository.ReplayAll ();

        IEnumerable<Assembly> result = finder.FindAssembliesInPath (path);
        Assert.That (EnumerableUtility.ToArray (result), Is.EquivalentTo (new object[] { _coreUnitTestsAssembly, _coreAssembly }));

        _mockRepository.VerifyAll ();
      }
      finally
      {
        Directory.Delete (path, true);
      }
    }

    private string CreateFile (string path, string fileName)
    {
      string fullPath = Path.Combine (path, fileName);
      using (File.CreateText (fullPath))
      {
      }
      return fullPath;
    }
  }
}