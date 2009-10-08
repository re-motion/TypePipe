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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Reflection;
using Rhino.Mocks;
using System.Linq;

namespace Remotion.UnitTests.Reflection
{
  [TestFixture]
  public class SearchPathRootAssemblyFinderTest
  {
    private IAssemblyLoader _loaderStub;

    [SetUp]
    public void SetUp ()
    {
      _loaderStub = MockRepository.GenerateStub<IAssemblyLoader> ();
    }

    [Test]
    public void CreateCombinedFinder_HoldsBaseDirectory ()
    {
      var finder = new SearchPathRootAssemblyFinder (_loaderStub, "baseDirectory", "relativeSearchPath", false, "dynamicDirectory");
      var finderDirectories = GetDirectoriesForCombinedFinder (finder);

      Assert.That (finderDirectories, List.Contains ("baseDirectory"));
    }

    [Test]
    public void CreateCombinedFinder_HoldsRelativeSearchPath ()
    {
      var finder = new SearchPathRootAssemblyFinder (_loaderStub, "baseDirectory", "relativeSearchPath", false, "dynamicDirectory");
      var finderDirectories = GetDirectoriesForCombinedFinder (finder);

      Assert.That (finderDirectories, List.Contains ("relativeSearchPath"));
    }

    [Test]
    public void CreateCombinedFinder_HoldsRelativeSearchPath_Split ()
    {
      var finder = new SearchPathRootAssemblyFinder (_loaderStub, "baseDirectory", "relativeSearchPath1;relativeSearchPath2", false, "dynamicDirectory");
      var finderDirectories = GetDirectoriesForCombinedFinder (finder);

      Assert.That (finderDirectories, List.Contains ("relativeSearchPath1"));
      Assert.That (finderDirectories, List.Contains ("relativeSearchPath2"));
    }

    [Test]
    public void CreateCombinedFinder_ConsiderDynamicDirectory_False ()
    {
      var finder = new SearchPathRootAssemblyFinder (_loaderStub, "baseDirectory", "relativeSearchPath", false, "dynamicDirectory");
      var finderDirectories = GetDirectoriesForCombinedFinder (finder);

      Assert.That (finderDirectories, List.Not.Contains ("dynamicDirectory"));
    }

    [Test]
    public void CreateCombinedFinder_ConsiderDynamicDirectory_True ()
    {
      var finder = new SearchPathRootAssemblyFinder (_loaderStub, "baseDirectory", "relativeSearchPath", true, "dynamicDirectory");
      var finderDirectories = GetDirectoriesForCombinedFinder (finder);

      Assert.That (finderDirectories, List.Contains ("dynamicDirectory"));
    }

    [Test]
    public void FindRootAssemblies_UsesCombinedFinder ()
    {
      var innerFinderStub = MockRepository.GenerateStub<IRootAssemblyFinder> ();
      innerFinderStub.Stub (stub => stub.FindRootAssemblies ()).Return (new[] { typeof (object).Assembly });
      innerFinderStub.Replay ();

      var finderMock = new MockRepository ().PartialMock<SearchPathRootAssemblyFinder> (
          _loaderStub,
          "baseDirectory",
          "relativeSearchPath",
          false,
          "dynamicDirectory");
      finderMock.Expect (mock => mock.CreateCombinedFinder ()).Return (new CompositeRootAssemblyFinder (new[] { innerFinderStub }));
      finderMock.Replay ();

      var result = finderMock.FindRootAssemblies ();
      Assert.That (result, Is.EqualTo (new[] { typeof (object).Assembly }));

      finderMock.VerifyAllExpectations ();
    }

    private string[] GetDirectoriesForCombinedFinder (SearchPathRootAssemblyFinder finder)
    {
      var combinedFinder = finder.CreateCombinedFinder ();
      return combinedFinder.InnerFinders.Cast<DirectoryRootAssemblyFinder> ().Select (f => f.SearchPath).ToArray ();
    }
  }
}