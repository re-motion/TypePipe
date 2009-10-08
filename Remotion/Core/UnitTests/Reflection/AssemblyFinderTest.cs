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
using System.Linq;

namespace Remotion.UnitTests.Reflection
{
  [TestFixture]
  public class AssemblyFinderTest
  {
    private Assembly _assembly1;
    private Assembly _assembly2;
    private Assembly _assembly3;

    [SetUp]
    public void SetUp ()
    {
      _assembly1 = typeof (object).Assembly;
      _assembly2 = typeof (AssemblyFinder).Assembly;
      _assembly3 = typeof (AssemblyFinderTest).Assembly;
    }

    [Test]
    public void FindAssemblies_FindsRootAssemblies ()
    {
      var loaderStub = MockRepository.GenerateStub<IAssemblyLoader> ();
      loaderStub.Replay ();

      var rootAssemblyFinderMock = MockRepository.GenerateMock<IRootAssemblyFinder> ();
      rootAssemblyFinderMock.Expect (mock => mock.FindRootAssemblies (loaderStub)).Return (new[] { _assembly1, _assembly2 });
      rootAssemblyFinderMock.Replay ();
      
      var finder = new AssemblyFinder (rootAssemblyFinderMock, loaderStub);
      var result = finder.FindAssemblies ();

      rootAssemblyFinderMock.VerifyAllExpectations ();
      Assert.That (result, Is.EquivalentTo (new[] { _assembly1, _assembly2 }));
    }

    [Test]
    public void FindAssemblies_FindsReferencedAssemblies ()
    {
      var loaderMock = MockRepository.GenerateMock<IAssemblyLoader> ();
      loaderMock
          .Expect (mock => mock.TryLoadAssembly (ArgReferenceMatchesDefinition (_assembly1), Arg.Is (_assembly3.FullName)))
          .Return (_assembly1);
      loaderMock
        .Expect (mock => mock.TryLoadAssembly (ArgReferenceMatchesDefinition (_assembly2), Arg.Is (_assembly3.FullName)))
        .Return (_assembly2);
      loaderMock.Replay ();
      
      var rootAssemblyFinderStub = MockRepository.GenerateStub<IRootAssemblyFinder> ();
      rootAssemblyFinderStub.Stub (stub => stub.FindRootAssemblies (loaderMock)).Return (new[] { _assembly3 });
      rootAssemblyFinderStub.Replay ();
      
      var finder = new AssemblyFinder (rootAssemblyFinderStub, loaderMock);
      var result = finder.FindAssemblies ();

      loaderMock.VerifyAllExpectations ();
      Assert.That (result, Is.EquivalentTo (new[] { _assembly1, _assembly2, _assembly3 }));
    }

    [Test]
    public void FindAssemblies_FindsReferencedAssemblies_Transitive ()
    {
      var mixinSamplesAssembly = typeof (Remotion.Mixins.Samples.EquatableMixin<>).Assembly;
      var remotionAssembly = typeof (AssemblyFinder).Assembly;
      var log4netAssembly = typeof (log4net.LogManager).Assembly;

      // dependency chain: mixinSamples -> remotion -> log4net
      Assert.That (IsAssemblyReferencedBy (remotionAssembly, mixinSamplesAssembly), Is.True);
      Assert.That (IsAssemblyReferencedBy (log4netAssembly, mixinSamplesAssembly), Is.False);
      Assert.That (IsAssemblyReferencedBy (log4netAssembly, remotionAssembly), Is.True);

      var loaderMock = MockRepository.GenerateMock<IAssemblyLoader> ();
      loaderMock
          .Expect (mock => mock.TryLoadAssembly (ArgReferenceMatchesDefinition (remotionAssembly), Arg.Is (mixinSamplesAssembly.FullName))) // load re-motion via samples
          .Return (remotionAssembly);
      loaderMock
        .Expect (mock => mock.TryLoadAssembly (ArgReferenceMatchesDefinition (log4netAssembly), Arg.Is (remotionAssembly.FullName))) // load log4net via re-motion
        .Return (log4netAssembly);
      loaderMock.Replay ();
      
      var rootAssemblyFinderStub = MockRepository.GenerateMock<IRootAssemblyFinder> ();
      rootAssemblyFinderStub.Stub (stub => stub.FindRootAssemblies (loaderMock)).Return (new[] { mixinSamplesAssembly });
      rootAssemblyFinderStub.Replay ();

      var finder = new AssemblyFinder (rootAssemblyFinderStub, loaderMock);
      var result = finder.FindAssemblies ();

      loaderMock.VerifyAllExpectations ();
      Assert.That (result, Is.EquivalentTo (new[] { mixinSamplesAssembly, remotionAssembly, log4netAssembly }));
    }

