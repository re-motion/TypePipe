// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using NUnit.Framework;
using Remotion.Reflection;
using Rhino.Mocks;

namespace Remotion.UnitTests.Reflection
{
  [TestFixture]
  public class ReflectionExtensionsTest
  {
    [Test]
    public void IsOriginalDeclaration_DeclaringTypeEqualsOrignalDeclaringType_True ()
    {
      var memberInfoStub = MockRepository.GenerateStub<IMemberInformation>();
      memberInfoStub.Stub (stub => stub.DeclaringType).Return (typeof (string));
      memberInfoStub.Stub (stub => stub.GetOriginalDeclaringType()).Return (typeof (string));
      Assert.That (memberInfoStub.IsOriginalDeclaration(), Is.True);
    }

    [Test]
    public void IsOriginalDeclaration_DeclaringTypeNotEqualToOrignalDeclaringType_False ()
    {
      var memberInfoStub = MockRepository.GenerateStub<IMemberInformation> ();
      memberInfoStub.Stub (stub => stub.DeclaringType).Return (typeof (string));
      memberInfoStub.Stub (stub => stub.GetOriginalDeclaringType ()).Return (typeof (object));

      Assert.That (memberInfoStub.IsOriginalDeclaration (), Is.False);
    }

    [Test]
    public void IsOriginalDeclaration_DeclaringTypeIsNull_False ()
    {
      var memberInfoStub = MockRepository.GenerateStub<IMemberInformation> ();
      memberInfoStub.Stub (stub => stub.DeclaringType).Return (null);
      memberInfoStub.Stub (stub => stub.GetOriginalDeclaringType ()).Return (typeof (object));

      Assert.That (memberInfoStub.IsOriginalDeclaration (), Is.False);
    }

    [Test]
    public void IsOriginalDeclaration_OrignalDeclaringTypeIsNull_False ()
    {
      var memberInfoStub = MockRepository.GenerateStub<IMemberInformation> ();
      memberInfoStub.Stub (stub => stub.DeclaringType).Return (typeof (string));
      memberInfoStub.Stub (stub => stub.GetOriginalDeclaringType ()).Return (null);

      Assert.That (memberInfoStub.IsOriginalDeclaration (), Is.False);
    }

    [Test]
    public void IsOriginalDeclaration_DeclaringTypeIsNullAndOrignalDeclaringTypeIsNull_True ()
    {
      var memberInfoStub = MockRepository.GenerateStub<IMemberInformation> ();
      memberInfoStub.Stub (stub => stub.DeclaringType).Return (null);
      memberInfoStub.Stub (stub => stub.GetOriginalDeclaringType ()).Return (null);

      Assert.That (memberInfoStub.IsOriginalDeclaration (), Is.True);
    }
  }
}