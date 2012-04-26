// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using Remotion.Reflection.MemberSignatures;
using Remotion.Reflection.MemberSignatures.SignatureStringBuilding;
using Remotion.UnitTests.Reflection.MemberSignatures.SignatureStringBuilding.TestDomain;

namespace Remotion.UnitTests.Reflection.MemberSignatures.SignatureStringBuilding
{
  [TestFixture]
  public class PropertySignatureStringBuilderTest
  {
    private PropertySignatureStringBuilder _builder;

    [SetUp]
    public void SetUp ()
    {
      _builder = new PropertySignatureStringBuilder ();
    }

    [Test]
    public void BuildSignatureString_PropertyInfo_NoParameters ()
    {
      var property = typeof (ClassForPropertySignatureStringBuilding).GetProperty ("PropertyWithoutParameters");
      var signature = _builder.BuildSignatureString (property);

      Assert.That (signature, Is.EqualTo ("System.Int32()"));
    }

    [Test]
    public void BuildSignatureString_PropertyInfo_WithParameters ()
    {
      var property = typeof (ClassForPropertySignatureStringBuilding).GetProperty ("Item");
      var signature = _builder.BuildSignatureString (property);

      Assert.That (signature, Is.EqualTo ("System.String(System.Int32,System.Double)"));
    }

    [Test]
    public void BuildSignatureString_ExplicitSignature_NoParameters ()
    {
      var propertyType = typeof (int);
      var indexParameterTypes = Type.EmptyTypes;
      var propertySignature = new PropertySignature(propertyType, indexParameterTypes);

      var signature = _builder.BuildSignatureString (propertySignature);

      Assert.That (signature, Is.EqualTo ("System.Int32()"));
    }

    [Test]
    public void BuildSignatureString_ExplicitSignature_WithParameters ()
    {
      var propertyType = typeof (string);
      var indexParameterTypes = new[] { typeof (int), typeof (double) };
      var propertySignature = new PropertySignature(propertyType, indexParameterTypes);

      var signature = _builder.BuildSignatureString (propertySignature);

      Assert.That (signature, Is.EqualTo ("System.String(System.Int32,System.Double)"));
    }
  }
}
