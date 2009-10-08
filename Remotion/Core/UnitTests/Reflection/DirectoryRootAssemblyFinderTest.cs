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
using Remotion.Reflection;
using Rhino.Mocks;

namespace Remotion.UnitTests.Reflection
{
  [TestFixture]
  public class DirectoryRootAssemblyFinderTest
  {
    private string _searchPath;
    private Assembly _assembly1;
    private Assembly _assembly2;

    [SetUp]
    public void SetUp ()
    {
      _searchPath = Path.Combine (Path.GetTempPath (), "DirectoryRootAssemblyFinderTest");
      if (Directory.Exists (_searchPath))
        Directory.Delete (_searchPath, true);
      Directory.CreateDirectory (_searchPath);

      _assembly1 = typeof (object).Assembly;
      _assembly2 = typeof (CompositeRootAssemblyFinder).Assembly;
    }

    [TearDown]
    public void TearDown ()
    {
      Directory.Delete (_searchPath, true);
    }

    [Test]
    public void FindRootAssemblies_LoadsExeFiles ()
    {
      var path1 = CreateFile ("Test1.exe");
      var path2 = CreateFile ("Test2.exe");

      var loaderMock = MockRepository.GenerateMock<IAssemblyLoader> ();
      loaderMock.Expect (mock => mock.LoadAssemblies (Arg<string[]>.List.Equal (new[] { path1, path2 }))).Return (new[] { _assembly1 });
      loaderMock.Replay ();

      var finder = new DirectoryRootAssemblyFinder (_searchPath);
      Assert.That (finder.FindRootAssemblies (loaderMock), Is.EquivalentTo (new[] { _assembly1 }));

      loaderMock.VerifyAllExpectations ();
    }

    [Test]
    public void FindRootAssemblies_LoadsDllFiles ()
    {
      var path1 = CreateFile ("Test1.dll");
      var path2 = CreateFile ("Test2.dll");

      var loaderMock = MockRepository.GenerateMock<IAssemblyLoader> ();
      loaderMock.Expect (mock => mock.LoadAssemblies (Arg<string[]>.List.Equal (new[] { path1, path2 }))).Return (new[] { _assembly1 });
      loaderMock.Replay ();

      var finder = new DirectoryRootAssemblyFinder (_searchPath);
      Assert.That (finder.FindRootAssemblies (loaderMock), Is.EquivalentTo (new[] { _assembly1 }));

      loaderMock.VerifyAllExpectations ();
    }

    [Test]
    public void FindRootAssemblies_CombinesFiles ()
    {
      var path1 = CreateFile ("Test1.exe");
      var path2 = CreateFile ("Test2.dll");

      var loaderMock = MockRepository.GenerateMock<IAssemblyLoader> ();
      loaderMock.Expect (mock => mock.LoadAssemblies (Arg<string[]>.List.Equal (new[] { path1 }))).Return (new[] { _assembly1 });
      loaderMock.Expect (mock => mock.LoadAssemblies (Arg<string[]>.List.Equal (new[] { path2 }))).Return (new[] { _assembly2 });
      loaderMock.Replay ();

      var finder = new DirectoryRootAssemblyFinder (_searchPath);
      Assert.That (finder.FindRootAssemblies (loaderMock), Is.EquivalentTo (new[] { _assembly1, _assembly2 }));

      loaderMock.VerifyAllExpectations ();
    }

    [Test]
    public void FindRootAssemblies_DoesNotLoadOtherFiles ()
    {
      CreateFile ("Test1");
      CreateFile ("Test2.txt");

      var loaderMock = MockRepository.GenerateMock<IAssemblyLoader> ();

      var finder = new DirectoryRootAssemblyFinder (_searchPath);
      Assert.That (finder.FindRootAssemblies (loaderMock), Is.Empty);

      loaderMock.AssertWasNotCalled (mock => mock.LoadAssemblies (Arg<string[]>.Is.Anything));
    }

    private string CreateFile (string fileName)
    {
      string fullPath = Path.Combine (_searchPath, fileName);
      using (File.CreateText (fullPath))
      {
      }
      return fullPath;
    }

  }
}