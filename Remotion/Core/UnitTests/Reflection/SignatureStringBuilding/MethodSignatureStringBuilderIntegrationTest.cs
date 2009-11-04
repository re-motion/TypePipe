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
using NUnit.Framework.SyntaxHelpers;
using Remotion.Reflection.SignatureStringBuilding;
using Remotion.UnitTests.Reflection.SignatureStringBuilding.TestDomain;

namespace Remotion.UnitTests.Reflection.SignatureStringBuilding
{
  [TestFixture]
  public class MethodSignatureStringBuilderIntegrationTest
  {
    private MethodSignatureStringBuilder _builder;

    [SetUp]
    public void SetUp ()
    {
      _builder = new MethodSignatureStringBuilder ();
    }

    [Test]
    public void MethodSignatureString_ClosedGenericType ()
    {
      var method = typeof (ClassForMethodSignatureStringBuilding<int, string>).GetMethod ("MethodWithClosedGenericType");
      var signature = _builder.BuildSignatureString (method);

      Assert.That (
          signature,
          Is.EqualTo (
              "System.Void"
              + "(Remotion.UnitTests.Reflection.SignatureStringBuilding.TestDomain.ClassForMethodSignatureStringBuilding`2[System.Int32,System.String])"));
    }

    [Test]
    public void MethodSignatureString_NestedType ()
    {
      var method = typeof (ClassForMethodSignatureStringBuilding<,>).GetMethod ("MethodWithNestedType");
      var signature = _builder.BuildSignatureString (method);

      Assert.That (
          signature,
          Is.EqualTo (
              "System.Void(Remotion.UnitTests.Reflection.SignatureStringBuilding.TestDomain.ClassForMethodSignatureStringBuilding`2+Nested"
              + "[[0/Remotion.UnitTests.Reflection.SignatureStringBuilding.TestDomain.ClassForMethodSignatureStringBuilding`2],"
              + "[1/Remotion.UnitTests.Reflection.SignatureStringBuilding.TestDomain.ClassForMethodSignatureStringBuilding`2]])"));
    }

    [Test]
    public void MethodSignatureString_NestedGenericType ()
    {
      var method = typeof (ClassForMethodSignatureStringBuilding<,>).GetMethod ("MethodWithNestedGenericType");
      var signature = _builder.BuildSignatureString (method);

      Assert.That (
          signature,
          Is.EqualTo (
              "System.Void(Remotion.UnitTests.Reflection.SignatureStringBuilding.TestDomain.ClassForMethodSignatureStringBuilding`2+NestedGeneric`1"
              + "[[0/Remotion.UnitTests.Reflection.SignatureStringBuilding.TestDomain.ClassForMethodSignatureStringBuilding`2],"
              + "[1/Remotion.UnitTests.Reflection.SignatureStringBuilding.TestDomain.ClassForMethodSignatureStringBuilding`2],"
              + "System.Int32])"));
    }

    [Test]
    public void MethodSignatureString_NestedGenericType_ClosedWithDifferentTypeParameters ()
    {
      var method = typeof (ClassForMethodSignatureStringBuilding<,>).GetMethod ("MethodWithPartiallyClosedNestedGenericType");
      var signature = _builder.BuildSignatureString (method);

      Assert.That (
          signature,
          Is.EqualTo (
              "System.Void("
              + "Remotion.UnitTests.Reflection.SignatureStringBuilding.TestDomain.ClassForMethodSignatureStringBuilding`2+NestedGeneric`1"
              + "[[0/Remotion.UnitTests.Reflection.SignatureStringBuilding.TestDomain.ClassForMethodSignatureStringBuilding`2],"
              + "[1/Remotion.UnitTests.Reflection.SignatureStringBuilding.TestDomain.ClassForMethodSignatureStringBuilding`2],"
              + "[0]],"
              + "Remotion.UnitTests.Reflection.SignatureStringBuilding.TestDomain.ClassForMethodSignatureStringBuilding`2+NestedGeneric`1"
              + "[[0/Remotion.UnitTests.Reflection.SignatureStringBuilding.TestDomain.ClassForMethodSignatureStringBuilding`2],"
              + "[1/Remotion.UnitTests.Reflection.SignatureStringBuilding.TestDomain.ClassForMethodSignatureStringBuilding`2],"
              + "[0/Remotion.UnitTests.Reflection.SignatureStringBuilding.TestDomain.ClassForMethodSignatureStringBuilding`2]])`1"));
    }

    [Test]
    public void MethodSignatureString_NestedGenericType_ClosedWithOuterTypeParameters ()
    {
      var method = typeof (ClassForMethodSignatureStringBuilding<int, string>).GetMethod ("MethodWithPartiallyClosedNestedGenericType");
      var signature = _builder.BuildSignatureString (method);

      Assert.That (
          signature,
          Is.EqualTo (
              "System.Void("
              + "Remotion.UnitTests.Reflection.SignatureStringBuilding.TestDomain.ClassForMethodSignatureStringBuilding`2+NestedGeneric`1[System.Int32,System.String,[0]],"
              +
              "Remotion.UnitTests.Reflection.SignatureStringBuilding.TestDomain.ClassForMethodSignatureStringBuilding`2+NestedGeneric`1[System.Int32,System.String,System.Int32]"
              + ")`1"));
    }
  }
}
