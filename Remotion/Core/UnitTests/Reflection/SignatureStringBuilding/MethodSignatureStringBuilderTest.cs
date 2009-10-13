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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Reflection.SignatureStringBuilding;
using Remotion.UnitTests.Reflection.SignatureStringBuilding.TestDomain;

namespace Remotion.UnitTests.Reflection.SignatureStringBuilding
{
  [TestFixture]
  public class MethodSignatureStringBuilderTest
  {
    private MethodSignatureStringBuilder _builder;

    [SetUp]
    public void SetUp ()
    {
      _builder = new MethodSignatureStringBuilder ();
    }

    [Test]
    public void BuildSignatureString_NoParameters ()
    {
      var method = typeof (ClassForMethodSignatureStringBuilding<,>).GetMethod ("MethodWithoutParameters");
      var signature = _builder.BuildSignatureString (method);

      Assert.That (signature, Is.EqualTo ("System.Void()"));
    }

    [Test]
    public void BuildSignatureString_WithParameters ()
    {
      var method = typeof (ClassForMethodSignatureStringBuilding<,>).GetMethod ("MethodWithParameters");
      var signature = _builder.BuildSignatureString (method);

      Assert.That (signature, Is.EqualTo ("System.Void(System.String,System.DateTime)"));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Closed generic methods are not supported.\r\nParameter name: methodInfo")]
    public void BuildSignatureString_ClosedGenericMethod ()
    {
      var method = typeof (ClassForMethodSignatureStringBuilding<,>).GetMethod ("MethodWithGenericParameters").MakeGenericMethod (typeof (int), typeof (int));
      _builder.BuildSignatureString (method);
    }

    [Test]
    public void BuildSignatureString_WithGenericParameters ()
    {
      var method = typeof (ClassForMethodSignatureStringBuilding<,>).GetMethod ("MethodWithGenericParameters");
      var signature = _builder.BuildSignatureString (method);

      Assert.That (signature, Is.EqualTo ("System.Void(System.String,System.DateTime)`2"));
    }
  }
}