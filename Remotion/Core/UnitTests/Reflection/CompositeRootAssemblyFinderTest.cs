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
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Reflection;
using Rhino.Mocks;

namespace Remotion.UnitTests.Reflection
{
  [TestFixture]
  public class CompositeRootAssemblyFinderTest
  {
    private Assembly _assembly1;
    private Assembly _assembly2;
    private Assembly _assembly3;
    private IAssemblyLoader _loaderStub;

    [SetUp]
    public void SetUp ()
    {
      _assembly1 = typeof (object).Assembly;
      _assembly2 = typeof (CompositeRootAssemblyFinder).Assembly;
      _assembly3 = typeof (CompositeRootAssemblyFinderTest).Assembly;
      _loaderStub = MockRepository.GenerateStub<IAssemblyLoader> ();
    }

    [Test]
    public void FindRootAssemblies_NoInnerFinders ()
    {
      var finder = new CompositeRootAssemblyFinder (new IRootAssemblyFinder[0]);

      var rootAssemblies = finder.FindRootAssemblies(_loaderStub);
      Assert.That (rootAssemblies, Is.Empty);
    }

    [Test]
    public void FindRootAssemblies_InnerFinder ()
    {
      IRootAssemblyFinder innerFinderStub = CreateInnerFinderStub(_assembly1, _assembly2);
      var finder = new CompositeRootAssemblyFinder (new[] { innerFinderStub });

      var rootAssemblies = finder.FindRootAssemblies (_loaderStub);
      Assert.That (rootAssemblies, Is.EquivalentTo (new[] { _assembly1, _assembly2 }));
    }

    [Test]
    public void FindRootAssemblies_MultipleInnerFinders ()
    {
      IRootAssemblyFinder innerFinderStub1 = CreateInnerFinderStub (_assembly1, _assembly2);
      IRootAssemblyFinder innerFinderStub2 = CreateInnerFinderStub (_assembly3);

      var finder = new CompositeRootAssemblyFinder (new[] { innerFinderStub1, innerFinderStub2 });

      var rootAssemblies = finder.FindRootAssemblies (_loaderStub);
      Assert.That (rootAssemblies, Is.EquivalentTo (new[] { _assembly1, _assembly2, _assembly3 }));
    }

    [Test]
    public void FindRootAssemblies_RemovesDuplicates ()
    {
      IRootAssemblyFinder innerFinderStub1 = CreateInnerFinderStub (_assembly1, _assembly2, _assembly2);
      IRootAssemblyFinder innerFinderStub2 = CreateInnerFinderStub (_assembly3, _assembly2, _assembly1);

      var finder = new CompositeRootAssemblyFinder (new[] { innerFinderStub1, innerFinderStub2 });

      var rootAssemblies = finder.FindRootAssemblies (_loaderStub);
      Assert.That (rootAssemblies, Is.EquivalentTo (new[] { _assembly1, _assembly2, _assembly3 }));
    }

    private IRootAssemblyFinder CreateInnerFinderStub (params Assembly[] assemblies)
    {
      var innerFinderStub = MockRepository.GenerateStub<IRootAssemblyFinder> ();
      innerFinderStub.Stub (stub => stub.FindRootAssemblies (_loaderStub)).Return (assemblies);
      innerFinderStub.Replay ();
      return innerFinderStub;
    }
  }
}