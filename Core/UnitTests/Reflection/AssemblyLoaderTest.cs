// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Development.UnitTesting;
using Remotion.Reflection;
using Remotion.Utilities;
using Rhino.Mocks;
using Mocks_Property = Rhino.Mocks.Constraints.Property;

namespace Remotion.UnitTests.Reflection
{
  [TestFixture]
  [Serializable]
  public class AssemblyLoaderTest
  {
    private MockRepository _mockRepository;
    private IAssemblyFinderFilter _filterMock;
    private AssemblyLoader _loader;

    [SetUp]
    public void SetUp ()
    {
      _mockRepository = new MockRepository();
      _filterMock = _mockRepository.StrictMock<IAssemblyFinderFilter>();
      _loader = new AssemblyLoader (_filterMock);
    }

    [Test]
    public void TryLoadAssembly ()
    {
      SetupFilterTrue();

      Assembly referenceAssembly = typeof (AssemblyLoaderTest).Assembly;
      string path = new Uri (referenceAssembly.EscapedCodeBase).AbsolutePath;
      Assembly loadedAssembly = _loader.TryLoadAssembly (path);
      Assert.That (loadedAssembly, Is.SameAs (referenceAssembly));
    }

    [Test]
    public void TryLoadAssembly_FilterConsiderTrue_IncludeTrue ()
    {
      Assembly referenceAssembly = typeof (AssemblyLoaderTest).Assembly;
      string path = new Uri (referenceAssembly.EscapedCodeBase).AbsolutePath;

      Expect.Call (_filterMock.ShouldConsiderAssembly (null))
          .Constraints (Mocks_Property.Value ("FullName", referenceAssembly.FullName))
          .Return (true);
      Expect.Call (_filterMock.ShouldIncludeAssembly (null))
          .Constraints (Mocks_Property.Value ("FullName", referenceAssembly.FullName))
          .Return (true);

      _mockRepository.ReplayAll();
      Assembly loadedAssembly = _loader.TryLoadAssembly (path);
      Assert.That (loadedAssembly, Is.SameAs (referenceAssembly));
      _mockRepository.VerifyAll();
    }

    [Test]
    public void TryLoadAssembly_FilterConsiderTrue_IncludeFalse ()
    {
      Assembly referenceAssembly = typeof (AssemblyLoaderTest).Assembly;
      string path = new Uri (referenceAssembly.EscapedCodeBase).AbsolutePath;

      Expect.Call (_filterMock.ShouldConsiderAssembly (null))
          .Constraints (Mocks_Property.Value ("FullName", referenceAssembly.FullName))
          .Return (true);
      Expect.Call (_filterMock.ShouldIncludeAssembly (null))
          .Constraints (Mocks_Property.Value ("FullName", referenceAssembly.FullName))
          .Return (false);

      _mockRepository.ReplayAll();
      Assembly loadedAssembly = _loader.TryLoadAssembly (path);
      Assert.That (loadedAssembly, Is.Null);
      _mockRepository.VerifyAll();
    }

    [Test]
    public void TryLoadAssembly_FilterConsiderFalse ()
    {
      Assembly referenceAssembly = typeof (AssemblyLoaderTest).Assembly;
      string path = new Uri (referenceAssembly.EscapedCodeBase).AbsolutePath;

      Expect.Call (_filterMock.ShouldConsiderAssembly (null))
          .Constraints (Mocks_Property.Value ("FullName", referenceAssembly.FullName))
          .Return (false);

      _mockRepository.ReplayAll();
      Assembly loadedAssembly = _loader.TryLoadAssembly (path);
      Assert.That (loadedAssembly, Is.Null);
      _mockRepository.VerifyAll();
    }

    [Test]
    public void TryLoadAssembly_WithBadImageFormatException ()
    {
      SetupFilterTrue();

      const string path = "Invalid.dll";
      using (File.CreateText (path))
      {
        // no contents
      }

      try
      {
        Assembly loadedAssembly = _loader.TryLoadAssembly (path);
        Assert.That (loadedAssembly, Is.Null);
      }
      finally
      {
        FileUtility.DeleteAndWaitForCompletion (path);
      }
    }