    [Test]
    public void FindAssemblies_FindsReferencedAssemblies_Transitive_NotTwice ()
    {
      // dependency chain: _assembly3 -> _assembly2 -> _assembly1; _assembly3 -> _assembly1
      Assert.That (IsAssemblyReferencedBy (_assembly2, _assembly3), Is.True);
      Assert.That (IsAssemblyReferencedBy (_assembly1, _assembly3), Is.True);
      Assert.That (IsAssemblyReferencedBy (_assembly1, _assembly2), Is.True);

      // because _assembly1 is already loaded via _assembly3, it's not tried again via _assembly2

      var loaderMock = MockRepository.GenerateMock<IAssemblyLoader> ();
      loaderMock
          .Expect (mock => mock.TryLoadAssembly (ArgReferenceMatchesDefinition (_assembly2), Arg.Is (_assembly3.FullName))) // load _assembly2 via _assembly3
          .Return (_assembly2);
      loaderMock
          .Expect (mock => mock.TryLoadAssembly (ArgReferenceMatchesDefinition (_assembly1), Arg.Is (_assembly3.FullName))) // load _assembly1 via _assembly3
          .Return (null);
      loaderMock
        .Expect (mock => mock.TryLoadAssembly (ArgReferenceMatchesDefinition (_assembly1), Arg.Is (_assembly2.FullName))) // _assembly1 already loaded, no second time
        .Repeat.Never ()
        .Return (_assembly2);
      loaderMock.Replay ();
      
      var rootAssemblyFinderStub = MockRepository.GenerateMock<IRootAssemblyFinder> ();
      rootAssemblyFinderStub.Stub (stub => stub.FindRootAssemblies (loaderMock)).Return (new[] { _assembly3 });
      rootAssemblyFinderStub.Replay ();

      var finder = new AssemblyFinder (rootAssemblyFinderStub, loaderMock);
      finder.FindAssemblies ();

      loaderMock.VerifyAllExpectations ();
    }

    [Test]
    public void FindAssemblies_NoDuplicates ()
    {
      var loaderMock = MockRepository.GenerateMock<IAssemblyLoader> ();
      loaderMock
          .Expect (mock => mock.TryLoadAssembly (ArgReferenceMatchesDefinition (_assembly2), Arg.Is (_assembly3.FullName)))
          .Return (_assembly2);
      loaderMock.Replay ();

      var rootAssemblyFinderStub = MockRepository.GenerateMock<IRootAssemblyFinder> ();
      rootAssemblyFinderStub.Stub (stub => stub.FindRootAssemblies (loaderMock)).Return (new[] { _assembly3, _assembly2 });
      rootAssemblyFinderStub.Replay ();

      var finder = new AssemblyFinder (rootAssemblyFinderStub, loaderMock);
      var result = finder.FindAssemblies ();

      loaderMock.VerifyAllExpectations ();
      Assert.That (result, Is.EquivalentTo (new[] { _assembly2, _assembly3 }));
      Assert.That (result.Length, Is.EqualTo (2));
    }

    private AssemblyName ArgReferenceMatchesDefinition (Assembly referencedAssembly)
    {
      return Arg<AssemblyName>.Matches (name => AssemblyName.ReferenceMatchesDefinition (name, referencedAssembly.GetName()));
    }

    private bool IsAssemblyReferencedBy (Assembly referenced, Assembly origin)
    {
      return origin.GetReferencedAssemblies ()
          .Where (assemblyName => AssemblyName.ReferenceMatchesDefinition (assemblyName, referenced.GetName ()))
          .Any ();
    }
  }
}