    // Assembly.Load will lock a file when it throws a FileLoadException, making it impossible to restore the previous state
    // for naive tests. We therefore run the actual test in another process using Process.Start; that way, the locked file
    // will be unlocked when the process exits and we can delete it after the test has run.
    [Test]
    public void TryLoadAssembly_WithFileLoadException ()
    {
      string program = Compile (
          "Reflection\\TestAssemblies\\FileLoadExceptionConsoleApplication", "FileLoadExceptionConsoleApplication.exe", true, null);
      string delaySignAssembly = Compile ("Reflection\\TestAssemblies\\DelaySignAssembly", "DelaySignAssembly.dll", false, "/delaysign+ /keyfile:Reflection\\TestAssemblies\\DelaySignAssembly\\PublicKey.snk");

      try
      {
        ProcessStartInfo startInfo = new ProcessStartInfo (program);
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;
        startInfo.RedirectStandardOutput = true;
        startInfo.Arguments = delaySignAssembly + " false";

        Process process = Process.Start (startInfo);
        string output = process.StandardOutput.ReadToEnd ();
        process.WaitForExit ();
        Assert.That (process.ExitCode, Is.EqualTo (0), output);
      }
      finally
      {
        FileUtility.DeleteAndWaitForCompletion (program);
        FileUtility.DeleteAndWaitForCompletion (delaySignAssembly);
      }
    }

    [Test]
    public void TryLoadAssembly_WithFileLoadException_AndShadowCopying ()
    {
      string program = Compile (
          "Reflection\\TestAssemblies\\FileLoadExceptionConsoleApplication", "FileLoadExceptionConsoleApplication.exe", true, null);
      string delaySignAssembly = Compile ("Reflection\\TestAssemblies\\DelaySignAssembly", "DelaySignAssembly.dll", false, "/delaysign+ /keyfile:Reflection\\TestAssemblies\\DelaySignAssembly\\PublicKey.snk");

      try
      {
        ProcessStartInfo startInfo = new ProcessStartInfo (program);
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;
        startInfo.RedirectStandardOutput = true;
        startInfo.Arguments = delaySignAssembly + " true";

        Process process = Process.Start (startInfo);
        string output = process.StandardOutput.ReadToEnd ();
        process.WaitForExit ();
        Assert.That (process.ExitCode, Is.EqualTo (0), output);
      }
      finally
      {
        FileUtility.DeleteAndWaitForCompletion (program);
        FileUtility.DeleteAndWaitForCompletion (delaySignAssembly);
      }
    }

    [Test]
    public void LoadAssemblies ()
    {
      Assembly referenceAssembly1 = typeof (AssemblyLoaderTest).Assembly;
      Assembly referenceAssembly2 = typeof (AssemblyLoader).Assembly;

      AssemblyLoader loaderPartialMock = _mockRepository.PartialMock<AssemblyLoader> (_filterMock);
      Expect.Call (loaderPartialMock.TryLoadAssembly ("abc")).Return (null);
      Expect.Call (loaderPartialMock.TryLoadAssembly ("def")).Return (referenceAssembly1);
      Expect.Call (loaderPartialMock.TryLoadAssembly ("ghi")).Return (null);
      Expect.Call (loaderPartialMock.TryLoadAssembly ("jkl")).Return (referenceAssembly2);

      _mockRepository.ReplayAll();

      IEnumerable<Assembly> assemblies = loaderPartialMock.LoadAssemblies ("abc", "def", "ghi", "jkl");
      Assert.That (EnumerableUtility.ToArray (assemblies), Is.EqualTo (new object[] { referenceAssembly1, referenceAssembly2 }));
      _mockRepository.VerifyAll();
    }

    private void SetupFilterTrue ()
    {
      SetupResult.For (_filterMock.ShouldConsiderAssembly (null)).IgnoreArguments().Return (true);
      SetupResult.For (_filterMock.ShouldIncludeAssembly (null)).IgnoreArguments().Return (true);

      _mockRepository.ReplayAll();
    }


    private string Compile (string sourceDirectory, string outputAssemblyName, bool generateExecutable, string compilerOptions)
    {
      AssemblyCompiler compiler = new AssemblyCompiler (sourceDirectory, outputAssemblyName, typeof (AssemblyLoader).Assembly.Location);

      compiler.CompilerParameters.GenerateExecutable = generateExecutable;
      compiler.CompilerParameters.CompilerOptions = compilerOptions;
      
      compiler.Compile();
      return compiler.OutputAssemblyPath;
    }
  }
}
